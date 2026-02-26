# Research: Facility & Equipment Management

**Branch**: `001-equipment-management` | **Date**: 2026-02-25  
**Phase**: 0 — Pre-design research

---

## Research 1: ProcessParameters Storage Strategy (JSONB vs. Owned Entity vs. Rows Table)

**Decision:** Raw JSONB column with `HasColumnType("jsonb")` + `HasConversion<string>()` via `System.Text.Json`

**Rationale:**
- `Dictionary<string, string>` is **not supported** by EF Core's `ToJson()` / `OwnsOne()` approach — EF requires statically-typed owned entity shapes, not open-ended dictionaries. This is a hard blocker for Option 2.
- The composite B-tree index on `(Technology, Status)` lives on regular typed columns and is completely unaffected by the JSONB storage choice.
- JSONB is schema-free by design, which is correct for technology-specific parameters that vary by enum value. Adding or removing parameter keys never generates an EF Core migration — only a code change to the domain validator.
- A GIN index (`jsonb_path_ops`) on `ProcessParameters` can be added later if containment queries are needed; premature for the initial implementation.
- Validation of required keys per technology type (e.g., FDM must have `VolumetricFlowRateMm3PerSec` and `MinLayerTimeSec`) is enforced in the Application layer by a per-technology validator — not at the DB schema level.

**Alternatives considered:**
- **`ToJson()` owned entity**: Ruled out — requires concrete strongly-typed class per technology; incompatible with `Dictionary<string, string>`.
- **Separate `ProcessParameter` rows table**: Better for cross-equipment queries by parameter key. Rejected because parameters are always read/written as a unit per machine, and a join table adds unnecessary complexity for this access pattern.
- **PostgreSQL `CHECK` constraint per technology**: Viable complement to JSONB but deferred — application-layer validation is sufficient for Phase 1.

---

## Research 2: PostgreSQL xmin Optimistic Concurrency in EF Core 10

**Decision:** `uint` property + `IsRowVersion()` only. Omit `HasColumnType("xid")` — Npgsql infers it. Use global exception middleware to map `DbUpdateConcurrencyException` → HTTP 409. Expose `rowVersion` in DTOs. Use MassTransit EF Core Bus Outbox.

**Rationale:**
- `HasColumnType("xid")` is redundant — Npgsql's EF Core provider automatically maps `uint` + `IsRowVersion()` to the PostgreSQL `xid` type. The older `ForNpgsqlUseXminAsConcurrencyToken()` extension is obsolete as of Npgsql 6+.
- `DbUpdateConcurrencyException` must be caught and translated to HTTP 409 Conflict per MALIEV architectural standards. Best placement: global problem-details exception middleware in `Program.cs`, not in individual controllers.
- The xmin token **must** cross the wire in API responses (as `rowVersion: uint`) and be required on all update/patch requests so the server can set `OriginalValues` before calling `SaveChanges`. Shadow-only concurrency tokens only work for internal-only entities that never leave the service boundary.
- The field must be named `rowVersion` (not `xmin`) in DTOs — leaking the internal column name is an implementation detail exposure.

**Alternatives considered:**
- **`byte[]` RowVersion**: Works in SQL Server; incorrect for PostgreSQL xmin which is a 32-bit unsigned integer.
- **ETag header pattern**: More RESTful for external APIs; adds complexity unnecessary for internal service APIs. Deferred.

---

## Research 3: IJobServiceClient Resilience Pattern

**Decision:** Fail-safe (block delete when JobService is unreachable). No manual Polly config — inherited from `AddServiceDefaults()`. HTTP 503 on unavailability; HTTP 409 if jobs exist.

**Rationale:**
- FR-006 and SC-006 mandate that hard delete is rejected 100% of the time if job history exists. The asymmetry of consequences demands fail-safe: a transient 503 is a minor inconvenience; a false-allow is an irreversible audit breach.
- `AddServiceDefaults()` already applies `AddStandardResilienceHandler` globally (30s attempt timeout, 60s total, 3 retries with exponential backoff, circuit breaker at 50% failure over 10+ requests). No per-client Polly configuration is needed or permitted.
- Client registration: `AddServiceClient<IJobServiceClient, JobServiceClient>("JobService")` — inherits all platform resilience automatically.

**HTTP response semantics:**

| Response | Meaning | Action |
|---|---|---|
| `200 OK` + non-empty | Jobs exist | Block delete → HTTP 409 |
| `200 OK` + empty | No jobs | Allow delete |
| `404 Not Found` | No records (treat as empty) | Allow delete |
| `5xx` / timeout / circuit open | JobService unavailable | Block delete → HTTP 503 |
| `401 / 403` | Auth misconfiguration | Block delete → log ERROR → HTTP 500 |

**Note:** The `GET /job/v1/jobs/history?equipmentId={id}` endpoint does not yet exist in JobService — it must be added as part of the cross-service contract before this feature is fully implemented.

**Alternatives considered:**
- **Fail-open**: Rejected — allows destructive operation under uncertainty; violates FR-006 and SC-006.
- **Cache last-known state**: Rejected — introduces stale data risk for a safety gate; complexity not justified.

---

## Research 4: MassTransit Transactional Outbox for Status Events

**Decision:** Use `MassTransit.EntityFrameworkCore` with `UsePostgres()` + `UseBusOutbox()`. Register `AddEntityFrameworkOutbox<FacilityDbContext>` inside the `AddMassTransitWithRabbitMq` configure lambda.

**Rationale:**
- Publishing `EquipmentStatusChangedEvent` directly to RabbitMQ after `SaveChangesAsync` without an outbox is a dual-write anti-pattern: a crash or broker outage between the two calls loses the event silently.
- SC-003 mandates delivery within 5 seconds of the status save — silent loss violates this criterion.
- Three existing MALIEV services (`CareerService`, `LifecycleService`, `SupplierService`) already use `MassTransit.EntityFrameworkCore [8.5.7, 9.0.0)` with `Npgsql.EFCore.PostgreSQL 10.0.0` — compatibility is proven.
- The overhead for 1–2 events per day is immeasurable: two extra tables (`OutboxMessage`, `OutboxState`) that will be near-empty at all times.
- `AddServiceDefaults()` does NOT configure the outbox — each service opts in by calling `AddEntityFrameworkOutbox<TDbContext>` in its own configure lambda (correct pattern; the platform cannot know which `DbContext` to bind).

**Implementation steps:**
1. Add `MassTransit.EntityFrameworkCore` to `Infrastructure.csproj`.
2. Call `AddEntityFrameworkOutbox<FacilityDbContext>` in the `configure:` lambda passed to `AddMassTransitWithRabbitMq`.
3. Call `modelBuilder.AddOutboxMessageEntity()` + `modelBuilder.AddOutboxStateEntity()` in `FacilityDbContext.OnModelCreating`.
4. Generate EF Core migration for the two outbox tables.

**Alternatives considered:**
- **Direct publish without outbox**: Rejected — silent event loss on broker unavailability violates SC-003.
- **`ITransactionalBus` + manual transaction**: Works but requires every use case to manage transaction scope explicitly — error-prone and non-idiomatic.
- **Polling reconciliation job**: Far more complex, introduces polling delay; still does not guarantee the 5s delivery target.

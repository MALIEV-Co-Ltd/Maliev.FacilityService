# Messaging Contracts: Facility & Equipment Management

**Branch**: `001-equipment-management` | **Date**: 2026-02-25 (Revised)
**Contract source of truth**: `Maliev.MessagingContracts` repository
**Transport**: RabbitMQ via MassTransit
**Delivery guarantee**: At-least-once via MassTransit EF Core Bus Outbox

---

## Events Published by FacilityService

### EquipmentStatusChangedEvent

**Published when**: A machine's `EquipmentStatus` transitions to any new value (`Active` ↔ `UnderMaintenance`, `OnLoan`, `Lost`, or `Decommissioned`).
**Consumed by**: `JobService` — to block new job assignments to unavailable machines and re-queue affected jobs.
**Delivery**: Transactional outbox — event is written atomically in the same `SaveChanges` transaction as the status update, then delivered to RabbitMQ by the background delivery service.

**JSON Schema** (to be added to `Maliev.MessagingContracts/contracts/schemas/facility/equipment-status-changed-event.json`):

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "EquipmentStatusChangedEvent",
  "description": "Published when a piece of equipment changes its operational status",
  "allOf": [
    { "$ref": "../shared/base-message.json" },
    {
      "type": "object",
      "properties": {
        "publishedBy":    { "type": "string", "const": "facility-service" },
        "consumedBy":     { "type": "array",  "const": ["job-service"] },
        "messageType":    { "type": "string", "const": "Event" },
        "messageVersion": { "type": "string", "const": "1.0" },
        "messageName":    { "type": "string", "const": "EquipmentStatusChangedEvent" },
        "payload": {
          "type": "object",
          "properties": {
            "equipmentId":    { "type": "string", "format": "uuid" },
            "assetCode":     { "type": "string" },
            "name":          { "type": "string" },
            "category":      { "type": "string", "enum": ["FdmPrinter", "SlaPrinter", "CncMachine", "Scanner3D", "InjectionMolding", "OfficeEquipment", "MeasuringEquipment", "ITEquipment", "HandTool", "Other"] },
            "previousStatus": { "type": "string", "enum": ["Active", "UnderMaintenance", "OnLoan", "Lost", "Decommissioned"] },
            "newStatus":      { "type": "string", "enum": ["Active", "UnderMaintenance", "OnLoan", "Lost", "Decommissioned"] }
          },
          "required": ["equipmentId", "assetCode", "name", "category", "previousStatus", "newStatus"]
        }
      }
    }
  ]
}
```

**Example payload:**
```json
{
  "messageId": "01HXYZ...",
  "correlationId": "01HABC...",
  "publishedBy": "facility-service",
  "consumedBy": ["job-service"],
  "messageType": "Event",
  "messageVersion": "1.0",
  "messageName": "EquipmentStatusChangedEvent",
  "sentAt": "2026-02-25T14:32:00Z",
  "payload": {
    "equipmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "assetCode": "MAL-CNC-0001",
    "name": "Haas VF-2",
    "category": "CncMachine",
    "previousStatus": "Active",
    "newStatus": "OnLoan"
  }
}
```

---

### LoanDocumentRequestedEvent

**Published when**: A customer loan is approved (`PendingApproval` → `Active` transition).
**Consumed by**: `Maliev.PdfService` — to generate a loan document PDF with borrower details, equipment details, loan terms, and return conditions.
**Delivery**: Transactional outbox — event is written atomically in the same `SaveChanges` transaction as the loan approval.

**JSON Schema** (to be added to `Maliev.MessagingContracts/contracts/schemas/facility/loan-document-requested-event.json`):

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "LoanDocumentRequestedEvent",
  "description": "Published when a customer loan is approved and a loan document PDF is needed",
  "allOf": [
    { "$ref": "../shared/base-message.json" },
    {
      "type": "object",
      "properties": {
        "publishedBy":    { "type": "string", "const": "facility-service" },
        "consumedBy":     { "type": "array",  "const": ["pdf-service"] },
        "messageType":    { "type": "string", "const": "Event" },
        "messageVersion": { "type": "string", "const": "1.0" },
        "messageName":    { "type": "string", "const": "LoanDocumentRequestedEvent" },
        "payload": {
          "type": "object",
          "properties": {
            "loanId":             { "type": "string", "format": "uuid" },
            "equipmentId":        { "type": "string", "format": "uuid" },
            "assetCode":          { "type": "string" },
            "equipmentName":      { "type": "string" },
            "brand":              { "type": "string" },
            "modelName":          { "type": "string" },
            "manufacturerSerial": { "type": "string" },
            "borrowerId":         { "type": "string", "format": "uuid" },
            "borrowerName":       { "type": "string" },
            "borrowerType":       { "type": "string", "const": "Customer" },
            "approvedByEmployeeId": { "type": "string", "format": "uuid" },
            "loanStartDate":      { "type": "string", "format": "date" },
            "expectedReturnDate": { "type": "string", "format": "date" },
            "purpose":            { "type": "string" },
            "documentLanguage":   { "type": "string", "const": "th-TH" }
          },
          "required": ["loanId", "equipmentId", "assetCode", "equipmentName", "borrowerId", "borrowerName", "borrowerType", "approvedByEmployeeId", "loanStartDate", "expectedReturnDate", "purpose", "documentLanguage"]
        }
      }
    }
  ]
}
```

**Example payload:**
```json
{
  "messageId": "01HXYZ...",
  "correlationId": "01HDEF...",
  "publishedBy": "facility-service",
  "consumedBy": ["pdf-service"],
  "messageType": "Event",
  "messageVersion": "1.0",
  "messageName": "LoanDocumentRequestedEvent",
  "sentAt": "2026-02-25T14:35:00Z",
  "payload": {
    "loanId": "3fa85f64-5717-4562-b3fc-2c963f66afa9",
    "equipmentId": "3fa85f64-5717-4562-b3fc-2c963f66afab",
    "assetCode": "MAL-INJ-0001",
    "equipmentName": "Arburg Allrounder 570",
    "brand": "Arburg",
    "modelName": "Allrounder 570",
    "manufacturerSerialNumber": "ARBURG-2023-0042",
    "borrowerId": "3fa85f64-5717-4562-b3fc-2c963f66afac",
    "borrowerName": "ABC Manufacturing Co., Ltd.",
    "borrowerType": "Customer",
    "approvedByEmployeeId": "3fa85f64-5717-4562-b3fc-2c963f66afad",
    "loanStartDate": "2026-02-25",
    "expectedReturnDate": "2026-03-25",
    "purpose": "Customer unit under repair — temporary loan of in-house machine",
    "documentLanguage": "th-TH"
  }
}
```

---

## Events Consumed by FacilityService

None in the initial implementation. FacilityService is a data source, not a reactor.

---

## Cross-Service HTTP Contract: JobService History Endpoint

FacilityService calls JobService via `IJobServiceClient` before allowing a hard delete.

**Required endpoint** (must be added to JobService before FacilityService delete is enabled):

```
GET /job/v1/jobs/history?equipmentId={guid}
```

**Permission required by caller**: `job.jobs.read` (FacilityService service identity must be granted this)

**Expected responses:**

| Response | Body | FacilityService interpretation |
|---|---|---|
| `200 OK` | `{ "hasHistory": true }` or non-empty list | Block delete → HTTP 409 |
| `200 OK` | `{ "hasHistory": false }` or empty list | Allow delete |
| `404 Not Found` | any | Treat as "no history" → allow delete |
| `5xx` | any | Block delete → HTTP 503 (fail-safe) |

**Note**: This endpoint is a dependency that blocks the delete functionality. It must be coordinated with the JobService team before the `DELETE /facility/v1/equipments/{id}` endpoint can be fully released.

---

## MassTransit Outbox Configuration Reference

```
Service:        FacilityService
DbContext:      FacilityDbContext
Outbox type:    Entity Framework Core Bus Outbox (UsePostgres)
Package:        MassTransit.EntityFrameworkCore [8.5.7, 9.0.0)
Tables added:   outbox_message, outbox_state
Registration:   AddEntityFrameworkOutbox<FacilityDbContext> inside AddMassTransitWithRabbitMq configure lambda
```

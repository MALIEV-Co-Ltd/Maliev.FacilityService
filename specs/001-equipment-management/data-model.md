# Data Model: Facility & Equipment Management

**Branch**: `001-equipment-management` | **Date**: 2026-02-25
**Phase**: 1 — Design (Revised)

---

## Architecture: Table-Per-Type (TPT)

FacilityService uses EF Core's **Table-Per-Type** pattern to ensure a clean database schema with zero nullable manufacturing columns on general equipment rows.

- **Base table**: `equipments` — contains all common fields for every asset type
- **Spec tables**: One per manufacturing subtype — `fdm_printer_specs`, `sla_printer_specs`, `cnc_machine_specs`, `scanner_3d_specs`, `injection_molding_specs`
- **General equipment subtypes** (`OfficeEquipmentItem`, `MeasuringEquipmentItem`, `ITEquipmentItem`, `HandToolItem`, `OtherEquipmentItem`) — **no spec table**, only the base `equipments` row

---

## Base Entity: Equipment

### Table: `equipments`

| Field                      | Type            | Constraints | Notes                                                                                         |
| -------------------------- | --------------- | ----------- | --------------------------------------------------------------------------------------------- |
| `Id`                       | `Guid`          | PK          | System-generated                                                                             |
| `AssetCode`                | `string`        | Unique, immutable | System-generated: `MAL-{PREFIX}-{SEQ}`, per-category sequence                            |
| `Brand`                    | `string?`       | Max(100)    | e.g. "Bambu Lab", "Haas", "Creality"                                                        |
| `ModelName`                | `string?`       | Max(100)    | e.g. "X1C", "VF-2-SE", "Raptor X"                                                           |
| `Name`                     | `string`        | Required, Max(200), Unique | Human-readable label: "Prusa MK4 #1", "Caliper Mitutoyo 500-196-30"                    |
| `ManufacturerSerialNumber` | `string?`       | Max(100)    | As printed on the physical device                                                            |
| `Category`                | `EquipmentCategory` | Stored as string | Enum: `FdmPrinter`, `SlaPrinter`, `CncMachine`, `Scanner3D`, `InjectionMolding`, `OfficeEquipment`, `MeasuringEquipment`, `ITEquipment`, `HandTool`, `Other` |
| `SubCategory`              | `string?`       | Max(100)    | Only used when `Category = Other`, e.g. "Cleaning Equipment", "Pneumatics"                |
| `Status`                   | `EquipmentStatus` | Stored as string | Enum: `Active`, `UnderMaintenance`, `OnLoan`, `Lost`, `Decommissioned`                 |
| `PurchaseDate`             | `DateOnly?`     |             | Date of acquisition                                                                         |
| `PurchasePriceTHB`         | `decimal?`      | Precision(12,2) | Purchase price in Thai Baht                                                                 |
| `WarrantyExpiryDate`       | `DateOnly?`     |             | Warranty expiration date                                                                     |
| `NextServiceDueDate`       | `DateOnly?`     |             | Denormalised from latest maintenance log entry                                              |
| `CreatedAt`                | `DateTime`      | Required, UTC | Auto-set on insert                                                                          |
| `UpdatedAt`                | `DateTime`      | Required, UTC | Auto-updated on any change                                                                 |
| `xmin`                     | `uint`          | Shadow, IsRowVersion() | PostgreSQL hidden system column for optimistic concurrency        |

### Indexes

| Index Name                      | Columns                     | Type   | Purpose                                       |
| ------------------------------- | --------------------------- | ------ | --------------------------------------------- |
| `PK_equipments`                | `Id`                        | B-tree | Primary key                                   |
| `UX_equipments_asset_code`     | `AssetCode`                 | Unique | Immutable asset code uniqueness              |
| `UX_equipments_name`           | `Name`                      | Unique | Human-readable name uniqueness               |
| `IX_equipments_category`       | `Category`                  | B-tree | Filter by category                           |
| `IX_equipments_status`         | `Status`                    | B-tree | Filter by status                             |
| `IX_equipments_category_status`| `(Category, Status)`        | B-tree | Combined filter for availability queries     |

---

## Manufacturing Spec Tables (TPT)

Each spec table joins to `equipments` via `EquipmentId` as both PK and FK. Only the relevant manufacturing columns are present — no nullable manufacturing fields on general equipment rows.

### Table: `fdm_printer_specs`

| Field                 | Type     | Constraints    | Notes                                    |
| --------------------- | -------- | ------------- | ---------------------------------------- |
| `EquipmentId`         | `Guid`   | PK, FK        | References `equipments.Id`               |
| `BuildVolumeXMm`      | `decimal`| Precision(10,2) | Maximum X-axis build dimension (mm)      |
| `BuildVolumeYMm`      | `decimal`| Precision(10,2) | Maximum Y-axis build dimension (mm)      |
| `BuildVolumeZMm`      | `decimal`| Precision(10,2) | Maximum Z-axis build dimension (mm)      |
| `HourlyRateTHB`       | `decimal`| Precision(10,2), ≥ 0 | Operating cost per hour (THB)         |
| `SetupFeeTHB`         | `decimal`| Precision(10,2), ≥ 0 | Per-job setup charge (THB)            |
| `NozzleDiameterMm`    | `decimal`| Precision(5,2)  | Standard nozzle diameter                |
| `MaxNozzleTempC`      | `int`    |               | Maximum hotend temperature (°C)          |
| `NumberOfExtruders`   | `int`    | Min(1)       | Number of extruders                     |
| `MinLayerHeightMm`    | `decimal`| Precision(5,3) | Minimum printable layer height (mm)    |
| `MaxLayerHeightMm`    | `decimal`| Precision(5,3) | Maximum printable layer height (mm)    |
| `ExtendedProperties`  | `string` | JSONB         | Additional machine-specific parameters    |

### Table: `sla_printer_specs`

| Field                 | Type                      | Constraints    | Notes                                    |
| --------------------- | ------------------------- | ------------- | ---------------------------------------- |
| `EquipmentId`         | `Guid`                    | PK, FK        | References `equipments.Id`               |
| `BuildVolumeXMm`      | `decimal`                 | Precision(10,2) | Maximum X-axis build dimension (mm)      |
| `BuildVolumeYMm`      | `decimal`                 | Precision(10,2) | Maximum Y-axis build dimension (mm)      |
| `BuildVolumeZMm`      | `decimal`                 | Precision(10,2) | Maximum Z-axis build dimension (mm)      |
| `HourlyRateTHB`       | `decimal`                 | Precision(10,2), ≥ 0 | Operating cost per hour (THB)         |
| `SetupFeeTHB`         | `decimal`                 | Precision(10,2), ≥ 0 | Per-job setup charge (THB)            |
| `XyResolutionMm`     | `decimal`                 | Precision(6,3)  | XY pixel resolution (mm)                |
| `LightSourceType`     | `SlaLightSourceType`      | Stored as string | Enum: `Laser`, `Dlp`, `Lcd`           |
| `WavelengthNm`        | `int?`                    |               | Light source wavelength (nm)            |
| `ExtendedProperties` | `string`                  | JSONB         | Additional machine-specific parameters    |

### Table: `cnc_machine_specs`

| Field                    | Type                   | Constraints    | Notes                                    |
| ------------------------ | ---------------------- | ------------- | ---------------------------------------- |
| `EquipmentId`            | `Guid`                 | PK, FK        | References `equipments.Id`               |
| `XTravelMm`              | `decimal`              | Precision(10,2) | X-axis travel (mm)                       |
| `YTravelMm`              | `decimal`              | Precision(10,2) | Y-axis travel (mm)                       |
| `ZTravelMm`             | `decimal`              | Precision(10,2) | Z-axis travel (mm)                       |
| `HourlyRateTHB`          | `decimal`              | Precision(10,2), ≥ 0 | Operating cost per hour (THB)       |
| `SetupFeeTHB`            | `decimal`              | Precision(10,2), ≥ 0 | Per-job setup charge (THB)          |
| `MaxSpindleSpeedRpm`     | `int`                  |               | Maximum spindle speed                    |
| `MaxSpindlePowerKw`      | `decimal`              | Precision(5,2)  | Maximum spindle power (kW)              |
| `NumberOfAxes`           | `int`                  | Min(3)        | Number of controllable axes              |
| `ToolInterface`          | `CncToolInterface`     | Stored as string | Enum: `Bt30`, `Bt40`, `Hsk43`, `Hsk63`, `Other` |
| `MaxToolDiameterMm`     | `decimal`              | Precision(6,2)  | Maximum tool diameter (mm)             |
| `ControllerBrand`        | `string?`              | Max(50)       | e.g. "Fanuc", "Siemens", " Haas"         |
| `ExtendedProperties`     | `string`               | JSONB         | Additional machine-specific parameters    |

### Table: `scanner_3d_specs`

| Field                    | Type                      | Constraints    | Notes                                    |
| ------------------------ | ------------------------- | ------------- | ---------------------------------------- |
| `EquipmentId`            | `Guid`                    | PK, FK        | References `equipments.Id`               |
| `HourlyRateTHB`          | `decimal`                 | Precision(10,2), ≥ 0 | Operating cost per hour (THB)       |
| `SetupFeeTHB`            | `decimal`                 | Precision(10,2), ≥ 0 | Per-job setup charge (THB)          |
| `MaxScanVolumeM3`        | `decimal`                 | Precision(8,3)  | Maximum scannable volume (m³)          |
| `AccuracyMm`            | `decimal`                 | Precision(6,4)  | Manufacturer-declared accuracy (mm)     |
| `ScanResolutions`       | `string`                  | JSONB         | Array of available resolutions in mm, e.g. `[0.05, 0.1, 0.2]` |
| `ScannerTechnology`      | `Scanner3DTechnology`    | Stored as string | Enum: `Laser`, `StructuredLight`, `Lidar`, `Photogrammetry` |
| `ExtendedProperties`    | `string`                  | JSONB         | Additional scanner-specific parameters    |

### Table: `injection_molding_specs`

| Field                        | Type     | Constraints    | Notes                                    |
| ---------------------------- | -------- | ------------- | ---------------------------------------- |
| `EquipmentId`                | `Guid`   | PK, FK        | References `equipments.Id`               |
| `HourlyRateTHB`              | `decimal`| Precision(10,2), ≥ 0 | Operating cost per hour (THB)       |
| `SetupFeeTHB`                | `decimal`| Precision(10,2), ≥ 0 | Per-job setup charge (THB)          |
| `MaxMoldXMm`                | `decimal`| Precision(10,2) | Maximum X-axis mould dimension (mm)    |
| `MaxMoldYMm`                | `decimal`| Precision(10,2) | Maximum Y-axis mould dimension (mm)    |
| `MaxMoldZMm`                | `decimal`| Precision(10,2) | Maximum Z-axis mould dimension (mm)    |
| `MaxShotSizeG`              | `decimal`| Precision(8,2)  | Maximum shot size (grams)              |
| `MaxTempC`                  | `int`    |               | Maximum material temperature (°C)       |
| `MaxInjectionPressureBar`   | `int`    |               | Maximum injection pressure (bar)        |
| `ExtendedProperties`        | `string` | JSONB         | Additional machine-specific parameters    |

---

## Child Entities

### Table: `equipment_notes`

| Field            | Type     | Constraints    | Notes                                           |
| ---------------- | -------- | ------------- | ----------------------------------------------- |
| `Id`             | `Guid`   | PK            |                                                 |
| `EquipmentId`    | `Guid`   | FK → equipments.Id, Not Null | Equipment that owns this note |
| `AuthorEmployeeId` | `Guid` | Not Null      | Employee who created this note               |
| `Content`        | `string` | Required, Max(2000) | Note text content                      |
| `CreatedAt`      | `DateTime`| UTC, Not Null | Timestamp of note creation                    |

**Indexes**: FK on `EquipmentId`, CreatedAt descending for chronological listing.

### Table: `equipment_loans`

| Field                  | Type                    | Constraints    | Notes                                                    |
| ---------------------- | ----------------------- | ------------- | -------------------------------------------------------- |
| `Id`                   | `Guid`                  | PK            |                                                          |
| `EquipmentId`          | `Guid`                  | FK → equipments.Id, Not Null | Equipment being loaned |
| `BorrowerId`           | `Guid`                  | Not Null      | EmployeeId or CustomerId of the borrower               |
| `BorrowerType`         | `LoanBorrowerType`      | Stored as string | Enum: `Employee`, `Customer`                       |
| `ApprovedByEmployeeId` | `Guid?`                 |               | Required for customer loans (approver)                |
| `LoanStartDate`        | `DateOnly`              | When the loan becomes Not Null      | effective                       |
| `ExpectedReturnDate`   | `DateOnly`              | Not Null      | Expected return date                                   |
| `ActualReturnDate`     | `DateOnly?`             |               | Set when the loan is returned                         |
| `Purpose`              | `string`                | Required, Max(500) | Reason for loan                                    |
| `ReturnConditionNotes` | `string?`               | Max(1000)     | Condition notes upon return                           |
| `LoanStatus`           | `LoanStatus`            | Stored as string | Enum: `PendingApproval`, `Active`, `Returned`, `Overdue` |
| `CreatedAt`            | `DateTime`              | UTC, Not Null |                                                          |
| `UpdatedAt`            | `DateTime`              | UTC, Not Null |                                                          |

**Indexes**: FK on `EquipmentId`, FK on `BorrowerId`, Index on `LoanStatus` for overdue detection.

### Table: `equipment_maintenance_logs`

| Field              | Type                 | Constraints    | Notes                                                |
| ------------------ | -------------------- | ------------- | ---------------------------------------------------- |
| `Id`               | `Guid`               | PK            |                                                      |
| `EquipmentId`      | `Guid`               | FK → equipments.Id, Not Null | Equipment being maintained |
| `LoggedByEmployeeId` | `Guid`             | Not Null      | Employee who logged the entry                       |
| `OccurredAt`       | `DateTime`           | UTC, Not Null | When the maintenance occurred                       |
| `Type`             | `MaintenanceType`    | Stored as string | Enum: `Scheduled`, `Unscheduled`, `Repair`     |
| `VendorName`       | `string?`            | Max(200)      | External vendor who performed the work              |
| `Description`      | `string`             | Required, Max(2000) | What was done                                 |
| `CostTHB`          | `decimal?`           | Precision(12,2) | Cost of maintenance in THB                         |
| `NextServiceDueDate` | `DateOnly?`        |               | Next scheduled service date (updates equipment)    |
| `CreatedAt`        | `DateTime`           | UTC, Not Null |                                                      |

**Indexes**: FK on `EquipmentId`, Index on `OccurredAt` descending for chronological listing.

### Table: `equipment_attachments`

| Field          | Type                | Constraints    | Notes                                         |
| -------------- | ------------------- | ------------- | --------------------------------------------- |
| `Id`           | `Guid`              | PK            |                                               |
| `EquipmentId`  | `Guid`              | FK → equipments.Id, Not Null | CNC machine this belongs to |
| `Name`         | `string`            | Required, Max(100) | e.g. "Kurt Vice 6"                        |
| `AttachmentType` | `AttachmentType`  | Stored as string | Enum: `Vise`, `ToolHolder`, `Fixture`, `Other` |
| `SerialNumber`  | `string?`           | Max(100)      | Manufacturer serial number                    |
| `IsActive`     | `bool`               | Not Null, Default(true) | Currently usable                      |
| `ConditionNotes` | `string?`          | Max(500)      | Free-text condition description               |
| `CreatedAt`    | `DateTime`          | UTC, Not Null |                                               |
| `UpdatedAt`    | `DateTime`          | UTC, Not Null |                                               |

**Indexes**: FK on `EquipmentId`, Index on `IsActive` for active-only queries.

---

## Enumerations

### EquipmentCategory

Stored as `string` in the database.

| Value                | Asset Code Prefix | Notes                               |
| -------------------- | ----------------- | ---------------------------------- |
| `FdmPrinter`         | `FDM`             | FDM 3D printers                    |
| `SlaPrinter`        | `SLA`             | SLA/DLP resin printers             |
| `CncMachine`        | `CNC`             | CNC milling machines                |
| `Scanner3D`          | `SCAN`            | 3D scanners                        |
| `InjectionMolding`  | `INJ`             | Injection moulding machines         |
| `OfficeEquipment`   | `OFC`             | Office printers, laminators, etc.  |
| `MeasuringEquipment`| `MEAS`            | Calipers, micrometers, CMMs        |
| `ITEquipment`       | `IT`              | Computers, monitors, tablets       |
| `HandTool`          | `TOOL`            | Manual hand tools                   |
| `Other`             | `OTH`             | Catch-all for unlisted categories   |

### EquipmentStatus

Stored as `string`. Permitted transitions:

```
Active ──────────────────────────────────► UnderMaintenance
  │         ◄─────────────────────────────     │
  │                                           │
  ├────────────────────────────────────────► OnLoan
  │         ◄─────────────────────────────     │
  │                                           │
  ▼                                           ▼
Lost ─────────────────────────────────────► Active (recovered)
  │
  ▼
Decommissioned ◄─────────────────────────── (terminal)
```

| Value              | Description                                                      |
| ------------------ | ---------------------------------------------------------------- |
| `Active`           | Available for internal use, pricing queries, and job assignment |
| `UnderMaintenance` | Temporarily unavailable; excluded from availability queries     |
| `OnLoan`           | Lent to employee or customer; excluded from availability queries |
| `Lost`             | Reported as misplaced; excluded from availability queries       |
| `Decommissioned`   | Permanently retired; terminal state — no further transitions   |

### SlaLightSourceType

| Value    | Description                      |
| -------- | -------------------------------- |
| `Laser`  | Laser-based stereolithography   |
| `Dlp`    | Digital Light Processing        |
| `Lcd`    | LCD masked stereolithography    |

### CncToolInterface

| Value     | Description                    |
| --------- | ------------------------------ |
| `Bt30`    | BT-30 tool interface           |
| `Bt40`    | BT-40 tool interface           |
| `Hsk43`   | HSK-43 tool interface          |
| `Hsk63`   | HSK-63 tool interface          |
| `Other`   | Non-standard interface         |

### Scanner3DTechnology

| Value             | Description                     |
| ----------------- | ------------------------------- |
| `Laser`           | Laser triangulation scanner     |
| `StructuredLight`| Structured light scanner        |
| `Lidar`           | LiDAR-based scanner             |
| `Photogrammetry`  | Photogrammetry-based scanning  |

### MaintenanceType

| Value          | Description                          |
| -------------- | ------------------------------------ |
| `Scheduled`    | Planned preventive maintenance       |
| `Unscheduled`  | Unplanned breakdown or repair        |
| `Repair`       | Corrective repair action             |

### LoanBorrowerType

| Value      | Description                    |
| ---------- | ------------------------------ |
| `Employee` | Loaned to a MALIEV employee    |
| `Customer` | Loaned to an external customer |

### LoanStatus

| Value             | Description                                    |
| ----------------- | --------------------------------------------- |
| `PendingApproval` | Customer loan awaiting manager approval       |
| `Active`          | Currently on loan                              |
| `Returned`        | Loan completed and equipment returned         |
| `Overdue`         | Expected return date passed, not yet returned |

### AttachmentType

| Value        | Description                          |
| ------------ | ------------------------------------ |
| `Vise`       | Machine vise or chuck                |
| `ToolHolder` | Tool holder or collet                |
| `Fixture`    | Work-holding fixture                 |
| `Other`      | Other miscellaneous attachment        |

---

## Asset Code Generation

Asset codes are generated on registration with a per-category sequence.

**Format**: `MAL-{PREFIX}-{ZERO_PADDED_SEQ}`

| Category           | Prefix | Example Asset Code    |
| ------------------ | ------ | --------------------- |
| FdmPrinter         | `FDM`  | `MAL-FDM-0001`        |
| SlaPrinter         | `SLA`  | `MAL-SLA-0001`        |
| CncMachine         | `CNC`  | `MAL-CNC-0001`        |
| Scanner3D          | `SCAN` | `MAL-SCAN-0001`       |
| InjectionMolding   | `INJ`  | `MAL-INJ-0001`        |
| OfficeEquipment    | `OFC`  | `MAL-OFC-0001`        |
| MeasuringEquipment | `MEAS` | `MAL-MEAS-0001`       |
| ITEquipment        | `IT`   | `MAL-IT-0001`         |
| HandTool           | `TOOL` | `MAL-TOOL-0001`       |
| Other              | `OTH`  | `MAL-OTH-0001`        |

The sequence is per-category (FDM sequence is independent of IT sequence).

---

## MassTransit Outbox Tables

Added via `modelBuilder.AddOutboxMessageEntity()` and `modelBuilder.AddOutboxStateEntity()`:

- `outbox_message` — stores pending events atomically within the domain `SaveChanges` transaction
- `outbox_state` — tracks delivery state per consumer group

These tables are managed entirely by MassTransit. Application code does not interact with them directly.

---

## Query Result Shapes

### ActiveManufacturingEquipmentQueryResult (PricingService / JobService)

Returned by `GET /facility/v1/equipments/active?category={category}`.

```
{
  "items": [
    {
      "id": "guid",
      "assetCode": "MAL-FDM-0001",
      "name": "string",
      "brand": "string",
      "modelName": "string",
      "category": "FdmPrinter",
      "spec": { /* typed spec object per category */ },
      "hourlyRateTHB": decimal,
      "setupFeeTHB": decimal,
      "extendedProperties": { /* JSON object */ }
    }
  ],
  "isOutsourced": false  // or true when items is empty
}
```

### EquipmentListResult (paginated)

Returned by `GET /facility/v1/equipments`.

```
{
  "items": [
    {
      "id": "guid",
      "assetCode": "MAL-FDM-0001",
      "name": "string",
      "brand": "string?",
      "modelName": "string?",
      "category": "FdmPrinter",
      "status": "Active",
      "purchasePriceTHB": decimal?,
      "updatedAt": "datetime"
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

### EquipmentDetailResult

Returned by `GET /facility/v1/equipments/{id}`.

```
{
  "id": "guid",
  "assetCode": "string",
  "brand": "string?",
  "modelName": "string?",
  "name": "string",
  "manufacturerSerialNumber": "string?",
  "category": "FdmPrinter",
  "subCategory": "string?",
  "status": "Active",
  "purchaseDate": "date?",
  "purchasePriceTHB": decimal?,
  "warrantyExpiryDate": "date?",
  "nextServiceDueDate": "date?",
  "spec": { /* typed spec object, null for general equipment */ },
  "notes": [
    { "id": "guid", "content": "string", "authorEmployeeId": "guid", "createdAt": "datetime" }
  ],
  "activeLoan": { /* current loan if status == OnLoan */ },
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "rowVersion": uint
}
```

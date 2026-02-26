# API Contract: Facility & Equipment Management

**Branch**: `001-equipment-management` | **Date**: 2026-02-25 (Revised)
**Base URL**: `/facility/v1`
**Scalar UI**: `/facility/scalar` (non-production only)
**Auth model**: GCP-style `[RequirePermission]` on every endpoint

---

## Permission Reference

| Permission constant | String value | Description |
|---|---|---|
| `FacilityPermissions.EquipmentsRead` | `facility.equipments.read` | View equipment records; granted to: Admin, Manager, Viewer roles; PricingService identity; JobService identity |
| `FacilityPermissions.EquipmentsWrite` | `facility.equipments.write` | Register and update equipment; granted to: Admin, Manager roles |
| `FacilityPermissions.EquipmentsManage` | `facility.equipments.manage` | Decommission and delete equipment; granted to: Admin role only |
| `FacilityPermissions.LoansRead` | `facility.loans.read` | View loan records; granted to: Admin, Manager roles |
| `FacilityPermissions.LoansWrite` | `facility.loans.write` | Create and return loans; granted to: Admin, Manager roles |
| `FacilityPermissions.LoansApprove` | `facility.loans.approve` | Approve or reject customer loan requests; granted to: Admin, Manager roles |
| `FacilityPermissions.MaintenanceRead` | `facility.maintenance.read` | View maintenance logs; granted to: all employee roles |
| `FacilityPermissions.MaintenanceWrite` | `facility.maintenance.write` | Create maintenance log entries; granted to: Admin, Manager, Technician roles |
| `FacilityPermissions.AttachmentsWrite` | `facility.attachments.write` | Add/update CNC attachments; granted to: Admin, Manager roles |

---

## Endpoints: Equipment

### GET /facility/v1/equipments

List all equipment records, optionally filtered and paginated.

**Permission**: `facility.equipments.read`
**Query parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `category` | string | No | Filter: `FdmPrinter`, `SlaPrinter`, `CncMachine`, `Scanner3D`, `InjectionMolding`, `OfficeEquipment`, `MeasuringEquipment`, `ITEquipment`, `HandTool`, `Other` |
| `status` | string | No | Filter: `Active`, `UnderMaintenance`, `OnLoan`, `Lost`, `Decommissioned` |
| `search` | string | No | Free-text search: matches `Name`, `AssetCode`, `Brand`, or `ModelName` |
| `page` | int | No | 1-based page number (default: 1) |
| `pageSize` | int | No | Items per page (default: 20, max: 100) |

**Success response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "assetCode": "MAL-FDM-0001",
      "name": "Prusa MK4 #1",
      "brand": "Prusa Research",
      "modelName": "MK4",
      "category": "FdmPrinter",
      "status": "Active",
      "purchasePriceTHB": 15000.00,
      "updatedAt": "2026-02-25T10:00:00Z"
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

---

### GET /facility/v1/equipments/{id}

Fetch a single equipment record by ID.

**Permission**: `facility.equipments.read`
**Path parameters**: `id` (Guid)

**Success response: 200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetCode": "MAL-FDM-0001",
  "brand": "Prusa Research",
  "modelName": "MK4",
  "name": "Prusa MK4 #1",
  "manufacturerSerialNumber": "PRUSA-2024-001234",
  "category": "FdmPrinter",
  "subCategory": null,
  "status": "Active",
  "purchaseDate": "2024-06-15",
  "purchasePriceTHB": 15000.00,
  "warrantyExpiryDate": "2025-06-15",
  "nextServiceDueDate": "2026-03-15",
  "spec": {
    "buildVolumeXMm": 250,
    "buildVolumeYMm": 210,
    "buildVolumeZMm": 220,
    "hourlyRateTHB": 45.00,
    "setupFeeTHB": 50.00,
    "nozzleDiameterMm": 0.4,
    "maxNozzleTempC": 280,
    "numberOfExtruders": 1,
    "minLayerHeightMm": 0.04,
    "maxLayerHeightMm": 0.28,
    "extendedProperties": {}
  },
  "notes": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "content": "Nozzle replaced with 0.6mm for larger prints",
      "authorEmployeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
      "createdAt": "2026-02-25T09:00:00Z"
    }
  ],
  "activeLoan": null,
  "createdAt": "2024-06-15T08:00:00Z",
  "updatedAt": "2026-02-25T10:00:00Z",
  "rowVersion": 12345678
}
```

**Error responses:**

| Status | Condition |
|---|---|
| 404 Not Found | No equipment with given `id` |

---

### GET /facility/v1/equipments/active

List all Active manufacturing equipment for a given category type. Primary query endpoint for PricingService and JobService.

**Permission**: `facility.equipments.read`
**Query parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `category` | string | Yes | `FdmPrinter`, `SlaPrinter`, `CncMachine`, `Scanner3D`, or `InjectionMolding` |

**Success response: 200 OK** *(always returns an envelope with `items` array and `isOutsourced` boolean — never an error)*
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "assetCode": "MAL-FDM-0001",
      "name": "Prusa MK4 #1",
      "brand": "Prusa Research",
      "modelName": "MK4",
      "category": "FdmPrinter",
      "spec": {
        "buildVolumeXMm": 250,
        "buildVolumeYMm": 210,
        "buildVolumeZMm": 220,
        "nozzleDiameterMm": 0.4,
        "maxNozzleTempC": 280,
        "numberOfExtruders": 1,
        "minLayerHeightMm": 0.04,
        "maxLayerHeightMm": 0.28,
        "extendedProperties": {}
      },
      "hourlyRateTHB": 45.00,
      "setupFeeTHB": 50.00,
      "extendedProperties": {}
    }
  ],
  "isOutsourced": false
}
```

**Empty result (no active machines for category):**
```json
{
  "items": [],
  "isOutsourced": true
}
```

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | `category` query parameter missing or invalid value |

---

### POST /facility/v1/equipments

Register a new piece of equipment.

**Permission**: `facility.equipments.write`
**Request body (example for FDM printer):**
```json
{
  "brand": "Prusa Research",
  "modelName": "MK4",
  "name": "Prusa MK4 #1",
  "manufacturerSerialNumber": "PRUSA-2024-001234",
  "category": "FdmPrinter",
  "purchaseDate": "2024-06-15",
  "purchasePriceTHB": 15000.00,
  "warrantyExpiryDate": "2025-06-15",
  "spec": {
    "buildVolumeXMm": 250,
    "buildVolumeYMm": 210,
    "buildVolumeZMm": 220,
    "hourlyRateTHB": 45.00,
    "setupFeeTHB": 50.00,
    "nozzleDiameterMm": 0.4,
    "maxNozzleTempC": 280,
    "numberOfExtruders": 1,
    "minLayerHeightMm": 0.04,
    "maxLayerHeightMm": 0.28,
    "extendedProperties": {}
  }
}
```

**Request body (example for Office Equipment):**
```json
{
  "brand": "HP",
  "modelName": "LaserJet Pro MFP M428fdw",
  "name": "Office Printer #1",
  "category": "OfficeEquipment",
  "purchaseDate": "2023-01-10",
  "purchasePriceTHB": 12000.00
}
```

**Validation rules:**
- `name`: required, ≤ 200 chars, must be unique across all equipment records
- `category`: required, must be one of the EquipmentCategory enum values
- `brand`, `modelName`: optional, max 100 chars each
- `manufacturerSerialNumber`: optional, max 100 chars
- `purchaseDate`, `purchasePriceTHB`, `warrantyExpiryDate`: optional
- Spec fields: required only for manufacturing categories (`FdmPrinter`, `SlaPrinter`, `CncMachine`, `Scanner3D`, `InjectionMolding`); forbidden for general categories
- Initial status is always `Active` — clients cannot set status at registration time
- `AssetCode` is auto-generated and immutable

**Success response: 201 Created**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetCode": "MAL-FDM-0001",
  "brand": "Prusa Research",
  "modelName": "MK4",
  "name": "Prusa MK4 #1",
  "category": "FdmPrinter",
  "status": "Active",
  "createdAt": "2026-02-25T10:00:00Z",
  "rowVersion": 1
}
```
`Location` header: `/facility/v1/equipments/{id}`

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | Missing required field, invalid category, spec provided for general equipment, or spec missing for manufacturing equipment |
| 409 Conflict | Equipment with the same `name` already exists |

---

### PUT /facility/v1/equipments/{id}

Update equipment details. Does NOT change status — use `PATCH /{id}/status` for that.

**Permission**: `facility.equipments.write`
**Request body:**
```json
{
  "brand": "Prusa Research",
  "modelName": "MK4",
  "name": "Prusa MK4 #1 (Updated)",
  "manufacturerSerialNumber": "PRUSA-2024-001234",
  "purchaseDate": "2024-06-15",
  "purchasePriceTHB": 16000.00,
  "warrantyExpiryDate": "2025-06-15",
  "spec": {
    "buildVolumeXMm": 250,
    "buildVolumeYMm": 210,
    "buildVolumeZMm": 220,
    "hourlyRateTHB": 50.00,
    "setupFeeTHB": 55.00,
    "nozzleDiameterMm": 0.6,
    "maxNozzleTempC": 280,
    "numberOfExtruders": 1,
    "minLayerHeightMm": 0.04,
    "maxLayerHeightMm": 0.28,
    "extendedProperties": {}
  },
  "rowVersion": 12345678
}
```

**`rowVersion`** is required. The server sets `OriginalValues["xmin"]` to this value before saving; if the row has been modified since, EF Core throws `DbUpdateConcurrencyException` → HTTP 409.

**Success response: 200 OK** — returns updated `EquipmentDetailResult` including new `rowVersion`.

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | Validation failure |
| 404 Not Found | Equipment not found |
| 409 Conflict | Concurrent modification detected (`rowVersion` stale) or duplicate `name` |

---

### PATCH /facility/v1/equipments/{id}/status

Change the operational status of equipment.

**Permission**: `facility.equipments.write`
**Request body:**
```json
{
  "newStatus": "UnderMaintenance",
  "rowVersion": 12345678
}
```

**Transition rules** (enforced by domain; invalid transitions return 422):

| From | To | Allowed |
|---|---|---|
| `Active` | `UnderMaintenance` | ✅ |
| `Active` | `OnLoan` | ✅ |
| `Active` | `Lost` | ✅ |
| `Active` | `Decommissioned` | ✅ |
| `UnderMaintenance` | `Active` | ✅ |
| `UnderMaintenance` | `OnLoan` | ✅ |
| `UnderMaintenance` | `Lost` | ✅ |
| `UnderMaintenance` | `Decommissioned` | ✅ |
| `OnLoan` | `Active` | ✅ (via loan return) |
| `OnLoan` | `Lost` | ✅ |
| `OnLoan` | `Decommissioned` | ✅ |
| `Lost` | `Active` | ✅ (equipment found) |
| `Decommissioned` | any | ❌ |

On success, publishes `EquipmentStatusChangedEvent` via MassTransit outbox.

**Success response: 200 OK** — returns updated `EquipmentDetailResult`.

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | `newStatus` is invalid string |
| 404 Not Found | Equipment not found |
| 409 Conflict | Concurrent modification (`rowVersion` stale) |
| 422 Unprocessable Entity | Transition not permitted (e.g., Decommissioned → Active) |

---

### DELETE /facility/v1/equipments/{id}

Permanently delete an equipment record. Blocked if the machine has any historical job records or active loans.

**Permission**: `facility.equipments.manage`
**Behaviour:**
1. Calls `IJobServiceClient.HasHistoricalJobsAsync(id)`.
2. If `true` → returns HTTP 409 with message suggesting decommissioning.
3. If JobService is unreachable → returns HTTP 503 (fail-safe: block delete under uncertainty).
4. Checks for active loans — if present, returns 409.
5. If all checks pass → performs hard delete (cascades notes, maintenance logs, attachments, loans) and returns HTTP 204.

**Success response: 204 No Content**

**Error responses:**

| Status | Condition |
|---|---|
| 404 Not Found | Equipment not found |
| 409 Conflict | Equipment has historical job records or has an active loan; use Decommissioned status |
| 503 Service Unavailable | JobService could not be reached to verify job history |

---

## Endpoints: Equipment Notes

### GET /facility/v1/equipments/{id}/notes

List all notes for an equipment record.

**Permission**: `facility.equipments.read`
**Path parameters**: `id` (Guid)

**Success response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "content": "Nozzle replaced with 0.6mm for larger prints",
      "authorEmployeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
      "createdAt": "2026-02-25T09:00:00Z"
    }
  ]
}
```

---

### POST /facility/v1/equipments/{id}/notes

Add a note to an equipment record. Notes are immutable once created.

**Permission**: `facility.equipments.write`
**Request body:**
```json
{
  "content": "Nozzle replaced with 0.6mm for larger prints"
}
```

**Success response: 201 Created**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "content": "Nozzle replaced with 0.6mm for larger prints",
  "authorEmployeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "createdAt": "2026-02-25T09:00:00Z"
}
```

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | `content` missing or exceeds 2000 chars |
| 404 Not Found | Equipment not found |

---

## Endpoints: Equipment Loans

### GET /facility/v1/equipments/{id}/loans

List all loans for an equipment record.

**Permission**: `facility.loans.read`
**Success response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa9",
      "borrowerId": "3fa85f64-5717-4562-b3fc-2c963f66afaa",
      "borrowerType": "Employee",
      "loanStartDate": "2026-02-20",
      "expectedReturnDate": "2026-02-27",
      "actualReturnDate": null,
      "purpose": "Training session",
      "loanStatus": "Active"
    }
  ]
}
```

---

### POST /facility/v1/equipments/{id}/loans

Create a loan record.

**Permission**: `facility.loans.write`

**For employee loans (immediate assignment):**
```json
{
  "borrowerId": "3fa85f64-5717-4562-b3fc-2c963f66afaa",
  "borrowerType": "Employee",
  "expectedReturnDate": "2026-02-27",
  "purpose": "Training session"
}
```
Equipment status changes to `OnLoan` immediately. No approval required.

**For customer loans (requires approval):**
```json
{
  "borrowerId": "3fa85f64-5717-4562-b3fc-2c963f66afab",
  "borrowerType": "Customer",
  "expectedReturnDate": "2026-03-01",
  "purpose": "Customer demo unit on loan"
}
```
Loan status is `PendingApproval`. Equipment status remains unchanged until approved.

**Success response: 201 Created** (employee loan: returns loan record with Active status; customer loan: returns loan record with PendingApproval status)

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | Validation failure, borrower not found |
| 404 Not Found | Equipment not found |
| 409 Conflict | Equipment is not in `Active` status |

---

### PATCH /facility/v1/equipments/{id}/loans/{loanId}/approve

Approve a customer loan request. Only valid for loans with `LoanStatus = PendingApproval`.

**Permission**: `facility.loans.approve`
**Behaviour:**
1. Sets `LoanStatus = Active`
2. Changes equipment status to `OnLoan`
3. Publishes `LoanDocumentRequestedEvent` to trigger PDF generation

**Success response: 200 OK**

**Error responses:**

| Status | Condition |
|---|---|
| 404 Not Found | Equipment or loan not found |
| 422 Unprocessable Entity | Loan is not in `PendingApproval` status |

---

### PATCH /facility/v1/equipments/{id}/loans/{loanId}/reject

Reject a customer loan request. Only valid for loans with `LoanStatus = PendingApproval`.

**Permission**: `facility.loans.approve`
**Request body:**
```json
{
  "rejectionReason": "Equipment currently under maintenance"
}
```

**Success response: 200 OK** — loan status becomes `Rejected` (or simply deleted)

**Error responses:**

| Status | Condition |
|---|---|
| 404 Not Found | Equipment or loan not found |
| 422 Unprocessable Entity | Loan is not in `PendingApproval` status |

---

### PATCH /facility/v1/equipments/{id}/loans/{loanId}/return

Record an equipment return.

**Permission**: `facility.loans.write`
**Request body:**
```json
{
  "returnConditionNotes": "Returned in good condition. Minor scratches on the base plate."
}
```

**Behaviour:**
1. Sets `LoanStatus = Returned`
2. Sets `ActualReturnDate` to current date
3. Changes equipment status back to `Active`
4. Publishes `EquipmentStatusChangedEvent`

**Success response: 200 OK**

**Error responses:**

| Status | Condition |
|---|---|
| 404 Not Found | Equipment or loan not found |
| 422 Unprocessable Entity | Loan is not in `Active` status |

---

## Endpoints: Maintenance Logs

### GET /facility/v1/equipments/{id}/maintenance

List all maintenance log entries for an equipment record.

**Permission**: `facility.maintenance.read`
**Success response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afac",
      "loggedByEmployeeId": "3fa85f64-5717-4562-b3fc-2c963f66afad",
      "occurredAt": "2026-02-20T10:00:00Z",
      "type": "Scheduled",
      "vendorName": "Prusa Service Center",
      "description": "Annual maintenance check. Replaced PTFE tube.",
      "costTHB": 1500.00,
      "nextServiceDueDate": "2026-03-20",
      "createdAt": "2026-02-20T10:30:00Z"
    }
  ]
}
```

---

### POST /facility/v1/equipments/{id}/maintenance

Add a maintenance log entry. Maintenance entries are immutable once created.

**Permission**: `facility.maintenance.write`
**Request body:**
```json
{
  "occurredAt": "2026-02-20T10:00:00Z",
  "type": "Scheduled",
  "vendorName": "Prusa Service Center",
  "description": "Annual maintenance check. Replaced PTFE tube.",
  "costTHB": 1500.00,
  "nextServiceDueDate": "2026-03-20"
}
```

**Success response: 201 Created**

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | Missing required fields, invalid type |
| 404 Not Found | Equipment not found |

---

## Endpoints: CNC Attachments

### GET /facility/v1/equipments/{id}/attachments

List all attachments for a CNC machine.

**Permission**: `facility.equipments.read`
**Success response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afae",
      "name": "Kurt Vice 6",
      "attachmentType": "Vise",
      "serialNumber": "KURT-V6-2024-0042",
      "isActive": true,
      "conditionNotes": "Good condition, recently cleaned",
      "createdAt": "2024-06-15T08:00:00Z",
      "updatedAt": "2026-02-20T10:00:00Z"
    }
  ]
}
```

---

### POST /facility/v1/equipments/{id}/attachments

Add an attachment to a CNC machine. Only valid for equipment with `Category = CncMachine`.

**Permission**: `facility.attachments.write`
**Request body:**
```json
{
  "name": "Kurt Vice 6",
  "attachmentType": "Vise",
  "serialNumber": "KURT-V6-2024-0042",
  "conditionNotes": "Good condition"
}
```

**Success response: 201 Created**

**Error responses:**

| Status | Condition |
|---|---|
| 400 Bad Request | Missing required fields |
| 404 Not Found | Equipment not found |
| 422 Unprocessable Entity | Equipment is not a CNC machine |

---

### PATCH /facility/v1/equipments/{id}/attachments/{attachmentId}

Update an attachment (e.g., mark as retired).

**Permission**: `facility.attachments.write`
**Request body:**
```json
{
  "isActive": false,
  "conditionNotes": "Retired due to worn jaws. Replaced with new vice."
}
```

**Success response: 200 OK**

**Error responses:**

| Status | Condition |
|---|---|
| 404 Not Found | Equipment or attachment not found |

---

### DELETE /facility/v1/equipments/{id}/attachments/{attachmentId}

Delete an attachment.

**Permission**: `facility.attachments.write`

**Success response: 204 No Content**

---

## Standard Error Body

All error responses use RFC 9457 Problem Details format:
```json
{
  "type": "https://maliev.internal/errors/conflict",
  "title": "Conflict",
  "status": 409,
  "detail": "Equipment 'Prusa MK4 #1' has associated job history. Use Decommissioned status instead of deletion.",
  "instance": "/facility/v1/equipments/3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

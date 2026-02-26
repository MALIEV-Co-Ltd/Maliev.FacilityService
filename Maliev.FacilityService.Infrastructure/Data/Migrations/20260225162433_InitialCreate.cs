using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Maliev.FacilityService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_code_sequences",
                columns: table => new
                {
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_sequence_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_code_sequences", x => x.category);
                });

            migrationBuilder.CreateTable(
                name: "equipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    model_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    manufacturer_serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sub_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: true),
                    purchase_price_thb = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    warranty_expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    next_service_due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "InboxState",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "equipment_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    attachment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    condition_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_attachments", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_attachments_equipments_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipment_loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    borrower_id = table.Column<Guid>(type: "uuid", nullable: false),
                    borrower_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    approved_by_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    loan_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expected_return_date = table.Column<DateOnly>(type: "date", nullable: false),
                    actual_return_date = table.Column<DateOnly>(type: "date", nullable: true),
                    purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    return_condition_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    loan_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_loans", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_loans_equipments_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equipment_maintenance_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    logged_by_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vendor_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    cost_thb = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    next_service_due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_maintenance_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_maintenance_logs_equipments_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipment_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_notes_equipments_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneralEquipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralEquipment", x => x.id);
                    table.ForeignKey(
                        name: "FK_GeneralEquipment_equipments_id",
                        column: x => x.id,
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingEquipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hourly_rate_thb = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    setup_fee_thb = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    extended_properties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingEquipment", x => x.id);
                    table.ForeignKey(
                        name: "FK_ManufacturingEquipment_equipments_id",
                        column: x => x.id,
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.CreateTable(
                name: "hand_tools",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hand_tools", x => x.id);
                    table.ForeignKey(
                        name: "FK_hand_tools_GeneralEquipment_id",
                        column: x => x.id,
                        principalTable: "GeneralEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "it_equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_it_equipment", x => x.id);
                    table.ForeignKey(
                        name: "FK_it_equipment_GeneralEquipment_id",
                        column: x => x.id,
                        principalTable: "GeneralEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "measuring_equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_measuring_equipment", x => x.id);
                    table.ForeignKey(
                        name: "FK_measuring_equipment_GeneralEquipment_id",
                        column: x => x.id,
                        principalTable: "GeneralEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "office_equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_office_equipment", x => x.id);
                    table.ForeignKey(
                        name: "FK_office_equipment_GeneralEquipment_id",
                        column: x => x.id,
                        principalTable: "GeneralEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "other_equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_other_equipment", x => x.id);
                    table.ForeignKey(
                        name: "FK_other_equipment_GeneralEquipment_id",
                        column: x => x.id,
                        principalTable: "GeneralEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cnc_machine_specs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    x_travel_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    y_travel_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    z_travel_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_spindle_speed_rpm = table.Column<int>(type: "integer", nullable: false),
                    max_spindle_power_kw = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    number_of_axes = table.Column<int>(type: "integer", nullable: false),
                    tool_interface = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_tool_diameter_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    controller_brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cnc_machine_specs", x => x.id);
                    table.ForeignKey(
                        name: "FK_cnc_machine_specs_ManufacturingEquipment_id",
                        column: x => x.id,
                        principalTable: "ManufacturingEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fdm_printer_specs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_volume_x_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    build_volume_y_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    build_volume_z_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    nozzle_diameter_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_nozzle_temp_c = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    number_of_extruders = table.Column<int>(type: "integer", nullable: false),
                    min_layer_height_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_layer_height_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fdm_printer_specs", x => x.id);
                    table.ForeignKey(
                        name: "FK_fdm_printer_specs_ManufacturingEquipment_id",
                        column: x => x.id,
                        principalTable: "ManufacturingEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "injection_molding_specs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_mold_x_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_mold_y_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_mold_z_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_shot_size_g = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_temp_c = table.Column<int>(type: "integer", nullable: false),
                    max_injection_pressure_bar = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_injection_molding_specs", x => x.id);
                    table.ForeignKey(
                        name: "FK_injection_molding_specs_ManufacturingEquipment_id",
                        column: x => x.id,
                        principalTable: "ManufacturingEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scanner_3d_specs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_scan_volume_m3 = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    accuracy_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    scan_resolutions = table.Column<string>(type: "text", nullable: true),
                    scanner_technology = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scanner_3d_specs", x => x.id);
                    table.ForeignKey(
                        name: "FK_scanner_3d_specs_ManufacturingEquipment_id",
                        column: x => x.id,
                        principalTable: "ManufacturingEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sla_printer_specs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    build_volume_x_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    build_volume_y_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    build_volume_z_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    xy_resolution_mm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    light_source_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    wavelength_nm = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sla_printer_specs", x => x.id);
                    table.ForeignKey(
                        name: "FK_sla_printer_specs_ManufacturingEquipment_id",
                        column: x => x.id,
                        principalTable: "ManufacturingEquipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_attachments_equipment_id",
                table: "equipment_attachments",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_attachments_is_active",
                table: "equipment_attachments",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_loans_equipment_id",
                table: "equipment_loans",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_loans_loan_status",
                table: "equipment_loans",
                column: "loan_status");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_maintenance_logs_equipment_id",
                table: "equipment_maintenance_logs",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_maintenance_logs_occurred_at",
                table: "equipment_maintenance_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_notes_equipment_id",
                table: "equipment_notes",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipments_asset_code",
                table: "equipments",
                column: "asset_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipments_category",
                table: "equipments",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_equipments_name",
                table: "equipments",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_equipments_status",
                table: "equipments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                table: "OutboxState",
                column: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_code_sequences");

            migrationBuilder.DropTable(
                name: "cnc_machine_specs");

            migrationBuilder.DropTable(
                name: "equipment_attachments");

            migrationBuilder.DropTable(
                name: "equipment_loans");

            migrationBuilder.DropTable(
                name: "equipment_maintenance_logs");

            migrationBuilder.DropTable(
                name: "equipment_notes");

            migrationBuilder.DropTable(
                name: "fdm_printer_specs");

            migrationBuilder.DropTable(
                name: "hand_tools");

            migrationBuilder.DropTable(
                name: "injection_molding_specs");

            migrationBuilder.DropTable(
                name: "it_equipment");

            migrationBuilder.DropTable(
                name: "measuring_equipment");

            migrationBuilder.DropTable(
                name: "office_equipment");

            migrationBuilder.DropTable(
                name: "other_equipment");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "scanner_3d_specs");

            migrationBuilder.DropTable(
                name: "sla_printer_specs");

            migrationBuilder.DropTable(
                name: "GeneralEquipment");

            migrationBuilder.DropTable(
                name: "InboxState");

            migrationBuilder.DropTable(
                name: "OutboxState");

            migrationBuilder.DropTable(
                name: "ManufacturingEquipment");

            migrationBuilder.DropTable(
                name: "equipments");
        }
    }
}

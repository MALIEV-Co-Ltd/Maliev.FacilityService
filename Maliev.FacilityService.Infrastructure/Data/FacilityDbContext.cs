using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Infrastructure.Data;

/// <summary>
/// EF Core database context for the Facility Service.
/// Uses Table-Per-Type (TPT) inheritance for the Equipment hierarchy and hosts the MassTransit outbox tables.
/// </summary>
public class FacilityDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="FacilityDbContext"/>.
    /// </summary>
    /// <param name="options">The EF Core context options.</param>
    public FacilityDbContext(DbContextOptions<FacilityDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets or sets the design-time options used by EF Core tooling (migrations).</summary>
    public static DbContextOptions<FacilityDbContext> DesignTimeOptions { get; set; } = null!;

    /// <summary>Gets the <see cref="Equipment"/> base table (TPT root).</summary>
    public DbSet<Equipment> Equipments => Set<Equipment>();

    /// <summary>Gets the FDM printer spec table.</summary>
    public DbSet<FdmPrinterEquipment> FdmPrinterSpecs => Set<FdmPrinterEquipment>();

    /// <summary>Gets the SLA printer spec table.</summary>
    public DbSet<SlaPrinterEquipment> SlaPrinterSpecs => Set<SlaPrinterEquipment>();

    /// <summary>Gets the CNC machine spec table.</summary>
    public DbSet<CncMachineEquipment> CncMachineSpecs => Set<CncMachineEquipment>();

    /// <summary>Gets the 3D scanner spec table.</summary>
    public DbSet<Scanner3DEquipment> Scanner3DSpecs => Set<Scanner3DEquipment>();

    /// <summary>Gets the injection molding machine spec table.</summary>
    public DbSet<InjectionMoldingEquipment> InjectionMoldingSpecs => Set<InjectionMoldingEquipment>();

    /// <summary>Gets the equipment notes table (append-only).</summary>
    public DbSet<EquipmentNote> EquipmentNotes => Set<EquipmentNote>();

    /// <summary>Gets the equipment loans table.</summary>
    public DbSet<EquipmentLoan> EquipmentLoans => Set<EquipmentLoan>();

    /// <summary>Gets the equipment maintenance logs table.</summary>
    public DbSet<EquipmentMaintenanceLog> EquipmentMaintenanceLogs => Set<EquipmentMaintenanceLog>();

    /// <summary>Gets the equipment attachments table (CNC tooling, fixtures, etc.).</summary>
    public DbSet<EquipmentAttachment> EquipmentAttachments => Set<EquipmentAttachment>();

    /// <summary>Gets the asset code sequence counters table (per-category sequence numbers).</summary>
    public DbSet<AssetCodeSequence> AssetCodeSequences => Set<AssetCodeSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("equipments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssetCode).HasColumnName("asset_code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Brand).HasColumnName("brand").HasMaxLength(100);
            entity.Property(e => e.ModelName).HasColumnName("model_name").HasMaxLength(200);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ManufacturerSerialNumber).HasColumnName("manufacturer_serial_number").HasMaxLength(100);
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubCategory).HasColumnName("sub_category").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
            entity.Property(e => e.PurchasePriceTHB).HasColumnName("purchase_price_thb").HasPrecision(10, 2);
            entity.Property(e => e.WarrantyExpiryDate).HasColumnName("warranty_expiry_date");
            entity.Property(e => e.NextServiceDueDate).HasColumnName("next_service_due_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.Property<uint>("xmin")
                .IsRowVersion()
                .HasColumnType("xid")
                .HasColumnName("xmin");

            entity.HasIndex(e => e.AssetCode).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<ManufacturingEquipment>(entity =>
        {
            entity.HasBaseType<Equipment>();
        });

        modelBuilder.Entity<FdmPrinterEquipment>(entity =>
        {
            entity.HasBaseType<ManufacturingEquipment>();
            entity.ToTable("fdm_printer_specs");
            entity.Property(e => e.BuildVolumeXMm).HasColumnName("build_volume_x_mm").HasPrecision(10, 2);
            entity.Property(e => e.BuildVolumeYMm).HasColumnName("build_volume_y_mm").HasPrecision(10, 2);
            entity.Property(e => e.BuildVolumeZMm).HasColumnName("build_volume_z_mm").HasPrecision(10, 2);
            entity.Property(e => e.NozzleDiameterMm).HasColumnName("nozzle_diameter_mm").HasPrecision(10, 2);
            entity.Property(e => e.MaxNozzleTempC).HasColumnName("max_nozzle_temp_c").HasPrecision(10, 2);
            entity.Property(e => e.NumberOfExtruders).HasColumnName("number_of_extruders");
            entity.Property(e => e.MinLayerHeightMm).HasColumnName("min_layer_height_mm").HasPrecision(10, 2);
            entity.Property(e => e.MaxLayerHeightMm).HasColumnName("max_layer_height_mm").HasPrecision(10, 2);
            entity.Property(e => e.HourlyRateTHB).HasColumnName("hourly_rate_thb").HasPrecision(10, 2);
            entity.Property(e => e.SetupFeeTHB).HasColumnName("setup_fee_thb").HasPrecision(10, 2);
            entity.Property(e => e.ExtendedProperties).HasColumnName("extended_properties");
        });

        modelBuilder.Entity<SlaPrinterEquipment>(entity =>
        {
            entity.HasBaseType<ManufacturingEquipment>();
            entity.ToTable("sla_printer_specs");
            entity.Property(e => e.BuildVolumeXMm).HasColumnName("build_volume_x_mm").HasPrecision(10, 2);
            entity.Property(e => e.BuildVolumeYMm).HasColumnName("build_volume_y_mm").HasPrecision(10, 2);
            entity.Property(e => e.BuildVolumeZMm).HasColumnName("build_volume_z_mm").HasPrecision(10, 2);
            entity.Property(e => e.XyResolutionMm).HasColumnName("xy_resolution_mm").HasPrecision(10, 2);
            entity.Property(e => e.LightSourceType).HasColumnName("light_source_type").HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.WavelengthNm).HasColumnName("wavelength_nm");
            entity.Property(e => e.HourlyRateTHB).HasColumnName("hourly_rate_thb").HasPrecision(10, 2);
            entity.Property(e => e.SetupFeeTHB).HasColumnName("setup_fee_thb").HasPrecision(10, 2);
            entity.Property(e => e.ExtendedProperties).HasColumnName("extended_properties");
        });

        modelBuilder.Entity<CncMachineEquipment>(entity =>
        {
            entity.HasBaseType<ManufacturingEquipment>();
            entity.ToTable("cnc_machine_specs");
            entity.Property(e => e.XTravelMm).HasColumnName("x_travel_mm").HasPrecision(10, 2);
            entity.Property(e => e.YTravelMm).HasColumnName("y_travel_mm").HasPrecision(10, 2);
            entity.Property(e => e.ZTravelMm).HasColumnName("z_travel_mm").HasPrecision(10, 2);
            entity.Property(e => e.MaxSpindleSpeedRpm).HasColumnName("max_spindle_speed_rpm");
            entity.Property(e => e.MaxSpindlePowerKw).HasColumnName("max_spindle_power_kw").HasPrecision(10, 2);
            entity.Property(e => e.NumberOfAxes).HasColumnName("number_of_axes");
            entity.Property(e => e.ToolInterface).HasColumnName("tool_interface").HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.MaxToolDiameterMm).HasColumnName("max_tool_diameter_mm").HasPrecision(10, 2);
            entity.Property(e => e.ControllerBrand).HasColumnName("controller_brand").HasMaxLength(100);
            entity.Property(e => e.HourlyRateTHB).HasColumnName("hourly_rate_thb").HasPrecision(10, 2);
            entity.Property(e => e.SetupFeeTHB).HasColumnName("setup_fee_thb").HasPrecision(10, 2);
            entity.Property(e => e.ExtendedProperties).HasColumnName("extended_properties");
        });

        modelBuilder.Entity<Scanner3DEquipment>(entity =>
        {
            entity.HasBaseType<ManufacturingEquipment>();
            entity.ToTable("scanner_3d_specs");
            entity.Property(e => e.MaxScanVolumeM3).HasColumnName("max_scan_volume_m3").HasPrecision(10, 2);
            entity.Property(e => e.AccuracyMm).HasColumnName("accuracy_mm").HasPrecision(10, 2);
            entity.Property(e => e.ScanResolutions).HasColumnName("scan_resolutions");
            entity.Property(e => e.ScannerTechnology).HasColumnName("scanner_technology").HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.HourlyRateTHB).HasColumnName("hourly_rate_thb").HasPrecision(10, 2);
            entity.Property(e => e.SetupFeeTHB).HasColumnName("setup_fee_thb").HasPrecision(10, 2);
            entity.Property(e => e.ExtendedProperties).HasColumnName("extended_properties");
        });

        modelBuilder.Entity<InjectionMoldingEquipment>(entity =>
        {
            entity.HasBaseType<ManufacturingEquipment>();
            entity.ToTable("injection_molding_specs");
            entity.Property(e => e.MaxMoldXMm).HasColumnName("max_mold_x_mm").HasPrecision(10, 2);
            entity.Property(e => e.MaxMoldYMm).HasColumnName("max_mold_y_mm").HasPrecision(10, 2);
            entity.Property(e => e.MaxMoldZMm).HasColumnName("max_mold_z_mm").HasPrecision(10, 2);
            entity.Property(e => e.MaxShotSizeG).HasColumnName("max_shot_size_g").HasPrecision(10, 2);
            entity.Property(e => e.MaxTempC).HasColumnName("max_temp_c");
            entity.Property(e => e.MaxInjectionPressureBar).HasColumnName("max_injection_pressure_bar");
            entity.Property(e => e.HourlyRateTHB).HasColumnName("hourly_rate_thb").HasPrecision(10, 2);
            entity.Property(e => e.SetupFeeTHB).HasColumnName("setup_fee_thb").HasPrecision(10, 2);
            entity.Property(e => e.ExtendedProperties).HasColumnName("extended_properties");
        });

        modelBuilder.Entity<GeneralEquipment>(entity =>
        {
            entity.HasBaseType<Equipment>();
        });

        modelBuilder.Entity<OfficeEquipmentItem>(entity =>
        {
            entity.HasBaseType<GeneralEquipment>();
            entity.ToTable("office_equipment");
        });

        modelBuilder.Entity<ITEquipmentItem>(entity =>
        {
            entity.HasBaseType<GeneralEquipment>();
            entity.ToTable("it_equipment");
        });

        modelBuilder.Entity<MeasuringEquipmentItem>(entity =>
        {
            entity.HasBaseType<GeneralEquipment>();
            entity.ToTable("measuring_equipment");
        });

        modelBuilder.Entity<HandToolItem>(entity =>
        {
            entity.HasBaseType<GeneralEquipment>();
            entity.ToTable("hand_tools");
        });

        modelBuilder.Entity<OtherEquipmentItem>(entity =>
        {
            entity.HasBaseType<GeneralEquipment>();
            entity.ToTable("other_equipment");
        });

        modelBuilder.Entity<EquipmentNote>(entity =>
        {
            entity.ToTable("equipment_notes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id").IsRequired();
            entity.Property(e => e.AuthorEmployeeId).HasColumnName("author_employee_id").IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").HasMaxLength(4000).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.HasIndex(e => e.EquipmentId);
            // Composite index to satisfy ORDER BY created_at DESC after equipment_id filter
            entity.HasIndex(e => new { e.EquipmentId, e.CreatedAt });

            entity.HasOne<Equipment>()
                .WithMany()
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EquipmentLoan>(entity =>
        {
            entity.ToTable("equipment_loans");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id").IsRequired();
            entity.Property(e => e.BorrowerId).HasColumnName("borrower_id").IsRequired();
            entity.Property(e => e.BorrowerType).HasColumnName("borrower_type").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.ApprovedByEmployeeId).HasColumnName("approved_by_employee_id");
            entity.Property(e => e.LoanStartDate).HasColumnName("loan_start_date").IsRequired();
            entity.Property(e => e.ExpectedReturnDate).HasColumnName("expected_return_date").IsRequired();
            entity.Property(e => e.ActualReturnDate).HasColumnName("actual_return_date");
            entity.Property(e => e.Purpose).HasColumnName("purpose").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ReturnConditionNotes).HasColumnName("return_condition_notes").HasMaxLength(1000);
            entity.Property(e => e.LoanStatus).HasColumnName("loan_status").HasConversion<string>().HasMaxLength(50).IsRequired();

            entity.Property<uint>("xmin")
                .IsRowVersion()
                .HasColumnType("xid")
                .HasColumnName("xmin");

            entity.HasIndex(e => e.EquipmentId);
            entity.HasIndex(e => e.LoanStatus);
            // Composite index for the common hot path: filter by equipment + status
            entity.HasIndex(e => new { e.EquipmentId, e.LoanStatus });

            entity.HasOne<Equipment>()
                .WithMany()
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EquipmentMaintenanceLog>(entity =>
        {
            entity.ToTable("equipment_maintenance_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id").IsRequired();
            entity.Property(e => e.LoggedByEmployeeId).HasColumnName("logged_by_employee_id").IsRequired();
            entity.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.VendorName).HasColumnName("vendor_name").HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
            entity.Property(e => e.CostTHB).HasColumnName("cost_thb").HasPrecision(10, 2);
            entity.Property(e => e.NextServiceDueDate).HasColumnName("next_service_due_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.HasIndex(e => e.EquipmentId);
            entity.HasIndex(e => e.OccurredAt);

            entity.HasOne<Equipment>()
                .WithMany()
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EquipmentAttachment>(entity =>
        {
            entity.ToTable("equipment_attachments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.AttachmentType).HasColumnName("attachment_type").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(e => e.ConditionNotes).HasColumnName("condition_notes").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.Property<uint>("xmin")
                .IsRowVersion()
                .HasColumnType("xid")
                .HasColumnName("xmin");

            entity.HasIndex(e => e.EquipmentId);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne<Equipment>()
                .WithMany()
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssetCodeSequence>(entity =>
        {
            entity.ToTable("asset_code_sequences");
            entity.HasKey(e => e.Category);
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastSequenceNumber).HasColumnName("last_sequence_number").IsRequired();
        });
    }
}

/// <summary>
/// Stores the last-used sequence number for asset code generation, keyed by equipment category.
/// </summary>
public class AssetCodeSequence
{
    /// <summary>Gets or sets the equipment category this sequence belongs to.</summary>
    public EquipmentCategory Category { get; set; }

    /// <summary>Gets or sets the last sequence number issued for this category.</summary>
    public int LastSequenceNumber { get; set; }
}

using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Services;

[Collection("PostgresCollection")]
public class AssetCodeGeneratorIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private AssetCodeGenerator _generator = null!;

    public AssetCodeGeneratorIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FacilityDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _context = new FacilityDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _generator = new AssetCodeGenerator(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_FdmPrinter_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter);

        Assert.StartsWith("MAL-FDM-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_SlaPrinter_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.SlaPrinter);

        Assert.StartsWith("MAL-SLA-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_CncMachine_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.CncMachine);

        Assert.StartsWith("MAL-CNC-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_Scanner3D_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.Scanner3D);

        Assert.StartsWith("MAL-3DS-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_InjectionMolding_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.InjectionMolding);

        Assert.StartsWith("MAL-IM-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_OfficeEquipment_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.OfficeEquipment);

        Assert.StartsWith("MAL-OFF-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_MeasuringEquipment_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.MeasuringEquipment);

        Assert.StartsWith("MAL-MEA-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_ITEquipment_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.ITEquipment);

        Assert.StartsWith("MAL-IT-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_HandTool_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.HandTool);

        Assert.StartsWith("MAL-HT-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_Other_ReturnsCorrectPrefix()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.Other);

        Assert.StartsWith("MAL-OTH-", result);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_SameCategory_ReturnsSequentialNumbers()
    {
        var result1 = await _generator.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter);
        var result2 = await _generator.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter);
        var result3 = await _generator.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter);

        var parts1 = result1.Split('-');
        var parts2 = result2.Split('-');
        var parts3 = result3.Split('-');

        var seq1 = int.Parse(parts1[^1]);
        var seq2 = int.Parse(parts2[^1]);
        var seq3 = int.Parse(parts3[^1]);

        Assert.Equal(1, seq1);
        Assert.Equal(2, seq2);
        Assert.Equal(3, seq3);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_DifferentCategories_HaveSeparateSequences()
    {
        var fdmCode = await _generator.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter);
        var slaCode = await _generator.GenerateAssetCodeAsync(EquipmentCategory.SlaPrinter);

        var fdmParts = fdmCode.Split('-');
        var slaParts = slaCode.Split('-');

        Assert.Equal("FDM", fdmParts[1]);
        Assert.Equal("SLA", slaParts[1]);
    }

    [Fact]
    public async Task GenerateAssetCodeAsync_FirstTime_CreatesNewSequence()
    {
        var result = await _generator.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter);

        var parts = result.Split('-');
        var sequence = int.Parse(parts[^1]);

        Assert.Equal(1, sequence);
    }
}

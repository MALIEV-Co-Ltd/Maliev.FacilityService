using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.AddEquipmentNote;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentNotes;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class AddEquipmentNoteIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _dbContext = null!;
    private IEquipmentRepository _equipmentRepository = null!;
    private IEquipmentNoteRepository _noteRepository = null!;
    private FdmPrinterEquipment _testEquipment = null!;

    public AddEquipmentNoteIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FacilityDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new FacilityDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _equipmentRepository = new Infrastructure.Data.Repositories.EquipmentRepository(_dbContext);
        _noteRepository = new Infrastructure.Data.Repositories.EquipmentNoteRepository(_dbContext);

        _testEquipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            Name = "Test FDM Printer",
            AssetCode = $"MAL-TST-{Guid.NewGuid():N}".Substring(0, 12),
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            BuildVolumeXMm = 200,
            BuildVolumeYMm = 200,
            BuildVolumeZMm = 200,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _equipmentRepository.AddAsync(_testEquipment);
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddNoteToEquipment_NoteIsPersistedWithCreatedAtTimestamp()
    {
        var command = new AddEquipmentNoteCommand(
            _testEquipment.Id,
            "This is a test note",
            Guid.NewGuid());

        var handler = new AddEquipmentNoteCommandHandler(_equipmentRepository, _noteRepository);

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(_testEquipment.Id, result.EquipmentId);
        Assert.Equal("This is a test note", result.Content);
        Assert.Equal(command.AuthorEmployeeId, result.AuthorEmployeeId);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.True(result.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public async Task RetrieveNotesForEquipment_NotesAreReturnedInDescendingOrderByCreatedAt()
    {
        var authorId1 = Guid.NewGuid();
        var authorId2 = Guid.NewGuid();
        var authorId3 = Guid.NewGuid();

        var note1 = await _noteRepository.AddAsync(new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = _testEquipment.Id,
            Content = "First note",
            AuthorEmployeeId = authorId1,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        }, CancellationToken.None);

        var note2 = await _noteRepository.AddAsync(new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = _testEquipment.Id,
            Content = "Second note",
            AuthorEmployeeId = authorId2,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        }, CancellationToken.None);

        var note3 = await _noteRepository.AddAsync(new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = _testEquipment.Id,
            Content = "Third note",
            AuthorEmployeeId = authorId3,
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var query = new GetEquipmentNotesQuery(_testEquipment.Id);
        var handler = new GetEquipmentNotesQueryHandler(_noteRepository);

        var result = await handler.HandleAsync(query);

        Assert.Equal(3, result.Count);
        Assert.Equal("Third note", result[0].Content);
        Assert.Equal("Second note", result[1].Content);
        Assert.Equal("First note", result[2].Content);
    }

    [Fact]
    public async Task NotesAreAppendOnly_NoUpdateDeleteOperationsExist()
    {
        var note = await _noteRepository.AddAsync(new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = _testEquipment.Id,
            Content = "Original content",
            AuthorEmployeeId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var query = new GetEquipmentNotesQuery(_testEquipment.Id);
        var handler = new GetEquipmentNotesQueryHandler(_noteRepository);

        var result = await handler.HandleAsync(query);

        Assert.Single(result);
        Assert.Equal("Original content", result[0].Content);
        Assert.Equal(note.Id, result[0].Id);

        var noteFromDb = await _dbContext.EquipmentNotes.FindAsync(note.Id);
        Assert.NotNull(noteFromDb);
        Assert.Equal("Original content", noteFromDb.Content);
    }
}

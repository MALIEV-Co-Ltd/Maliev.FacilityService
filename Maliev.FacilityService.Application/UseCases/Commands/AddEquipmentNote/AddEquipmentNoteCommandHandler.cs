using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.AddEquipmentNote;

/// <summary>
/// Handler for the <see cref="AddEquipmentNoteCommand"/>.
/// Appends a note to the equipment (append-only operation).
/// </summary>
public class AddEquipmentNoteCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentNoteRepository _noteRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="AddEquipmentNoteCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="noteRepository">The equipment note repository.</param>
    public AddEquipmentNoteCommandHandler(
        IEquipmentRepository equipmentRepository,
        IEquipmentNoteRepository noteRepository)
    {
        _equipmentRepository = equipmentRepository;
        _noteRepository = noteRepository;
    }

    /// <summary>
    /// Handles the addition of a note to equipment.
    /// </summary>
    /// <param name="command">The add note command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created note DTO.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    public async Task<NoteDto> HandleAsync(
        AddEquipmentNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await _equipmentRepository.GetByIdAsync(command.EquipmentId, cancellationToken);
        if (exists is null)
            throw new EquipmentNotFoundException(command.EquipmentId);

        var note = new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = command.EquipmentId,
            Content = command.Content,
            AuthorEmployeeId = command.AuthorEmployeeId,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _noteRepository.AddAsync(note, cancellationToken);
        return saved.ToDto();
    }
}

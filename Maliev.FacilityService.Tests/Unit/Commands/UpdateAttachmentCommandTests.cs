using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.UpdateAttachment;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Commands;

public class UpdateAttachmentCommandHandlerTests
{
    private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock;
    private readonly UpdateAttachmentCommandHandler _handler;

    public UpdateAttachmentCommandHandlerTests()
    {
        _attachmentRepositoryMock = new Mock<IAttachmentRepository>();
        _handler = new UpdateAttachmentCommandHandler(_attachmentRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidAttachment_UpdatesSuccessfully()
    {
        var equipmentId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var attachment = CreateTestAttachment(attachmentId, equipmentId);

        _attachmentRepositoryMock
            .Setup(r => r.GetByEquipmentIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EquipmentAttachment> { attachment });

        _attachmentRepositoryMock
            .Setup(r => r.UpdateAsync(attachment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);

        var command = new UpdateAttachmentCommand(
            equipmentId,
            attachmentId,
            "Updated Attachment Name",
            "SN-Updated-001",
            true,
            "Good condition",
            1);

        var result = await _handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Updated Attachment Name", attachment.Name);
        Assert.Equal("SN-Updated-001", attachment.SerialNumber);
        Assert.True(attachment.IsActive);
        _attachmentRepositoryMock.Verify(
            r => r.UpdateAsync(attachment, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AttachmentNotFound_ThrowsKeyNotFoundException()
    {
        var equipmentId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();

        _attachmentRepositoryMock
            .Setup(r => r.GetByEquipmentIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EquipmentAttachment>());

        var command = new UpdateAttachmentCommand(
            equipmentId,
            attachmentId,
            "Updated Attachment Name",
            "SN-Updated-001",
            true,
            "Good condition",
            1);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.HandleAsync(command));

        _attachmentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<EquipmentAttachment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static EquipmentAttachment CreateTestAttachment(Guid id, Guid equipmentId)
    {
        return new EquipmentAttachment
        {
            Id = id,
            EquipmentId = equipmentId,
            Name = "Original Attachment",
            AttachmentType = AttachmentType.Collet,
            SerialNumber = "SN-Original-001",
            IsActive = true,
            ConditionNotes = "Original condition"
        };
    }
}

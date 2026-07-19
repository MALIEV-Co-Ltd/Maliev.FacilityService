using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Tests.Unit.Domain;

public class DomainExceptionsTests
{
    [Fact]
    public void AttachmentNotAllowedException_Constructor_SetsPropertiesCorrectly()
    {
        var equipmentId = Guid.NewGuid();
        var category = EquipmentCategory.FdmPrinter.ToString();

        var exception = new AttachmentNotAllowedException(equipmentId, category);

        Assert.Equal(equipmentId, exception.EquipmentId);
        Assert.Equal(category, exception.EquipmentCategory);
        Assert.Contains(category, exception.Message);
    }

    [Fact]
    public void AttachmentNotAllowedException_CustomMessage_Constructor_SetsPropertiesCorrectly()
    {
        var equipmentId = Guid.NewGuid();
        var category = EquipmentCategory.SlaPrinter.ToString();
        var customMessage = "Custom error message";

        var exception = new AttachmentNotAllowedException(customMessage, equipmentId, category);

        Assert.Equal(equipmentId, exception.EquipmentId);
        Assert.Equal(category, exception.EquipmentCategory);
        Assert.Equal(customMessage, exception.Message);
    }

    [Fact]
    public void EquipmentNotFoundException_Constructor_SetsPropertiesCorrectly()
    {
        var equipmentId = Guid.NewGuid();

        var exception = new EquipmentNotFoundException(equipmentId);

        Assert.Equal(equipmentId, exception.EquipmentId);
        Assert.Contains(equipmentId.ToString(), exception.Message);
    }

    [Fact]
    public void InvalidStatusTransitionException_Constructor_SetsPropertiesCorrectly()
    {
        var fromStatus = EquipmentStatus.Active.ToString();
        var toStatus = EquipmentStatus.Decommissioned.ToString();

        var exception = new InvalidStatusTransitionException(fromStatus, toStatus);

        Assert.Equal(fromStatus, exception.CurrentStatus);
        Assert.Equal(toStatus, exception.TargetStatus);
        Assert.Contains(fromStatus, exception.Message);
        Assert.Contains(toStatus, exception.Message);
    }

    [Fact]
    public void LoanNotAllowedException_Constructor_SetsPropertiesCorrectly()
    {
        var equipmentId = Guid.NewGuid();
        var reason = "Equipment is under maintenance";

        var exception = new LoanNotAllowedException(equipmentId, reason);

        Assert.Equal(equipmentId, exception.EquipmentId);
        Assert.Equal(reason, exception.Reason);
        Assert.Contains(reason, exception.Message);
    }

    [Fact]
    public void JobServiceUnavailableException_Constructor_SetsPropertiesCorrectly()
    {
        var message = "Job service is unavailable";

        var exception = new JobServiceUnavailableException(message);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void JobServiceUnavailableException_InnerException_SetsInnerException()
    {
        var innerException = new InvalidOperationException("Inner error");
        var message = "Job service is unavailable";

        var exception = new JobServiceUnavailableException(message, innerException);

        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void EquipmentHasJobHistoryException_Constructor_SetsPropertiesCorrectly()
    {
        var equipmentId = Guid.NewGuid();

        var exception = new EquipmentHasJobHistoryException(equipmentId, 5);

        Assert.Equal(equipmentId, exception.EquipmentId);
        Assert.Equal(5, exception.JobCount);
        Assert.Contains(equipmentId.ToString(), exception.Message);
    }
}

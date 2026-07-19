using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Tests.Unit.Domain;

public class EquipmentTests
{
    [Fact]
    public void TransitionTo_ValidTransition_UpdatesStatus()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.Active };

        equipment.TransitionTo(EquipmentStatus.UnderMaintenance);

        Assert.Equal(EquipmentStatus.UnderMaintenance, equipment.Status);
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ThrowsInvalidStatusTransitionException()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.Decommissioned };

        Assert.Throws<InvalidStatusTransitionException>(() =>
            equipment.TransitionTo(EquipmentStatus.Active));
    }

    [Fact]
    public void TransitionTo_FromActive_CanTransitionToMultipleStatuses()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.Active };

        equipment.TransitionTo(EquipmentStatus.OnLoan);
        Assert.Equal(EquipmentStatus.OnLoan, equipment.Status);
    }

    [Fact]
    public void TransitionTo_FromOnLoan_CanReturnToActive()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.OnLoan };

        equipment.TransitionTo(EquipmentStatus.Active);

        Assert.Equal(EquipmentStatus.Active, equipment.Status);
    }

    [Fact]
    public void TransitionTo_FromOnLoan_CanTransitionToLost()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.OnLoan };

        equipment.TransitionTo(EquipmentStatus.Lost);

        Assert.Equal(EquipmentStatus.Lost, equipment.Status);
    }

    [Fact]
    public void TransitionTo_FromDecommissioned_CannotTransition()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.Decommissioned };

        Assert.Throws<InvalidStatusTransitionException>(() =>
            equipment.TransitionTo(EquipmentStatus.Active));

        Assert.Throws<InvalidStatusTransitionException>(() =>
            equipment.TransitionTo(EquipmentStatus.UnderMaintenance));

        Assert.Throws<InvalidStatusTransitionException>(() =>
            equipment.TransitionTo(EquipmentStatus.OnLoan));

        Assert.Throws<InvalidStatusTransitionException>(() =>
            equipment.TransitionTo(EquipmentStatus.Lost));
    }

    [Fact]
    public void TransitionTo_FromUnderMaintenance_CanReturnToActive()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.UnderMaintenance };

        equipment.TransitionTo(EquipmentStatus.Active);

        Assert.Equal(EquipmentStatus.Active, equipment.Status);
    }

    [Fact]
    public void TransitionTo_FromLost_CanReturnToActive()
    {
        var equipment = new TestEquipment { Status = EquipmentStatus.Lost };

        equipment.TransitionTo(EquipmentStatus.Active);

        Assert.Equal(EquipmentStatus.Active, equipment.Status);
    }

    private class TestEquipment : Equipment
    {
    }
}

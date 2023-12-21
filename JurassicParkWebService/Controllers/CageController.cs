using System;
using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;
using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Mvc;

namespace JurassicParkWebService.Controllers; 

[ApiController]
[Route("[controller]")]
public sealed class CageController : EntityController<Cage, InboundCageResource, OutboundCageResource> {
    private readonly ICageStore _cageStore;

    public CageController(ICageStore cageStore) : base(cageStore) {
        _cageStore = cageStore;
    }

    [HttpGet]
    public IActionResult Search([FromQuery] string? cageName, [FromQuery] string? powerStatus) {
        CagePowerStatus? powerStatusValue = null;
        if (powerStatus != null) {
            if (!Enum.TryParse<CagePowerStatus>(powerStatus, out var parsedPowerStatusValue)) {
                return StatusCode(400, "PowerStatus must be 'active' or 'down'.");
            }

            powerStatusValue = parsedPowerStatusValue;
        }

        var resources = _cageStore.Search(cageName, powerStatusValue).Select(x => new OutboundCageResource(x));

        return StatusCode(200, resources);
    }

    protected override Cage CreateFromInboundResource(InboundCageResource inboundResource) {
        return new Cage {
            Name = inboundResource.Name!,
            MaxCapacity = inboundResource.MaxCapacity!.Value
        };
    }

    protected override void UpdateFromInboundResource(Cage cage, InboundCageResource inboundResource) {
        cage.Name = inboundResource.Name!;
        cage.MaxCapacity = inboundResource.MaxCapacity!.Value;
        cage.PowerStatus = Enum.Parse<CagePowerStatus>(inboundResource.PowerStatus!);
    }

    protected override OutboundCageResource CreateOutboundResource(Cage cage) {
        return new OutboundCageResource(cage);
    }

    protected override string? ValidateInboundEntity(InboundCageResource inboundCageResource, Cage? cageToUpdate = null) {
        if (string.IsNullOrEmpty(inboundCageResource.Name)) {
            return "Name must be supplied.";
        }

        var cagesWithSameName = _cageStore.Search(inboundCageResource.Name, powerStatus: null);
        if (cagesWithSameName.Any(x => cageToUpdate == null || x.Id != cageToUpdate.Id)) {
            return "Name already exists.";
        }

        if (inboundCageResource.MaxCapacity == null) {
            return "MaxCapacity must be supplied.";
        }

        if (inboundCageResource.MaxCapacity <= 0) {
            return "MaxCapacity is invalid.";
        }

        if (cageToUpdate == null) {
            return null;
        }

        //this is validation that only applies to updates
        if (inboundCageResource.MaxCapacity < cageToUpdate.DinosaurCount) {
            return "MaxCapacity must be higher than DinosaurCount.";
        }

        if (string.IsNullOrEmpty(inboundCageResource.PowerStatus)) {
            return "PowerStatus must be supplied.";
        }

        if (!Enum.TryParse<CagePowerStatus>(inboundCageResource.PowerStatus, out var powerStatus)) {
            return "PowerStatus must be 'active' or 'down'.";
        }

        if (powerStatus == CagePowerStatus.Down && cageToUpdate.DinosaurCount > 0) {
            return "PowerStatus cannot be set to 'down' when DinosaurCount > 0.";
        }

        return null;
    }

    protected override string? ValidateDeleteEntity(Cage cageToDelete) {
        if (cageToDelete.DinosaurCount > 0) {
            return "Cannot delete cage if DinosaurCount > 0.";
        }

        return null;
    }
}
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
    private readonly IDinosaurStore _dinosaurStore;

    public CageController(ICageStore cageStore, IDinosaurStore dinosaurStore) : base(cageStore) {
        _cageStore = cageStore;
        _dinosaurStore = dinosaurStore;
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

        var cages = _cageStore.Search(cageName, powerStatusValue);
        var resources = cages.Select(CreateOutboundResource);

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
        return new OutboundCageResource(cage, GetDinosaurCount(cage));
    }

    protected override string? ValidateInboundEntity(InboundCageResource inboundResource, Cage? cageToUpdate = null) {
        if (string.IsNullOrEmpty(inboundResource.Name)) {
            return "Name must be supplied.";
        }

        var cagesWithSameName = _cageStore.Search(inboundResource.Name, powerStatus: null);
        if (cagesWithSameName.Any(x => cageToUpdate == null || x.Id != cageToUpdate.Id)) {
            return "Name already exists.";
        }

        if (inboundResource.MaxCapacity == null) {
            return "MaxCapacity must be supplied.";
        }

        if (inboundResource.MaxCapacity <= 0) {
            return "MaxCapacity is invalid.";
        }

        if (cageToUpdate == null) {
            return null;
        }

        //this is validation that only applies to updates
        var dinosaurCount = GetDinosaurCount(cageToUpdate);
        if (inboundResource.MaxCapacity < dinosaurCount) {
            return "MaxCapacity must be higher than DinosaurCount.";
        }

        if (string.IsNullOrEmpty(inboundResource.PowerStatus)) {
            return "PowerStatus must be supplied.";
        }

        if (!Enum.TryParse<CagePowerStatus>(inboundResource.PowerStatus, out var powerStatus)) {
            return "PowerStatus must be 'active' or 'down'.";
        }

        if (powerStatus == CagePowerStatus.Down && dinosaurCount > 0) {
            return "PowerStatus cannot be set to 'down' when DinosaurCount > 0.";
        }

        return null;
    }

    protected override string? ValidateDeleteEntity(Cage cageToDelete) {
        if (GetDinosaurCount(cageToDelete) > 0) {
            return "Cannot delete cage if DinosaurCount > 0.";
        }

        return null;
    }

    private int GetDinosaurCount(Cage cage) {
        return _dinosaurStore.Search(name: null, speciesId: null, cageId: cage.Id).Count;
    }
}
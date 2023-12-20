using System;
using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;
using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Mvc;

namespace JurassicParkWebService.Controllers; 

[ApiController]
[Route("[controller]")]
public sealed class CageController : ControllerBase {
    private readonly ICageStore _cageStore;

    public CageController(ICageStore cageStore) {
        _cageStore = cageStore;
    }


    [HttpPost]
    public IActionResult Add([FromBody] InboundCageResource? inboundCageResource) {
        var validationError = ValidateCageValues(inboundCageResource);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        var newCage = new Cage {
            Name = inboundCageResource!.Name!,
            MaxCapacity = inboundCageResource.MaxCapacity!.Value
        };

        _cageStore.Add(newCage);

        return StatusCode(200, new OutboundCageResource(newCage));
    }

    private string? ValidateCageValues(InboundCageResource? inboundCageResource, Cage? cageToUpdate = null) {
        if (inboundCageResource == null) {
            return "Body must be supplied.";
        }

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

    [HttpGet("{cageId}")]
    public IActionResult Get(int cageId) {
        var cage = _cageStore.Get(cageId);
        if (cage == null) {
            return StatusCode(404, "Cage not found.");
        }

        var resource = new OutboundCageResource(cage);

        return StatusCode(200, resource);
    }

    [HttpPut("{cageId}")]
    public IActionResult Update(int cageId, [FromBody] InboundCageResource? inboundCageResource) {
        var cage = _cageStore.Get(cageId);
        if (cage == null) {
            return StatusCode(404, "Cage not found.");
        }

        var validationError = ValidateCageValues(inboundCageResource, cage);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        cage.Name = inboundCageResource!.Name!;
        cage.MaxCapacity = inboundCageResource.MaxCapacity!.Value;
        cage.PowerStatus = Enum.Parse<CagePowerStatus>(inboundCageResource.PowerStatus!);

        _cageStore.Update(cage);

        return StatusCode(200, new OutboundCageResource(cage));
    }

    [HttpDelete("{cageId}")]
    public IActionResult Delete(int cageId) {
        var cage = _cageStore.Get(cageId);
        if (cage == null) {
            return StatusCode(404, "Cage not found.");
        }

        if (cage.DinosaurCount > 0) {
            return StatusCode(400, "Cannot delete cage if DinosaurCount > 0.");
        }

        _cageStore.Delete(cageId);

        return StatusCode(200);
    }
}
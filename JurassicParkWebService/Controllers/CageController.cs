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
    public IActionResult Add([FromForm] string? cageName, [FromForm] int? maxCapacity) {
        var validationError = ValidateCageValues(cageName, maxCapacity);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        var newCage = new Cage {
            Name = cageName!,
            MaxCapacity = maxCapacity!.Value
        };

        _cageStore.Add(newCage);

        return StatusCode(200, new CageResource(newCage));
    }

    private string? ValidateCageValues(string? cageName, int? maxCapacity, Cage? updateCage = null) {
        if (string.IsNullOrEmpty(cageName)) {
            return "cageName must be supplied.";
        }
        
        var existingCage = _cageStore.Search(cageName, powerStatus: null);
        if (existingCage.Any(x => updateCage == null || x.Id != updateCage.Id)) {
            return "Name already exists.";
        }

        if (maxCapacity == null) {
            return "maxCapacity must be supplied.";
        }
        
        if (maxCapacity <= 0) {
            return "maxCapacity is invalid.";
        }

        if (updateCage != null && maxCapacity < updateCage.DinosaurCount) {
            return "maxCapacity must be higher than dinosaurCount.";
        }

        return null;
    }

    [HttpGet]
    public IActionResult Search([FromQuery] string? cageName, [FromQuery] string? powerStatus) {
        CagePowerStatus? powerStatusValue = null;
        if (powerStatus != null) {
            if (!Enum.TryParse<CagePowerStatus>(powerStatus, out var parsedPowerStatusValue)) {
                return StatusCode(400, "powerStatus must be 'active' or 'down'.");
            }

            powerStatusValue = parsedPowerStatusValue;
        }

        var resources = _cageStore.Search(cageName, powerStatusValue).Select(x => new CageResource(x));

        return StatusCode(200, resources);
    }

    [HttpGet("{cageId}")]
    public IActionResult Get(int cageId) {
        var cage = _cageStore.Get(cageId);
        if (cage == null) {
            return StatusCode(404, "Cage not found.");
        }

        var resource = new CageResource(cage);

        return StatusCode(200, resource);
    }

    [HttpPut("{cageId}")]
    public IActionResult Update(int cageId, [FromForm] string? cageName, [FromForm] int? maxCapacity) {
        var cage = _cageStore.Get(cageId);
        if (cage == null) {
            return StatusCode(404, "Cage not found.");
        }

        var validationError = ValidateCageValues(cageName, maxCapacity, cage);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        cage.Name = cageName!;
        cage.MaxCapacity = maxCapacity!.Value;

        _cageStore.Update(cage);

        return StatusCode(200, new CageResource(cage));
    }
}
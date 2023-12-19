using System.Linq;
using System.Resources;
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
        var validationError = ValidateNewCage(cageName, maxCapacity);
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

    private string? ValidateNewCage(string? cageName, int? maxCapacity) {
        if (string.IsNullOrEmpty(cageName)) {
            return "cageName must be supplied.";
        }
        
        var existingCage = _cageStore.Search(name: cageName);
        if (existingCage.Any()) {
            return "Name already exists.";
        }

        if (maxCapacity == null) {
            return "maxCapacity must be supplied.";
        }
        
        if (maxCapacity <= 0) {
            return "maxCapacity is invalid.";
        }

        return null;
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

    [HttpGet]
    public IActionResult Search() {
        var resources = _cageStore.Search(null).Select(x => new CageResource(x));

        return StatusCode(200, resources);
    }
}
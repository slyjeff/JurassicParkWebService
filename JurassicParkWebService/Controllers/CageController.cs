using System.Linq;
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

    [HttpGet]
    public IActionResult Search() {
        var resources = _cageStore.Search().Select(x => new CageResource(x));

        return StatusCode(200, resources);
    }
}
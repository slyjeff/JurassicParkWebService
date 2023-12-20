using System;
using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;
using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Mvc;

namespace JurassicParkWebService.Controllers; 

[ApiController]
[Route("[controller]")]
public sealed class SpeciesController : ControllerBase {
    private readonly ISpeciesStore _speciesStore;

    public SpeciesController(ISpeciesStore speciesStore) {
        _speciesStore = speciesStore;
    }

    [HttpPost]
    public IActionResult Add([FromBody] InboundSpeciesResource? inboundSpeciesResource) {
        var validationError = ValidateSpeciesValues(inboundSpeciesResource);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        var newSpecies = new Species {
            Name = inboundSpeciesResource!.Name!,
            SpeciesType = Enum.Parse<SpeciesType>(inboundSpeciesResource.SpeciesType!) 
        };

        _speciesStore.Add(newSpecies);

        return StatusCode(200, new OutboundSpeciesResource(newSpecies));
    }

    private string? ValidateSpeciesValues(InboundSpeciesResource? inboundSpeciesResource, Species? speciesToUpdate = null) {
        if (inboundSpeciesResource == null) {
            return "Body must be supplied.";
        }

        if (string.IsNullOrEmpty(inboundSpeciesResource.Name)) {
            return "Name must be supplied.";
        }

        var speciesWithSameName = _speciesStore.Search(inboundSpeciesResource.Name);
        if (speciesWithSameName.Any(x => speciesToUpdate == null || x.Id != speciesToUpdate.Id)) {
            return "Name already exists.";
        }

        if (string.IsNullOrEmpty(inboundSpeciesResource.SpeciesType)) {
            return "SpeciesType must be supplied.";
        }

        //validate SpeciesType when creating
        if (speciesToUpdate == null) {
            if (!Enum.TryParse<SpeciesType>(inboundSpeciesResource.SpeciesType, out _)) {
                return "SpeciesType must be 'carnivore' or 'herbivore'.";
            }

            return null;
        }

        //validate SpeciesType doesn't change when updating
        if (inboundSpeciesResource.SpeciesType != speciesToUpdate.SpeciesType.ToString()) {
            return "SpeciesType cannot be changed.";
        }

        return null;
    }

    [HttpGet("{speciesId}")]
    public IActionResult Get(int speciesId) {
        var species = _speciesStore.Get(speciesId);
        if (species == null) {
            return StatusCode(404, "Species not found.");
        }

        var resource = new OutboundSpeciesResource(species);

        return StatusCode(200, resource);
    }

    [HttpPut("{speciesId}")]
    public IActionResult Update(int speciesId, [FromBody] InboundSpeciesResource? inboundSpeciesResource) {
        var species = _speciesStore.Get(speciesId);
        if (species == null) {
            return StatusCode(404, "Species not found.");
        }

        var validationError = ValidateSpeciesValues(inboundSpeciesResource, species);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        species.Name = inboundSpeciesResource!.Name!;

        _speciesStore.Update(species);

        return StatusCode(200, new OutboundSpeciesResource(species));
    }

    [HttpDelete("{speciesId}")]
    public IActionResult Delete(int speciesId) {
        var cage = _speciesStore.Get(speciesId);
        if (cage == null) {
            return StatusCode(404, "Species not found.");
        }


        //todo: don't allow delete if any dinosaurs are assigned to this species

        _speciesStore.Delete(speciesId);

        return StatusCode(200);
    }
}
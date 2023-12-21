using System;
using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;
using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Mvc;

namespace JurassicParkWebService.Controllers; 

[ApiController]
[Route("[controller]")]
public sealed class SpeciesController : EntityController<Species, InboundSpeciesResource, OutboundSpeciesResource> {
    private readonly ISpeciesStore _speciesStore;
    private readonly IDinosaurStore _dinosaurStore;

    public SpeciesController(ISpeciesStore speciesStore, IDinosaurStore dinosaurStore) : base (speciesStore) {
        _speciesStore = speciesStore;
        _dinosaurStore = dinosaurStore;
    }

    protected override Species CreateFromInboundResource(InboundSpeciesResource inboundResource) {
        return new Species {
            Name = inboundResource.Name!,
            SpeciesType = Enum.Parse<SpeciesType>(inboundResource.SpeciesType!)
        };
    }

    protected override OutboundSpeciesResource CreateOutboundResource(Species species) {
        return new OutboundSpeciesResource(species);
    }

    protected override void UpdateFromInboundResource(Species species, InboundSpeciesResource inboundResource) {
        species.Name = inboundResource.Name!;
    }

    protected override string? ValidateInboundEntity(InboundSpeciesResource inboundSpeciesResource, Species? speciesToUpdate = null) {
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

    protected override string? ValidateDeleteEntity(Species speciesToDelete) {
        if (_dinosaurStore.Search(name: null, speciesToDelete.Id).Any()) {
            return "Cannot delete while Dinosaurs of this species exist.";
        }

        return null;
    }
}
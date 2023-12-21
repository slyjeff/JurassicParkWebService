using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;
using JurassicParkWebService.Stores;
using Microsoft.AspNetCore.Mvc;

namespace JurassicParkWebService.Controllers; 

[ApiController]
[Route("[controller]")]
public sealed class DinosaurController : EntityController<Dinosaur, InboundDinosaurResource, OutboundDinosaurResource> {
    private readonly IDinosaurStore _dinosaurStore;
    private readonly ISpeciesStore _speciesStore;

    public DinosaurController(IDinosaurStore dinosaurStore, ISpeciesStore speciesStore) : base (dinosaurStore) {
        _dinosaurStore = dinosaurStore;
        _speciesStore = speciesStore;
    }

    protected override Dinosaur CreateFromInboundResource(InboundDinosaurResource inboundResource) {
        return new Dinosaur {
            Name = inboundResource.Name!,
            SpeciesId = inboundResource.SpeciesId!.Value
        };
    }

    protected override OutboundDinosaurResource CreateOutboundResource(Dinosaur dinosaur) {
        var species = _speciesStore.Get(dinosaur.SpeciesId);
        return new OutboundDinosaurResource(dinosaur, species!);
    }

    protected override void UpdateFromInboundResource(Dinosaur dinosaur, InboundDinosaurResource inboundResource) {
        dinosaur.Name = inboundResource.Name!;
        dinosaur.SpeciesId = inboundResource.SpeciesId!.Value;
    }

    protected override string? ValidateInboundEntity(InboundDinosaurResource inboundResource, Dinosaur? dinosaurToUpdate = null) {
        if (string.IsNullOrEmpty(inboundResource.Name)) {
            return "Name must be supplied.";
        }

        var dinosaurWithSameName = _dinosaurStore.Search(name: inboundResource.Name);
        if (dinosaurWithSameName.Any(x => dinosaurToUpdate == null || x.Id != dinosaurToUpdate.Id)) {
            return "Name already exists.";
        }

        if (inboundResource.SpeciesId == null) {
            return "SpeciesId must be supplied.";
        }

        var species = _speciesStore.Get(inboundResource.SpeciesId.Value);
        if (species == null) {
            return "SpeciesId is invalid.";
        } 

        if (dinosaurToUpdate == null) {
            return null;
        }

        if (dinosaurToUpdate.CageId != null && dinosaurToUpdate.SpeciesId != species.Id) {
            return "Species cannot be changed for a dinosaur in a cage.";
        }

        return null;
    }

    protected override string? ValidateDeleteEntity(Dinosaur speciesToDelete) {
        //no validation is required when deleting dinosaurs
        return null;
    }
}
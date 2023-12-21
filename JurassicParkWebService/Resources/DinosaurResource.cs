using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Resources; 

public sealed class OutboundDinosaurResource {
    public OutboundDinosaurResource(Dinosaur sourceDinosaur, Species species) {
        Id = sourceDinosaur.Id;
        Name = sourceDinosaur.Name;
        SpeciesId = species.Id;
        SpeciesName = species.Name;
    }

    public int Id { get; }
    public string Name { get; }
    public int SpeciesId { get; }
    public string SpeciesName { get; }
}

public sealed class InboundDinosaurResource {
    public string? Name { get; set; }
    public int? SpeciesId { get; set; }
}
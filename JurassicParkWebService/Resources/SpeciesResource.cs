using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Resources; 

public sealed class OutboundSpeciesResource {
    public OutboundSpeciesResource(Species sourceSpecies) {
        Id = sourceSpecies.Id;
        Name = sourceSpecies.Name;
        SpeciesType = sourceSpecies.SpeciesType.ToString();
    }

    public int Id { get; }
    public string Name { get; }
    public string SpeciesType { get; }
}

public sealed class InboundSpeciesResource {
    public string? Name { get; set; }
    public string? SpeciesType { get; set; }
}
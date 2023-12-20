using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Resources; 

public sealed class OutboundCageResource {
    public OutboundCageResource(Cage sourceCage) {
        Id = sourceCage.Id;
        Name = sourceCage.Name;
        MaxCapacity = sourceCage.MaxCapacity;
        DinosaurCount = sourceCage.DinosaurCount;
        PowerStatus = sourceCage.PowerStatus.ToString();
    }

    public int Id { get; }
    public string Name { get; }
    public int MaxCapacity { get; }
    public int DinosaurCount { get; }
    public string PowerStatus { get; }
}

public sealed class InboundCageResource {
    public string? Name { get; set; }
    public int? MaxCapacity { get; set; }
    public string? PowerStatus { get; set; }
}
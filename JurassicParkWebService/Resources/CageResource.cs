using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Resources; 

public sealed class CageResource {
    public CageResource(Cage sourceCage) {
        Id = sourceCage.Id;
        MaxCapacity = sourceCage.MaxCapacity;
        DinosaurCount = sourceCage.DinosaurCount;
        PowerStatus = sourceCage.PowerStatus.ToString();
    }

    public int Id { get; }
    public int MaxCapacity { get; }
    public int DinosaurCount { get; }
    public string PowerStatus { get; }
}
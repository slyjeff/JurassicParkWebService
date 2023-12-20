namespace JurassicParkWebService.Entities; 

public enum CagePowerStatus { Active, Down }

public sealed class Cage {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public int DinosaurCount { get; set; } //todo: DinosaurCount should be read-only, determined when loading from the database
    public CagePowerStatus PowerStatus { get; set; } = CagePowerStatus.Active;

    public override bool Equals(object? obj) {
        if (obj is not Cage cage) {
            return false;
        }

        return Id == cage.Id
            && Name == cage.Name
            && MaxCapacity == cage.MaxCapacity
            && DinosaurCount == cage.DinosaurCount
            && PowerStatus == cage.PowerStatus;
    }
}
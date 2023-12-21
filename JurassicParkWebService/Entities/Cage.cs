namespace JurassicParkWebService.Entities; 

public enum CagePowerStatus { Active, Down }

public sealed class Cage : IdentifiableEntity {
    public string Name { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public CagePowerStatus PowerStatus { get; set; } = CagePowerStatus.Active;

    public override bool Equals(object? obj) {
        if (obj is not Cage cage) {
            return false;
        }

        return Id == cage.Id
            && Name == cage.Name
            && MaxCapacity == cage.MaxCapacity
            && PowerStatus == cage.PowerStatus;
    }
}
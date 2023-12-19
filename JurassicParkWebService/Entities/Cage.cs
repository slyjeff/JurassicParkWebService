namespace JurassicParkWebService.Entities; 

public enum CagePowerStatus { Up, Down }

public sealed class Cage {
    public int Id { get; set; }
    public int MaxCapacity { get; set; }
    public int DinosaurCount { get; set; }
    public CagePowerStatus PowerStatus { get; set; } = CagePowerStatus.Up;
}
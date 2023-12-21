namespace JurassicParkWebService.Entities; 

public enum SpeciesType { Carnivore, Herbivore }

public sealed class Species : IdentifiableEntity {
    public string Name { get; set; } = string.Empty;
    public SpeciesType SpeciesType { get; set; } = SpeciesType.Carnivore;

    public override bool Equals(object? obj) {
        if (obj is not Species species) {
            return false;
        }

        return Id == species.Id
            && Name == species.Name
            && SpeciesType == species.SpeciesType;
    }
}
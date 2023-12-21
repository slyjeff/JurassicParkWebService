namespace JurassicParkWebService.Entities; 

public sealed class Dinosaur : IdentifiableEntity {
    public string Name { get; set; } = string.Empty;
    public int SpeciesId { get; set; }
    public int? CageId { get; set; }

    public override bool Equals(object? obj) {
        if (obj is not Dinosaur dinosaur) {
            return false;
        }

        return Id == dinosaur.Id
            && Name == dinosaur.Name
            && SpeciesId == dinosaur.SpeciesId
            && CageId == dinosaur.CageId;
    }
}
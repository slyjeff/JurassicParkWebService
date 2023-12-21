using JurassicParkWebService.Entities;
using System.Collections.Generic;

namespace JurassicParkWebService.Stores;

public interface IDinosaurStore : IStore<Dinosaur> {
    IList<Dinosaur> Search(string? name = null, int? speciesId = null, int? cageId = null, bool? isCarnivore = null);
}

internal sealed class DinosaurStore : Store<Dinosaur>, IDinosaurStore {
    public DinosaurStore(IDatabaseConfiguration databaseConfiguration) : base(databaseConfiguration) { }

    public IList<Dinosaur> Search(string? name, int? speciesId, int? cageId, bool? isCarnivore = null) {
        var searchParameters = new Dictionary<string, object>();
        var join = string.Empty;

        if (!string.IsNullOrEmpty(name)) {
            searchParameters.Add("Name", name);
        }

        if (speciesId != null) {
            searchParameters.Add("SpeciesId", speciesId.Value);
        }

        if (cageId != null) {
            searchParameters.Add("CageId", cageId.Value);
        }

        if (isCarnivore != null) {
            join += "JOIN Species on (Dinosaur.SpeciesId = Species.Id)";
            var speciesType = isCarnivore.Value ? SpeciesType.Carnivore : SpeciesType.Herbivore;
            searchParameters.Add("SpeciesType", speciesType.ToString());
        }

        return Search(searchParameters, join);
    }
}
using JurassicParkWebService.Entities;
using System.Collections.Generic;

namespace JurassicParkWebService.Stores;

public interface IDinosaurStore : IStore<Dinosaur> {
    IList<Dinosaur> Search(string? name = null, int? speciesId = null, int? cageId = null, bool? isCarnivore = null);
}

internal sealed class DinosaurStore : Store<Dinosaur>, IDinosaurStore {
    public IList<Dinosaur> Search(string? name, int? speciesId, int? cageId, bool? isCarnivore = null) {
        return new List<Dinosaur>();
    }
}
using JurassicParkWebService.Entities;
using System.Collections.Generic;

namespace JurassicParkWebService.Stores;

public interface IDinosaurStore : IStore<Dinosaur> {
    IList<Dinosaur> Search(string? name, int? speciesId);
}

internal sealed class DinosaurStore : Store<Dinosaur>, IDinosaurStore {
    public IList<Dinosaur> Search(string? name, int? speciesId) {
        return new List<Dinosaur>();
    }
}
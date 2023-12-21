using JurassicParkWebService.Entities;
using System.Collections.Generic;

namespace JurassicParkWebService.Stores;

public interface ISpeciesStore : IStore<Species> {
    IList<Species> Search(string? name = null);
}

internal sealed class SpeciesStore : Store<Species>, ISpeciesStore {
    public IList<Species> Search(string? name = null) {
        return new List<Species>();
    }
}
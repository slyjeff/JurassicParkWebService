using JurassicParkWebService.Entities;
using System.Collections.Generic;

namespace JurassicParkWebService.Stores;

public interface ISpeciesStore : IStore<Species> {
    IList<Species> Search(string? name = null);
}

internal sealed class SpeciesStore : Store<Species>, ISpeciesStore {

    public SpeciesStore(IDatabaseConfiguration databaseConfiguration) : base(databaseConfiguration) { }

    public IList<Species> Search(string? name = null) {
        var searchParameters  = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(name)) {
            searchParameters.Add("name", name);
        }

        return Search(searchParameters);
    }
}
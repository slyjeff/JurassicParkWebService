using System.Collections.Generic;
using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Stores;

public interface ICageStore : IStore<Cage> {
    IList<Cage> Search(string? name, CagePowerStatus? powerStatus);
}

internal sealed class CageStore : Store<Cage>, ICageStore {
    public CageStore(IDatabaseConfiguration databaseConfiguration) : base(databaseConfiguration) { }

    public IList<Cage> Search(string? name, CagePowerStatus? powerStatus) {
        var searchParameters = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(name)) {
            searchParameters.Add("name", name);
        }

        if (powerStatus != null) {
            searchParameters.Add("powerStatus", powerStatus.ToString()!);
        }

        return Search(searchParameters);
    }
}
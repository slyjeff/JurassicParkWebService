using System.Collections.Generic;
using JurassicParkWebService.Entities;
using Microsoft.Data.SqlClient;

namespace JurassicParkWebService.Stores;

public interface ICageStore : IStore<Cage> {
    IList<Cage> Search(string? name, CagePowerStatus? powerStatus);
}

internal sealed class CageStore : Store<Cage>, ICageStore {
    public CageStore(IDatabaseConfiguration databaseConfiguration) : base(databaseConfiguration) { }

    public IList<Cage> Search(string? name, CagePowerStatus? powerStatus) {
        return new List<Cage>();
    }
}
using System.Collections.Generic;
using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Stores;

public interface ICageStore : IStore<Cage> {
    IList<Cage> Search(string? name, CagePowerStatus? powerStatus);
}

internal sealed class CageStore : Store<Cage>, ICageStore {
    public IList<Cage> Search(string? name, CagePowerStatus? powerStatus) {
        return new List<Cage>();
    }
}
using System.Collections;
using System.Collections.Generic;
using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Stores;

public interface ICageStore {
    void Add(Cage cage);
    IList<Cage> Search(string? name);
}

internal sealed class CageStore : ICageStore {
    public void Add(Cage cage) {
    }

    public IList<Cage> Search(string? name) {
        return new List<Cage>();
    }
}
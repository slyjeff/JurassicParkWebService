using System.Collections;
using System.Collections.Generic;
using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Stores;

public interface ICageStore {
    IList<Cage> Search();
}

internal sealed class CageStore : ICageStore {
    public IList<Cage> Search() {
        return new List<Cage>();
    }
}
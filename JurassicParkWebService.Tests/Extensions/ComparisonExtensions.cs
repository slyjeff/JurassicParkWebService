using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JurassicParkWebService.Tests.Extensions; 

internal static class ComparisonExtensions {
    public static bool EqualsResource(this Cage cage, CageResource cageResource) {
        return cage.Id == cageResource.Id
               && cage.MaxCapacity == cageResource.MaxCapacity
               && cage.DinosaurCount == cageResource.DinosaurCount
               && cage.PowerStatus.ToString() == cageResource.PowerStatus;
    }

    public static bool EqualsResourceList(this IList<Cage> cages, object? value) {
        if (value is not IEnumerable<CageResource> resources) {
            return false;
        }

        var resourcesList = resources.ToList();
        for (var x = 0; x < cages.Count; x++) {
            if (!cages[x].EqualsResource(resourcesList[x])) {
                return false;
            }
        }

        return true;
    }
}
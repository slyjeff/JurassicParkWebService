using System.Collections.Generic;
using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;

namespace JurassicParkWebService.Tests.Extensions; 

internal static class ComparisonExtensions {
    public static bool EqualsResource(this Cage cage, OutboundCageResource? cageResource) {
        if (cageResource == null) {
            return false;
        }

        return cage.Id == cageResource.Id
            && cage.Name == cageResource.Name
            && cage.MaxCapacity == cageResource.MaxCapacity
            && cage.PowerStatus.ToString() == cageResource.PowerStatus;
    }

    public static bool EqualsResourceList(this IList<Cage> cages, object? value) {
        if (value is not IEnumerable<OutboundCageResource> resources) {
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

    public static bool EqualsResource(this Species species, OutboundSpeciesResource? speciesResource) {
        if (speciesResource == null) {
            return false;
        }

        return species.Id == speciesResource.Id
            && species.Name == speciesResource.Name
            && species.SpeciesType.ToString() == speciesResource.SpeciesType;
    }

    public static bool EqualsResource(this Dinosaur dinosaur, OutboundDinosaurResource? dinosaurResource) {
        if (dinosaurResource == null) {
            return false;
        }

        return dinosaur.Id == dinosaurResource.Id
            && dinosaur.Name == dinosaurResource.Name
            && dinosaur.SpeciesId == dinosaurResource.SpeciesId;
    }
}
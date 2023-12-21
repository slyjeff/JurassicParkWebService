using System.Linq;
using System;
using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Tests.Utils;

internal static class GenerateRandom {
    private static readonly Random Random = new();

    public static string String(int length = 0) {
        if (length == 0) {
            length = Random.Next(10, 20);
        }


        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
    public static int Int(int min = 0, int max = 1000) {
        return Random.Next(min, max);
    }

    public static CagePowerStatus PowerStatus() {
        return Int(0, 1) == 1 ? CagePowerStatus.Active : CagePowerStatus.Down;
    }

    public static Cage Cage() {
        var maxCapacity = Int(1, 10);
        var powerStatus = PowerStatus();

        return new Cage {
            Id = Int(),
            Name = String(),
            MaxCapacity = maxCapacity,
            PowerStatus = powerStatus
        };
    }

    public static Dinosaur Dinosaur() {
        return new Dinosaur {
            Id = Int(),
            Name = String(),
            SpeciesId = Int()
        };
    }

    public static SpeciesType SpeciesType() {
        return Int(0, 1) == 1 ? Entities.SpeciesType.Carnivore : Entities.SpeciesType.Herbivore;
    }

    public static Species Species() {
        return new Species {
            Id = Int(),
            Name = String(),
            SpeciesType = SpeciesType()
        };
    }
}
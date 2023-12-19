using System.Linq;
using System;

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
}
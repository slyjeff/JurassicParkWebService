using System.Collections.Generic;
using JurassicParkWebService.Controllers;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Stores;
using JurassicParkWebService.Tests.Extensions;
using JurassicParkWebService.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JurassicParkWebService.Tests.ControllerTests; 

[TestClass]
public class CageControllerTests {
    [TestMethod]
    public void SearchWithoutParametersMustReturnAllCages() {
        //arrange
        var mockCages = new List<Cage>();
        for (var x = 0; x < GenerateRandom.Int(2, 10); x++){
            mockCages.Add(GenerateRandomCage());
        }

        var mockCageStore = new Mock<ICageStore>();
        mockCageStore.Setup(x => x.Search()).Returns(mockCages);

        var cageController = new CageController(mockCageStore.Object);

        //act
        var result = cageController.Search() as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(mockCages.EqualsResourceList(result.Value));
    }

    private Cage GenerateRandomCage() {
        var maxCapacity = GenerateRandom.Int(1, 10);
        var powerStatus = GenerateRandom.Int(0, 1) == 1 ? CagePowerStatus.Up : CagePowerStatus.Down;
        var dinosaurCount = powerStatus == CagePowerStatus.Up ? GenerateRandom.Int(0, maxCapacity) : 0;

        return new Cage {
            Id = GenerateRandom.Int(),
            MaxCapacity = maxCapacity,
            DinosaurCount = dinosaurCount,
            PowerStatus = powerStatus
        };
    }
}
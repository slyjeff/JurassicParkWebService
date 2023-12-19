using System.Collections.Generic;
using JurassicParkWebService.Controllers;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Resources;
using JurassicParkWebService.Stores;
using JurassicParkWebService.Tests.Extensions;
using JurassicParkWebService.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JurassicParkWebService.Tests.ControllerTests; 

[TestClass]
public class CageControllerTests {
    private Mock<ICageStore> _mockCageStore = null!;
    private CageController _cageController = null!;

    [TestInitialize]
    public void Setup() {
        _mockCageStore = new Mock<ICageStore>();
        _mockCageStore.Setup(x => x.Search(It.IsAny<string>())).Returns(new List<Cage>( ));


        _cageController = new CageController(_mockCageStore.Object);
    }

    #region Add
    [TestMethod]
    public void AddMustCreateCageInTheDatabase() {
        //arrange
        var randomGeneratedId = GenerateRandom.Int();

        var cageName = GenerateRandom.String();
        var maxCapacity = GenerateRandom.Int(1, 10);

        _mockCageStore.Setup(x => x.Add(It.IsAny<Cage>())).Callback((Cage c) => c.Id = randomGeneratedId);

        //act
        var result = _cageController.Add(cageName: cageName, maxCapacity: maxCapacity) as ObjectResult;

        //assert
        var expectedCage = new Cage {
            Id = randomGeneratedId,
            Name = cageName,
            MaxCapacity = maxCapacity,
            DinosaurCount = 0,
            PowerStatus = CagePowerStatus.Up
        };

        _mockCageStore.Verify(x => x.Add(It.Is<Cage>(y => y.Equals(expectedCage))));

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        var resource = result.Value as CageResource;
        Assert.IsNotNull(resource);
        Assert.IsTrue(expectedCage.EqualsResource(resource));
    }

    [TestMethod]
    public void AddMustReturnErrorIfNameNotSupplied() {
        //arrange
        var maxCapacity = GenerateRandom.Int(1, 10);

        //act
        var result = _cageController.Add(cageName: null, maxCapacity: maxCapacity) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("cageName must be supplied.", result.Value);
    }
    
    [TestMethod]
    public void AddMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var cageName = GenerateRandom.String();
        var maxCapacity = GenerateRandom.Int(1, 10);

        _mockCageStore.Setup(x => x.Search(cageName)).Returns(new List<Cage>{GenerateRandomCage()});

        //act
        var result = _cageController.Add(cageName: cageName, maxCapacity: maxCapacity) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name already exists.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfMaxCapacityIsNotSupplied() {
        //arrange
        var cageName = GenerateRandom.String();

        //act
        var result = _cageController.Add(cageName: cageName, maxCapacity: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("maxCapacity must be supplied.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfMaxCapacityIsNotGreaterThanZero() {
        //arrange
        var cageName = GenerateRandom.String();

        //act
        var result = _cageController.Add(cageName: cageName, maxCapacity: 0) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("maxCapacity is invalid.", result.Value);
    }
    #endregion

    #region Search
    [TestMethod]
    public void SearchWithoutParametersMustReturnAllCages() {
        //arrange
        var mockCages = new List<Cage>();
        for (var x = 0; x < GenerateRandom.Int(2, 10); x++) {
            mockCages.Add(GenerateRandomCage());
        }

        _mockCageStore.Setup(x => x.Search(null)).Returns(mockCages);

        //act
        var result = _cageController.Search() as ObjectResult;

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
    #endregion
}
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
public sealed class CageControllerTests {
    private Mock<ICageStore> _mockCageStore = null!;
    private CageController _cageController = null!;

    [TestInitialize]
    public void Setup() {
        _mockCageStore = new Mock<ICageStore>();
        _mockCageStore.Setup(x => x.Search(It.IsAny<string?>(), It.IsAny<CagePowerStatus?>())).Returns(new List<Cage>( ));

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
            PowerStatus = CagePowerStatus.Active
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
        var result = _cageController.Add(cageName: null, maxCapacity) as ObjectResult;

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

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage>{GenerateRandomCage()});

        //act
        var result = _cageController.Add(cageName, maxCapacity) as ObjectResult;

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
        var result = _cageController.Add(cageName, maxCapacity: null) as ObjectResult;

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
        var result = _cageController.Add(cageName, maxCapacity: 0) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("maxCapacity is invalid.", result.Value);
    }
    #endregion

    #region Get
    [TestMethod]
    public void GetMustReturnCageById() {
        //arrange
        var cage = GenerateRandomCage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Get(cage.Id) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(cage.EqualsResource(result.Value as CageResource));
    }

    [TestMethod]
    public void GetMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();

        //act
        var result = _cageController.Get(unknownId) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }
    #endregion

    #region Update
    [TestMethod]
    public void UpdateMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();
        var cageName = GenerateRandom.String();
        var maxCapacity = GenerateRandom.Int(1, 10);

        //act
        var result = _cageController.Update(unknownId, cageName, maxCapacity) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameNotSupplied() {
        //arrange
        var cage = GenerateRandomCage();
        var maxCapacity = GenerateRandom.Int(1, 10);

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Update(cage.Id, cageName: null, maxCapacity) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("cageName must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var cage = GenerateRandomCage();
        var cageName = GenerateRandom.String();
        var maxCapacity = GenerateRandom.Int(1, 10);

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage> { cage, GenerateRandomCage() });

        //act
        var result = _cageController.Update(cage.Id, cageName, maxCapacity) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name already exists.", result.Value);
    }

    [TestMethod]
    public void UpdateMustNotReturnErrorIfExistingNameIsTheCageBeingUpdated() {
        //arrange
        var cage = GenerateRandomCage();
        var cageName = GenerateRandom.String();
        var maxCapacity = cage.DinosaurCount + GenerateRandom.Int(0, 5);

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage> { cage });

        //act
        var result = _cageController.Update(cage.Id, cageName, maxCapacity) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfMaxCapacityIsNotSupplied() {
        //arrange
        var cage = GenerateRandomCage();
        var cageName = GenerateRandom.String();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Update(cage.Id, cageName, maxCapacity: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("maxCapacity must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfMaxCapacityIsNotGreaterThanZero() {
        //arrange
        var cage = GenerateRandomCage();
        var cageName = GenerateRandom.String();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Update(cage.Id, cageName, maxCapacity: 0) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("maxCapacity is invalid.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfMaxCapacityIsSetBelowDinosaurCount() {
        var cage = GenerateRandomCage();
        cage.DinosaurCount = GenerateRandom.Int(2, 5);
        var cageName = GenerateRandom.String();
        var maxCapacity = cage.DinosaurCount - 1;

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Update(cage.Id, cageName, maxCapacity) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("maxCapacity must be higher than dinosaurCount.", result.Value);
    }

    [TestMethod]
    public void UpdateMustSaveNewValues() {
        var cage = GenerateRandomCage();
        var cageName = GenerateRandom.String();
        var maxCapacity = cage.DinosaurCount + GenerateRandom.Int(0, 5);

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Update(cage.Id, cageName, maxCapacity) as ObjectResult;

        //assert
        var expectedCage = new Cage {
            Id = cage.Id,
            Name = cageName,
            MaxCapacity = maxCapacity,
            PowerStatus = cage.PowerStatus
        };
        _mockCageStore.Verify(x => x.Update(It.Is<Cage>(y => expectedCage.Equals(y))), Times.Once());

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(cage.EqualsResource(result.Value as CageResource));
    }

    #endregion

    #region Search

    [TestMethod]
    public void SearchWithInvalidPowerStatusMustReturnError() {
        //arrange
        var powerStatus = GenerateRandom.String();

        //act
        var result = _cageController.Search(cageName: null, powerStatus: powerStatus) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("powerStatus must be 'active' or 'down'.", result.Value);
    }

    [TestMethod]
    public void SearchMustPassParametersToStore() {
        //arrange
        var cageName = GenerateRandom.String();
        var powerStatus = GenerateRandom.Int(0, 1) == 1 ? CagePowerStatus.Active : CagePowerStatus.Down ;
        _mockCageStore.Setup(x => x.Search(cageName, powerStatus)).Returns(new List<Cage>());

        //act
        _cageController.Search(cageName, powerStatus.ToString());

        //assert
        _mockCageStore.Verify(x => x.Search(cageName, powerStatus), Times.Once);
    }

    [TestMethod]
    public void SearchMustReturnAllCagesAsResources() {
        //arrange
        var mockCages = new List<Cage>();
        for (var x = 0; x < GenerateRandom.Int(2, 10); x++) {
            mockCages.Add(GenerateRandomCage());
        }

        _mockCageStore.Setup(x => x.Search(null, null)).Returns(mockCages);

        //act
        var result = _cageController.Search(cageName: null, powerStatus: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(mockCages.EqualsResourceList(result.Value));
    }

    #endregion

    private static Cage GenerateRandomCage() {
        var maxCapacity = GenerateRandom.Int(1, 10);
        var powerStatus = GenerateRandom.Int(0, 1) == 1 ? CagePowerStatus.Active : CagePowerStatus.Down;
        var dinosaurCount = powerStatus == CagePowerStatus.Active ? GenerateRandom.Int(0, maxCapacity) : 0;

        return new Cage {
            Id = GenerateRandom.Int(),
            MaxCapacity = maxCapacity,
            DinosaurCount = dinosaurCount,
            PowerStatus = powerStatus
        };
    }
}
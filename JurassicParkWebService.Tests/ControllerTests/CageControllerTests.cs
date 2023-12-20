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
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = maxCapacity,
        };
        var result = _cageController.Add(inboundResource) as ObjectResult;

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
        var resource = result.Value as OutboundCageResource;
        Assert.IsNotNull(resource);
        Assert.IsTrue(expectedCage.EqualsResource(resource));
    }

    [TestMethod]
    public void AddMustReturnErrorIfBodyIsNotSupplied() {
        //arrange
        //act
        var result = _cageController.Add(inboundCageResource: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Body must be supplied.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfNameNotSupplied() {
        //arrange
        var maxCapacity = GenerateRandom.Int(1, 10);

        //act
        var inboundResource = new InboundCageResource {
            Name = null,
            MaxCapacity = maxCapacity,
        };
        var result = _cageController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name must be supplied.", result.Value);
    }
    
    [TestMethod]
    public void AddMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var cageName = GenerateRandom.String();
        var maxCapacity = GenerateRandom.Int(1, 10);

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage>{GenerateRandomCage()});

        //act
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = maxCapacity,
        };
        var result = _cageController.Add(inboundResource) as ObjectResult;

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
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = null,
        };
        var result = _cageController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("MaxCapacity must be supplied.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfMaxCapacityIsNotGreaterThanZero() {
        //arrange
        var cageName = GenerateRandom.String();

        //act
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = 0,
        };
        var result = _cageController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("MaxCapacity is invalid.", result.Value);
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
        Assert.IsTrue(cage.EqualsResource(result.Value as OutboundCageResource));
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
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = maxCapacity,
        };
        var result = _cageController.Update(unknownId, inboundResource) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfBodyIsNotSupplied() {
        //arrange
        var cage = GenerateRandomCage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Update(cage.Id, inboundCageResource: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Body must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameNotSupplied() {
        //arrange
        var cage = GenerateRandomCage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = null,
            MaxCapacity = cage.MaxCapacity,
            PowerStatus = cage.PowerStatus.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var cage = GenerateRandomCage();
        var cageName = GenerateRandom.String();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage> { cage, GenerateRandomCage() });

        //act
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = cage.MaxCapacity,
            PowerStatus = cage.PowerStatus.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

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

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage> { cage });

        //act
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = cage.MaxCapacity,
            PowerStatus = cage.PowerStatus.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfMaxCapacityIsNotSupplied() {
        //arrange
        var cage = GenerateRandomCage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = cage.Name,
            MaxCapacity = null,
            PowerStatus = cage.PowerStatus.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("MaxCapacity must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfMaxCapacityIsNotGreaterThanZero() {
        //arrange
        var cage = GenerateRandomCage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = cage.Name,
            MaxCapacity = 0,
            PowerStatus = cage.PowerStatus.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("MaxCapacity is invalid.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfMaxCapacityIsSetBelowDinosaurCount() {
        var cage = GenerateRandomCage();
        cage.DinosaurCount = GenerateRandom.Int(2, 5);
        var maxCapacity = cage.DinosaurCount - 1;

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = cage.Name,
            MaxCapacity = maxCapacity,
            PowerStatus = cage.PowerStatus.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("MaxCapacity must be higher than DinosaurCount.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfPowerStatusNotSupplied() {
        //arrange
        var cage = GenerateRandomCage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = cage.Name,
            MaxCapacity = cage.MaxCapacity,
            PowerStatus = null
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("PowerStatus must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfPowerStatusInvalid() {
        //arrange
        var cage = GenerateRandomCage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = cage.Name,
            MaxCapacity = cage.MaxCapacity,
            PowerStatus = GenerateRandom.String()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("PowerStatus must be 'active' or 'down'.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfPowerStatusIsSetToDownWithDinosaursInCage() {
        //arrange
        var cage = GenerateRandomCage();
        cage.MaxCapacity = 2;
        cage.DinosaurCount = 1;

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = cage.Name,
            MaxCapacity = cage.MaxCapacity,
            PowerStatus = CagePowerStatus.Down.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("PowerStatus cannot be set to 'down' when DinosaurCount > 0.", result.Value);
    }

    [TestMethod]
    public void UpdateMustSaveNewValues() {
        var cage = GenerateRandomCage();
        cage.DinosaurCount = 0; //if DinosaurCount isn't zero, setting the PowerStatus to 'down' will return a validation error
        cage.PowerStatus = GenerateRandom.Int(0, 1) == 1 ? CagePowerStatus.Active : CagePowerStatus.Down;
        
        //the new values must be different from the existing values so we can detect changes
        var cageName = GenerateRandom.String();
        var maxCapacity = cage.MaxCapacity + 1;
        var powerStatus = cage.PowerStatus == CagePowerStatus.Active ? CagePowerStatus.Down : CagePowerStatus.Active;

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var inboundResource = new InboundCageResource {
            Name = cageName,
            MaxCapacity = maxCapacity,
            PowerStatus = powerStatus.ToString()
        };
        var result = _cageController.Update(cage.Id, inboundResource) as ObjectResult;

        //assert
        var expectedCage = new Cage {
            Id = cage.Id,
            Name = cageName,
            MaxCapacity = maxCapacity,
            PowerStatus = powerStatus
        };
        _mockCageStore.Verify(x => x.Update(It.Is<Cage>(y => expectedCage.Equals(y))), Times.Once());

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(cage.EqualsResource(result.Value as OutboundCageResource));
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
        Assert.AreEqual("PowerStatus must be 'active' or 'down'.", result.Value);
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
            Name = GenerateRandom.String(),
            MaxCapacity = maxCapacity,
            DinosaurCount = dinosaurCount,
            PowerStatus = powerStatus
        };
    }
}
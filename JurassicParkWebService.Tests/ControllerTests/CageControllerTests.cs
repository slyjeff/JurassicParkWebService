using System.Collections.Generic;
using System.Linq;
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
    private Mock<IDinosaurStore> _mockDinosaurStore = null!;
    private CageController _cageController = null!;
    private Mock<ISpeciesStore> _mockSpeciesStore = null!;

    [TestInitialize]
    public void Setup() {
        _mockCageStore = new Mock<ICageStore>();
        _mockCageStore.Setup(x => x.Search(It.IsAny<string?>(), It.IsAny<CagePowerStatus?>())).Returns(new List<Cage>( ));

        _mockDinosaurStore = new Mock<IDinosaurStore>();
        _mockDinosaurStore.Setup(x => x.Search(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>())).Returns(new List<Dinosaur>());

        _mockSpeciesStore = new Mock<ISpeciesStore>();

        _cageController = new CageController(_mockCageStore.Object, _mockDinosaurStore.Object, _mockSpeciesStore.Object);
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
        var result = _cageController.Add(inboundResource: null) as ObjectResult;

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

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage>{GenerateRandom.Cage()});

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
        var cage = GenerateRandom.Cage();

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

    #region Delete

    [TestMethod]
    public void DeleteMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();

        //act
        var result = _cageController.Delete(unknownId) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }

    [TestMethod]
    public void DeleteMustReturnErrorIfDinosaurCountIsGreaterThanZero() {
        //arrange
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var dinosaursInCage = new List<Dinosaur> { new() };
        _mockDinosaurStore.Setup(x => x.Search(It.IsAny<string?>(), It.IsAny<int?>(), cage.Id, It.IsAny<bool?>())).Returns(dinosaursInCage);

        //act
        var result = _cageController.Delete(cage.Id) as ObjectResult;

        //assert
        _mockCageStore.Verify(x => x.Delete(cage.Id), Times.Never);

        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Cannot delete cage if DinosaurCount > 0.", result.Value);
    }

    [TestMethod]
    public void DeleteMustCallStore() {
        //arrange
        var cage = GenerateRandom.Cage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Delete(cage.Id) as StatusCodeResult;

        //assert
        _mockCageStore.Verify(x => x.Delete(cage.Id), Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
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

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfBodyIsNotSupplied() {
        //arrange
        var cage = GenerateRandom.Cage();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.Update(cage.Id, inboundResource: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Body must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameNotSupplied() {
        //arrange
        var cage = GenerateRandom.Cage();

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
        var cage = GenerateRandom.Cage();
        var cageName = GenerateRandom.String();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        _mockCageStore.Setup(x => x.Search(cageName, null)).Returns(new List<Cage> { cage, GenerateRandom.Cage() });

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
        var cage = GenerateRandom.Cage();
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
        var cage = GenerateRandom.Cage();

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
        var cage = GenerateRandom.Cage();

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
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;
        cage.MaxCapacity = 10;

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        const int maxCapacity = 2;
        var dinosaursInCage = new List<Dinosaur> { new(), new(), new() };
        _mockDinosaurStore.Setup(x => x.Search(It.IsAny<string?>(), It.IsAny<int?>(), cage.Id, It.IsAny<bool?>())).Returns(dinosaursInCage);

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
        var cage = GenerateRandom.Cage();

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
        var cage = GenerateRandom.Cage();

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
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;
        cage.MaxCapacity = 2;

        var dinosaursInCage = new List<Dinosaur> { new() };
        _mockDinosaurStore.Setup(x => x.Search(It.IsAny<string?>(), It.IsAny<int?>(), cage.Id, It.IsAny<bool?>())).Returns(dinosaursInCage);

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
        var cage = GenerateRandom.Cage();
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
        var result = _cageController.Search(name: null, powerStatus: powerStatus) as ObjectResult;

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
    public void SearchMustReturnCagesAsResources() {
        //arrange
        var mockCages = new List<Cage>();
        for (var x = 0; x < GenerateRandom.Int(2, 10); x++) {
            mockCages.Add(GenerateRandom.Cage());
        }

        _mockCageStore.Setup(x => x.Search(null, null)).Returns(mockCages);

        //act
        var result = _cageController.Search(name: null, powerStatus: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(mockCages.EqualsResourceList(result.Value));
    }

    #endregion

    #region GetDinosaurs
    [TestMethod]
    public void GetDinosaursMustReturnErrorIfCageNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();

        //act
        var result = _cageController.GetDinosaurs(unknownId) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }

    [TestMethod]
    public void GetDinosaursMustReturnListAsResources() {
        //arrange
        var cageId = GenerateRandom.Int();
        _mockCageStore.Setup(x => x.Get(cageId)).Returns(new Cage());

        var species = GenerateRandom.Species();
        var mockSpecies = new List<Species> { species };

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        var mockDinosaurs = new List<Dinosaur>();
        for (var x = 0; x < GenerateRandom.Int(2, 10); x++) {
            var mockDinosaur = GenerateRandom.Dinosaur();
            mockDinosaur.SpeciesId = mockSpecies.First().Id;
            mockDinosaurs.Add(mockDinosaur);
        }

        _mockDinosaurStore.Setup(x => x.Search(null, null, cageId, null)).Returns(mockDinosaurs);

        //act
        var result = _cageController.GetDinosaurs(cageId) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(mockDinosaurs.EqualsResourceList(result.Value, mockSpecies));
    }
    #endregion

    #region AddDinosaur
    [TestMethod]
    public void AddDinosaursMustReturnErrorIfCageNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();
        var dinosaur = GenerateRandom.Dinosaur();

        //act
        var result = _cageController.AddDinosaur(unknownId, dinosaur.Id) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }

    [TestMethod]
    public void AddDinosaurMustReturnErrorIfDinosaurNotFound() {
        //arrange
        var cage = GenerateRandom.Cage();
        var unknownId = GenerateRandom.Int();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.AddDinosaur(cage.Id, unknownId) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Dinosaur not found.", result.Value);
    }

    [TestMethod]
    public void AddDinosaurMustReturnErrorIfPowerStatusIsDown() {
        //arrange
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Down;
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var species = GenerateRandom.Species();
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.SpeciesId = species.Id;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _cageController.AddDinosaur(cage.Id, dinosaur.Id) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Dinosaur cannot be added if PowerStatus is 'down'.", result.Value);
    }

    [TestMethod]
    public void AddDinosaurMustReturnSuccessIfAlreadyInCage() {
        //arrange
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var species = GenerateRandom.Species();
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.SpeciesId = species.Id;
        dinosaur.CageId = cage.Id;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _cageController.AddDinosaur(cage.Id, dinosaur.Id) as ObjectResult;

        //arrange
        _mockDinosaurStore.Verify(x => x.Update(It.IsAny<Dinosaur>()), Times.Never);

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(dinosaur.EqualsResource(result.Value as OutboundDinosaurResource, species));
    }

    [TestMethod]
    public void AddDinosaurMustUpdateIfNotAlreadyInCage() {
        //arrange
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var species = GenerateRandom.Species();
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.SpeciesId = species.Id;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _cageController.AddDinosaur(cage.Id, dinosaur.Id) as ObjectResult;

        //arrange
        _mockDinosaurStore.Verify(x => x.Update(It.Is<Dinosaur>(y => y == dinosaur && dinosaur.CageId == cage.Id)), Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(dinosaur.EqualsResource(result.Value as OutboundDinosaurResource, species));
    }

    [TestMethod]
    public void AddDinosaurMustReturnErrorIfAddingWouldExceedMaxCapacity() {
        //arrange
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;
        cage.MaxCapacity = 1;
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var species = GenerateRandom.Species();
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.SpeciesId = species.Id;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        _mockDinosaurStore.Setup(x => x.Search(null, null, cage.Id, null)).Returns(new List<Dinosaur> { new() });

        //act
        var result = _cageController.AddDinosaur(cage.Id, dinosaur.Id) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Dinosaur cannot cage is at MaxCapacity.", result.Value);
    }

    [TestMethod]
    public void AddCarnivoreMustReturnErrorIfHerbivoreAlreadyInCage() {
        //arrange
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var species = GenerateRandom.Species();
        species.SpeciesType = SpeciesType.Carnivore;
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.SpeciesId = species.Id;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        _mockDinosaurStore.Setup(x => x.Search(null, null, cage.Id, false)).Returns(new List<Dinosaur> { new() });

        //act
        var result = _cageController.AddDinosaur(cage.Id, dinosaur.Id) as ObjectResult;

        //arrange
        _mockDinosaurStore.Verify(x => x.Update(It.Is<Dinosaur>(y => y == dinosaur && dinosaur.CageId == cage.Id)), Times.Never);

        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Cannot put a carnivore and a herbivore in the same cage.", result.Value);
    }

    [TestMethod]
    public void AddHerbivoreMustReturnErrorIfCarnivoreAlreadyInCage() {
        //arrange
        var cage = GenerateRandom.Cage();
        cage.PowerStatus = CagePowerStatus.Active;
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var species = GenerateRandom.Species();
        species.SpeciesType = SpeciesType.Herbivore;
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.SpeciesId = species.Id;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        _mockDinosaurStore.Setup(x => x.Search(null, null, cage.Id, true)).Returns(new List<Dinosaur> { new() });

        //act
        var result = _cageController.AddDinosaur(cage.Id, dinosaur.Id) as ObjectResult;

        //arrange
        _mockDinosaurStore.Verify(x => x.Update(It.Is<Dinosaur>(y => y == dinosaur && dinosaur.CageId == cage.Id)), Times.Never);

        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Cannot put a carnivore and a herbivore in the same cage.", result.Value);
    }
    #endregion

    #region RemoveDinosaur
    [TestMethod]
    public void RemoveDinosaursMustReturnErrorIfCageNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();
        var dinosaur = GenerateRandom.Dinosaur();

        //act
        var result = _cageController.RemoveDinosaur(unknownId, dinosaur.Id) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Cage not found.", result.Value);
    }

    [TestMethod]
    public void RemoveDinosaurMustReturnErrorIfDinosaurNotFound() {
        //arrange
        var cage = GenerateRandom.Cage();
        var unknownId = GenerateRandom.Int();

        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        //act
        var result = _cageController.RemoveDinosaur(cage.Id, unknownId) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Dinosaur not found.", result.Value);
    }

    [TestMethod]
    public void RemoveDinosaursMustNotUpdateIfAlreadyRemoved() {
        //arrange
        var cage = GenerateRandom.Cage();
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.CageId = null;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _cageController.RemoveDinosaur(cage.Id, dinosaur.Id) as StatusCodeResult;

        //arrange
        _mockDinosaurStore.Verify(x => x.Update(It.IsAny<Dinosaur>()), Times.Never);

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public void RemoveDinosaursMustSetCageToNullIfInCage() {
        //arrange
        var cage = GenerateRandom.Cage();
        _mockCageStore.Setup(x => x.Get(cage.Id)).Returns(cage);

        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.CageId = cage.Id;
        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _cageController.RemoveDinosaur(cage.Id, dinosaur.Id) as StatusCodeResult;

        //arrange
        _mockDinosaurStore.Verify(x => x.Update(It.Is<Dinosaur>(y => y == dinosaur && dinosaur.CageId == null)), Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    #endregion
}
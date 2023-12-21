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
public sealed class DinosaurControllerTests {
    private DinosaurController _dinosaurController = null!;
    private Mock<ISpeciesStore> _mockSpeciesStore = null!;
    private Mock<IDinosaurStore> _mockDinosaurStore = null!;

    [TestInitialize]
    public void Setup() {
        _mockDinosaurStore = new Mock<IDinosaurStore>();
        _mockDinosaurStore.Setup(x => x.Search(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>())).Returns(new List<Dinosaur>());

        _mockSpeciesStore = new Mock<ISpeciesStore>();
        _mockSpeciesStore.Setup(x => x.Get(It.IsAny<int>())).Returns((int id) => new Species{Id = id});

        _dinosaurController = new DinosaurController(_mockDinosaurStore.Object, _mockSpeciesStore.Object);
    }

    #region Add
    [TestMethod]
    public void AddMustCreateDinosaurInTheDatabase() {
        //arrange
        var randomGeneratedId = GenerateRandom.Int();
        var dinosaurName = GenerateRandom.String();

        var species = GenerateRandom.Species();
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        _mockDinosaurStore.Setup(x => x.Add(It.IsAny<Dinosaur>())).Callback((Dinosaur c) => c.Id = randomGeneratedId);

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = species.Id,
        };
        var result = _dinosaurController.Add(inboundResource) as ObjectResult;

        //assert
        var expectedDinosaur = new Dinosaur {
            Id = randomGeneratedId,
            Name = dinosaurName,
            SpeciesId = species.Id
        };

        _mockDinosaurStore.Verify(x => x.Add(It.Is<Dinosaur>(y => y.Equals(expectedDinosaur))));

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        var resource = result.Value as OutboundDinosaurResource;
        Assert.IsNotNull(resource);
        Assert.IsTrue(expectedDinosaur.EqualsResource(resource, species));
    }

    [TestMethod]
    public void AddMustReturnErrorIfBodyIsNotSupplied() {
        //arrange
        //act
        var result = _dinosaurController.Add(inboundResource: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Body must be supplied.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfNameNotSupplied() {
        //arrange
        var speciesId = GenerateRandom.Int();

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = null,
            SpeciesId = speciesId,
        };
        var result = _dinosaurController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name must be supplied.", result.Value);
    }
    
    [TestMethod]
    public void AddMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var dinosaurName = GenerateRandom.String();
        var speciesId = GenerateRandom.Int();

        _mockDinosaurStore.Setup(x => x.Search(dinosaurName, null, null, null)).Returns(new List<Dinosaur> {GenerateRandom.Dinosaur()});

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = speciesId,
        };
        var result = _dinosaurController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name already exists.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfSpeciesIdNotSupplied() {
        //arrange
        var dinosaurName = GenerateRandom.String();

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = null,
        };
        var result = _dinosaurController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesId must be supplied.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfSpeciesIdNotValid() {
        //arrange
        var dinosaurName = GenerateRandom.String();
        var speciesId = GenerateRandom.Int();

        _mockSpeciesStore.Setup(x => x.Get(speciesId)).Returns((Species?)null);

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = speciesId,
        };
        var result = _dinosaurController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesId is invalid.", result.Value);
    }
    #endregion

    #region Get
    [TestMethod]
    public void GetMustReturnDinosaurById() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();

        var species = GenerateRandom.Species();
        dinosaur.SpeciesId = species.Id;
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _dinosaurController.Get(dinosaur.Id) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(dinosaur.EqualsResource(result.Value as OutboundDinosaurResource, species));
    }

    [TestMethod]
    public void GetMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();

        //act
        var result = _dinosaurController.Get(unknownId) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Dinosaur not found.", result.Value);
    }
    #endregion

    #region Delete

    [TestMethod]
    public void DeleteMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();

        //act
        var result = _dinosaurController.Delete(unknownId) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Dinosaur not found.", result.Value);
    }

    [TestMethod]
    public void DeleteMustCallStore() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _dinosaurController.Delete(dinosaur.Id) as StatusCodeResult;

        //assert
        _mockDinosaurStore.Verify(x => x.Delete(dinosaur.Id), Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    #endregion

    #region Update
    [TestMethod]
    public void UpdateMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();
        var dinosaurName = GenerateRandom.String();
        var speciesId = GenerateRandom.Int();

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = speciesId,
        };
        var result = _dinosaurController.Update(unknownId, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Dinosaur not found.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfBodyIsNotSupplied() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Body must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameNotSupplied() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = null,
            SpeciesId = dinosaur.SpeciesId
        };
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();
        var dinosaurName = GenerateRandom.String();

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);
        _mockDinosaurStore.Setup(x => x.Search(dinosaurName, null, null, null)).Returns(new List<Dinosaur> { dinosaur, GenerateRandom.Dinosaur() });

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = dinosaur.SpeciesId
        };
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name already exists.", result.Value);
    }

    [TestMethod]
    public void UpdateMustNotReturnErrorIfExistingNameIsTheDinosaurBeingUpdated() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();
        var dinosaurName = GenerateRandom.String();

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);
        _mockDinosaurStore.Setup(x => x.Search(dinosaurName, null, null, null)).Returns(new List<Dinosaur> { dinosaur });

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = dinosaur.SpeciesId
        };
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfSpeciesIdIsNotSupplied() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaur.Name,
            SpeciesId = null
        };
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesId must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfSpeciesIdDoesNotMatchExistingValueAndDinosaurIsInCage() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.CageId = GenerateRandom.Int();

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaur.Name,
            SpeciesId = GenerateRandom.Int()
        };
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Species cannot be changed for a dinosaur in a cage.", result.Value);
    }

    [TestMethod]
    public void UpdateMustNotReturnErrorIfSpeciesIdDoesNotMatchExistingValueAndDinosaurIsNotInCage() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.CageId = null;

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaur.Name,
            SpeciesId = GenerateRandom.Int()
        };
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public void UpdateMustSaveNewValues() {
        //arrange
        var dinosaur = GenerateRandom.Dinosaur();
        dinosaur.CageId = null;
        
        var dinosaurName = GenerateRandom.String();
        var species = GenerateRandom.Species();
        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        _mockDinosaurStore.Setup(x => x.Get(dinosaur.Id)).Returns(dinosaur);

        //act
        var inboundResource = new InboundDinosaurResource {
            Name = dinosaurName,
            SpeciesId = species.Id
        };
        var result = _dinosaurController.Update(dinosaur.Id, inboundResource) as ObjectResult;

        //assert
        var expectedDinosaur = new Dinosaur {
            Id = dinosaur.Id,
            Name = dinosaurName,
            SpeciesId = species.Id
        };
        _mockDinosaurStore.Verify(x => x.Update(It.Is<Dinosaur>(y => expectedDinosaur.Equals(y))), Times.Once());

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(dinosaur.EqualsResource(result.Value as OutboundDinosaurResource, species));
    }

    #endregion

    #region Search

    [TestMethod]
    public void SearchWithInvalidSpeciesIdMustReturnError() {
        //arrange
        var speciesId = GenerateRandom.Int();

        _mockSpeciesStore.Setup(x => x.Get(speciesId)).Returns((Species?)null);

        //act
        var result = _dinosaurController.Search(name: null, speciesId: speciesId) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesId is invalid.", result.Value);
    }

    [TestMethod]
    public void SearchMustPassParametersToStore() {
        //arrange
        var dinosaurName = GenerateRandom.String();
        var speciesId = GenerateRandom.Int();

        _mockDinosaurStore.Setup(x => x.Search(dinosaurName, speciesId, null, null)).Returns(new List<Dinosaur>());

        //act
        _dinosaurController.Search(dinosaurName, speciesId);

        //assert
        _mockDinosaurStore.Verify(x => x.Search(dinosaurName, speciesId, null, null), Times.Once);
    }

    [TestMethod]
    public void SearchMustReturnDinosaursAsResources() {
        //arrange
        var species = GenerateRandom.Species();
        var mockSpecies = new List<Species> { species };

        var mockDinosaurs = new List<Dinosaur>();
        for (var x = 0; x < GenerateRandom.Int(2, 10); x++) {
            var mockDinosaur = GenerateRandom.Dinosaur();
            mockDinosaur.SpeciesId = mockSpecies.First().Id;
            mockDinosaurs.Add(mockDinosaur);
        }
        _mockDinosaurStore.Setup(x => x.Search(null, null, null, null)).Returns(mockDinosaurs);

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        //act
        var result = _dinosaurController.Search(name: null, speciesId: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(mockDinosaurs.EqualsResourceList(result.Value, mockSpecies));
    }

    #endregion
}
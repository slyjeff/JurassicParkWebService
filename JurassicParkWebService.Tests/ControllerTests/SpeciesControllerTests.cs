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
public sealed class SpeciesControllerTests {
    private Mock<ISpeciesStore> _mockSpeciesStore = null!;
    private SpeciesController _speciesController = null!;
    private Mock<IDinosaurStore> _mockDinosaurStore = null!;

    [TestInitialize]
    public void Setup() {
        _mockSpeciesStore = new Mock<ISpeciesStore>();
        _mockDinosaurStore = new Mock<IDinosaurStore>();

        _mockSpeciesStore.Setup(x => x.Search(It.IsAny<string?>())).Returns(new List<Species>());

        _speciesController = new SpeciesController(_mockSpeciesStore.Object, _mockDinosaurStore.Object);
    }

    #region Add
    [TestMethod]
    public void AddMustCreateSpeciesInTheDatabase() {
        //arrange
        var randomGeneratedId = GenerateRandom.Int();

        var speciesName = GenerateRandom.String();
        var speciesType = GenerateRandomSpeciesType();

        _mockSpeciesStore.Setup(x => x.Add(It.IsAny<Species>())).Callback((Species c) => c.Id = randomGeneratedId);

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = speciesType.ToString(),
        };
        var result = _speciesController.Add(inboundResource) as ObjectResult;

        //assert
        var expectedSpecies = new Species {
            Id = randomGeneratedId,
            Name = speciesName,
            SpeciesType = speciesType
        };

        _mockSpeciesStore.Verify(x => x.Add(It.Is<Species>(y => y.Equals(expectedSpecies))));

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        var resource = result.Value as OutboundSpeciesResource;
        Assert.IsNotNull(resource);
        Assert.IsTrue(expectedSpecies.EqualsResource(resource));
    }

    [TestMethod]
    public void AddMustReturnErrorIfBodyIsNotSupplied() {
        //arrange
        //act
        var result = _speciesController.Add(inboundSpeciesResource: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Body must be supplied.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfNameNotSupplied() {
        //arrange
        var speciesType = GenerateRandomSpecies();

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = null,
            SpeciesType = speciesType.ToString(),
        };
        var result = _speciesController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name must be supplied.", result.Value);
    }
    
    [TestMethod]
    public void AddMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var speciesName = GenerateRandom.String();
        var speciesType = GenerateRandomSpecies();

        _mockSpeciesStore.Setup(x => x.Search(speciesName)).Returns(new List<Species>{GenerateRandomSpecies()});

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = speciesType.ToString(),
        };
        var result = _speciesController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name already exists.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfSpeciesNotSupplied() {
        //arrange
        var speciesName = GenerateRandom.String();

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = null,
        };
        var result = _speciesController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesType must be supplied.", result.Value);
    }

    [TestMethod]
    public void AddMustReturnErrorIfSpeciesNotValid() {
        //arrange
        var speciesName = GenerateRandom.String();

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = GenerateRandom.String(),
        };
        var result = _speciesController.Add(inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesType must be 'carnivore' or 'herbivore'.", result.Value);
    }
    #endregion

    #region Get
    [TestMethod]
    public void GetMustReturnSpeciesById() {
        //arrange
        var species = GenerateRandomSpecies();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        //act
        var result = _speciesController.Get(species.Id) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(species.EqualsResource(result.Value as OutboundSpeciesResource));
    }

    [TestMethod]
    public void GetMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();

        //act
        var result = _speciesController.Get(unknownId) as ObjectResult;

        //arrange
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Species not found.", result.Value);
    }
    #endregion

    #region Delete

    [TestMethod]
    public void DeleteMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();

        //act
        var result = _speciesController.Delete(unknownId) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Species not found.", result.Value);
    }

    [TestMethod]
    public void DeleteMustReturnErrorIfDinosaursOfSpeciesExist() {
        //arrange
        var species = GenerateRandomSpecies();
        var dinosaursWithSpecies = new List<Dinosaur> { new(), new() };

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);
        _mockDinosaurStore.Setup(x => x.Search(null, species.Id)).Returns(dinosaursWithSpecies);

        //act
        var result = _speciesController.Delete(species.Id) as ObjectResult;

        //assert
        _mockSpeciesStore.Verify(x => x.Delete(species.Id), Times.Never);

        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Cannot delete while Dinosaurs of this species exist.", result.Value);
    }

    [TestMethod]
    public void GetMustCallStore() {
        //arrange
        var species = GenerateRandomSpecies();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);
        _mockDinosaurStore.Setup(x => x.Search(null, species.Id)).Returns(new List<Dinosaur>());

        //act
        var result = _speciesController.Delete(species.Id) as StatusCodeResult;

        //assert
        _mockSpeciesStore.Verify(x => x.Delete(species.Id), Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    #endregion

    #region Update
    [TestMethod]
    public void UpdateMustReturnErrorIfNotFound() {
        //arrange
        var unknownId = GenerateRandom.Int();
        var speciesName = GenerateRandom.String();
        var speciesType = GenerateRandomSpeciesType();

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = speciesType.ToString(),
        };
        var result = _speciesController.Update(unknownId, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
        Assert.AreEqual("Species not found.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfBodyIsNotSupplied() {
        //arrange
        var species = GenerateRandomSpecies();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        //act
        var result = _speciesController.Update(species.Id, inboundSpeciesResource: null) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Body must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameNotSupplied() {
        //arrange
        var species = GenerateRandomSpecies();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = null,
            SpeciesType = species.SpeciesType.ToString()
        };
        var result = _speciesController.Update(species.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfNameAlreadyExists() {
        //arrange
        var species = GenerateRandomSpecies();
        var speciesName = GenerateRandom.String();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        _mockSpeciesStore.Setup(x => x.Search(speciesName)).Returns(new List<Species> { species, GenerateRandomSpecies() });

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = species.SpeciesType.ToString()
        };
        var result = _speciesController.Update(species.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("Name already exists.", result.Value);
    }

    [TestMethod]
    public void UpdateMustNotReturnErrorIfExistingNameIsTheSpeciesBeingUpdated() {
        //arrange
        var species = GenerateRandomSpecies();
        var speciesName = GenerateRandom.String();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        _mockSpeciesStore.Setup(x => x.Search(speciesName)).Returns(new List<Species> { species });

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = species.SpeciesType.ToString()
        };
        var result = _speciesController.Update(species.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfSpeciesTypeIsNotSupplied() {
        //arrange
        var species = GenerateRandomSpecies();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = species.Name,
            SpeciesType = null
        };
        var result = _speciesController.Update(species.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesType must be supplied.", result.Value);
    }

    [TestMethod]
    public void UpdateMustReturnErrorIfSpeciesTypeDoesNotMatchExistingValue() {
        //arrange
        var species = GenerateRandomSpecies();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = species.Name,
            SpeciesType = GenerateRandom.String()
        };
        var result = _speciesController.Update(species.Id, inboundResource) as ObjectResult;

        //assert
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
        Assert.AreEqual("SpeciesType cannot be changed.", result.Value);
    }

    [TestMethod]
    public void UpdateMustSaveNewValues() {
        var species = GenerateRandomSpecies();
        
        //the new values must be different from the existing values so we can detect changes
        var speciesName = GenerateRandom.String();

        _mockSpeciesStore.Setup(x => x.Get(species.Id)).Returns(species);

        //act
        var inboundResource = new InboundSpeciesResource {
            Name = speciesName,
            SpeciesType = species.SpeciesType.ToString()
        };
        var result = _speciesController.Update(species.Id, inboundResource) as ObjectResult;

        //assert
        var expectedSpecies = new Species {
            Id = species.Id,
            Name = speciesName,
            SpeciesType = species.SpeciesType
        };
        _mockSpeciesStore.Verify(x => x.Update(It.Is<Species>(y => expectedSpecies.Equals(y))), Times.Once());

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.IsTrue(species.EqualsResource(result.Value as OutboundSpeciesResource));
    }

    #endregion

    private static Species GenerateRandomSpecies() {
        return new Species {
            Id = GenerateRandom.Int(),
            Name = GenerateRandom.String(),
            SpeciesType = GenerateRandomSpeciesType()
        };
    }

    private static SpeciesType GenerateRandomSpeciesType() {
        return GenerateRandom.Int(0, 1) == 1 ? SpeciesType.Carnivore : SpeciesType.Herbivore;
    }
}
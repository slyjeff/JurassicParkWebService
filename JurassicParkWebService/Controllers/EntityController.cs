using JurassicParkWebService.Entities;
using Microsoft.AspNetCore.Mvc;
using JurassicParkWebService.Stores;

namespace JurassicParkWebService.Controllers; 

public abstract class EntityController<TEntity, TInboundResource, TOutboundResource> : ControllerBase where TEntity : IdentifiableEntity {
    private readonly IStore<TEntity> _store;
    private readonly string _entityName;

    protected EntityController(IStore<TEntity> store) {
        _store = store;
        _entityName = typeof(TEntity).Name;
    }

    [HttpPost]
    public IActionResult Add([FromBody] TInboundResource? inboundResource) {
        if (inboundResource == null) {
            return StatusCode(400, "Body must be supplied.");
        }

        var validationError = ValidateInboundEntity(inboundResource);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        var newEntity = CreateFromInboundResource(inboundResource);

        _store.Add(newEntity);

        return StatusCode(200, CreateOutboundResource(newEntity));
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id) {
        var entity = _store.Get(id);
        return entity == null 
             ? StatusCode(404, $"{_entityName} not found.") 
             : StatusCode(200, CreateOutboundResource(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] TInboundResource? inboundResource) {
        var entity = _store.Get(id);
        if (entity == null) {
            return StatusCode(404, $"{_entityName} not found.");
        }

        if (inboundResource == null) {
            return StatusCode(400, "Body must be supplied.");
        }

        var validationError = ValidateInboundEntity(inboundResource, entity);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        UpdateFromInboundResource(entity, inboundResource);

        _store.Update(entity);

        return StatusCode(200, CreateOutboundResource(entity));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) {
        var entity = _store.Get(id);
        if (entity == null) {
            return StatusCode(404, $"{_entityName} not found.");
        }

        var validationError = ValidateDeleteEntity(entity);
        if (!string.IsNullOrEmpty(validationError)) {
            return StatusCode(400, validationError);
        }

        _store.Delete(id);

        return StatusCode(200);
    }

    protected abstract TEntity CreateFromInboundResource(TInboundResource inboundResource);
    protected abstract void UpdateFromInboundResource(TEntity entityToUpdate, TInboundResource inboundResource);
    protected abstract TOutboundResource CreateOutboundResource(TEntity entity);
    protected abstract string? ValidateInboundEntity(TInboundResource inboundResource, TEntity? entityToUpdate = null);
    protected abstract string? ValidateDeleteEntity(TEntity entityToDelete);
}
namespace JurassicParkWebService.Stores;

public interface IStore<T> {
    void Add(T entity);
    void Update(T entity);
    T? Get(int id);
    void Delete(int id);
}

internal abstract class Store<T> : IStore<T> where T : class {
    public virtual void Add(T entity) {
    }

    public virtual void Update(T entity) {
    }
    
    public T? Get(int id) {
        return null;
    }

    public void Delete(int id) {
    }
}
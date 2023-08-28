using ORM.Querying.Abstract;

namespace ORM.Querying;

public interface IRepository<T> where T : new()
{
    void Create(T model);
    IEnumerable<T> Find();
    IEnumerable<T> Find(Where? where);
    IEnumerable<T> Find(Order? order);
    IEnumerable<T> Find(Where? where, Order? order);
    T? FindOne(Where where);
    void Update(Where where, T model);
    void Delete(Where where);
}


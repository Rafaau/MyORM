using System.Linq.Expressions;
using MyORM.Querying.Enums;

namespace MyORM.Querying.Repository;

public interface IRepository<T> where T : class, new()
{
	int Create(T model);
	void Save(T model);
	void Delete(T model);
	void Delete();
	IEnumerable<T> Find();
	T? FindOne();
	Repository<T> OrderBy<TResult>(Expression<Func<T, TResult>> selector, OrderBy order = Enums.OrderBy.ASC);
	Repository<T> Where(Expression<Func<T, bool>> predicate);
	Repository<T> Select<TResult>(Expression<Func<T, TResult>> selector);
}


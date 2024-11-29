using System.Linq.Expressions;
using MyORM.Querying.Enums;

namespace MyORM.Querying.Repository;

/// <summary>
/// Interface for repository.
/// </summary>
/// <typeparam name="T">Type of model.</typeparam>
public interface IRepository<T> where T : class, new()
{
    /// <summary>
    /// Creates a model.
    /// </summary>
    /// <param name="model">Model to create.</param>
    /// <returns>Returns the id of the created model.</returns>
    int Create(T model);

    /// <summary>
    /// Saves a model.
    /// </summary>
    /// <param name="model">Model to save.</param>
    void Save(T model);

    /// <summary>
    /// Deletes a model.
    /// </summary>
    /// <param name="model">Model to delete.</param>
	void Delete(T model);

    /// <summary>
    /// Deletes predicated models.
    /// </summary>
	void Delete();

    /// <summary>
    /// Finds models.
    /// </summary>
    /// <returns></returns>
	IEnumerable<T> Find();

    /// <summary>
    /// Finds one model.
    /// </summary>
    /// <returns>Returns the model.</returns>
	T? FindOne();

    /// <summary>
    /// Order by predicate.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="selector">Selector to order by.</param>
    /// <param name="order">Order by.</param>
    /// <returns>Returns the repository.</returns>
	Repository<T> OrderBy<TResult>(Expression<Func<T, TResult>> selector, OrderBy order = Enums.OrderBy.ASC);

    /// <summary>
    /// Where predicate.
    /// </summary>
    /// <param name="predicate">Predicate to where.</param>
    /// <returns>Returns the repository.</returns>
	Repository<T> Where(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Select predicate.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="selector">Selector to select.</param>
    /// <returns>Returns the repository.</returns>
	Repository<T> Select<TResult>(Expression<Func<T, TResult>> selector);
}


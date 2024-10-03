﻿using MyORM.Querying.Abstract;
using System.Linq.Expressions;

namespace MyORM.Querying.Repository;

public interface IRepository<T> where T : class, new()
{
	int Create(T model);
	void Update(T model);
	void UpdateMany(T model);
	void Delete(T model);
	void Delete();
	IEnumerable<T> Find();
	Repository<T> OrderBy(string columnName, string order = "ASC");
	Repository<T> Where(Expression<Func<T, bool>> predicate);
	Repository<T> Select<TResult>(Expression<Func<T, TResult>> selector);
}


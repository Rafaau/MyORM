using MyORM.Models;

namespace MyORM.Abstract;

public abstract class AbstractSnapshot
{
	public abstract string GetMetadata();
	public abstract void CreateDBFromSnapshot(DbHandler dbHandler);
	public abstract List<ModelStatement> GetModelsStatements();
}

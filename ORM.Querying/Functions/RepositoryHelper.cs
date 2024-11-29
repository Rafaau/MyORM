using MyORM.DBMS;
using MyORM.Methods;
using MyORM.Models;
using MyORM.Querying.Models;
using System.Collections;
using System.Data;
using System.Reflection;

namespace MyORM.Querying.Functions;

/// <summary>
/// Helper class for the repository.
/// </summary>
internal class RepositoryHelper
{
    /// <summary>
    /// List of all columns.
    /// </summary>
    public List<string> AllColumnsList { get; set; } = new();

    /// <summary>
    /// List of model statements.
    /// </summary>
    public List<ModelStatement> StatementList { get; set; }

    /// <summary>
    /// String of all columns.
    /// </summary>
    public string AllColumnsString
    {
        get
        {
            return string.Join(", ", AllColumnsList);
        }
    }

    /// <summary>
    /// Database handler instance.
    /// </summary>
    private DbHandler dbHandler;

    /// <summary>
    /// Data converter instance.
    /// </summary>
    private DataConverter dataConverter;

    /// <summary>
    /// Model type.
    /// </summary>
    private Type model;

    /// <summary>
    /// Model statement.
    /// </summary>
    private ModelStatement statement
    {
        get
        {
            return StatementList.GetModelStatement(model.Name);
        }
    }

    /// <summary>
    /// Model properties.
    /// </summary>
    private AttributeHelpers.ClassProps ModelProps
    {
        get
        {
            return AttributeHelpers.GetPropsByModel(model);
        }
    }

    /// <summary>
    /// Select columns.
    /// </summary>
    private string _selectColumns;

    /// <summary>
    /// Constructor for the <see cref="RepositoryHelper"/> class.
    /// </summary>
    /// <param name="dbHandler">Database handler instance</param>
    /// <param name="model">Model type</param>
    /// <param name="selectColumns">Select columns</param>
    public RepositoryHelper(DbHandler dbHandler, Type model, string selectColumns)
    {
        model = model;
        dbHandler = dbHandler;
        _selectColumns = selectColumns;

        Initialize();
        dataConverter = new DataConverter(StatementList);
    }

    /// <summary>
    /// Initializes the repository helper.
    /// </summary>
    /// <exception cref="Exception">Error getting model statements from Access Layer</exception>
    public void Initialize()
    {
        try
        {
            var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(Snapshot), dbHandler.AccessLayer.Options.GetMigrationsAssembly()).Last();
            var method = snapshotProps.Methods.Find(x => x.Name == "GetModelsStatements");
            StatementList = (List<ModelStatement>)method!.Invoke(snapshotProps.Instance, null)!;
        }
        catch (Exception e)
        {
            throw new Exception("Error getting model statements from Access Layer", e);
        }

        bool tableExists = dbHandler.CheckIfTableExists(ModelProps.TableName);

        if (!tableExists)
        {
            throw new Exception($"Table {ModelProps.TableName} does not exist. Please migrate data structure to database.");
        }
    }

    /// <summary>
    /// Adds the property to the selected columns.
    /// </summary>
    /// <param name="property">Property to add</param>
    /// <param name="modelName">Model name</param>
    public void AddToSelectedColumns(AttributeHelpers.Property property, string modelName)
    {
        var statement = StatementList.Find(x => x.Name == modelName);
        string tableName = statement.TableName;
        string? columnName = statement.Columns.Find(x => x.ColumnName == property.ColumnName)?.ColumnName;
        if (columnName is not null)
        {
            AllColumnsList.Add(ScriptBuilder.BuildSelect(tableName, columnName));
        }
    }

    /// <summary>
    /// Finds all relations of the model.
    /// </summary>
    /// <param name="modelProps">Properties of the model</param>
    /// <param name="parentType">Type of the parent</param>
    /// <returns>Returns the join string</returns>
    public string FindAllRelations(AttributeHelpers.ClassProps modelProps, Type parentType)
    {
        string joinString = string.Empty;
        Dictionary<AttributeHelpers.ClassProps, (string columnName, string fieldName, bool isList)> relatedModels = new();

        foreach (var property in modelProps.Properties)
        {
            AddToSelectedColumns(property, modelProps.ClassName);

            if (property.Type.Name == parentType?.Name) continue;

            if (property.HasAttribute("OneToOne"))
            {
                var relatedModel = AttributeHelpers.GetPropsByModel(property.Type);
                string columnName = relatedModel.Properties.WithNameAndAttribute(modelProps.ClassName, "OneToOne").ColumnName;

                joinString += $"LEFT JOIN {relatedModel.TableName} ON {relatedModel.TableName}.{columnName} = {modelProps.TableName}.{property.ParentClass.PrimaryKeyColumnName} ";
                relatedModels.Add(relatedModel, (columnName, property.ColumnName, false));
            }
            else if (property.HasAttribute("OneToMany"))
            {
                var relatedModel = AttributeHelpers.GetPropsByModel(property.Type.GetGenericArguments()[0]);
                string columnName = relatedModel.Properties.WithNameAndAttribute(modelProps.ClassName, "ManyToOne").ColumnName;

                joinString += $"LEFT JOIN {relatedModel.TableName} ON {relatedModel.TableName}.{columnName} = {modelProps.TableName}.{property.ParentClass.PrimaryKeyColumnName} ";
                relatedModels.Add(relatedModel, (columnName, property.ColumnName, true));
            }
        }

        foreach (var relatedModel in relatedModels)
        {
            joinString += FindAllRelations(relatedModel.Key, modelProps.Instance.GetType());
        }

        return joinString;
    }

    /// <summary>
    /// Inserts the model into the database.
    /// </summary>
    /// <param name="model">Model to insert</param>
    /// <param name="id">Id of the inserted record</param>
    /// <returns>Returns the inserted id</returns>
    public int InsertInto(object model, int id = 0)
    {
        AttributeHelpers.ClassProps modelProps = AttributeHelpers.GetPropsByModel(model.GetType());
        List<string> columns = new();
        List<string> values = new();
        List<(object Value, string PropertyName)> modelQueue = new();

        foreach (var property in AttributeHelpers.GetPropsByModel(model.GetType()).Properties)
        {
            bool isRelational = property.HasAttribute("OneToOne");
            var columnName = property.ColumnName;
            var columnValue = property.GetValue(model);

            if ((columnValue is null && !isRelational)
                || property.HasAttribute("PrimaryGeneratedColumn")
                || property.HasAttribute("ManyToMany")
                || property.HasAttribute("OneToMany"))
            {
                continue;
            }

            if (columnValue?.GetType() == typeof(string) && !isRelational)
            {
                columnValue = $"'{columnValue}'";
            }

            if (isRelational)
            {
                Relationship relationship = modelProps.Properties.GetRelationship(property.Name);

                if (relationship == Relationship.Optional)
                {
                    modelQueue.Add((columnValue, property.Name));
                    continue;
                }
                else
                {
                    columnName = $"{property.Name}Id";
                    columnValue = id;
                }
            }

            columns.Add(columnName);
            values.Add(columnValue.ToString());
        }

        var sql =
            $"INSERT INTO {modelProps.TableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)}); " +
            $"{ScriptBuilder.BuildIdentity(modelProps.TableName, "Id")}";

        var result = dbHandler.Query(sql);
        int insertedId = Convert.ToInt32(result.Rows[0][0]);

        foreach (var modelValue in modelQueue)
        {
            string fkName = statement.GetColumnName(modelValue.PropertyName);
            int relInsertedId = InsertInto(modelValue.Value, insertedId);
            sql =
                $"UPDATE {modelProps.TableName} SET {fkName} = {relInsertedId} " +
                $"WHERE {statement.GetPrimaryKeyColumnName()} = {insertedId}";
            dbHandler.Execute(sql);
        }

        return insertedId;
    }

    /// <summary>
    /// Updates the model in the database.
    /// </summary>
    /// <param name="updateData">List of models to update</param>
    public void Update(List<UpdateData> updateData)
    {
        foreach (var data in updateData)
        {
            int affected = 0;
            string sql;

            if (data.ColumnValues.Count > 0)
            {
                sql = $"UPDATE {data.TableName} SET {string.Join(", ", data.ColumnValues)} WHERE {data.WhereClause}";
                affected = dbHandler.Execute(sql);
            }

            if (affected == 0 && data.ColumnValues.Count > 0)
            {
                sql = $"INSERT INTO {data.TableName} ({string.Join(", ", data.Columns)}) VALUES ({string.Join(", ", data.Values)})";
                dbHandler.Execute(sql);

                if (data.RelationUpdate is not null
                    && data.ForeignKeyColumnName is not null)
                {
                    sql = $" {ScriptBuilder.BuildIdentity(data.TableName, data.PrimaryKeyColumnName)}";
                    var result = dbHandler.Query(sql);
                    int insertedId = Convert.ToInt32(result.Rows[0][0]);

                    sql =
                        $"UPDATE {data.RelationUpdate.TableName} " +
                        $"SET {data.ForeignKeyColumnName} = {insertedId} " +
                        $"WHERE {data.RelationUpdate.WhereClause}";

                    dbHandler.Execute(sql);
                }

                if (data.ManyToManyData is not null)
                {
                    sql = $" {ScriptBuilder.BuildIdentity(data.TableName, data.PrimaryKeyColumnName)}";
                    var result = dbHandler.Query(sql);
                    int insertedId = Convert.ToInt32(result.Rows[0][0]);

                    sql = $"INSERT INTO {data.ManyToManyData.TableName} " +
                        $"({data.ManyToManyData.ColumnName2}, {data.ManyToManyData.ColumnName}) " +
                        $"VALUES ({insertedId}, {data.ManyToManyData.ColumnValue})";

                    dbHandler.Execute(sql);
                }
            }
            else if (data.ManyToManyData is not null)
            {
                sql = $"SELECT * FROM {data.ManyToManyData.TableName} WHERE " +
                    $"{data.ManyToManyData.ColumnName} = {data.ManyToManyData.ColumnValue} " +
                    $"AND {data.ManyToManyData.ColumnName2} = {data.ManyToManyData.ColumnValue2}";

                var result = dbHandler.Query(sql);
                bool exists = result.Rows.Count > 0;

                if (!exists)
                {
                    sql = $"INSERT INTO {data.ManyToManyData.TableName} " +
                        $"({data.ManyToManyData.ColumnName2}, {data.ManyToManyData.ColumnName}) " +
                        $"VALUES ({data.ManyToManyData.ColumnValue2}, {data.ManyToManyData.ColumnValue})";
                    dbHandler.Execute(sql);
                }

                sql = $"SELECT * FROM {data.ManyToManyData.TableName} WHERE " +
                    $"{data.ManyToManyData.ColumnName} = {data.ManyToManyData.ColumnValue2} " +
                    $"AND {data.ManyToManyData.ColumnName2} = {data.ManyToManyData.ColumnValue}";

                dbHandler.Execute(sql);
                exists = result.Rows.Count > 0;

                if (!exists
                    && (data.ManyToManyData.ColumnName2.Contains("1") || data.ManyToManyData.ColumnName.Contains("1")))
                {
                    sql = $"INSERT INTO {data.ManyToManyData.TableName} " +
                        $"({data.ManyToManyData.ColumnName2}, {data.ManyToManyData.ColumnName}) " +
                        $"VALUES ({data.ManyToManyData.ColumnValue}, {data.ManyToManyData.ColumnValue2})";
                    dbHandler.Execute(sql);
                }
            }
        }
    }

    /// <summary>
    /// Gets the update string for the model.
    /// </summary>
    /// <typeparam name="T">Type of the model</typeparam>
    /// <param name="model">Model to update</param>
    /// <param name="updateData">List of update data</param>
    /// <param name="parentType">Type of the parent</param>
    /// <param name="pkValue">Primary key value</param>
    /// <param name="relationUpdate">Relation update data</param>
    /// <param name="foreignKeyColumnName">Foreign key column name</param>
    public void GetUpdateString<T>(
        T model,
        List<UpdateData> updateData,
        Type parentType = null,
        int? pkValue = null,
        UpdateData relationUpdate = null,
        string foreignKeyColumnName = null) where T : class, new()
    {
        List<string> columns = new();
        List<string> values = new();
        List<string> whereString = new();

        UpdateData data = new UpdateData();

        ModelStatement statement = StatementList.GetModelStatement(model.GetType().Name);

        if (parentType is not null)
        {
            ModelStatement parentStatement = StatementList.GetModelStatement(parentType.Name);

            if (parentType.HasToManyAttribute("ManyToMany", model.GetType().Name))
            {
                string relationName = model
                    .GetType()
                    .GetProperties()
                    .First(x => x.HasAttribute("ManyToMany")
                        && x.PropertyType.GetGenericArguments()[0].Name == parentType.Name)
                    .Name;

                RelationStatement relationStatement = statement.GetRelationStatement(relationName);
                string parentPkName = parentStatement.GetPrimaryKeyColumnName();

                data.ManyToManyData = new ManyToManyData
                {
                    TableName = relationStatement.TableName,
                    ColumnName = $"{relationStatement.ColumnName_1}",
                    ColumnValue = pkValue.ToString(),
                    ColumnName2 = $"{relationStatement.ColumnName}",
                    ColumnValue2 = model.GetPropertyValue(statement.GetPrimaryKeyPropertyName()).ToString()
                };
            }
            else if (parentType.HasToManyAttribute("OneToMany", model.GetType().Name))
            {
                string fkName = statement.GetColumn(parentType.Name).ColumnName;

                if (!columns.Contains($"{statement.TableName}.{fkName}"))
                {
                    columns.Add($"{statement.TableName}.{fkName}");
                    values.Add(pkValue.ToString());
                }
            }
        }

        foreach (var property in model.GetType().GetProperties())
        {
            if (parentType?.Name == property.PropertyType.Name)
            {
                if (!columns.Contains($"{statement.TableName}.{statement.GetColumn(property.Name).ColumnName}"))
                {
                    columns.Add($"{statement.TableName}.{statement.GetColumn(property.Name).ColumnName}");
                    values.Add(pkValue.ToString());
                }

                continue;
            }

            if (property.PropertyType.GetGenericArguments().Count() > 0
                    && parentType?.Name == property.PropertyType.GetGenericArguments()[0].Name)
            {
                continue;
            }

            if (property.HasAttribute("OneToOne"))
            {
                ModelStatement relStatement = StatementList.GetModelStatement(property.PropertyType.Name);

                if (property.GetValue(model) != null)
                {
                    var idObj = model?
                        .GetPropertyValue(property.Name)?
                        .GetPropertyValue(relStatement.GetPrimaryKeyPropertyName());

                    if (idObj != null)
                    {
                        columns.Add($"{statement.TableName}.{statement.GetColumn(property.Name).ColumnName}");
                        values.Add(idObj.ToString());
                    }

                    GetUpdateString(
                        property.GetValue(model),
                        updateData,
                        model.GetType(),
                        (int)model.GetPropertyValue(statement.GetPrimaryKeyPropertyName()),
                        data,
                        statement.GetColumnName(property.Name));
                }
                else
                {
                    if (!columns.Contains($"{statement.TableName}.{statement.GetColumn(property.Name).ColumnName}"))
                    {
                        columns.Add($"{statement.TableName}.{statement.GetColumn(property.Name).ColumnName}");
                        values.Add("NULL");
                    }
                }

                continue;
            }

            if (property.HasAttribute("OneToMany") || property.HasAttribute("ManyToMany"))
            {
                foreach (var item in (IEnumerable)property.GetValue(model))
                    GetUpdateString(
                        item,
                        updateData,
                        model.GetType(),
                        (int)model.GetPropertyValue(statement.GetPrimaryKeyPropertyName()),
                        data);

                continue;
            }

            if (property.HasAttribute("ManyToMany") || property.HasAttribute("OneToMany") || property.HasAttribute("ManyToOne"))
            {
                continue;
            }

            var columnName = statement.GetColumn(property.Name).ColumnName;
            var columnValue = property.GetValue(model);

            if (property.HasAttribute("PrimaryGeneratedColumn"))
            {
                whereString.Add($"{columnName} = {property.GetValue(model)}");
                continue;
            }

            if (columnValue is null)
                continue;

            if (columnValue.GetType() == typeof(string))
            {
                columnValue = $"'{columnValue}'";
            }
            else if (columnValue.GetType() == typeof(DateTime))
            {
                DateTime dateTimeValue = (DateTime)columnValue;
                columnValue = $"'{dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss")}'";
            }

            if (!columns.Contains(columnName))
            {
                columns.Add($"{columnName}");
                values.Add(columnValue.ToString());
            }
        }

        data.TableName = statement.TableName;
        data.Columns = columns;
        data.Values = values;
        data.WhereClause = string.Join(" AND ", whereString);
        data.PrimaryKeyColumnName = statement.GetPrimaryKeyColumnName();
        data.ForeignKeyColumnName = foreignKeyColumnName;
        data.RelationUpdate = relationUpdate;

        updateData.Add(data);
    }

    /// <summary>
    /// Adds the property to the selected columns.
    /// </summary>
    /// <typeparam name="S">Type of the model</typeparam>
    /// <param name="data">List of data</param>
    /// <param name="instance">Instance of the model</param>
    /// <param name="parentType">Type of the parent</param>
    /// <returns>Returns the mapped data</returns>
    public IEnumerable<S> MapManyToMany<S>(IEnumerable<S> data, S instance = null, Type parentType = null) where S : class, new()
    {
        ModelStatement statement;
        PropertyInfo[] props;

        if (instance is null)
        {
            statement = StatementList.Find(x => x.Name == typeof(S).Name);
            props = typeof(S).GetProperties();
        }
        else
        {
            statement = StatementList.Find(x => x.Name == instance.GetType().Name);
            props = instance.GetType().GetProperties();
        }

        foreach (var obj in data)
        {
            if (obj is null)
                continue;

            foreach (var prop in props.WithAttributes(["ManyToMany"]))
            {
                var relStatement = statement.Relationships.Find(x => x.PropertyName == prop.Name);
                string joinString = FindAllRelations(ModelProps, null);
                var modelProps = ModelProps;
                Type typeOfList = prop.PropertyType.GetGenericArguments()[0];
                var nestedInstance = Activator.CreateInstance(typeOfList);

                if (instance is not null)
                {
                    AllColumnsList.Clear();
                    joinString = string.Empty;
                    modelProps = AttributeHelpers.GetPropsByModel(prop.PropertyType.GetGenericArguments()[0]);

                    foreach (var property in modelProps.Properties)
                    {
                        AddToSelectedColumns(property, modelProps.ClassName);
                    }
                }

                string select = _selectColumns.Length > 0
                    ? _selectColumns
                    : AllColumnsString;

                AllColumnsList.Clear();
                var sql =
                    $"SELECT {select} FROM {modelProps.TableName} {joinString} " +
                    $"LEFT JOIN {relStatement.TableName} ON {relStatement.TableName}.{relStatement.ColumnName} = {obj.GetPropertyValue("Id")} " +
                    $"WHERE {modelProps.TableName}.{statement.GetPrimaryKeyColumnName()} = {relStatement.TableName}.{relStatement.ColumnName_1}";
                var sqlResult = dbHandler.Query(sql);
                var result = dataConverter.MapData(sqlResult, nestedInstance);

                IList relList = (IList)Activator.CreateInstance(prop.PropertyType);
                foreach (var item in result)
                {
                    relStatement = statement.Relationships.Find(x => x.PropertyName == prop.Name);
                    relList.Add(item);
                }

                prop.SetValue(obj, relList);
            }

            foreach (var prop in props.WithAttributes(["OneToOne"]).Where(x => x.PropertyType.Name != parentType?.Name))
            {
                object relListInstance = Activator.CreateInstance(prop.PropertyType);
                var objValue = obj.GetPropertyValue(prop.Name);
                IEnumerable<object> relList = new List<object>()
                {
                    objValue
                };

                var result = MapManyToMany(relList, relListInstance, obj.GetType());
                prop.SetValue(obj, result.FirstOrDefault());
            }

            foreach (var prop in props.WithAttributes(["OneToMany"]))
            {
                List<object> relObj = new List<object>();
                var objValue = obj.GetPropertyValue(prop.Name);
                Type typeOfList = prop.PropertyType.GetGenericArguments()[0];
                var nestedInstance = Activator.CreateInstance(typeOfList);

                foreach (var item in objValue as IEnumerable)
                {
                    relObj.Add(item);
                }

                var result = MapManyToMany(relObj, nestedInstance);

                IList relList = (IList)Activator.CreateInstance(prop.PropertyType);
                foreach (var item in result)
                {
                    relList.Add(item);
                }

                prop.SetValue(obj, relList);
            }
        }

        return data;
    }

    /// <summary>
    /// Converts the data to the specified type.
    /// </summary>
    /// <typeparam name="S">Type of the model</typeparam>
    /// <param name="table">Table to convert</param>
    /// <returns>Returns the converted data</returns>
    public IEnumerable<S> ConvertData<S>(DataTable table) where S : class, new()
    {
        var data = dataConverter.MapData<S>(table);
        data = MapManyToMany(data.ToList());

        return data;
    }
}

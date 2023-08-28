using ORM.Querying.Abstract;

namespace ORM.Querying;

public static class StringHelpers
{
    internal static string GetWhereString(this Where? where)
    {
        string whereString = "";

        if (where != null)
        {
            whereString += "WHERE ";
            int count = 0;
            foreach (var item in where)
            {
                whereString += $"{item.Key} IN (";
                int count2 = 0;
                foreach (var value in item.Value)
                {
                    whereString += value.GetType() == typeof(string) ? $"'{value}'" : value;
                    if (count2 < item.Value.Count() - 1)
                    {
                        whereString += ", ";
                    }
                    count2++;
                }
                whereString += ")";
                if (count < where.Count() - 1)
                {
                    whereString += " AND ";
                }
                count++;
            }
        }

        return whereString;
    }

    internal static string GetOrderString(this Order? order)
    {
        string orderByString = "";

        if (order != null)
        {
            orderByString += "ORDER BY ";
            int count = 0;
            foreach (var item in order)
            {
                string columns = "";
                int count2 = 0;
                foreach (var value in item.Value)
                {
                    columns += value;
                    if (count2 < item.Value.Count() - 1)
                    {
                        columns += ", ";
                    }
                    count2++;
                }

                orderByString += $"{columns} {item.Key}";
                if (count < order.Count() - 1)
                {
                    orderByString += ", ";
                }
                count++;
            }
        }

        return orderByString;
    }

    internal static string GetUpdateString(object model)
    {
        string updateString = "";
        int count = 0;

        foreach (var item in model.GetType().GetProperties())
        {
            var itemValue = item.GetValue(model);
            itemValue = itemValue.GetType() == typeof(string) ? $"'{itemValue}'" : itemValue;
            updateString += $"{item.Name} = {itemValue}";
            if (count < model.GetType().GetProperties().Count() - 1)
            {
                updateString += ", ";
            }
            count++;
        }

        return updateString;
    }
}



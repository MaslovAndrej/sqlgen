using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
  public static class Generator
  {
    #region GenerateCreate.

    public static string GenerateCreatePGQuery<T>()
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

      var keyField = string.Empty;
      var fieldElements = new List<string>();
      var valueElements = new List<string>();

      var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
      foreach (var property in properties)
      {
        var attributes = property.GetCustomAttributes();
        var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
        var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

        if (fieldAttribute == null)
          continue;

        if (fieldAttribute.Key)
        {
          fieldElements.Add(string.Format("{0} serial4 NOT NULL,", fieldAttribute.Name));
          continue;
        }

        if (navigateAttribute != null)
        {

          if (navigateAttribute.Required)
            fieldElements.Add(string.Format("\"{0}\" int4 NOT NULL,", fieldAttribute.Name));
          else
            fieldElements.Add(string.Format("\"{0}\" int4 NULL,", fieldAttribute.Name));
          continue;
        }

        var fieldType = GetPGFieldType(property.PropertyType, fieldAttribute.Size);

        fieldElements.Add(string.Format("\"{0}\" {1},", fieldAttribute.Name, fieldType));
      }

      var lastFieldElement = fieldElements.Last();
      fieldElements[fieldElements.Count - 1] = lastFieldElement.Substring(0, lastFieldElement.Length - 1);

      var queryElements = new StringBuilder();
      queryElements.AppendLine(string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}", DBInitializer.Schema, mainTable));
      queryElements.AppendLine("(");
      queryElements.AppendLine(string.Join(Environment.NewLine, fieldElements));
      queryElements.AppendLine(")");

      return queryElements.ToString();
    }

    #endregion

    #region GenerateInsert.

    public static string GenerateInsertQuery<T>(T entity)
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

      var keyField = string.Empty;
      var fieldElements = new List<string>();
      var valueElements = new List<string>();

      var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
      foreach (var property in properties)
      {
        var attributes = property.GetCustomAttributes();
        var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
        var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

        if (fieldAttribute == null)
          continue;

        if (fieldAttribute.Key)
        {
          keyField = fieldAttribute.Name;
          continue;
        }

        var value = property.GetValue(entity);
        if (value == null)
          continue;

        if (navigateAttribute != null)
        {
          var navigateProperty = value.GetType().GetProperties()
            .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute && (xx as FieldAttribute).Name == navigateAttribute.FieldName))
            .FirstOrDefault();
          var navigatePropertyValue = navigateProperty.GetValue(value);
          if (navigatePropertyValue == null)
            continue;

          value = navigatePropertyValue;
        }

        fieldElements.Add(string.Format("\"{0}\",", fieldAttribute.Name));
        valueElements.Add(GetFieldValue(property.PropertyType, value) + ",");
      }

      var lastFieldElement = fieldElements.Last();
      fieldElements[fieldElements.Count - 1] = lastFieldElement.Substring(0, lastFieldElement.Length - 1);

      var lastValueElement = valueElements.Last();
      valueElements[valueElements.Count - 1] = lastValueElement.Substring(0, lastValueElement.Length - 1);

      var queryElements = new StringBuilder();
      queryElements.AppendLine(string.Format("INSERT INTO {0}.{1}", DBInitializer.Schema, mainTable));
      queryElements.AppendLine("(");
      queryElements.AppendLine(string.Join(Environment.NewLine, fieldElements));
      queryElements.AppendLine(")");
      queryElements.AppendLine("VALUES");
      queryElements.AppendLine("(");
      queryElements.AppendLine(string.Join(Environment.NewLine, valueElements));
      queryElements.AppendLine(")");

      if (!string.IsNullOrEmpty(keyField))
        queryElements.AppendLine(string.Format("RETURNING {0}", keyField));

      return queryElements.ToString();
    }

    #endregion

    #region GenerateUpdate.

    public static string GenerateUpdateQuery<T>(T entity)
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

      var keyField = string.Empty;
      var keyValue = string.Empty;
      var updateElements = new List<string>();

      var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
      foreach (var property in properties)
      {
        var attributes = property.GetCustomAttributes();
        var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
        var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

        if (fieldAttribute == null)
          continue;

        if (fieldAttribute.Key)
        {
          keyField = fieldAttribute.Name;
          var propValue = property.GetValue(entity);
          var isKeyNumeric = property.PropertyType == typeof(int) || property.PropertyType == typeof(decimal) || property.PropertyType == typeof(double);
          keyValue = string.Format(isKeyNumeric ? "{0}" : "'{0}'", isKeyNumeric ? propValue.ToString().Replace(",", ".") : propValue.ToString());
          continue;
        }

        var value = property.GetValue(entity);
        if (value == null)
          continue;

        if (navigateAttribute != null)
        {
          var navigateProperty = value.GetType().GetProperties()
            .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute && (xx as FieldAttribute).Name == navigateAttribute.FieldName))
            .FirstOrDefault();
          var navigatePropertyValue = navigateProperty.GetValue(value);
          if (navigatePropertyValue == null)
            continue;

          value = navigatePropertyValue;
        }

        var updateValue = GetFieldValue(property.PropertyType, value);
        updateElements.Add(string.Format("{0} = {1},", fieldAttribute.Name, updateValue));
      }

      var lastUpdateElement = updateElements.Last();
      updateElements[updateElements.Count - 1] = lastUpdateElement.Substring(0, lastUpdateElement.Length - 1);

      var queryElements = new StringBuilder();
      queryElements.AppendLine(string.Format("UPDATE {0}.{1}", DBInitializer.Schema, mainTable));
      queryElements.AppendLine("SET");
      queryElements.AppendLine(string.Join(Environment.NewLine, updateElements));
      queryElements.AppendLine("WHERE");
      queryElements.AppendLine(string.Format("{0} = {1}", keyField, keyValue));

      return queryElements.ToString();
    }

    #endregion

    #region GenerateDelete.

    public static string GenerateDeleteQuery<T>()
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
      var mainAlias = mainTable.Substring(0, 3).ToLower();

      return string.Format("DELETE FROM {0}.{1} {2}", DBInitializer.Schema, mainTable, mainAlias);
    }

    public static string GenerateDeleteQuery<T>(AddWhere<T> addWhere)
    {
      var query = GenerateDeleteQuery<T>();

      var whereElements = new List<string>();
      whereElements.Add("WHERE");
      whereElements.Add(addWhere.Result);

      return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
    }

    public static string GenerateDeleteQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
    {
      var query = GenerateDeleteQuery<T>();

      var whereElements = new List<string>();
      whereElements.Add("WHERE");
      foreach (var addWhere in addWhereList)
      {
        whereElements.Add(addWhere.Result);
        whereElements.Add(addWhereCondition);
      }
      whereElements.RemoveAt(whereElements.Count - 1);

      return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
    }

    public static string GenerateDeleteQuery<T>(T entity)
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

      var property = type.GetProperties()
        .Where(x => x.GetCustomAttributes()
          .Where(xx => xx is FieldAttribute)
          .Cast<FieldAttribute>()
          .Any(xxx => xxx.Key)).FirstOrDefault();

      if (property == null)
        throw new Exception("Удаление возможно только при наличии ключевого свойства.");

      var value = property.GetValue(entity);
      if (value == null)
        throw new Exception("Удаление возможно только при наличии значения ключевого свойства.");

      var keyField = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault().Name;
      var keyValue = GetFieldValue(property.PropertyType, value);

      var queryElements = new StringBuilder();
      queryElements.AppendLine(string.Format("DELETE FROM {0}.{1}", DBInitializer.Schema, mainTable));
      queryElements.AppendLine("WHERE");
      queryElements.AppendLine(string.Format("{0} = {1}", keyField, keyValue));

      return queryElements.ToString();
    }

    #endregion

    #region GenerateSelect.

    public static string GenerateSelectQuery<T>(AddWhere<T> addWhere, AddOrder<T> addOrder, string direction)
    {
      var query = GenerateSelectQuery<T>(addWhere);

      var whereElements = new List<string>();
      whereElements.Add("ORDER BY");
      whereElements.Add(addOrder.Result);
      whereElements.Add(direction);

      return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
    }

    public static string GenerateSelectQuery<T>(AddOrder<T> addOrder, string direction)
    {
      var query = GenerateSelectQuery<T>();

      var whereElements = new List<string>();
      whereElements.Add("ORDER BY");
      whereElements.Add(addOrder.Result);
      whereElements.Add(direction);

      return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
    }

    public static string GenerateSelectQuery<T>(AddWhere<T> addWhere)
    {
      var query = GenerateSelectQuery<T>();

      var whereElements = new List<string>();
      whereElements.Add("WHERE");
      whereElements.Add(addWhere.Result);

      return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
    }

    public static string GenerateSelectQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
    {
      var query = GenerateSelectQuery<T>();

      var whereElements = new List<string>();
      whereElements.Add("WHERE");
      foreach (var addWhere in addWhereList)
      {
        whereElements.Add(addWhere.Result);
        whereElements.Add(addWhereCondition);
      }
      whereElements.RemoveAt(whereElements.Count - 1);

      return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
    }

    public static string GenerateSelectQuery<T>()
    {
      var selectElementTemplate = "{0}.{1} as {2},";
      var fromElementTemplate = "FROM {0}.{1} {2}";
      var joinElementTemplate = @"{0} {1}.{2} {3} ON {3}.{4} = {5}.{6}";

      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
      var mainAlias = mainTable.Substring(0, 3).ToLower();
      var fromElement = string.Format(fromElementTemplate, DBInitializer.Schema, mainTable, mainAlias);

      var selectElements = GetSelectElements(type, mainAlias);
      var joinElements = new List<string>();
      var joinAliases = new List<string>();

      var properties = type.GetProperties()
        .Where(x => x.GetCustomAttributes().Any(xx => xx is NavigateAttribute));
      foreach (var property in properties)
      {
        var attributes = property.GetCustomAttributes();
        var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
        var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

        if (fieldAttribute == null)
          continue;

        var selectElement = string.Format(selectElementTemplate, mainAlias, fieldAttribute.Name, property.Name);
        selectElements.Add(selectElement);

        if (navigateAttribute != null)
        {
          var joinType = "JOIN";
          if (!navigateAttribute.Required)
            joinType = "LEFT JOIN";

          var joinPostfix = 0;
          var joinAlias = navigateAttribute.TableName.Substring(0, 3).ToLower();
          while (joinAliases.Contains(joinAlias))
          {
            joinPostfix++;
            joinAlias = joinAlias + joinPostfix.ToString();
          }
          joinAliases.Add(joinAlias);

          var joinElement = string.Format(joinElementTemplate, joinType, DBInitializer.Schema, navigateAttribute.TableName,
                                                               joinAlias, navigateAttribute.FieldName, mainAlias, fieldAttribute.Name);
          joinElements.Add(joinElement);

          var propertyType = property.PropertyType;
          selectElements.AddRange(GetSelectElements(propertyType, joinAlias));
        }
      }

      var lastElement = selectElements.Last();
      selectElements[selectElements.FindIndex(x => x.Equals(lastElement))] = lastElement.Substring(0, lastElement.Length - 1);

      var queryElements = new List<string>();
      queryElements.Add("SELECT");
      queryElements.Add(string.Join(Environment.NewLine, selectElements));
      queryElements.Add(fromElement);
      if (joinElements.Any())
        queryElements.Add(string.Join(Environment.NewLine, joinElements));

      return string.Join(Environment.NewLine, queryElements);
    }

    private static List<string> GetSelectElements(Type type, string alias)
    {
      var selectElementTemplate = "{0}.{1} as {2},";

      var selectElements = new List<string>();

      var onlyFieldProperies = type.GetProperties()
        .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute) &&
                    !x.GetCustomAttributes().Any(xx => xx is NavigateAttribute));

      foreach (var property in onlyFieldProperies)
      {
        var attributes = property.GetCustomAttributes();
        var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

        var selectElement = string.Format(selectElementTemplate, alias, fieldAttribute.Name, property.Name);
        selectElements.Add(selectElement);
      }

      return selectElements;
    }

    #endregion

    #region GetFieldValue.

    public static string GetFieldValue(Type type, object value)
    {
      string fieldValue;

      if (type == typeof(int) || type == typeof(decimal) || type == typeof(double))
        fieldValue = value.ToString().Replace(",", ".");
      else if (type == typeof(DateTime))
        fieldValue = string.Format("'{0}'", (value as DateTime?).Value.ToString("yyyy-MM-dd HH:mm:ss"));
      //else if (type == typeof(bool))
      //  fieldValue = (value as bool?).Value ? "1" : "0";
      else
        fieldValue = string.Format("'{0}'", value);

      return fieldValue;
    }

    #endregion

    #region GetPGFieldType.

    public static string GetPGFieldType(Type type, int size)
    {
      var fieldType = string.Empty;

      if (type == typeof(string))
        fieldType = "varchar({0}) NULL";
      if (type == typeof(short))
        fieldType = "int2 NOT NULL";
      if (type == typeof(short?))
        fieldType = "int2 NULL";
      if (type == typeof(int))
        fieldType = "int4 NOT NULL";
      if (type == typeof(int?))
        fieldType = "int4 NULL";
      if (type == typeof(long))
        fieldType = "int6 NOT NULL";
      if (type == typeof(long?))
        fieldType = "int6 NULL";
      if (type == typeof(double))
        fieldType = "numeric({0}) NOT NULL";
      if (type == typeof(double?))
        fieldType = "numeric({0}) NULL";
      if (type == typeof(decimal))
        fieldType = "numeric({0}) NOT NULL";
      if (type == typeof(decimal?))
        fieldType = "numeric({0}) NULL";
      if (type == typeof(bool))
        fieldType = "bool NOT NULL";
      if (type == typeof(bool?))
        fieldType = "bool NULL";
      if (type == typeof(DateTime))
        fieldType = "timestamp NOT NULL";
      if (type == typeof(DateTime?))
        fieldType = "timestamp NULL";

      if (fieldType.Contains("{0}"))
      {
        if (size > 0)
          fieldType = string.Format(fieldType, size);
        else
          fieldType = fieldType.Replace("({0})", string.Empty);
      }

      return fieldType;
    }

    #endregion
  }

  #region AddWhere.

  public static class AddWhereCondition
  {
    public const string OR = "OR";
    public const string AND = "AND";
  }

  public class AddWhere<T>
  {
    public string Result { get; set; }

    public AddWhere(string propertyName, string expression)
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
      var mainAlias = mainTable.Substring(0, 3).ToLower();

      var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
      var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

      var fieldName = string.Format("{0}.{1}", mainAlias, fieldAttribute.Name);

      Result = string.Format("{0}.{1} {2}", mainAlias, fieldAttribute.Name, expression);
    }

    public AddWhere(string propertyName, string expression, object value)
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
      var mainAlias = mainTable.Substring(0, 3).ToLower();

      var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
      var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

      var fieldName = string.Format("{0}.{1}", mainAlias, fieldAttribute.Name);
      var fieldValue = QueryGenerator.GetFieldValue(property.PropertyType, value);

      if (expression == Constants.Expression.Like)
      {
        fieldName = string.Format("lower({0})", fieldName);
        fieldValue = string.Format("'%{0}%'", fieldValue.Replace("'", ""));
      }

      Result = string.Format("{0}.{1} {2} {3}", mainAlias, fieldAttribute.Name, expression, fieldValue);
    }
  }

  #endregion

  #region AddOrder.

  public static class AddOrderDirection
  {
    public const string ASC = "ASC";
    public const string DESC = "DESC";
  }

  public class AddOrder<T>
  {
    public string Result { get; set; }

    public AddOrder(string propertyName)
    {
      var type = typeof(T);

      var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
      var mainAlias = mainTable.Substring(0, 3).ToLower();

      var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
      var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

      Result = string.Format("{0}.{1}", mainAlias, fieldAttribute.Name);
    }
  }

  #endregion
}

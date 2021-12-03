using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;

namespace C200.WebApi.Services;

public interface IDBService
{
   List<ModelClass> GetList<ModelClass>(string sql, params object[] list);
   List<dynamic> GetList(string sql, params object[] list);
   DataTable GetTable(string sql, params object[] list);
   int ExecSQL(string sql, params object[] list);
   string GetLastErrMsg();
}

public class DBService : IDBService
{
   private static readonly string? env = 
      Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
   private static readonly IConfiguration config =
      new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json")
         .AddJsonFile($"appsettings.{env}.json")
         .Build()
         .GetSection("ConnectionStrings");

   private static readonly string? DB_CONNECTION = config.GetValue<String>("DefaultConnection");
   private string DB_SQL = "";
   private string DB_Message = "";

   public string GetLastErrMsg()
   {
      return DB_Message;
   }

   public List<dynamic> GetList(string sql, params object[] list)
   {
      return GetTable(sql, list).ToDynamic();
   }

   public List<ModelClass> GetList<ModelClass>(string sql, params object[] list)
   {
      return GetTable(sql, list).ToStatic<ModelClass>();
   }

   public DataTable GetTable(string sql, params object[] list)
   {
      for (int i = 0; i < list.Length; i++)
         if (list[i] is string @string)
            list[i] = ((string)@string.ReturnEmptyIfNull()).EscQuote();

      DB_SQL = String.Format(sql, list);

      DataTable dt = new();
      using SqlConnection dbConn = new(DB_CONNECTION);
      using SqlDataAdapter dAdptr = new(DB_SQL, dbConn);
      try
      {
         dAdptr.Fill(dt);
      }
      catch (System.Exception ex)
      {
         DB_Message = ex.Message;
      }
      return dt;
      // dbConn and dAdptr automatically close
   }

   public int ExecSQL(string sql, params object[] list)
   {
      for (int i = 0; i < list.Length; i++)
         if (list[i] is string @string) 
            list[i] = ((string)@string.ReturnEmptyIfNull()).EscQuote();
         
      DB_SQL = String.Format(sql, list);

      int rowsAffected = 0;
      using (SqlConnection dbConn = new(DB_CONNECTION))
      using (SqlCommand dbCmd = dbConn.CreateCommand())
      {
         try
         {
            dbConn.Open();
            dbCmd.CommandText = DB_SQL;
            rowsAffected = dbCmd.ExecuteNonQuery();
         }
         catch (System.Exception ex)
         {
            DB_Message = ex.Message;
            rowsAffected = -1;
         }
      }
      return rowsAffected;
   }
}

public static class DBServiceHelper
{
   public static List<DTO> ToStatic<DTO>(this DataTable dt)
   {
      var list = new List<DTO>();
      foreach (DataRow row in dt.Rows)
      {
         DTO obj = (DTO)Activator.CreateInstance(typeof(DTO));
         foreach (DataColumn column in dt.Columns)
         {
            PropertyInfo Prop = obj.GetType().GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
            if (row[column] == DBNull.Value)
               Prop?.SetValue(obj, null);
            else
            {
               //Debug.WriteLine(row[column].GetType() + " " + Prop?.PropertyType); 
               if (row[column].GetType() == Prop?.PropertyType)
                  Prop?.SetValue(obj, row[column]);
            }
         }
         list.Add(obj);
      }
      return list;
   }

   public static List<dynamic> ToDynamic(this DataTable dt)
   {
      var dynamicDt = new List<dynamic>();
      foreach (DataRow row in dt.Rows)
      {
         dynamic dyn = new ExpandoObject();
         foreach (DataColumn column in dt.Columns)
         {
            var dic = (IDictionary<string, object>)dyn;
            dic[column.ColumnName] = row[column];
         }
         dynamicDt.Add(dyn);
      }
      return dynamicDt;
   }

   public static List<T> ToListABC<T>(this DataTable dataTable) where T : new()
   {
      var dataList = new List<T>();

      //Define what attributes to be read from the class
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

      //Read Attribute Names and Types
      var objFieldNames = typeof(T).GetProperties(flags).Cast<PropertyInfo>().
          Select(item => new
          {
             Name = item.Name,
             Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType
          }).ToList();

      //Read Datatable column names and types
      var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().
          Select(item => new
          {
             Name = item.ColumnName,
             Type = item.DataType
          }).ToList();

      foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
      {
         var classObj = new T();

         foreach (var dtField in dtlFieldNames)
         {
            PropertyInfo propertyInfos = classObj.GetType().GetProperty(dtField.Name);

            var field = objFieldNames.Find(x => x.Name == dtField.Name);

            if (field != null)
            {

               if (propertyInfos.PropertyType == typeof(DateTime))
               {
                  propertyInfos.SetValue
                  (classObj, convertToDateTime(dataRow[dtField.Name]), null);
               }
               else if (propertyInfos.PropertyType == typeof(int))
               {
                  propertyInfos.SetValue
                  (classObj, ConvertToInt(dataRow[dtField.Name]), null);
               }
               else if (propertyInfos.PropertyType == typeof(long))
               {
                  propertyInfos.SetValue
                  (classObj, ConvertToLong(dataRow[dtField.Name]), null);
               }
               else if (propertyInfos.PropertyType == typeof(decimal))
               {
                  propertyInfos.SetValue
                  (classObj, ConvertToDecimal(dataRow[dtField.Name]), null);
               }
               else if (propertyInfos.PropertyType == typeof(String))
               {
                  if (dataRow[dtField.Name].GetType() == typeof(DateTime))
                  {
                     propertyInfos.SetValue
                     (classObj, ConvertToDateString(dataRow[dtField.Name]), null);
                  }
                  else
                  {
                     propertyInfos.SetValue
                     (classObj, ConvertToString(dataRow[dtField.Name]), null);
                  }
               }
            }
         }
         dataList.Add(classObj);
      }
      return dataList;
   }

   private static string ConvertToDateString(object date)
   {
      if (date == null)
         return string.Empty;
      return String.Format("yyyy-MM-dd hh:mm:ss", date);
   }

   private static string ConvertToString(object value)
   {
      return Convert.ToString((string) ReturnEmptyIfNull(value));
   }

   public static object ReturnEmptyIfNull(this object value)
   {
      if (value == DBNull.Value)
         return string.Empty;
      if (value == null)
         return string.Empty;
      return value;
   }

   public static object ReturnZeroIfNull(this object value)
   {
      if (value == DBNull.Value)
         return 0;
      if (value == null)
         return 0;
      return value;
   }

   private static int ConvertToInt(object value)
   {
      return Convert.ToInt32(ReturnZeroIfNull(value));
   }

   private static long ConvertToLong(object value)
   {
      return Convert.ToInt64(ReturnZeroIfNull(value));
   }

   private static decimal ConvertToDecimal(object value)
   {
      return Convert.ToDecimal(ReturnZeroIfNull(value));
   }

   private static DateTime convertToDateTime(object date)
   {
      return Convert.ToDateTime(ReturnDateTimeMinIfNull(date));
   }

   public static object ReturnDateTimeMinIfNull(this object value)
   {
      if (value == DBNull.Value)
         return DateTime.MinValue;
      if (value == null)
         return DateTime.MinValue;
      return value;
   }

   public static string EscQuote(this string line)
   {
      return line.Replace("'", "''");
   }
}


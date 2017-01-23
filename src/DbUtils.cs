using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace CustomDataSourceSample
{
    /// <summary>
    /// Utility methods for databases
    /// </summary>
    public class DbUtils
    {
        // There is no generic AddWithValue on DbParameter class, so we use that utility method.
        public static void AddParameterWithValue(DbCommand command, String name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        public static DbConnection CreateConnection(CustomDataSourceConfiguration configuration)
        {
            DbConnection con = null;
            switch (configuration.Driver)
            {
                case "SqlClient":
                    con = new SqlConnection(configuration.Url);
                    break;
                case "OleDb":
                    con = new OleDbConnection(configuration.Url);
                    break;
                default:
                    throw new System.ArgumentException("Unknown database driver " + configuration.Driver);
            }
            return con;
        }
    }
}

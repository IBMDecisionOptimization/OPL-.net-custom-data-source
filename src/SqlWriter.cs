using System;
using System.Data.Common;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.OPL;

namespace CustomDataSourceSample
{
    /// <summary>
    /// This class contains utility classes to write data to a database.
    /// </summary>
    class SqlWriter
    {
        CustomDataSourceConfiguration Configuration { set; get; }
        OplModel Model { set; get;  }

        static String DROP_STATEMENT = "DROP TABLE [{0}]";

        public SqlWriter(CustomDataSourceConfiguration configuration, OplModel model)
        {
            Configuration = configuration;
            Model = model;
        }


        /// <summary>
        /// Writes the specified data set as the specified target table.
        /// </summary>
        /// <param name="name">The name of the OPL data set</param>
        /// <param name="targetTable">The target table</param>
        public void WriteTable(DbConnection con, string name, String targetTable)
        {
            // The opl element to write
            OplElement element = this.Model.GetElement(name);
            ITupleSet tupleSet = element.AsTupleSet();
            ITupleSchema tupleSchema = tupleSet.Schema;
            // drop existing table
            DropTable(con, targetTable);
            // Create table
            String[] tableDef = TableDefinitionFromOplTupleSchema(tupleSchema, true);
            CreateTable(con, tableDef, targetTable); 
            // Populate table
            String insertCommand = CreateInsertCommand(tupleSchema, targetTable);
            foreach (ITuple t in tupleSet) {
                InsertIntoTable(con, tupleSchema, t, insertCommand);
            }
        }


        /// <summary>
        /// Drops the specified table, ignoring errors.
        /// </summary>
        /// <param name="con"></param>
        /// <param name="name"></param>
        public void DropTable(DbConnection con, String name)
        {
            using (DbCommand drop = con.CreateCommand())
            {
                drop.CommandText = String.Format(DROP_STATEMENT, name);
                try
                {
                    drop.ExecuteNonQuery();
                }
                catch (DbException)
                {
                    // Ignore SqlException here, for when the table did not exist previously
                }
            }
        }

        /// <summary>
        /// Create targetTable based on definition of the columns.
        /// </summary>
        /// <param name="con">The SQL Connection</param>
        /// <param name="columns">Definition of the columns like in SQL</param>
        /// <param name="targetTable">The name of the table.</param>
        public void CreateTable(DbConnection con, string[] columns, string targetTable)
        {
            String statement = "CREATE TABLE " + targetTable + "(" + String.Join(",", columns) + ")";
            Console.WriteLine("Create query = " + statement);
            using(DbCommand create = con.CreateCommand())
            {
                create.CommandText = statement;
                create.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Creates the SQL insert command.
        /// 
        /// </summary>
        /// <param name="tupleSchema">The tuple schema</param>
        /// <param name="table">The target table</param>
        /// <returns></returns>
        private String CreateInsertCommand(ITupleSchema tupleSchema, string table)
        {
            String[] values = new String[tupleSchema.Size];
            for (int i = 0; i < tupleSchema.Size; i++)
            {
                values[i] = "@value" + i;
            }
            String[] fields = TableDefinitionFromOplTupleSchema(tupleSchema, false);

            String insertCommand = "INSERT INTO " + table + "(" + String.Join(",", fields) + ") VALUES(" + String.Join(",", values) + ")";
            return insertCommand;
        }

        private void InsertIntoTable(DbConnection con, ITupleSchema tupleSchema, ITuple tuple, string insertCommand)
        {
            OplElementDefinition elementDefinition = this.Model.ModelDefinition.getElementDefinition(tupleSchema.Name);
            OplTupleSchemaDefinition tupleDef = elementDefinition.asTupleSchema();

            using (DbCommand insert = con.CreateCommand())
            {
                insert.CommandText = insertCommand;
                for (int i = 0; i < tupleSchema.Size; i++)
                {
                    OplElementDefinitionType.Type oplType = tupleDef.getComponent(i).getElementDefinitionType();
                    object oplValue = null;
                    if (oplType == OplElementDefinitionType.Type.INTEGER)
                        oplValue = tuple.GetIntValue(i);
                    else if (oplType == OplElementDefinitionType.Type.FLOAT)
                        oplValue = tuple.GetNumValue(i);
                    else if (oplType == OplElementDefinitionType.Type.STRING)
                        oplValue = tuple.GetStringValue(i);
                    DbUtils.AddParameterWithValue(insert, "@value" + i, oplValue);
                }
                insert.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Returns a table definition from an OPL tuple schema.
        /// 
        /// The table definition is an array of N strings, each describing an element of the tuple.
        /// </summary>
        /// <param name="tupleSchema">The OPL Tuple schema</param>
        /// <param name="includeType">If True, The table definition has the form "NAME TYPE" otherwise just "NAME"</param>
        /// <returns>The table definition</returns>
        private String[] TableDefinitionFromOplTupleSchema(ITupleSchema tupleSchema, bool includeType)
        {
            OplElementDefinition elementDefinition = this.Model.ModelDefinition.getElementDefinition(tupleSchema.Name);
            OplTupleSchemaDefinition oplTupleSchema = elementDefinition.asTupleSchema();
            String[] fields = new String[tupleSchema.Size];
            for (int i = 0; i < tupleSchema.Size; i++)
            {
                String columnName = tupleSchema.GetColumnName(i);
                String field = columnName;
                if (includeType)
                {
                    OplElementDefinitionType.Type oplType = oplTupleSchema.getComponent(i).getElementDefinitionType();

                    String sqlType = null;
                    if (oplType == OplElementDefinitionType.Type.INTEGER)
                        sqlType = "INT";
                    else if (oplType == OplElementDefinitionType.Type.FLOAT)
                        sqlType = "FLOAT";
                    else if (oplType == OplElementDefinitionType.Type.STRING)
                        sqlType = "VARCHAR(30)";
                    field += " " + sqlType;
                }
                fields[i] = field;
            }
            return fields;
        }



        /// <summary>
        /// Writes the results.
        /// 
        /// The result tables are defined in the CustomDataSourceConfiguration passed in the constructor
        /// of SqlWriter.
        /// </summary>
        public void WriteResults()
        {
            using (DbConnection con = DbUtils.CreateConnection(Configuration))
            {
                con.Open();
                foreach (KeyValuePair<string, string> output in this.Configuration.WriteTables)
                {
                    String name = output.Key;
                    String table = output.Value;
                    Console.WriteLine("Writing " + name + " using table " + table);
                    WriteTable(con, name, table);
                }
            }
        }
    }
}

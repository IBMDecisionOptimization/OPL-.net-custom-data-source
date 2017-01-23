using System;
using System.Data.Common;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.OPL;

namespace CustomDataSourceSample
{
    /// <summary>
    /// A CustomOplDataSource to read data from a SQL Server database.
    /// </summary>
    public class SqlCustomDataSource : CustomOplDataSource
    {
        internal OplModelDefinition modelDefinition { get; set; }
        internal CustomDataSourceConfiguration configuration { get; set; }

        /// <summary>
        /// Creates a new SqlCustomDataSource.
        /// 
        /// The connection string to MS SQL Server and the data reading queries
        /// are defined in a XML file used to initialize a CustomReaderConfiguration.
        /// </summary>
        /// <param name="oplF">The OPL Factory class</param>
        /// <param name="modelDef">The OPL model definition</param>
        /// <param name="configuration">The reader configuration.</param>
        public SqlCustomDataSource(OplFactory oplF, OplModelDefinition modelDef,
            CustomDataSourceConfiguration configuration)
            : base(oplF)
        {
            this.modelDefinition = modelDef;
            this.configuration = configuration;
        }

        /// <summary>
        /// Reads the set or tuple from the specified data reader.
        /// </summary>
        /// <param name="name">set or tuple name.</param>
        /// <param name="reader">data source.</param>
        private void ReadSetOrTuple(String name, DbDataReader reader)
        {
            OplElementDefinition def = this.modelDefinition.getElementDefinition(name);
            OplElementDefinitionType.Type type = def.getElementDefinitionType();
            OplElementDefinitionType.Type leaf = def.getLeaf().getElementDefinitionType();
            if (type == OplElementDefinitionType.Type.SET)
            {
                if (leaf == OplElementDefinitionType.Type.TUPLE)
                {
                    this.ReadTupleSet(name, reader);
                }
                else
                {
                    this.ReadSet(leaf, name, reader);
                }
            }
        }

        /// <summary>
        /// Reads a table and inject it as a tuple set in OPL.
        /// </summary>
        /// <param name="name">The name of the Data Set</param>
        /// <param name="reader">The SqlDataReader to read from</param>
        private void ReadTupleSet(String name, DbDataReader reader)
        {
            OplDataHandler handler = DataHandler;

            OplElement element = handler.getElement(name);
            ITupleSchema schema = element.AsTupleSet().Schema;
            int size = schema.Size;

            String[] oplFieldsName = new String[size];
            OplElementDefinitionType.Type[] oplFieldsType = new OplElementDefinitionType.Type[size];

            this.FillNamesAndTypes(schema, oplFieldsName, oplFieldsType);

            handler.StartElement(name);
            handler.StartSet();
            while (reader.Read())
            {
                handler.StartTuple();
                for (int column=0 ; column < oplFieldsName.Length; column++)
                {
                    String columnName = oplFieldsName[column];
                    HandleColumnValue(handler, oplFieldsType[column], reader[columnName]);
                }
                handler.EndTuple();
            }
            handler.EndSet();

        }

        /// <summary>
        /// Reads a table and inject the value as a Set in OPL.
        /// </summary>
        /// <param name="type">The type of the Set elements</param>
        /// <param name="name">The name of the Set</param>
        /// <param name="reader">The SqlDataReader to read from</param>
        private void ReadSet(OplElementDefinitionType.Type type, String name, DbDataReader reader)
        {
            OplDataHandler handler = DataHandler;

            handler.StartElement(name);
            handler.StartSet();

            while (reader.Read())
            {
                HandleColumnValue(handler, type, reader.GetValue(0));
            }

            handler.EndSet();
        }


        /// <summary>
        /// Populates array names and types with the information from the tuple schema.
        /// </summary>
        /// <param name="schema">The tuple definition</param>
        /// <param name="names">Contains the field names</param>
        /// <param name="types">Contains the field types</param>
        private void FillNamesAndTypes(ITupleSchema schema, string[] names, OplElementDefinitionType.Type[] types)
        {
            OplElementDefinition elementDefinition = this.modelDefinition.getElementDefinition(schema.Name);
            OplTupleSchemaDefinition tupleSchema = elementDefinition.asTupleSchema();
            for (int i = 0; i < schema.Size; i++)
            {
                types[i] = tupleSchema.getComponent(i).getElementDefinitionType();
                names[i] = schema.GetColumnName(i);
            }
        }

       
        /// <summary>
        /// Add a new item (field) to the Set currently in the Data handler.
        /// 
        /// The value of the field is converted and added depending on fieldType.
        /// </summary>
        /// <param name="handler">The OPL data handler</param>
        /// <param name="fieldType">The type of the field to add</param>
        /// <param name="value">The value to convert</param>
        private void HandleColumnValue(OplDataHandler handler, OplElementDefinitionType.Type fieldType, object value)
        {
            if (fieldType == OplElementDefinitionType.Type.INTEGER)
            {
                handler.AddIntItem(Convert.ToInt32(value));
            }
            else if (fieldType == OplElementDefinitionType.Type.FLOAT)
            {
                handler.AddNumItem(Convert.ToDouble(value));
            }
            else if (fieldType == OplElementDefinitionType.Type.STRING)
            {
                handler.AddStringItem(Convert.ToString(value));
            }
        }


        /// <summary>
        /// Overrides the CustomRead() method in the CustomOplDataSource.
        /// 
        /// This is the method called for populating the tables when opl.Generate() is called.
        /// </summary>
        public override void CustomRead()
        {
            OplDataHandler handler = DataHandler;

            using (DbConnection con = DbUtils.CreateConnection(this.configuration))
            {
                con.Open();
                // read queries
                foreach (KeyValuePair<string, string> query in this.configuration.ReadQueries) {
                    Console.WriteLine("Reading table " + query.Key + " with query " + query.Value);
                    DbCommand command = con.CreateCommand();
                    command.CommandText = query.Value;
                    using (DbDataReader reader = command.ExecuteReader()) {
                        this.ReadSetOrTuple(query.Key, reader);
                    }
                }
            }
        }
    }
}

// --------------------------------------------------------------- -*- C# -*-
// File: CustomDataSourceSample.cs
// --------------------------------------------------------------------------
// Apache License
// --------------------------------------------------------------------------

using System;
using System.IO;
using System.Data.Common;
using ILOG.OPL;

namespace CustomDataSourceSample
{
    class CustomDataSourceSample
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Assume we run in bin/Debug or bin/Release
            const string DATADIR = "../..";

            int status = 127;

            CommandLineParser parser = new CommandLineParser();
            // set some default values for the sample
            parser.modelFileName = DATADIR + "/data/oil.mod";
            parser.configurationFileName = DATADIR + "/data/db_mssql.xml";

            parser.Parse(args);


            // set default datafilename if none specified

            if (parser.dataFileNames.Count == 0)
               parser.dataFileNames.Add(DATADIR + "/data/oil.dat");

            // parse the data source configuration
            CustomDataSourceConfiguration configuration = new CustomDataSourceConfiguration(parser.configurationFileName);

            // if this is the default sample, write some default data
            if (parser.CreateSampleDatabase)
            {
                CreateDefaultSampleDatabase(configuration);
            }

            try
            {
                OplFactory.DebugMode = true;
                OplFactory oplF = new OplFactory();
                OplErrorHandler errHandler = oplF.CreateOplErrorHandler(Console.Out);

                OplRunConfiguration rc = null;
 

                if (parser.dataFileNames.Count == 0)
                    rc = oplF.CreateOplRunConfiguration(parser.modelFileName);
                else
                    rc = oplF.CreateOplRunConfiguration(parser.modelFileName,
                        (String[])parser.dataFileNames.ToArray(typeof(String)));

                OplModel opl = rc.GetOplModel();
                OplModelDefinition def = opl.ModelDefinition;

                OplDataSource dataSource = new SqlCustomDataSource(oplF, def, configuration);
                opl.AddDataSource(dataSource);

                opl.Generate();

                if (parser.externalDataName != null)
                {
                    using (TextWriter wt = File.CreateText(parser.externalDataName))
                        opl.PrintExternalData(wt);
                }

                bool success = opl.HasCplex ? opl.Cplex.Solve() : opl.CP.Solve();

                if (success)
                {
                    opl.PostProcess();
                    // Write results to result table
                    Console.WriteLine("Writing results");
                    SqlWriter writer = new SqlWriter(configuration, opl);
                    writer.WriteResults();
                }

                oplF.End();
                status = 0;
            }
            catch (ILOG.OPL.OplException ex)
            {
                Console.WriteLine(ex.Message);
                status = 2;
            }
            catch (ILOG.Concert.Exception ex)
            {
                Console.WriteLine(ex.Message);
                status = 3;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.Message);
                status = 4;
            }

            Environment.ExitCode = status;

            Console.WriteLine("--Press <Enter> to exit--");
            Console.ReadLine();
        }


        internal static void WriteTableData(DbConnection con, String insertCommand, object[][] data)
        {
            foreach (object[] gv in data)
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = insertCommand;
                    for (int i = 0; i < gv.Length; i++)
                    {
                        DbUtils.AddParameterWithValue(cmd, "@v" + i, gv[i]);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// This method creates a sample database and write data for demonstration with data/oil.mod
        /// </summary>
        /// <param name="configuration">The data source configuration</param>
        internal static void CreateDefaultSampleDatabase(CustomDataSourceConfiguration configuration)
        {
            Console.WriteLine("Creating default sample database.");
            SqlWriter writer = new SqlWriter(configuration, null);
            using (DbConnection con = DbUtils.CreateConnection(configuration))
            {
                con.Open();
                // Creates the OilData table
                String OilData = "OilData";
                writer.DropTable(con, OilData);
                String[] OilColumnsDef = { "NAME		VARCHAR(30)",
	                                       "CAPACITY  	INT",
	                                       "PRICE		DECIMAL(6,2)",
	                                       "OCTANE		DECIMAL(6,2)",
	                                       "LEAD		DECIMAL(6,2)"};
                writer.CreateTable(con, OilColumnsDef, OilData);
                object[][] OilValues = {
	                new object[] {"Super", 3000, 70, 10, 1},
                    new object[] {"Regular", 2000, 60, 8, 2},
	                new object[] {"Diesel", 1000, 50, 6, 1}                                  
                };
                String insertCommand = @"INSERT INTO OilData(NAME, CAPACITY, PRICE, OCTANE, LEAD)
                                         VALUES (@v0, @v1, @v2, @v3, @v4)";
                WriteTableData(con, insertCommand, OilValues);
                // Creates the GasData table
                String GasData = "GasData";
                writer.DropTable(con, GasData);
                String[] GasColumnsDef = { "NAME		VARCHAR(30)",
	                                       "DEMAND  	INT",
	                                       "PRICE		DECIMAL(6,2)",
	                                       "OCTANE		DECIMAL(6,2)",
	                                       "LEAD		DECIMAL(6,2)"};
                writer.CreateTable(con, GasColumnsDef, GasData);
                object[][] GasValues = {
	                new object[] {"Super", 3000, 70, 10, 1},
                    new object[] {"Regular", 2000, 60, 8, 2},
	                new object[] {"Diesel", 1000, 50, 6, 1}                                  
                };
                insertCommand = @"INSERT INTO GasData(NAME, DEMAND, PRICE, OCTANE, LEAD)
                                         VALUES (@v0, @v1, @v2, @v3, @v4)";
                WriteTableData(con, insertCommand, GasValues);
            }
        } // CreateDefaultSampleDatabase
    }
}


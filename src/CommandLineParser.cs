using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CustomDataSourceSample
{
    /// <summary>
    /// A simple command line parser
    /// </summary>
    class CommandLineParser
    {

        public string externalDataName { set; get; }
        public string modelFileName { set; get; }
        public string configurationFileName { set; get; }
        public ArrayList dataFileNames { set; get; }
        public bool CreateSampleDatabase { set; get; }

        /// <summary>
        /// Creates a new parser
        /// </summary>
        public CommandLineParser()
        {
            externalDataName = null;
            modelFileName = null;
            configurationFileName = null;
            dataFileNames = new ArrayList();
            CreateSampleDatabase = false;
        }

        /// <summary>
        /// Print usage of the program then exit.
        /// </summary>
        void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("CustomDataSourceSample [options] model-file [data-file ...]");
            Console.WriteLine("  options ");
            Console.WriteLine("    -h                        this help message");
            Console.WriteLine("    -export [dat-file]        write external data");
            Console.WriteLine("    -properties [prop-file]   use given properties");
            Console.WriteLine("    -create_sample            create sample database");
            Console.WriteLine();
            Environment.Exit(0);
        }

        /// <summary>
        /// Parse the arguments
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public void Parse(String[] args)
        {
            int i = 0;
            for (i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h")
                    PrintUsage();
                else if (args[i] == "-create_sample")
                {
                    CreateSampleDatabase = true;
                }
                else if (args[i] == "-export")
                {
                    i++;
                    externalDataName = args[i];
                }
                else if (args[i] == "-configuration" || args[i] == "-properties")
                {
                    i++;
                    configurationFileName = args[i];
                }
                else
                {
                    break;
                }
            }
            if (i < args.Length)
            {
                modelFileName = args[i];
                for (i++; i < args.Length; i++)
                {
                    dataFileNames.Add(args[i]);
                }
            }
        }
    }
}

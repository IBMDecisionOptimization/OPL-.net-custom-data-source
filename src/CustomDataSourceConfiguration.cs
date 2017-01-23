using System;
using System.Collections.Generic;
using System.Xml;

namespace CustomDataSourceSample
{
    /// <summary>
    /// This class stores the configuration parameters for the SqlCustomDataSource.
    /// </summary>
    public class CustomDataSourceConfiguration
    {
        /// <summary>
        /// The driver to use.
        /// </summary>
        public String Driver { set; get; }

        /// <summary>
        /// The connection string.
        /// </summary>
        public String Url { get; set; }

        /// <summary>
        /// Maps input data set to SQL query to read the table.
        /// </summary>
        public Dictionary<String, String> ReadQueries { get; set; }

        /// <summary>
        /// Maps output data set name to table name.
        /// </summary>
        public Dictionary<String, String> WriteTables { get; set; }

        public CustomDataSourceConfiguration(String filename)
        {
            this.Url = null;
            this.ReadQueries = new Dictionary<string, string>();
            this.WriteTables = new Dictionary<string, string>();
            this.ReadFromXML(filename);
        }

        public void ReadFromXML(String filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNode root = doc.DocumentElement;

            XmlNode urlNode = root.SelectSingleNode("//datasource/url");
            String url = urlNode.LastChild.InnerText;
            this.Url = url;

            XmlNode driverNode = root.SelectSingleNode("//datasource/driver");
            if (driverNode != null)
            {
                Driver = driverNode.LastChild.InnerText;
            }

            // Read queries
            XmlNodeList readQueries = root.SelectNodes("//datasource/read/query");
            foreach (XmlNode readQuery in readQueries)
            {
                String name = readQuery.Attributes["name"].Value;
                String q = readQuery.LastChild.InnerText;
                this.ReadQueries[name] = q;
            }

            // Write table mapping
            XmlNodeList writeTables = root.SelectNodes("//datasource/write/table");
            foreach (XmlNode writeTable in writeTables)
            {
                String name = writeTable.Attributes["name"].Value;
                String target = writeTable.Attributes["target"].Value;
                this.WriteTables[name] = target;
            }
        }

    }
}

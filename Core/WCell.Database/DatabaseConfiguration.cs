using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace WCell.Database
{
	//TODO: Is this crap even needed anymore?
    /*public static class DatabaseConfiguration
    {
        private static Dictionary<string, string> s_configurationMappings;

        static DatabaseConfiguration()
        {
            s_configurationMappings = new Dictionary<string, string>();

            s_configurationMappings.Add("mssql", "Configurations.SQLServerConfiguration.arconfig");
			s_configurationMappings.Add("mssql2005", "Configurations.SQLServer2005Configuration.arconfig");
			s_configurationMappings.Add("mysql", "Configurations.MySQLConfiguration.arconfig");
			s_configurationMappings.Add("mysql5", "Configurations.MySQL5Configuration.arconfig");
			s_configurationMappings.Add("oracle", "Configurations.OracleConfiguration.arconfig");
			s_configurationMappings.Add("pgsql", "Configurations.PostgreSQLServerConfiguration.arconfig");
			s_configurationMappings.Add("db2", "Configurations.DB2Configuration.arconfig");
			s_configurationMappings.Add("firebird", "Configurations.FireBirdConfiguration.arconfig");
			s_configurationMappings.Add("sqlLite", "Configurations.SQLLiteConfiguration.arconfig");
		}

        public static TextReader GetARConfiguration(string dbType, string connString)
        {
            var dbTypeLowerCase = dbType.ToLower();
            if (!s_configurationMappings.ContainsKey(dbTypeLowerCase))
            {
                return null;
            }

            Stream configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                typeof(DatabaseConfiguration), s_configurationMappings[dbTypeLowerCase]);

            if(configStream == null)
            {
                return null;
            }
            
            StreamReader rdr = new StreamReader(configStream);

            string config = rdr.ReadToEnd();

            // Workaround for:
            // MySQL's unique "feature" of allowing invalid dates in a DATE field,
            // especially using 0000-00-00 as a default value for DATE NOT NULL columns.
            // When such a date is encountered, it throws an exception when converting itself to a DateTime
            if ((dbTypeLowerCase == "mysql" || dbTypeLowerCase == "mysql5") && !connString.ToLower().Contains("convert zero datetime"))
                connString += "Convert Zero DateTime=true;";

            config = config.Replace("{0}", connString);

            return new StringReader(config);
        }
    }*/
}
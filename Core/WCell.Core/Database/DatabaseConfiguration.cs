using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace WCell.Core.Database
{
    public static class DatabaseConfiguration
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
            if (!s_configurationMappings.ContainsKey(dbType.ToLower()))
            {
                return null;
            }

            Stream configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                typeof(DatabaseConfiguration), s_configurationMappings[dbType.ToLower()]);

            if(configStream == null)
            {
                return null;
            }
            
            StreamReader rdr = new StreamReader(configStream);

            string config = rdr.ReadToEnd();

            config = config.Replace("{0}", connString);

            return new StringReader(config);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DispenserManagement.Model
{
    public class PostgresUtility
    {
        public static string GetConnectionString(string key)
        {
            // Defines the sources of configuration information for the 
            // application.
            var builder = new ConfigurationBuilder().AddJsonFile("ConnString.json");
            // Create the configuration object that the application will
            // use to retrieve configuration information.
            var configuration = builder.Build();
            // Retrieve the configuration information.
            var configValue = configuration[key];
            return configValue;
        }
    }
}

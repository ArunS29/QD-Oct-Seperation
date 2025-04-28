using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace qd.utilities
{
    public class ConfigurationHelper
    {
        private readonly IConfiguration _localConfiguration;
        private readonly IConfiguration _azureConfiguration;

        /// <summary>
        /// Initializes the ConfigurationHelper with local and Azure configurations.
        /// </summary>
        /// <param name="localConfiguration">Configuration loaded from appsettings.json.</param>
        /// <param name="azureConnectionString">Connection string for Azure App Configuration.</param>
        public ConfigurationHelper(IConfiguration localConfiguration, string azureConnectionString)
        {
            _localConfiguration = localConfiguration;

            if (!string.IsNullOrEmpty(azureConnectionString))
            {
                var builder = new ConfigurationBuilder();
                builder.AddAzureAppConfiguration(azureConnectionString);
                _azureConfiguration = builder.Build();
            }
        }

        /// <summary>
        /// Gets the value of a configuration key. Checks local configuration first and then Azure App Configuration.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configuration value.</returns>
        public string GetConfigurationValue(string key)
        {
            // Check the value in local configuration first.
            var value = _localConfiguration[key];
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Check the value in Azure App Configuration if it's not in local.
            if (_azureConfiguration != null)
            {
                value = _azureConfiguration[key];
            }

            return value;
        }
    }
}

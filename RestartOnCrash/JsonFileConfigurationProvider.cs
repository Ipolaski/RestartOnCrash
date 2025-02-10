using Newtonsoft.Json;

using System;
using System.IO;
using System.Linq;

namespace RestartOnCrash;

public class JsonFileConfigurationProvider
{
    private readonly string _configurationFilePath;

    public JsonFileConfigurationProvider(string configurationFilePath)
    {
        _configurationFilePath = configurationFilePath;
    }

    /// <summary>
    /// Get configuration from configuration.json.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Configuration Get()
    {
        if (!File.Exists(_configurationFilePath))
            throw new Exception($"{_configurationFilePath} not found near this application executable");

        using ( StreamReader reader = new( _configurationFilePath ) )
        {
            string configurationRaw = reader.ReadToEnd();
            reader.Close();
            Configuration configuration = JsonConvert.DeserializeObject<Configuration>(configurationRaw)!;
            if (configuration.PathToApplicationsToMonitor == null)
                return configuration;

            if (configuration.PathToApplicationsToMonitor.Length == 0)
                return configuration;

            var existingPaths = configuration
                .PathToApplicationsToMonitor
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Where(path => File.Exists(Path.GetFullPath(path)))
                .Select(Path.GetFullPath)
                .ToArray();

            return configuration with
            {
                PathToApplicationsToMonitor = existingPaths
            };
        }
    }
}
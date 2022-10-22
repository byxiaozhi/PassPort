using PassPort.Models;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace PassPort.Utilities
{
    public static class ConfigLoader
    {
        public static Config LoadConfig(string filePath)
        {
            using var configReader = File.OpenText(filePath);
            var deserializer = CreateDeserializer();
            var rawConfig = deserializer.Deserialize<RawConfig>(configReader);
            var config = new Config();
            LoadBasicConfig(rawConfig, config);
            LoadGraphConfig(rawConfig, config);
            return config;
        }

        private static void LoadBasicConfig(RawConfig rawConfig, Config config)
        {
            config.LogFile = rawConfig.LogFile;
        }

        private static void LoadGraphConfig(RawConfig rawConfig, Config config)
        {
            if (rawConfig.Graph is null)
                throw new ArgumentException($"Config \"graph\" is missing.");
            foreach (var (name, properties) in rawConfig.Graph)
                config.Graph.Add(name, NodeFactory.CreateNode(new(properties)));
        }

        private static IDeserializer CreateDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
        }

        private class RawConfig
        {
            public string? LogFile { get; set; }

            public Dictionary<string, Dictionary<string, string>>? Graph { get; set; }
        };
    }
}

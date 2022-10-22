using PassPort.Models;
using YamlDotNet.Serialization;

namespace PassPort.Utilities
{
    public static class ConfigLoader
    {
        public static Config LoadConfig(string filePath)
        {
            using var configReader = File.OpenText(filePath);
            var deserializer = CreateDeserializer();
            var config = deserializer.Deserialize(configReader) as dynamic;
            var logFile = config?["log_file"] as string;
            var chains = LoadChainsConfig(config?["chains"]);
            return new(logFile, chains);
        }

        private static IReadOnlyDictionary<string, Node> LoadChainsConfig(object chainsConfig)
        {
            if (chainsConfig is not Dictionary<object, object> chainsConfigDic)
                throw new ArgumentException(null, nameof(chainsConfig));
            var chains = new Dictionary<string, Node>();
            foreach (var (name, value) in chainsConfigDic)
            {
                if (value is not Dictionary<object, object> objDic)
                    throw new ArgumentException(null, nameof(chainsConfig));
                var properties = objDic.ToDictionary(x => (string)x.Key, x => x.Value);
                chains.Add((string)name, NodeFactory.CreateNode((string)name, new(properties)));
            }
            return chains;
        }

        private static IDeserializer CreateDeserializer()
        {
            return new DeserializerBuilder().Build();
        }
    }
}

using PassPort.Interfaces;
using PassPort.Models;
using PassPort.Modules;
using System.Collections.ObjectModel;

namespace PassPort.Utilities
{
    public static class NodeFactory
    {
        public static Node CreateNode(string name, ReadOnlyDictionary<string, object> properties)
        {
            if (!properties.TryGetValue("module", out var module))
                throw new ArgumentException("Property \"module\" is missing.");
            if (module is not string)
                throw new ArgumentException("Property \"module\" is not a string.");
            return new Node(name, CreateModule((string)module), properties);
        }

        private static IModule CreateModule(string module)
        {
            return module switch
            {
                "TCPClient" => new TCPClient(),
                "TCPServer" => new TCPServer(),
                "RequestHostReplace" => new RequestHostReplace(),
                _ => throw new ArgumentOutOfRangeException($"Module \"${module}\" not found."),
            };
        }
    }
}

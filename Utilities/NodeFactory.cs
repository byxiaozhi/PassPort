using PassPort.Interfaces;
using PassPort.Models;
using PassPort.Modules;
using System.Collections.ObjectModel;

namespace PassPort.Utilities
{
    public static class NodeFactory
    {
        public static Node CreateNode(ReadOnlyDictionary<string, string> properties)
        {
            if (!properties.TryGetValue("module", out var module))
                throw new ArgumentException("Property \"module\" is missing.");
            return new Node(CreateModule(module), properties);
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

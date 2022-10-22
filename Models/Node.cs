using PassPort.Interfaces;
using System.Collections.ObjectModel;

namespace PassPort.Models
{
    public class Node
    {
        public string Name { get; }

        public IModule Module { get; }

        public IReadOnlyDictionary<string, object> Properties { get; }

        public Node(string name, IModule module, ReadOnlyDictionary<string, object> properties)
        {
            Name = name;
            Module = module;
            Properties = properties;
        }
    }
}

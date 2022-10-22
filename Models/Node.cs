using PassPort.Interfaces;
using System.Collections.ObjectModel;

namespace PassPort.Models
{
    public class Node
    {
        public IModule Module { get; }

        public IReadOnlyDictionary<string, string> Properties { get; }

        public Node(IModule module, ReadOnlyDictionary<string, string> properties)
        {
            Module = module;
            Properties = properties;
        }
    }
}

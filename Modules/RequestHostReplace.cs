using PassPort.Interfaces;
using PassPort.Models;
using PassPort.Services;
using System.Text;

namespace PassPort.Modules
{
    public class RequestHostReplace : IModule
    {
        private Node? node;

        private Node? next;

        public void Initialize(Node node)
        {
            this.node = node;
            VerifyProperties();
            next = Program.Config!.Graph[node!.Properties["next"]];
        }

        private void VerifyProperties()
        {
            if (!node!.Properties.ContainsKey("host"))
                throw new ArgumentException("RequestHostReplace module property \"host\" is missing.");
            if (!node!.Properties.ContainsKey("next"))
                throw new ArgumentException("RequestHostReplace module property \"next\" is missing.");
        }

        public Task BackwardAsync(Context ctx) => ctx.BackwardAsync(ctx.Data);

        public async Task ForwardAsync(Context ctx)
        {
            if (ctx.Data != null && ctx.Data.Length > 4 && Encoding.ASCII.GetString(ctx.Data[^4..]) == "\r\n\r\n")
            {
                var header = Encoding.UTF8.GetString(ctx.Data).Split("\r\n").ToList();
                for (int i = 0; i < header.Count; i++)
                    if (header[i].StartsWith("Host: "))
                        header[i] = $"Host: {node!.Properties["host"]}";
                var modified = Encoding.UTF8.GetBytes(string.Join("\r\n", header));
                await ctx.ForwardAsync(next!, modified);
            }
            else
            {
                await ctx.ForwardAsync(next!, ctx.Data);
            }
        }
    }
}

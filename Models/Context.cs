namespace PassPort.Models
{
    public class Context
    {
        public byte[]? Data { get; }

        public Dictionary<string, object> Session { get; }

        public IReadOnlyList<Node> Path { get; }

        private Context(IReadOnlyList<Node> path, byte[]? data, Dictionary<string, object> session)
        {
            Path = path;
            Data = data;
            Session = session;
        }

        public static Context Create(Node node)
        {
            return new Context(new List<Node>() { node }, Array.Empty<byte>(), new());
        }

        public async Task ForwardAsync(Node next, byte[]? data = null)
        {
            var stack = Path.Append(next).ToList();
            var ctx = new Context(stack, data, Session);
            try
            {
                await next.Module.ForwardAsync(ctx);
            }
            catch (NotImplementedException)
            {
                next.Module.Forward(ctx);
            }
        }

        public async Task BackwardAsync(byte[]? data = null)
        {
            var stack = Path.Take(Path.Count - 1).ToList();
            var ctx = new Context(stack, data, Session);
            var next = stack.Last();
            try
            {
                await next.Module.BackwardAsync(ctx);
            }
            catch (NotImplementedException)
            {
                next.Module.Backward(ctx);
            }
        }
    }
}

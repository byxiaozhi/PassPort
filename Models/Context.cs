namespace PassPort.Models
{
    public class Context
    {
        public byte[]? Data { get; private set; }

        public Dictionary<string, object> Session { get; }

        public IReadOnlyList<Node> Stack { get; }

        private Context(IReadOnlyList<Node> stack, byte[]? data, Dictionary<string, object> session)
        {
            Stack = stack;
            Data = data;
            Session = session;
        }

        public static Context Create(Node node)
        {
            return new Context(new List<Node>() { node }, Array.Empty<byte>(), new());
        }

        public async Task ForwardAsync(Node next, byte[]? data = null)
        {
            var stack = Stack.Append(next).ToList();
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
            var stack = Stack.Take(Stack.Count - 1).ToList();
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

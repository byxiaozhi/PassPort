using PassPort.Interfaces;
using PassPort.Models;
using PassPort.Services;
using System.Net;
using System.Net.Sockets;

namespace PassPort.Modules
{
    public class TCPServer : IModule
    {
        private Node? node;

        private Node? next;

        public void Initialize(Node node)
        {
            this.node = node;
            VerifyProperties();

            next = Program.Config!.Graph[node!.Properties["next"]];
            Listen();

            Console.WriteLine("TCPServer module is running.");
        }

        private void VerifyProperties()
        {
            if (!node!.Properties.ContainsKey("address"))
                throw new ArgumentException("TCPServer module property \"address\" is missing.");
            if (!node!.Properties.ContainsKey("port"))
                throw new ArgumentException("TCPServer module property \"port\" is missing.");
            if (!node!.Properties.ContainsKey("next"))
                throw new ArgumentException("TCPServer module property \"next\" is missing.");
        }

        private void Listen()
        {
            var address = IPAddress.Parse(node!.Properties["address"]);
            var port = int.Parse(node!.Properties["port"]);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(address, port);
            serverSocket.Bind(endPoint);
            serverSocket.Listen();
            AcceptLoop(serverSocket);
        }

        private async void AcceptLoop(Socket socket)
        {
            while (true)
            {
                var client = await socket.AcceptAsync();
                // Console.WriteLine($"Connected: {client.RemoteEndPoint} -> {client.LocalEndPoint}");
                ReceiveLoop(client);
            }
        }

        private async void ReceiveLoop(Socket socket)
        {
            await OnInboundConnectedAsync(socket);
            var buffer = new byte[8192];
            while (socket.Connected)
            {
                try
                {
                    var length = await socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (length <= 0)
                        throw new SocketException();
                    // Console.WriteLine($"Received: {socket.RemoteEndPoint} -> {socket.LocalEndPoint}");
                    await OnInboundReceivedAsync(socket, buffer[..length]);
                }
                catch
                {
                    if (socket.Connected)
                        socket.Disconnect(false);
                    break;
                }
            }
            // Console.WriteLine($"Disconnected: {socket.RemoteEndPoint} -> {socket.LocalEndPoint}");
            await OnInboundDisconnectAsync(socket);
        }

        public async Task BackwardAsync(Context ctx)
        {
            if (ctx.Session["inbound_socket"] is Socket socket)
            {
                switch (ctx.Session["outbound_action"] as string)
                {
                    case "received":
                        await OnOutboundReceivedAsync(socket, ctx.Data!);
                        // Console.WriteLine($"Sent: {socket.RemoteEndPoint} <- {socket.LocalEndPoint}");
                        break;
                    case "disconnected":
                        socket.Disconnect(false);
                        break;
                }
            }
        }

        private async Task OnInboundConnectedAsync(Socket socket)
        {
            var ctx = Context.Create(node!);
            ctx.Session["inbound_socket"] = socket;
            ctx.Session["inbound_action"] = "connected";
            await ctx.ForwardAsync(next!);
        }

        private async Task OnInboundReceivedAsync(Socket socket, byte[] data)
        {

            var ctx = Context.Create(node!);
            ctx.Session["inbound_socket"] = socket;
            ctx.Session["inbound_action"] = "received";
            await ctx.ForwardAsync(next!, data);
        }

        private async Task OnInboundDisconnectAsync(Socket socket)
        {
            var ctx = Context.Create(node!);
            ctx.Session["inbound_socket"] = socket;
            ctx.Session["inbound_action"] = "disconnected";
            await ctx.ForwardAsync(next!);
        }

        private async Task OnOutboundReceivedAsync(Socket socket, byte[] data)
        {
            try
            {
                await socket.SendAsync(data, SocketFlags.None);
            }
            catch { }
        }
    }
}

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
                Console.WriteLine($"Connected: {client.RemoteEndPoint} -> {client.LocalEndPoint}");
                ReceiveLoop(client);
            }
        }

        private async void ReceiveLoop(Socket socket)
        {
            await OnConnectedAsync(socket);
            var buffer = new byte[8192];
            while (socket.Connected)
            {
                var length = await socket.ReceiveAsync(buffer, SocketFlags.None);
                if (length > 0)
                {
                    Console.WriteLine($"Received: {socket.RemoteEndPoint} -> {socket.LocalEndPoint}");
                    await OnReceivedAsync(socket, buffer[..length]);
                }
            }
            Console.WriteLine($"Disconnected: {socket.RemoteEndPoint} -> {socket.LocalEndPoint}");
            await OnDisconnectedAsync(socket);
        }

        private async Task OnConnectedAsync(Socket socket)
        {
            var ctx = Context.Create(node!);
            ctx.Session.Add("inbound_socket", socket);
            ctx.Session.Add("inbound_action", "connected");
            await ctx.ForwardAsync(next!);
        }

        private async Task OnReceivedAsync(Socket socket, byte[] data)
        {

            var ctx = Context.Create(node!);
            ctx.Session.Add("inbound_socket", socket);
            ctx.Session.Add("inbound_action", "received");
            await ctx.ForwardAsync(next!, data);
        }

        private async Task OnDisconnectedAsync(Socket socket)
        {
            var ctx = Context.Create(node!);
            ctx.Session.Add("inbound_socket", socket);
            ctx.Session.Add("inbound_action", "disconnected");
            await ctx.ForwardAsync(next!);
        }

        public async Task BackwardAsync(Context ctx)
        {
            if (ctx.Session["inbound_socket"] is Socket socket)
            {
                await socket.SendAsync(ctx.Data!, SocketFlags.None);
                Console.WriteLine($"Sent: {socket.RemoteEndPoint} <- {socket.LocalEndPoint}");
            }
        }
    }
}

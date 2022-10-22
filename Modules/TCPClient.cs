using PassPort.Interfaces;
using PassPort.Models;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace PassPort.Modules
{
    public class TCPClient : IModule
    {
        private Node? node;

        private static readonly ConditionalWeakTable<Socket, Socket> inboundToOutBountSocketTable = new();

        public void Initialize(Node node)
        {
            this.node = node;
            VerifyProperties();
        }

        private void VerifyProperties()
        {
            if (!node!.Properties.ContainsKey("address"))
                throw new ArgumentException("TCPClient module property \"address\" is missing.");
            if (!node!.Properties.ContainsKey("port"))
                throw new ArgumentException("TCPClient module property \"port\" is missing.");
        }

        public async Task ForwardAsync(Context ctx)
        {
            if (ctx.Session["inbound_socket"] is Socket socket)
            {
                switch (ctx.Session["inbound_action"] as string)
                {
                    case "connected":
                        await OnInboundConnectedAsync(socket, ctx);
                        break;
                    case "received":
                        await OnInboundReceivedAsync(socket, ctx.Data!);
                        break;
                    case "disconnected":
                        OnInboundDisconnect(socket);
                        break;
                }
            }
        }

        private async Task OnInboundConnectedAsync(Socket socket, Context ctx)
        {
            var address = IPAddress.Parse(node!.Properties["address"]);
            var port = int.Parse(node!.Properties["port"]);
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await clientSocket.ConnectAsync(address, port);
            Console.WriteLine($"Connected: {clientSocket.LocalEndPoint} -> {clientSocket.RemoteEndPoint}");
            inboundToOutBountSocketTable.Add(socket, clientSocket);
            OutBoundReceiveLoop(clientSocket, ctx);
        }

        private async void OutBoundReceiveLoop(Socket socket, Context ctx)
        {
            var buffer = new byte[8192];
            while (socket.Connected)
            {
                var length = await socket.ReceiveAsync(buffer, SocketFlags.None);
                if (length > 0)
                {
                    Console.WriteLine($"Received: {socket.LocalEndPoint} <- {socket.RemoteEndPoint}");
                    await ctx.BackwardAsync(buffer[..length]);
                }
            }
            Console.WriteLine($"Disconnected: {socket.LocalEndPoint} -> {socket.RemoteEndPoint}");
            inboundToOutBountSocketTable.Remove(socket);
        }

        private async Task OnInboundReceivedAsync(Socket socket, byte[] data)
        {
            if (inboundToOutBountSocketTable.TryGetValue(socket, out var clientSocket))
            {
                await clientSocket.SendAsync(data, SocketFlags.None);
                Console.WriteLine($"Sent: {clientSocket.LocalEndPoint} -> {clientSocket.RemoteEndPoint}");
            }
        }

        private void OnInboundDisconnect(Socket socket)
        {
            if (inboundToOutBountSocketTable.TryGetValue(socket, out var clientSocket))
            {
                clientSocket.Disconnect(false);
            }
        }
    }
}

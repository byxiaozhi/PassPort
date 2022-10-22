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

        private static readonly ConditionalWeakTable<Socket, Socket> in2OutSocketTable = new();

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
            var address = IPAddress.Parse((string)node!.Properties["address"]);
            var port = int.Parse((string)node!.Properties["port"]);
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await clientSocket.ConnectAsync(address, port);
            // Console.WriteLine($"Connected: {clientSocket.LocalEndPoint} -> {clientSocket.RemoteEndPoint}");
            await OnOutboundConnectedAsync(clientSocket, ctx);
            in2OutSocketTable.Add(socket, clientSocket);
            OutBoundReceiveLoop(clientSocket, ctx);
        }

        private async Task OnInboundReceivedAsync(Socket socket, byte[] data)
        {
            if (in2OutSocketTable.TryGetValue(socket, out var clientSocket))
            {
                await clientSocket.SendAsync(data, SocketFlags.None);
                // Console.WriteLine($"Sent: {clientSocket.LocalEndPoint} -> {clientSocket.RemoteEndPoint}");
            }
        }

        private void OnInboundDisconnect(Socket socket)
        {
            if (in2OutSocketTable.TryGetValue(socket, out var clientSocket))
            {
                clientSocket.Disconnect(false);
            }
        }

        private async void OutBoundReceiveLoop(Socket socket, Context ctx)
        {
            var buffer = new byte[8192];
            while (socket.Connected)
            {
                try
                {
                    var length = await socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (length <= 0)
                        throw new SocketException();
                    // Console.WriteLine($"Received: {socket.LocalEndPoint} <- {socket.RemoteEndPoint}");
                    await OnOutboundReceivedAsync(socket, ctx, buffer[..length]);
                }
                catch
                {
                    try
                    {
                        socket.Disconnect(false);
                    }
                    catch { }
                    break;
                }
            }
            // Console.WriteLine($"Disconnected: {socket.LocalEndPoint} -> {socket.RemoteEndPoint}");
            await OnOutboundDisconnectedAsync(socket, ctx);
            in2OutSocketTable.Remove(socket);
        }

        private async Task OnOutboundConnectedAsync(Socket socket, Context ctx)
        {
            ctx.Session["outbound_socket"] = socket;
            ctx.Session["outbound_action"] = "connected";
            await ctx.BackwardAsync();
        }

        private async Task OnOutboundReceivedAsync(Socket socket, Context ctx, byte[] data)
        {
            ctx.Session["outbound_socket"] = socket;
            ctx.Session["outbound_action"] = "received";
            await ctx.BackwardAsync(data);
        }

        private async Task OnOutboundDisconnectedAsync(Socket socket, Context ctx)
        {
            ctx.Session["outbound_socket"] = socket;
            ctx.Session["outbound_action"] = "disconnected";
            await ctx.BackwardAsync();
        }
    }
}

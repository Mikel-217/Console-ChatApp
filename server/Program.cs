using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server;

public class Clients {
    public int ID { get; set; }
    public string? Username { get; set; }
}

public class PMAin {

    public static async Task Main(string[] args) {
        MainServer main = new MainServer();
        await main.serverStart();
    }
}

public class MainServer {
    List<Clients> clients = new List<Clients>();
    List<Socket> clientSockets = new List<Socket>();

    public async Task serverStart() {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipaddr = host.AddressList[0];
        IPEndPoint localEndpoint = new IPEndPoint(ipaddr, 1300);
        int clientID = 0;

        try {
            Socket listener = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndpoint);
            listener.Listen(10);

            Console.WriteLine($"Waiting for connections on {ipaddr}:{localEndpoint.Port}");

            while (true) {
                Socket client = await listener.AcceptAsync();
                clientSockets.Add(client);
                Console.WriteLine("A new client has connected.");
                
                string message = "Please enter Username:";
                await sendData(message, client);

                string _user = await receiveData(client);
                if (string.IsNullOrWhiteSpace(_user)) {
                    Console.WriteLine("Invalid username. Closing connection.");
                    client.Close();
                    clientSockets.Remove(client);
                    continue;
                }

                message = "Username Registered. You can send Messages.";
                await sendData(message, client);

                clientID++;
                Clients newClient = new Clients {
                    ID = clientID,
                    Username = _user
                };
                clients.Add(newClient);
                message = $"{clientID}";
                Console.WriteLine(message);
                await sendData(message, client);

                _ = handleClient(client);
            }
        } catch (SocketException e) {
            Console.WriteLine(e.Message);
        }
    }

    public async Task handleClient(Socket client) {
        try {
            while (client.Connected) {
                string receivedMessage = await receiveData(client);

                if (string.IsNullOrWhiteSpace(receivedMessage)) {
                    Console.WriteLine("Empty message received. Ignoring.");
                    continue;
                }
                Console.WriteLine(receivedMessage);
                string[] parts = receivedMessage.Split(':', 2);
                Console.WriteLine($"{parts[0]} {parts[1]})");
                if (parts.Length < 2 || !int.TryParse(parts[0], out int clientID)) {
                    Console.WriteLine("Invalid message format. Expected 'clientID:message'.");
                    continue;
                }

                string data = parts[1];
                await sendBroadcast(data, clientID);
            }
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        } finally {
            lock (clientSockets) {
                clientSockets.Remove(client);
            }
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }

    public async Task sendBroadcast(string data, int clientID) {
        Clients? username = clients.FirstOrDefault(client => client.ID == clientID);
        if (username == null) {
            Console.WriteLine($"Client with ID {clientID} not found.");
            return;
        }

        string sendingUser = username.Username ?? "Unknown";
        string message = $"{sendingUser}: {data}";
        Console.WriteLine($"Broadcasting message: {message}");

        byte[] sendingData = Encoding.UTF8.GetBytes(message);
        lock (clientSockets) {
            foreach (var client in clientSockets) {
                if (client.Connected) {
                    try {
                        client.Send(sendingData);
                    } catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }

    public async Task sendData(string _message, Socket client) {
        byte[] bytes = Encoding.UTF8.GetBytes(_message);
        await client.SendAsync(bytes, SocketFlags.None);
    }

    public async Task<string> receiveData(Socket client) {
        byte[] buffer = new byte[1024];
        int bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);

        if (bytesRead == 0) {
            throw new Exception("Client disconnected.");
        }
        return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
    }
}

using System.Data.Common;
using System.Dynamic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;


namespace Server;

public class Clients {
    public int ID { get; set; }
    public string? Username { get; set; }

}


public class PMAin {

    public static void Main(string[] args) {
        MainServer main = new MainServer();
        main.serverStart();       
    }
}

public class MainServer {
    List<Clients> clients = new List<Clients>();
    List<Socket> clientSockets = new List<Socket>();

    public void serverStart() {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipaddr = host.AddressList[0];
        IPEndPoint localEndpoint = new IPEndPoint(ipaddr, 1300);
        int clientID = 0;

        try {
            Socket listener = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndpoint);
            listener.Listen(10);
            Console.WriteLine($"Waiting for Connection on {ipaddr}:{localEndpoint}");
            while (true) {
                Socket client = listener.Accept();
                clientSockets.Add(client);

                byte[] bytes;
                bytes = new byte[1024];
                string _message = "Pls enter Username: ";
                bytes = Encoding.UTF8.GetBytes(_message, 0, _message.Length);
                client.Send(bytes);
                client.Receive(bytes);
                string _user = Encoding.UTF8.GetString(bytes);
                Console.WriteLine(_user);
                _message = "Username Registrerd. You can send Messages";
                bytes = Encoding.UTF8.GetBytes(_message);
                client.Send(bytes);
                clientID++;
                string _message2 = Convert.ToString(clientID);
                //Sending USer ID to Client
                bytes = new byte[256];
                bytes = Encoding.UTF8.GetBytes(_message2);
                client.Send(bytes);
                Console.WriteLine(clientID);
                Clients newClient = new Clients {
                    ID = clientID,
                    Username = _user
                };
                clients.Add(newClient);
                
                Thread clientThread = new Thread(() => handleClient(client));
                clientThread.Start();
            }
        } catch(SocketException e) {
            Console.WriteLine(e.Message);
        }
    }
    public void handleClient(Socket client) {
        string? data = null;
        byte[] bytes;
        
        while (client.Connected) {
            bytes = new byte[1024];
            client.Receive(bytes);
            string recivedMessage = Encoding.UTF8.GetString(bytes);
            string[] parts = recivedMessage.Split(':');
            int clientID = int.Parse(parts[0]);
            data = parts[1];

            Console.WriteLine($"{clientID} {data}");
            Thread sending = new Thread(() => sendBroadcast(data, client, clientID));
            sending.Start();
            //Testing purpose:
            // Console.WriteLine($"Message: {data}");
            if(!client.Connected) {
                Console.WriteLine($"Client Disconectet {client.RemoteEndPoint}");
            }
        } 
    }

    public void sendBroadcast(string data, Socket client, int clientID) {
        Clients? username = clients.FirstOrDefault(client => client.ID == clientID);
        string? sendingUser = username!.Username;
        Console.WriteLine(sendingUser);

        foreach (var clients in clientSockets) {
            string _message = sendingUser+":" + data;
            Console.WriteLine(_message);
            byte[] buffer = new byte[1024];
            buffer = Encoding.UTF8.GetBytes(_message);
            client.Send(buffer);
        }
    }
}
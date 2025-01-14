using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using ClientData;

namespace Server;

public class PMAin {

    public static void Main(string[] args) {
        MainServer main = new MainServer();
        main.serverStart();       
    }

}

public class MainServer {
    List<Message> messages = new List<Message>();
    Message _message = new Message();

    public void serverStart() {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipaddr = host.AddressList[0];
        IPEndPoint localEndpoint = new IPEndPoint(ipaddr, 1300);

        try {
            Socket listener = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndpoint);
            listener.Listen(10);
            Console.WriteLine($"Waiting for Connection on {ipaddr}:{localEndpoint}");
            while (true) {
                Socket client = listener.Accept();
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

        
        string _message = "Pls enter Username: ";
        bytes = Encoding.UTF8.GetBytes(_message, 0, _message.Length);
        client.Send(bytes);
        bytes = new byte[256];
        client.Receive(bytes);
        _message = Encoding.UTF8.GetString(bytes);
        Console.WriteLine(_message);
        _message = "Username Registrerd. You can send Messages";
        bytes = Encoding.UTF8.GetBytes(_message);
        client.Send(bytes);
        
        while (client.Connected) {
            bytes = new byte[256];
            client.Receive(bytes);
            data = Encoding.UTF8.GetString(bytes, 0 , bytes.Length);
        
            Console.WriteLine($"Message: {data}");

            if(!client.Connected) {
                Console.WriteLine($"Client Disconectet {client.RemoteEndPoint}");
            }
        } 
    }

}
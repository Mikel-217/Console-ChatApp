using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

public class MainClient {

    public static void Main(string[] args) {
        MainClientConnect main = new MainClientConnect();
        main.ConnectToServer();
    }
}

public class MainClientConnect {
    public void ConnectToServer() {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddr = host.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 1300);

        try {
            Socket tcpClient = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcpClient.Connect(endPoint);

            byte[] buffer = new byte[1024];
            Console.WriteLine($"Connected to {endPoint}");
            int bytesRead = tcpClient.Receive(buffer);
            string _message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            Console.WriteLine(_message);

            // sending username
            string _answerUser = Console.ReadLine()!.Trim();
            buffer = Encoding.UTF8.GetBytes(_answerUser);
            tcpClient.Send(buffer);

            bytesRead = tcpClient.Receive(buffer);
            _message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            Console.WriteLine(_message);

            // reciving ID
            bytesRead = tcpClient.Receive(buffer);
            string _anserID = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            Console.WriteLine($"ID: {_anserID}");
            Thread thread = new Thread(() => receiveMessages(tcpClient));
            thread.Start();

            while (tcpClient.Connected) {
                Console.Write("> ");
                string message = Console.ReadLine()!.Trim();
                if (!string.IsNullOrWhiteSpace(message)) {
                    string _messageSend = $"{_anserID}:{message}";
                    byte[] data = Encoding.UTF8.GetBytes(_messageSend);
                    tcpClient.Send(data);
                }
            }

        } catch (SocketException e) {
            Console.WriteLine(e.Message);
        }
    }

    public void receiveMessages(Socket tcpClient) {
        try {
            byte[] buffer = new byte[1024];
            while (tcpClient.Connected) {
                int bytesRead = tcpClient.Receive(buffer);
                if (bytesRead > 0) {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"\n{message}");
                    Console.Write("> ");
                }
            }
        } catch (SocketException e) {
            Console.WriteLine(e.Message);
        }
    }
}

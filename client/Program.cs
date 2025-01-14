﻿using System.Net;
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

            if (tcpClient.Connected) {

                byte[] buffer = new byte[256];
                Console.WriteLine($"Conected to {endPoint}");
                tcpClient.Receive(buffer);
                string _message = Encoding.UTF8.GetString(buffer);
                Console.WriteLine(_message);
                string _answerUser = Console.ReadLine()!;
                buffer = new byte[256];
                buffer = Encoding.UTF8.GetBytes(_answerUser);
                tcpClient.Send(buffer);
                Array.Clear(buffer, 0, buffer.Length);
                tcpClient.Receive(buffer);
                _message = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Console.WriteLine(_message);
            }
            Thread thread = new Thread(() => reseveMessages(tcpClient));
            thread.Start();
            while (tcpClient.Connected) {
                string message = Console.ReadLine()!;
                byte[] data = Encoding.UTF8.GetBytes(message, 0, message.Length);
                tcpClient.Send(data);
            }

        } catch (SocketException e) {
            Console.WriteLine(e.Message);
        }
    }

    public void reseveMessages(Socket tcpClient) {
        string messageOther;
        byte[] resevedData;

        while(tcpClient.Connected) {
            resevedData = new byte[256];
            tcpClient.Receive(resevedData);
            messageOther = Encoding.UTF8.GetString(resevedData, 0 , resevedData.Length);
            Console.WriteLine(messageOther);
        }
    }
}
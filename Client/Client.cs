using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;

namespace Client
{
    class Client // Wrapper-class, makes it easier to have all data-types in one class
    {
        RSA rsa;
        string address;
        short port;
        TcpClient tcpClient;
        NetworkStream tcpStream;
        BigInteger serverPublicKey;

        public Client (string address, short port, RSA rsa)
        {
            this.address = address;
            this.port = port;
            tcpClient = new TcpClient();
            this.rsa = rsa;
        }

        public Client () { }

        public void Disconnect()
        {
            tcpClient.Close();
            Console.WriteLine("Disconnected.");
        }

        public void ConnectToServer()
        {
            byte[] buffer = new byte[128];
            tcpClient.Connect(address, port);
            tcpStream = tcpClient.GetStream(); // Attaching stream-object to current socket

            Console.WriteLine("Receiving server public key...");
            tcpStream.Read(buffer, 0, buffer.Length); // Get server's public key
            serverPublicKey = new BigInteger(buffer); // Store public key
            buffer.Clear();

            // The client's public key will be sent with two byte-blocks
            byte[] firstHalf = new byte[64], SecondHalf = new byte[64];
            buffer = rsa.Encrypt(rsa.n.ToByteArray(), serverPublicKey); // Encrypts the clients public key

            for (int i = 0; i < 64; ++i) // Dividing the public key into two blocks
            {
                firstHalf[i] = buffer[i];
                SecondHalf[i] = buffer[64 + i];
            }

            Console.WriteLine("Sending public key...");
            tcpStream.Write(firstHalf, 0, firstHalf.Length); // Sending first half of clients public key
            System.Threading.Thread.Sleep(500);
            tcpStream.Write(SecondHalf, 0, SecondHalf.Length); // Sending second half of clients public key

            Console.WriteLine("Connection established!");

        }

        public void SendMessage(string msg)
        {
            byte[] cipherData;
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            cipherData = rsa.Encrypt(buffer, serverPublicKey);

            tcpStream.Write(cipherData, 0, cipherData.Length);

        }

    }
}

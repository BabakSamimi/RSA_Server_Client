using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Numerics;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            RSA rsa;
            string address;
            short port;
            TcpClient tcpClient;
            NetworkStream tcpStream;
            byte[] buffer;
            int msgLength;
            string msg;
            BigInteger serverModulusKey;

            rsa = new RSA(1024);
            
            try
            {
                Thread rsaAsync = new Thread(rsa.GenerateKeys);

                rsaAsync.Start();

                Console.WriteLine("IP-Address to chosen server: ");
                address = Console.ReadLine();
                Console.WriteLine("Port: ");
                while (!Int16.TryParse(Console.ReadLine(), out port))
                    Console.WriteLine("Please choose a port between 1 and 65 535");

                if (rsaAsync.IsAlive) // Wait for keys to be generated before we connect to a server
                {
                    Console.WriteLine("Waiting for encryption to complete...");
                    rsaAsync.Join();
                }

                Console.WriteLine("Connecting...");

                tcpClient = new TcpClient();
                tcpClient.Connect(address, port);
                Console.WriteLine("Connected.");

                tcpStream = tcpClient.GetStream(); // Attach stream object
                buffer = new byte[128];

                Console.WriteLine(rsa.n);

                msgLength = tcpStream.Read(buffer, 0, buffer.Length); // Await server's modulus key
                serverModulusKey = new BigInteger(buffer);
                buffer.Clear();

                byte[] b1 = new byte[64], b2 = new byte[64];

                byte[] half = rsa.n.ToByteArray();

                for (int i = 0; i < 64; ++i)
                {
                    b1[i] = half[i];
                    b2[i] = half[64 + i + 1];

                }

                b1 = rsa.Encrypt(b1, serverModulusKey); // Encrypts the clients public key WITH server's public encryption key
                b2 = rsa.Encrypt(b2, serverModulusKey);
                tcpStream.Write(b1, 0, b1.Length); // Sending clients public key
                tcpStream.Write(b2, 0, b2.Length);

                Console.WriteLine("Key: " + serverModulusKey);


                while (true)
                {
                    msg = "";
                    buffer.Clear();
                    Console.WriteLine("Send message: ");

                    msg = Console.ReadLine();

                    if (msg.Equals("quit")) break;

                    byte[] temp = Encoding.UTF8.GetBytes(msg);
                    buffer = rsa.Encrypt(temp, serverModulusKey); // message is now encrypted
                    tcpStream.Write(buffer, 0, buffer.Length);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Catched error: " + e.Message);
            }

        }
    }
}

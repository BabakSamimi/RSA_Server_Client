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

                Console.WriteLine(rsa.n + "\n");

                msgLength = tcpStream.Read(buffer, 0, buffer.Length); // Await server's modulus key
                serverModulusKey = new BigInteger(buffer);
                buffer.Clear();

                byte[] b1 = new byte[64], b2 = new byte[64];
                byte[] fresh = new byte[128];
                fresh = rsa.Encrypt(rsa.n.ToByteArray(), serverModulusKey); // Encrypts the key

                for (int i = 0; i < 64; ++i)
                {
                    b1[i] = fresh[i];
                    b2[i] = fresh[64 + i];
                }

                tcpStream.Write(b1, 0, b1.Length); // Sending clients public key
                Thread.Sleep(500);
                tcpStream.Write(b2, 0, b2.Length);

                Console.WriteLine("Key: " + serverModulusKey);


                while (true)
                {
                    msg = "";
                    buffer.Clear();
                    fresh.Clear();

                    Console.WriteLine("Send message: ");

                    msg = Console.ReadLine();

                    if (msg.Equals("quit")) break;

                    fresh = Encoding.UTF8.GetBytes(msg);
                    buffer = rsa.Encrypt(fresh, serverModulusKey); // message is now encrypted
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

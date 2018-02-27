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
            string msg;


            Client client;

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

                Console.WriteLine("Pkey: " + rsa.n);

                Console.WriteLine("Connecting...");

                client = new Client(address, port, rsa);
                client.ConnectToServer();

                while (true)
                {
                    msg = "";
                    Console.WriteLine("Send a message to the server: ");
                    msg = Console.ReadLine();

                    if (msg.ToLower().Equals("disconnect"))
                    {
                        Console.WriteLine("Disconnecting from the server...");
                        client.Disconnect();
                        break;
                    }

                    client.SendMessage(msg);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Catched error: " + e.Message);
            }

            Console.WriteLine("End of program... press any key to exit");
            Console.ReadKey();

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Numerics;

namespace RSA_Server
{
    class Program
    {
        static Dictionary<Socket, BigInteger> clientss = new Dictionary<Socket, BigInteger>();

        static void HandleClient(Socket socket, RSA rsa)
        {
            // Handles the client on a different thread, increasing the complexity of the server so we can handle multiplie clients
            new Thread(() =>
            {
                byte[] buffer = new byte[128];
                byte[] data;
                string plaintext;
                int msgLength;

                Console.WriteLine(clientss.GetPublicKey(socket));

                try
                {
                    Console.WriteLine(socket.RemoteEndPoint + " connected.");

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    
                }

                while (true) // Receive & handle client data packets
                {
                    try
                    {
                        msgLength = socket.Receive(buffer);
                        plaintext = "";
                        
                        data  = rsa.Decrypt(buffer, rsa.n); // Decrypt clients message into plaintext
                        plaintext = Encoding.UTF8.GetString(data);
                        /*for (int i = 0; i < msgLength; ++i) // Convert bytes into chars
                        {
                            plaintext += Convert.ToChar(data[i]);
                        }*/

                        Console.WriteLine("Message from client: " + plaintext);

                        if (plaintext.Equals("disconnect"))
                        {
                            Console.WriteLine(socket.RemoteEndPoint + " disconnected from the server.");
                            clientss.Remove(socket);
                            socket.Disconnect(false);
                            break;
                        }

                        buffer.Clear();
                        data.Clear();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error occurred: " + e.Message);
                        break;
                    }
                }
            }).Start();
            
        }

        static void Main(string[] args)
        {

            TcpListener tcpListener = null;
            IPAddress serverIp = null;
            byte[] buffer;
            Socket socket;
            RSA rsa;

            rsa = new RSA(1024);
            rsa.GenerateKeys();

            Console.WriteLine(rsa.n + "\n");

            try
            {
                serverIp = IPAddress.Parse("127.0.0.1");
                Console.WriteLine("Setup complete...");
                tcpListener = new TcpListener(serverIp, 1337);
                tcpListener.Start();
                Console.WriteLine("Awaiting connections...");

            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured: " + e.Message);
            }

            buffer = new byte[128];

            while (true)
            {
                try
                {
                    byte[] firstHalf = new byte[64];
                    byte[] secondHalf = new byte[64];

                    socket = tcpListener.AcceptSocket();
                    socket.Send(rsa.n.ToByteArray()); // Send public key to client.


                    socket.Receive(firstHalf); // Wait for clients public key in an encrypted format - 64x2
                    socket.Receive(secondHalf);

                    for(int i = 0; i < 64; ++i)
                    {
                        buffer[i] = firstHalf[i];
                        buffer[64 + i] = secondHalf[i];
                    }

                    byte[] realData = rsa.Decrypt(buffer, rsa.n); // Decrypt, this byte-array contains clients public key.
                    BigInteger temp = new BigInteger(realData); // Holds the public key


                    clientss.Add(socket, temp);
                    HandleClient(socket, rsa); // Handle the communication with the client on another thread

                    buffer.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine("The error message was: " + e.Message);
                    break;
                }
            }
            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Numerics;
using System.Net;


namespace RSA_Server
{
    class Server
    {
        Dictionary<Socket, BigInteger> clients = new Dictionary<Socket, BigInteger>(); // List of clients, Socket is clients, BigInteger is their public key
        RSA rsa;
        TcpListener tcpListener;
        short port;
        Socket socket;
        IPAddress iPAddress;
        

        public Server(TcpListener tcpListener, string ip, Socket socket, RSA rsa, short port)
        {
            this.rsa = rsa;
            this.tcpListener = tcpListener;
            this.socket = socket;
            this.iPAddress = IPAddress.Parse(ip);
            this.port = port;
        }


        public void StartListening()
        {
            tcpListener = new TcpListener(iPAddress, port);
            tcpListener.Start();

            while (true)
            {
                byte[] firstHalf = new byte[64]; // Key will come in as two 64-byte blocks
                byte[] secondHalf = new byte[64];
                byte[] buffer = new byte[128];

                socket = tcpListener.AcceptSocket(); // Wait for a connection
                Console.WriteLine(socket.RemoteEndPoint + " connected.");
                socket.Send(rsa.n.ToByteArray()); // Send public key to client.


                socket.Receive(firstHalf); // Wait for clients public key in an encrypted format - 64x2
                socket.Receive(secondHalf);

                for (int i = 0; i < 64; ++i)
                {
                    buffer[i] = firstHalf[i];
                    buffer[64 + i] = secondHalf[i];
                }

                byte[] realData = rsa.Decrypt(buffer, rsa.n); // Decrypt. This byte-array contains clients public key.
                BigInteger publicKey = new BigInteger(realData); // Hold the public key

                buffer.Clear();
                firstHalf.Clear();
                secondHalf.Clear();

                clients.Add(socket, publicKey);
                HandleClient(socket);
                //HandleClient(socket, rsa); // Handle the communication with the client on another thread

                
            }
        }


        public void HandleClient(Socket s)
        {
            new Thread(() => // Handles packets on another thread
            {
                byte[] cipherData = new byte[128];
                byte[] data;
                string plaintext;

                while (true)
                {
                    try
                    {
                        plaintext = "";
                        socket.Receive(cipherData); // Wait for data

                        data = rsa.Decrypt(cipherData, clients.GetPublicKey(socket)); // Decrypt data into plaintext
                        plaintext = Encoding.UTF8.GetString(data);

                        Console.WriteLine("Message from client: " + plaintext);

                        if (plaintext.Equals("disconnect"))
                        {
                            Console.WriteLine(socket.RemoteEndPoint + " disconnected from the server.");
                            clients.Remove(socket);
                            socket.Disconnect(false);
                            break;
                        }

                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error caught: " + e.Message);
                    }
                }

            }).Start();
        }

    }
}

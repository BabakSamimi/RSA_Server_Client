using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Numerics;
using System.Net;
using System.Xml;
using System.Diagnostics;
using System.IO;

namespace RSA_Server
{
    class Server
    {
        Dictionary<Socket, BigInteger> clients = new Dictionary<Socket, BigInteger>(); // List of clients, Socket is clients, BigInteger is their public key
        List<Message> messages = new List<Message>(); // Messages from client will be stored in an XML file.
        RSA rsa;
        TcpListener tcpListener;
        short port;
        Socket socket;
        IPAddress iPAddress;
        XmlDocument xmlDocument;
        string fileName;

        public Server(string ip, short port, RSA rsa, string fileName, XmlDocument xmlDocument)
        {
            this.rsa = rsa;
            iPAddress = IPAddress.Parse(ip);
            this.port = port;
            this.fileName = fileName;
            this.xmlDocument = xmlDocument;

        }

        public void DisplayAllMessages()
        {
            Console.Clear();
            if(messages.Count == 0)
            {
                Console.Write("\r\nNo messages to display.\n");
            }
            else
            {
                foreach (Message message in messages)
                {
                    message.Print();
                }
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        private void SaveMessageToXml(Message message)
        {
            XmlElement messagesNode;
            XmlElement messageNode;
            XmlElement cipherNode;
            XmlElement plaintextNode;
            XmlElement dateNode;
            XmlElement ipNode;

            XmlNodeList elements = xmlDocument.SelectNodes("messages");

            if (elements.Count == 0)
            {
                messagesNode = xmlDocument.CreateElement("messages");
                xmlDocument.AppendChild(messagesNode);
            }
            else { messagesNode = (XmlElement)elements.Item(0); }

            messageNode = xmlDocument.CreateElement("message");
            messagesNode.AppendChild(messageNode);

            cipherNode = xmlDocument.CreateElement("ciphertext");
            cipherNode.InnerText = message.Cipher; // Apply encrypted plaintext
            messageNode.AppendChild(cipherNode);

            plaintextNode = xmlDocument.CreateElement("plaintext");
            plaintextNode.InnerText = message.Plaintext; // Apply plaintext
            messageNode.AppendChild(plaintextNode);

            dateNode = xmlDocument.CreateElement("date");
            dateNode.InnerText = message.Date; // Apply Date
            messageNode.AppendChild(dateNode);

            ipNode = xmlDocument.CreateElement("ip");
            ipNode.InnerText = message.IpAddress; // Apply ip
            messageNode.AppendChild(ipNode);

            xmlDocument.Save(fileName);
        }


        public void StartListening()
        {
            tcpListener = new TcpListener(iPAddress, port);
            tcpListener.Start();

            byte[] firstHalf = new byte[64]; // Key will come in as two 64-byte blocks
            byte[] secondHalf = new byte[64];
            byte[] buffer = new byte[128]; // This will contain the whole key

            while (true)
            {
                try
                {
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

                    byte[] plainData = rsa.Decrypt(buffer, rsa.n); // Decrypt. This byte-array contains clients public key.
                    BigInteger publicKey = new BigInteger(plainData); // Hold the public key

                    buffer.Clear();
                    firstHalf.Clear();
                    secondHalf.Clear();

                    clients.Add(socket, publicKey);
                    HandleClient(socket);

                }
                catch(Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                    socket.Disconnect(true);
                }   
                
            }
        }


        public void HandleClient(Socket s)
        {
            byte[] cipherData = new byte[128];
            byte[] data;
            string plaintext = "";
            string cipherText = "";
            Message message;

            new Thread(() => // Handles data packets on a separate thread
            {

                while (true)
                {
                    
                    try
                    {
                        socket.Receive(cipherData); // Blocking method that waits for data

                        plaintext = "";
                        cipherText = "";

                        cipherText = new BigInteger(cipherData).ToString(); // Encrypted text
                        data = rsa.Decrypt(cipherData, rsa.n); // Decrypt data into plaintext
                        plaintext = Encoding.UTF8.GetString(data); // Convert byte[] into string

                        cipherData.Clear();
                        data.Clear();

                        Console.WriteLine("Message from client: " + plaintext);

                        message = new Message(cipherText, plaintext, DateTime.Today.ToLongDateString(), socket.RemoteEndPoint.ToString());
                        messages.Add(message); // Store message in a list
                        SaveMessageToXml(message);

                        if (plaintext.Equals("disconnect"))
                        {

                            break;
                        }

                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error caught: " + e.Message);
                        break;
                    }
                }

                clients.Remove(socket);
                socket.Disconnect(false);
                Console.WriteLine("Dropped " + socket.RemoteEndPoint + " from the server...");

            }).Start();
        }

    }
}

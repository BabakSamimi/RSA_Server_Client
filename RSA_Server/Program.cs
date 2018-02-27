using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Numerics;
using System.Xml;

namespace RSA_Server
{
    class Program
    {

        static void Main(string[] args)
        {

            Server server = null;
            XmlDocument xmlDocument = new XmlDocument();
            RSA rsa;

            try // Try to load the existing XML log file
            {
                xmlDocument.Load("messages.xml");
            }
            catch // If it's missing elements, create them
            {
                if(!(xmlDocument.FirstChild.NodeType == XmlNodeType.XmlDeclaration)) // Check if Xml-declaration is already there
                {
                    XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
                    xmlDocument.AppendChild(xmlDeclaration);
                }
            }

            rsa = new RSA(1024);
            rsa.GenerateKeys();

            try
            {

                server = new Server("127.0.0.1", 1337, rsa, "messages.xml", xmlDocument);
                Console.WriteLine("Awaiting connections...");

            }
            catch (Exception e)
            {
                Console.WriteLine("0. Error occured: " + e.Message);
            }

            new Thread(() => 
            {
                while(true)
                {
                    Console.Write("\r1. Show XML logs\n\r2. Exit\n");
                    ConsoleKeyInfo key = Console.ReadKey();

                    if (key.Key == ConsoleKey.D1) server.DisplayAllMessages();
                    else if(key.Key == ConsoleKey.D2)
                    {
                        Console.WriteLine("Exiting");
                        Environment.Exit(0);
                    }
                }

            }).Start();

            server.StartListening();

            Console.ReadKey();
        }
    }
}

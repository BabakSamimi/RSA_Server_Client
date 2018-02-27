using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA_Server
{
    class Message
    {
        string cipher;
        string plaintext;
        string date;
        string ipAddress;

        public Message(string cipher, string plaintext, string date, string ipAddress)
        {
            this.cipher = cipher;
            this.plaintext = plaintext;
            this.date = date;
            this.ipAddress = ipAddress;
        }

        // Shorthand getters didn't work for some reasons...
        public string Cipher { get { return cipher; } }
        public string Plaintext { get { return plaintext; } }
        public string Date { get { return date; } }
        public string IpAddress { get { return ipAddress; } }

        public void Print()
        {
           Console.WriteLine("\n=================================\n" +
                "The message was received: " + date + "\n" +
                "From the IP address: + " + ipAddress + "\n" +
                "The encrypted message contained: " + cipher + "\n" +
                "The plaintext contained: " + plaintext + "\n=================================\n");
        }

    }
}

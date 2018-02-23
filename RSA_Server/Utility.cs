using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace RSA_Server
{
    public static class Utility
    { 
        public static void Clear(this byte[] b) // Resets an array to 0
        {
            if (!b.Equals(0))
            {
                for (int i = 0; i < b.Length; ++i)
                    b[i] = 0;
            } 

        }

        /*public static void RemoveClient(this Dictionary<Socket, BigInteger> clients, Socket socket) // Removes chosen client from the global client-list
        {
            foreach (KeyValuePair<Socket, BigInteger> client in clients)
            {
                if (socket.Equals(client.Key))
                {
                    clients.Remove(client.Key);
                    break;
                }
            }
        }*/

        public static BigInteger GetPublicKey(this Dictionary<Socket, BigInteger> clientss, Socket socket)
        {
            BigInteger temp = new BigInteger();
            
            foreach (KeyValuePair<Socket, BigInteger> client in clientss) // Gets the public key/modulus 
            {
                if (socket.Equals(client.Key))
                {
                    temp = client.Value;
                    break;
                }
            }

            return temp;

        }
        
    }
}

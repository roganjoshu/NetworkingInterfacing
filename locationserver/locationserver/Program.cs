using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Collections;

namespace locationserver
{
    class Program
    {
        static List<Person> userLocation = new List<Person>();
        static List<Person> temp = new List<Person>();

        static void runServer()
        {
            TcpListener listener; // class that provides methods for listening and accepting incoming connection requests
            Socket connection; //class that provides methods for network coms. Performs data transfer between client and server
            NetworkStream socketStream; //sending data over stream sockets in blocking mode.
            try
            {
                listener = new TcpListener(IPAddress.Any, 43); //instantiate listener and declare any IP on port 43.
                listener.Start(); //start listener with above arguquitments.
                Console.WriteLine(@"Server started listening...");
                while (true) //always listening
                {
                    connection = listener.AcceptSocket(); //returns a socket that sends/receives data
                    socketStream = new NetworkStream(connection); //instantiate new networkstream with socket returned from listener.acceptSocket()
                    Console.WriteLine(@"Connection received.");
                    doRequest(socketStream); //call doRequest method which reads data from console
                    socketStream.Close(); //closes networkStream.
                    connection.Close(); //closes socket.
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }

        }

        static void doRequest(NetworkStream socketStream)
        {
            try
            {
                int i = 0;
                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);
                string line = sr.ReadLine().Trim();
                string[] sections = line.Split(new char[] { ' ' }, 2);
                if(sections.Length == 1)
                {
                    if (userLocation.Count > 1)
                    {
                        foreach (Person p in userLocation)
                        {
                            if (p.name == sections[0])
                            {
                                Console.WriteLine(p.name + " is located in " + p.location);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: no entries found");
                    }
                }
                if(sections.Length == 2)
                {
                    temp.Add(new Person(sections[0], sections[1]));
                    for (int k = 0; k < temp.Count; k++)
                    {
                        if (sections[0] != temp[k].name)
                        {
                            temp[k].location = sections[1];
                        }
                        else
                        {
                            Console.WriteLine("User not located. Creating new user.");
                            Console.WriteLine(@sections[0] + " has been created and location updated to " + sections[1]);
                            Person p = new Person(sections[0], sections[1]);
                            userLocation.Add(p);
                            i++;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Something went wrong.");
            }
        }

        static void Main(string[] args)
        {
            runServer();
        }
    }
}
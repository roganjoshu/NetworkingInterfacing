using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Threading;

namespace locationserver
{
    class Program
    {
        static void runServer()
        {
            TcpListener listener; // class that provides methods for listening and accepting incoming connection requests
            Socket connection; //class that provides methods for network coms. Performs data transfer between client and server          
            try
            {
                listener = new TcpListener(IPAddress.Any, 43); //instantiate listener and declare any IP on port 43.
                listener.Start(); //start listener with above arguquitments.
                Console.WriteLine(@"Server started listening...");
                while (true) //always listening
                {
                    connection = listener.AcceptSocket(); //returns a socket that sends/receives data
                    Class_Handler handler = new Class_Handler();    //instantiate handler class in order to perform concurrent threads 
                    Thread new_thread = new Thread(() => handler.doRequest(connection));    //instantiate new thread handler to create new threads using lambda expression                   
                    Console.WriteLine(@"Terminated connection.");
                    new_thread.Start(); //Start thread
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }
        }

        static void Main(string[] args)
        {
            runServer();
        }
    }

    class Class_Handler
    {
        static Dictionary<string, string> userLocation = new Dictionary<string, string>();
        List<string> client_data = new List<string>();

        public void doRequest(Socket connection)
        {
            NetworkStream socketStream; //sending data over stream sockets in blocking mode.
            socketStream = new NetworkStream(connection); //instantiate new networkstream with socket returned from listener.acceptSocket()
            Console.WriteLine("Server started listening...");
            Console.WriteLine("Connection received.");
            int do_WHOIS = 0;   //counter to see if whois or not

            try
            {
                StreamWriter sw = new StreamWriter(socketStream);//streamreader/writer instantiated for use in conjunction with client
                StreamReader sr = new StreamReader(socketStream);
                //socketStream.ReadTimeout = 1000;
                //socketStream.WriteTimeout = 1000;
                sw.AutoFlush = true;//"Gets or sets a value indicating whether the StreamWriter will flush its buffer to the underlying stream after every call to Write"

                string username = "";
                string location = "";
                string data = "";

                try     //place in try catch as length of data is undertermined and will throw error if not accounted for.
                {
                    data = sr.ReadLine();
                }
                catch
                {

                }

                string[] section = data.Split(' '); // split input based on space

                if (section[0] == ("POST") || section[0] == ("GET") || section[0] == ("PUT"))   //check to see what type of request
                {
                    do_WHOIS = 1;   //if HTTP request set do_WHOIS to 1 so not to enter whois
                    string line;    //instantiate line to pass server data to
                    int index_start;    //instatntiate index start, remove all uneseccary information from server response

                    if (section[0] == "POST")   //if POST do this
                    {
                        for (int i = 1; i < section.Length; i++)    //check to see if HTTP/1.0 or HTTP/1.1
                        {
                            if (section[i] == "HTTP/1.0")   //if HTTP/1.0 request
                            {
                                while (sr.Peek() >= 0)  //read all server data and add to server data list
                                {
                                    line = sr.ReadLine();
                                    client_data.Add(line);  //add data to list of strings
                                    line = null;
                                }
                                location = client_data[2]; //3rd index of client_Data is always going to be location 
                                string[] name = section[1].Split('/');  //Separate the / from the name and pass to username
                                username = name[1];
                                if (userLocation.ContainsKey(username)) //see if exists in dict
                                {
                                    userLocation[username] = location;
                                }
                                else    //else create a new user with location given
                                {
                                    userLocation.Add(username, location);
                                }
                                sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n");
                            }
                            if (section[i] == "HTTP/1.1")   //if HTTP/1.1 request
                            {
                                while (sr.Peek() >= 0)  //read all server data and add to server data list
                                {
                                    line = sr.ReadLine();
                                    client_data.Add(line);  //add data to list of strings
                                    line = null;
                                }
                                index_start = client_data.IndexOf("") + 1;  //ignore unecessary information locate username/location
                                for (int k = index_start; k < client_data.Count; k++)
                                {
                                    string[] name = client_data[k].Split('&'); //split username from location
                                    username = name[0].Remove(0, 5);    //remove "name=" and store
                                    location = name[1].Remove(0, 9);    //remove "location=" and store
                                }
                                if (userLocation.ContainsKey(username)) //see if exists in dict
                                {
                                    userLocation[username] = location;
                                }
                                else //else create a new user with location given
                                {
                                    userLocation.Add(username, location);
                                }
                                sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n");
                            }
                        }
                    }
                    if (section[0] == "GET")
                    {
                        if (section.Length == 2) //i.e. if request is HTTP/0.9
                        {
                            username = section[0].Remove(0, 1); //remove the / attached to the name and set username = to sections[0]
                            if (userLocation.ContainsKey(username))
                            {
                                sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n" + username + "\r\n");
                            }
                            else
                            {
                                sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n");
                            }
                        }
                        if (section.Length == 3)    //i.e. if request is HTTP/1.1
                        {
                            for (int i = 0; i < section.Length; i++)
                            {
                                if (section[i] == "HTTP/1.1")   
                                {
                                    string[] name = section[1].Split('=');
                                    username = name[1];
                                }
                            }
                            if (userLocation.ContainsKey(username))
                            {
                                sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n" + username + "\r\n");
                            }
                            else
                            {
                                sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n");
                            }
                        }
                    }
                    if (section[0] == "PUT")
                    {
                        location = sr.ReadLine();
                        username = section[1].Remove(0, 1);
                        if (userLocation.ContainsKey(username))
                        {
                            userLocation[username] = location;
                        }
                        else
                        {
                            userLocation.Add(username, location);
                        }
                        sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n");
                    }
                }
                else if (do_WHOIS == 0)//if not http request do this
                {
                    string[] sections = data.Split(new char[] { ' ' }, 2);//split input from client

                    if (sections.Length == 1)   //if only a username is input do this
                    {
                        username = sections[0];

                        if (userLocation.ContainsKey(username)) //if dictionary contains username, try to access the location and write it
                        {
                            userLocation.TryGetValue(username, out location);
                            sw.WriteLine(location);
                        }
                        else
                        {
                            sw.WriteLine("ERROR: no entries found"); //else no entries found
                        }
                    }
                    if (sections.Length == 2)   //if username and location input from client
                    {
                        username = sections[0];
                        location = sections[1];


                        if (userLocation.ContainsKey(username)) //if dictionary contains username
                        {
                            userLocation[username] = location; //usernames location is appended with new location
                            sw.WriteLine("OK");
                        }
                        else
                        {
                            userLocation.Add(username, location);   //else add the username and location to the dictionary
                            sw.WriteLine("OK");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            socketStream.Close();
            connection.Close();

        }
    }
}
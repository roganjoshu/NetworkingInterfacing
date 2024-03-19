using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Name: location
/// A client to connect to the server localhost created for Networking & Interfaces
/// Author: 2020
/// Version 1: February 2020
/// </summary>
public class location
{
    static List<string> server_data = new List<string>();
    static void Main(string[] args)
    {
        string username = null;
        string location = null;

        //default server arguments should none be given
        string server_name = "whois.net.dcs.hull.ac.uk";    
        string port_number = "43";
        string protocol = "whois";

        for (int i = 0; i < args.Length; i++)   //Loop through list of arguments
        {
            switch (args[i])    //switch based on args[i]
            {
                case "-h":
                    server_name = args[++i];    //if arg is -h, pass the next arg to server_name and increment i.
                    break;
                case "-p":
                    port_number = args[++i];    //if the arg is -p, pass the next arg to port_number and increment i.
                    break;
                case "-h0":
                    protocol = args[i]; //if the arg is -h0, pass arg[i] to protocol.
                    break;
                case "-h1":
                    protocol = args[i]; //if the arg is -h1, pass arg[i] to protocol.
                    break;
                case "-h9":
                    protocol = args[i]; //if the arg is -h9, pass arg[i] to protocol.
                    break;
                default:
                    if (username == null)   //if username is null, argument must be a username.
                    {
                        username = args[i];
                    }
                    else if (location == null)  //if username is not null but location is, argument must be location.
                    {
                        location = args[i];
                    }
                    else
                    {
                        Console.WriteLine("Please check input. Too many arguments provided."); //if neither name nor location is null, too many arguments given.
                        return;
                    }
                    break;
            }
        }
        if (username == null)   //if all args have been processed and username is empty, not enough args provided.
        {
            Console.WriteLine("Please check input. Not enough arguments provided.");
            return;
        }

        try
        {
            TcpClient client = new TcpClient();
            client.Connect(server_name, int.Parse(port_number));
            StreamWriter sw = new StreamWriter(client.GetStream()); //sw writes data to server
            StreamReader sr = new StreamReader(client.GetStream()); //sr reads data from server
            sw.AutoFlush = true;    //"Gets or sets a value indicating whether the StreamWriter will flush its buffer to the underlying stream after every call to Write". - text taken from microsoft .NET properties page
            //client.ReceiveTimeout = 1000;   //timeout
            //client.SendTimeout = 1000;

            try     //try catch to catch any errors
            {
                string server_response = null;  //string for storing server responses

                switch (protocol)   //switch statement based on protocol taken from args list
                {
                    case "-h1": //http 1.1
                        string s = "name=&location=";   //calculate content length
                        string header_lines = null;
                        int index_start;
                        if (location == null)    //check to see if lookup/query
                        {
                            sw.WriteLine("GET /?name=" + username + " HTTP/1.1" + "\r\n" + "Host: " + server_name + "\r\n\r\n");   //write http request to server
                            if (int.Parse(port_number) == 80)   //if the requested port is the same as port 80
                            {
                                while (sr.Peek() >= 0)  //detects when nothing else is left to read if negative then no characters left to read
                                {
                                    header_lines = sr.ReadLine();   //read the line and store in string
                                    server_data.Add(header_lines); //store string in list of strings
                                }
                                index_start = server_data.IndexOf("") + 1; //identifies starting index of information required
                                header_lines = null;
                                for (int j = index_start; j < server_data.Count; j++)
                                {
                                    header_lines += server_data[j]; //write data from server_data to string headerlines
                                    header_lines += "\r\n"; //concatonate with carriage return and new line
                                }
                                server_data.Clear();    //clear data from server
                                Console.WriteLine(header_lines);    //write lines from cerver to client
                            }
                            else
                            {
                                Console.WriteLine(sr.ReadLine());
                            }
                        }
                        else    //if not lookup/query then change location
                        {
                            int content_length = location.Length + username.Length + s.Length;  //calculate content length for http1.1
                            sw.WriteLine("POST / HTTP/1.1" + "\r\n" + "Host: " + server_name + "\r\n" + "Content-Length: " + content_length + "\r\n\r\n" + "name=" + username + "&location=" + location);
                            while(sr.Peek() >= 0)
                            {
                                server_response += sr.ReadLine(); //read server response
                            }
                            if (server_response.EndsWith("text/plain")) //if server successfully changes location, do this
                            {
                                Console.WriteLine(username + "'s location changed to be: " + location);
                            }
                            else //else user not successfully changed
                            {
                                Console.WriteLine("ERROR: " + server_response);
                            }
                        }
                        break;

                    case "-h0": //http 1.0
                        if (location == null)   //check to see if lookup/query
                        {
                            sw.WriteLine("GET /?" + username + " HTTP/1.0");    //write http 1.0 request to the server
                            sw.WriteLine();
                            sw.WriteLine();
                            Console.WriteLine(username + " is " + sr.ReadToEnd());
                        }
                        else    //if not lookup/query then change location
                        {
                            sw.WriteLine("POST /" + username + " HTTP/1.0" + "\r\n" + "Content-Length: " + location.Length + "\r\n\r\n" + location);
                            while (sr.Peek() >= 0)
                            {
                                server_response += sr.ReadLine(); //read server response
                            }
                            if (server_response.EndsWith("text/plain")) //if server successfully changes location, do this
                            {
                                Console.WriteLine(username + "'s location changed to be: " + location);
                            }
                            else //else user not successfully changed
                            {
                                Console.WriteLine("ERROR: " + server_response);
                            }
                        }
                        break;

                    case "-h9": // http 0.9
                        if (location == null)    //check to see if lookup/query
                        {
                            sw.WriteLine("GET /" + username);
                            Console.WriteLine(username + " is " + sr.ReadToEnd());
                        }
                        else    //if not lookup/query then change location
                        {
                            sw.WriteLine("PUT /" + username + "\r\n" + location + "\r\n");
                            while (sr.Peek() >= 0)
                            {
                                server_response += sr.ReadLine(); //read server response
                            }
                            if (server_response.EndsWith("text/plain")) //if server successfully changes location, do this
                            {
                                Console.WriteLine(username + "'s location changed to be: " + location);
                            }
                            else //else user not successfully changed
                            {
                                Console.WriteLine("ERROR: " + server_response);
                            }
                        }
                        break;

                    case "whois":   //whois
                        if (location == null)    //check to see if query for location as location will be ampty if only username input
                        {
                            sw.WriteLine(username); //pass username to server
                            Console.WriteLine(username + " is " + sr.ReadToEnd());  //write location passed from  server
                        }
                        else    //else must be location update for user as username and location have been input
                        {
                            sw.WriteLine(username + " " + location);    //pass username and location to server
                            server_response = sr.ReadLine();    //read server response
                            if (server_response.EndsWith("OK")) //if server successfully changes location, do this
                            {
                                Console.WriteLine(username + "'s location changed to be: " + location);
                            }
                            else //else user not succesfully changed
                            {
                                Console.WriteLine("ERROR: unexpected reply");
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }
}
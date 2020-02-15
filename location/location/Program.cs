using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
/// <summary>
/// Name: location
/// A client to connect to the server localhost created for Networking & Interfaces
/// Author: 2020
/// Version 1: February 2020
/// </summary>
public class location
{
    static void Main(string[] args)
    {
        int c;
        TcpClient client = new TcpClient();
        client.Connect("localhost", 43);   //creates a socket connecting the local host to named host and port
        StreamWriter sw = new StreamWriter(client.GetStream());
        StreamReader sr = new StreamReader(client.GetStream());
        try
        {
            if (args.Length == 2)
            {
                sw.WriteLine(args[0] + " " + args[1]);
                sw.Flush();
                Console.WriteLine(args[0] + " location changed to be " + args[1]);
            }
            else if (args.Length == 1)
            {
                sw.WriteLine(args[0]);
                sw.Flush();
                Console.WriteLine(args[0] + " is " + sr.ReadToEnd());
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("No Arguments available.");
        }

    }
}

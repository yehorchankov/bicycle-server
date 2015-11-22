using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Utility;

namespace DZ_2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Server started");

            var xml = XmlParser.GetXmlContent(@"../../Biis.config");
            var port = XmlParser.GetPort(xml);
            var host = XmlParser.GetHost(xml);
            var directory = XmlParser.GetPath(xml);
            Socket listenerSocket = null;

            try
            {
                var ipHost = Dns.GetHostEntry(host);
                var ipAddress = ipHost.AddressList[0];
                var ipEndPoint = new IPEndPoint(ipAddress, port);
                listenerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listenerSocket.Bind(ipEndPoint);
                listenerSocket.Listen(10);
                while (true)
                {
                    Console.WriteLine("\nWaiting..");
                    using (var handler = listenerSocket.Accept())
                    {
                        string data = string.Empty;
                        string reply;

                        //Receiving request
                        var bytes = new byte[4096];
                        var byteRec = handler.Receive(bytes, bytes.Length, 0);
                        data += Encoding.UTF8.GetString(bytes, 0, byteRec);

                        var parser = new HtmlTools();
                        var request = parser.ParseRequest(data);
                        Console.WriteLine("Request received");

                        if (!parser.PageExist(request.Path, directory))
                            reply = parser.GetNotFoundResponse();

                        else if (parser.IsStaticPage(request))
                            reply = parser.ResponseForStaticPage(request.Path, directory);

                        else
                            reply = parser.ResponseForDynamicPage(request.Path, directory, request);

                        //Sending response
                        var msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                        Console.WriteLine("Response sent");

                        handler.Shutdown(SocketShutdown.Both);
                    }
                }
            }
            catch (FileNotFoundException exc)
            {
                Console.WriteLine("Internal error occured");
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Couldn't connect to host");
            }
            finally
            {
                if (listenerSocket != null) listenerSocket.Close();
                Console.WriteLine("Server shutdown");
            }
        }
    }
}
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cloudy.Examples.Chat.Shared;
using Cloudy.Examples.Chat.Shared.Enums;
using Cloudy.Examples.Chat.Shared.Values;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;
using Cloudy.Networking.IP;

namespace Cloudy.Examples.Chat.Server
{
    public static class Program
    {
        private static MessageStream outputStream;

        private static MessageDispatcher dispatcher;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting IP endpoint server ...");
            new IPEndPointServer(Options.EndPointDiscoveryPortNumber).Start();

            Console.WriteLine("Starting listening ...");
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any,
                Options.MessagingPortNumber));
            Console.WriteLine("Accepting the connection ...");
            listener.Start(1);
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Accepted.");
            dispatcher = new MessageDispatcher(Options.ServerId, ResolveStream,
                outputStream = new MessageStream(new StreamSenderReceiver(client.GetStream())));
            ThreadPool.QueueUserWorkItem(DispatchMessages);
            Console.WriteLine("Started dispatching.");
            while (true)
            {
                int? tag;
                Guid from;
                MessagingAsyncResult ar;
                ICastableValue dto = dispatcher.Receive(out from, out tag);
                if (tag == Tags.Says)
                {
                    Console.WriteLine("Client says: {0}",
                        dto.Get<SaysValue>().Message);
                }
                else if (tag == Tags.Leaves)
                {
                    Console.WriteLine("Client leaves.");
                    break;
                }
                else
                {
                    Console.WriteLine("What?...");
                }
                Console.Write("Say something > ");
                string line = Console.ReadLine();
                if (!String.IsNullOrEmpty(line))
                {
                    ar = dispatcher.BeginSend(
                        Options.ClientId, new SaysValue { Message = line }, 
                        Tags.Says, null, null);
                    Console.WriteLine("Sending ...");
                    try
                    {
                        dispatcher.EndSend(ar, new TimeSpan(0, 0, 10));
                        Console.WriteLine("Delivered!");
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("Timed out :(");
                    }
                }
                else
                {
                    ar = dispatcher.BeginSend(Options.ClientId, new LeavesValue(), 
                        Tags.Leaves, null, null);
                    Console.WriteLine("Leaving ...");
                    dispatcher.EndSend(ar, new TimeSpan(0, 0, 10));
                    break;
                }
            }
            dispatcher = null;
        }

        private static void DispatchMessages(object state)
        {
            while (dispatcher != null)
            {
                dispatcher.ProcessIncomingMessages(1);
            }
        }

        private static bool ResolveStream(Guid id, out MessageStream stream)
        {
            stream = outputStream;
            return id == Options.ClientId;
        }
    }
}

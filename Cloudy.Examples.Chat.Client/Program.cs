using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cloudy.Examples.Chat.Shared;
using Cloudy.Examples.Chat.Shared.Enums;
using Cloudy.Examples.Chat.Shared.Values;
using Cloudy.Messaging;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;

namespace Cloudy.Examples.Chat.Client
{
    public static class Program
    {
        private static MessageDispatcher dispatcher;

        public static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            Console.WriteLine("Connecting ...");
            client.Connect(new IPEndPoint(IPAddress.Loopback, Options.PortNumber));
            Console.WriteLine("Connected.");
            dispatcher = new MessageDispatcher(Options.ClientId,
                ResolveStream, new MessageStream(client.GetStream()));
            ThreadPool.QueueUserWorkItem(DispatchMessages);
            Console.WriteLine("Started dispatching.");
            while (true)
            {
                MessagingAsyncResult ar;
                Console.Write("Say something > ");
                string line = Console.ReadLine();
                if (String.IsNullOrEmpty(line))
                {
                    ar = dispatcher.BeginSend(Options.ServerId, new LeavesValue(),
                        Tags.Leaves, null, null);
                    Console.WriteLine("Leaving ...");
                    dispatcher.EndSend(ar, new TimeSpan(0, 0, 10));
                    break;
                }
                ar = dispatcher.BeginSend(Options.ServerId,
                    new SaysValue { Message = line }, Tags.Says, null, null);
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
                int? tag;
                Guid fromId;
                ICastableValue dto = dispatcher.Receive(out fromId, out tag);
                if (tag == Tags.Says)
                {
                    Console.WriteLine("Server says: {0}", dto.Get<SaysValue>().Message);
                }
                else if (tag == Tags.Leaves)
                {
                    Console.WriteLine("Server leaves.");
                    break;
                }
                else
                {
                    Console.WriteLine("What?...");
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
            stream = dispatcher.InputStream;
            return id == Options.ServerId;
        }
    }
}

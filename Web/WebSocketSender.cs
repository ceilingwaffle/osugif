using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web
{
    public class WebSocketSender
    {
        public WebSocketSender(string webServerUrl, int? webServerPort)
        {
            // TODO: actually use the url and port.
            // TODO: Ensure port is available.
            WebServerUrl = webServerUrl;
            WebServerPort = webServerPort ?? 80;
        }

        public string WebServerUrl { get; }

        public int? WebServerPort { get; }

        protected List<IWebSocketConnection> WebSocketConnections { get; set; }

        public void Init()
        {
            FleckLog.Level = LogLevel.Debug;
            WebSocketConnections = new List<IWebSocketConnection>();
            var server = new WebSocketServer($"ws://0.0.0.0:8081/Osu/BPM");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Web socket connection opened.");
                    WebSocketConnections.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Web socket connection closed.");
                    WebSocketConnections.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    //Console.WriteLine($"Sending WS message: \"{message}\".");
                    //Sockets.ToList().ForEach(s => s.Send("Echo: " + message));
                };
            });
        }

        public void Send(string message)
        {
            Console.WriteLine($"Sending WS message to connected sockets: \"{message}\".");

            foreach (var socket in WebSocketConnections.ToList())
            {
                socket.Send(message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using WebSocketManager;

namespace Web
{
    public class Startup
    {
        private WebSocket _webSocket;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketManager();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            // TODO: start the osu presenter
            var osuPresenter = new OsuStatePresenter.OsuPresenter();

            // serve static files from exe output directory
            var currentExeDirectory = Path.Combine(Environment.CurrentDirectory);
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(currentExeDirectory, "public")),
                RequestPath = "/Osu",
                EnableDirectoryBrowsing = false
            });

            // init web sockets
            app.UseWebSockets();
            app.MapWebSocketManager("/ws", serviceProvider.GetService<Osu.OsuMessageHandler>());

            // init request handling
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        //WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        _webSocket = await context.WebSockets.AcceptWebSocketAsync();

                        //await WebSocketEcho(context, _webSocket);
                        byte[] buffer = Encoding.ASCII.GetBytes("test string");
                        //await _webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                        await _webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, 1024), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }

                await next.Invoke();

                // redirect root
                if (context.Request.GetEncodedPathAndQuery() == "/")
                {
                    context.Response.Redirect("/Osu");
                }
            });

            // TODO: Add 404 page to every request except for "/Osu"
        }

        private async Task WebSocketEcho(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}

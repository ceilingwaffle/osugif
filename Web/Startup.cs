using DVPF.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using OsuStatePresenter;
using OsuStatePresenter.Nodes;
using System;
using System.IO;

namespace Web
{
    public class Startup
    {
        protected WebSocketSender WebSocketSender { get; set; } = null;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            OsuPresenter presenter = new OsuPresenter();

            // handle value changed on a specific node
            //RegisterBpmNodeValueChangedHandler(presenter);

            // handle new game state created event
            presenter.StatePresenter.AddEventHandler_NewStateCreated(HandleNewGameState);

            // start the Presenter
            presenter.Start();
            Console.WriteLine("Started the Osu State Presenter.");

            // serve static files from exe output directory
            var currentExeDirectory = Path.Combine(Environment.CurrentDirectory);
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(currentExeDirectory, "public")),
                RequestPath = "/Osu",
                EnableDirectoryBrowsing = false
            });

            // init request handling
            app.Use(async (context, next) =>
            {
                if (WebSocketSender is null)
                {
                    WebSocketSender = new WebSocketSender(context.Request.Host.Host, context.Request.Host.Port);
                    WebSocketSender.Init();
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

        private void RegisterBpmNodeValueChangedHandler(OsuPresenter presenter)
        {
            if (presenter.TryGetNode(typeof(BpmNode), out Node bpmNode))
            {
                if (bpmNode is null)
                {
                    Console.WriteLine($"Error: Unable to get BPM Node from Presenter.");
                    return;
                }

                bpmNode.OnValueChange += (s, e) => HandleBpmChange(bpmNode, s, e);
                Console.WriteLine($"BPM value change handler registered.");
            }
        }

        private void HandleBpmChange(Node bpmNode, object sender, NodeEventArgs e)
        {
            Console.WriteLine($"BPM: {bpmNode.GetValue()}");

            //if (WebSocketSender != null)
            //{
            //    WebSocketSender.Send($"{bpmNode.GetValue().ToString()}");
            //}
        }

        private void HandleNewGameState(DVPF.Core.State state)
        {
            Console.WriteLine($"New Game State:\n{state.ToString()}");

            if (WebSocketSender != null)
            {
                string json = BuildJsonStringFromStateObject(state);

                WebSocketSender.Send(json);
            }
        }

        private string BuildJsonStringFromStateObject(State state)
        {
            return JsonConvert.SerializeObject(state);
        }
    }
}

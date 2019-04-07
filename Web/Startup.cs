using DVPF.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
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
            // TODO: start the osu presenter
            OsuPresenter presenter = new OsuPresenter();

            // handle value changed on a specific node
            if (presenter.TryGetNode(typeof(BpmNode), out var bpmNode))
            {
                bpmNode.OnValueChange += (s, e) => HandleBpmChange(bpmNode, s, e);
            }

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

        private void HandleBpmChange(Node bpmNode, object sender, NodeEventArgs e)
        {
            if (bpmNode is null || bpmNode.GetValue() is null)
            {
                return;
            }

            float bpm = 0f;

            try
            {
                bpm = (float)bpmNode.GetValue();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught Exception. Unable to cast bpm to float: {ex.Message}");
                return;
            }

            Console.WriteLine($"BPM: {bpmNode.GetValue()}");

            if (WebSocketSender != null)
            {
                WebSocketSender.Send($"{bpm}");
            }
        }
    }
}

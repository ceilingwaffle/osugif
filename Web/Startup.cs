using System;
using System.IO;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using OsuStatePresenter;
using OsuStatePresenter.Nodes;

namespace Web
{
    public class Startup
    {

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            // TODO: start the osu presenter
            OsuPresenter presenter = new OsuPresenter();

            // handle value changed on a specific node
            if (presenter.TryGetNode(typeof(BpmNode), out var bpmNode))
            {
                bpmNode.OnValueChange += (sender, e) =>
                {
                    Console.WriteLine($"BPM: {bpmNode.GetValue()}");

                    // TODO: Send WS message
                };
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
                await next.Invoke();

                // redirect root
                if (context.Request.GetEncodedPathAndQuery() == "/")
                {
                    context.Response.Redirect("/Osu");
                }
            });

            // TODO: Add 404 page to every request except for "/Osu"
        }

    }
}

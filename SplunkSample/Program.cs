using System;
using System.Linq;
using System.Net;
using Serilog;
using Serilog.Enrichers;
using Serilog.Sinks.Splunk;
using Splunk;
using Splunk.Client;
using Context = Splunk.Client.Context;

namespace SplunkSample
{
    public class Program
    {
        static void Main()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            //Generally it is not advised to log to Splunk via HTTP/HTTPS due to volume of data  
            //This is available for mobile apps and other special cases.
         
            const string host = "127.0.0.1";

            //Only used for HTTP/HTTPS scenarios
            var generalSplunkContext = new Context(Scheme.Https, host, 8089);

            var transmitterArgs = new TransmitterArgs
            {
                Source = "Splunk.Sample",
                SourceType = "Splunk Sample Source"
            };
            const string username = "my splunk user";
            const string password = "my splunk password";
            const string splunkIndex = "mysplunktest";

            var serilogContext = new SplunkContext(generalSplunkContext, splunkIndex, username, password, null, transmitterArgs);

            Log.Logger = new LoggerConfiguration()
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.WithMachineName()
                .WriteTo.ColoredConsole()
                .WriteTo.SplunkViaHttp(serilogContext, 10, TimeSpan.FromSeconds(5))

                //See http://docs.splunk.com/Documentation/Splunk/6.1.3/Data/Monitornetworkports
                .WriteTo.SplunkViaUdp(host, 10000)
                .WriteTo.SplunkViaTcp(host, 10001)

                .CreateLogger();

            var serilogLogger = Log.ForContext<Program>();

            serilogLogger.Information("Hello from Serilog, running as {Username}!", Environment.UserName);

            var items = Enumerable.Range(1, 1000);

            foreach (var item in items)
            {
               serilogLogger.Information("Logging an int, what fun {Item}", item);
            }

            Console.WriteLine("OK");
            Console.ReadLine();
        }
    }
}
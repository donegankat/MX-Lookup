using DnsClient;
using Microsoft.Extensions.Configuration;
using MXLookup.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MXLookup
{
    class Program
    {
        private static Settings _settings;

        #region Console Logging

        private static void Log(LogTypes type, string text)
        {
            switch (type)
            {
                case LogTypes.Success:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogTypes.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogTypes.Log:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogTypes.Debug:
                case LogTypes.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogTypes.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default: // Shouldn't actually hit this
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            Console.WriteLine(text);

            if (type == LogTypes.Error)
            {
                throw new Exception("ERROR ENCOUNTERED");
            }
        }
        #endregion

        #region Setup

        /// <summary>
        /// Reference: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/index?tabs=basicconfiguration
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Settings _loadAppSettings(IConfiguration config)
        {
            var settings = new Settings();

            // Read configuration
            var knownServers = config.GetSection("knownServers");

            for (int i = 0; i < knownServers.GetChildren().Count(); i++)
            {
                settings.KnownServers.Add(new KnownServer
                {
                    Parent_Server = config[$"knownServers:{i}:parentServer"],
                    Server_Pattern = config[$"knownServers:{i}:serverPattern"]
                });
            }

            return settings;
        }

        #endregion


        static void Main(string[] args)
        {
            // Adding JSON file into IConfiguration.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            // Setup known MX server records
            _settings = _loadAppSettings(config);

            while (true) // Loop until we get valid input
            {
                Log(LogTypes.Info, "Enter the email domain that you wish to perform an MX lookup on:");
                Log(LogTypes.Log, "Example: gmail.com");
                Console.WriteLine();

                var input = Console.ReadLine();                
                if (!string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, @"^\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.IgnoreCase)) // Make sure we recieved input and it was in the form of a valid email domain
                {
                    Console.WriteLine();
                    Log(LogTypes.Info, "Performing MX lookup on " + input);

                    PerformMxLookup(input);

                    break; // Don't continue to prompt for proper input
                }
                else // If we reached this point then the user did not provide valid input. Continue the while() loop until we get something valid.
                {
                    Console.WriteLine();
                    Log(LogTypes.Warning, "Invalid domain input");
                }
            }

            Console.WriteLine();
            Log(LogTypes.Info, "Process complete. Press any key to exit.");
            Console.ReadKey();
        }

        public static void PerformMxLookup(string domain)
        {
            try
            {
                List<string> fullRecords = new List<string>(); // Used to hold the complete string for all MX records associated with the email domain

                bool foundMatch = false;

                var lookup = new LookupClient();
                var result = lookup.Query(domain, QueryType.ANY);

                foreach (var record in result.Answers.MxRecords())
                {
                    if (record == null)
                    {
                        Log(LogTypes.Warning, "MX record is null");
                        continue;
                    }

                    var recordMx = record.Exchange.Value;

                    if (string.IsNullOrWhiteSpace(recordMx))
                    {
                        Log(LogTypes.Warning, "Record MX exchange is null");
                        continue;
                    }

                    Log(LogTypes.Log, recordMx);

                    if (!foundMatch) // Only check to see if we recognize the server if we haven't yet found a match.
                    {
                        var knownDomainMatch = _matchKnownDomain(recordMx);
                        if (!string.IsNullOrWhiteSpace(knownDomainMatch))
                        {
                            foundMatch = true;
                            Log(LogTypes.Success, $"Domain match found: {knownDomainMatch}");
                        }
                    }

                    fullRecords.Add(recordMx);
                }

                // ******
                // TODO: Do something with the full records and known records that we've found.
                // ******

            }
            catch (Exception ex)
            {
                Log(LogTypes.Error, "ERROR: Failed to retrieve MX Record - " + ex.Message);
            }
        }

        /// <summary>
        /// Attempt to match a record to one of our known servers
        /// </summary>
        /// <param name="foundRecord"></param>
        /// <returns></returns>
        private static string _matchKnownDomain(string foundRecord)
        {
            foreach (var knownServer in _settings.KnownServers)
            {
                if (foundRecord.ToLower().Contains(knownServer.Server_Pattern.ToLower()))
                {
                    return knownServer.Parent_Server;
                }
            }

            return null;
        }
    }
}

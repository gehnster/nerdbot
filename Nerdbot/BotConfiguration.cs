using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nerdbot
{
    class BotConfiguration
    {
        private Logger _log;
        private IConfiguration Configuration { get; set; }
        public String Token { get; private set; }
        private string credsFileName { get; } = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        public int TotalShards { get; private set; }

        public BotConfiguration()
        {
            _log = LogManager.GetCurrentClassLogger();

            if (!File.Exists(credsFileName))
                _log.Warn($"config.json is missing.");

            try
            {
                var configurationBuilder =
                    new ConfigurationBuilder();
                configurationBuilder
                  .AddJsonFile(credsFileName,
                    false);
                Configuration = configurationBuilder.Build();
                Token = Configuration[nameof(Token)];

                int ts = 1;
                int.TryParse(Configuration[nameof(TotalShards)], out ts);
                TotalShards = ts < 1 ? 1 : ts;

                if (string.IsNullOrWhiteSpace(Token))
                    throw new ArgumentNullException(nameof(Token), "Token is missing from credentials.json or Environment varibles.");
            }
            catch(Exception ex)
            {
                _log.Fatal(ex.Message);
                _log.Fatal(ex);
                throw;
            }       
        }
    }
}

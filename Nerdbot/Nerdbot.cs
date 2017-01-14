using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Threading.Tasks;

namespace Nerdbot
{
    class Nerdbot
    {
        private Logger _log;
        public static Color OkColor { get; } = new Color(0x71cd40);
        public static Color ErrorColor { get; } = new Color(0xee281f);
        public static DiscordShardedClient Client { get; private set; }
        private static IServiceCollection Service { get; set; }
        public static BotConfiguration Configuration { get; private set; }

        static Nerdbot()
        {
            SetupLogger();
            Configuration = new BotConfiguration();
            Service = new ServiceCollection();
            ConfigureServices();
        }

        private static void ConfigureServices()
        {
            
        }

        public async Task RunAsync(params string[] args)
        {
            _log = LogManager.GetCurrentClassLogger();

            _log.Info("Starting Nerdbot");

            //create client
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Outgoing,
                MessageCacheSize = 10,
                LogLevel = LogSeverity.Warning,
                TotalShards = Configuration.TotalShards,
                ConnectionTimeout = int.MaxValue
            });

            //connect
            await Client.LoginAsync(TokenType.Bot, Configuration.Token).ConfigureAwait(false);
            await Client.ConnectAsync().ConfigureAwait(false);
            await Client.DownloadAllUsersAsync().ConfigureAwait(false);

            _log.Info("Connected");
            // Hook into the MessageReceived event on DiscordSocketClient
            Client.MessageReceived += async (message) =>
            {
                await ProcessMessage(message);
            };
        }

        public async Task RunAndBlockAsync(params string[] args)
        {
            await RunAsync(args).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static void SetupLogger()
        {
            try
            {
                var logConfig = new LoggingConfiguration();
                var consoleTarget = new ColoredConsoleTarget();

                consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} | ${message}";

                logConfig.AddTarget("Console", consoleTarget);

                logConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));

                LogManager.Configuration = logConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task ProcessMessage(SocketMessage message)
        {
            var messageParts = message.Content.Split(' ');
            if(messageParts.Length > 0)
            {
                if(messageParts[0] == "/r")
                {
                    ProcessRandomNumber(messageParts, message);
                }
            }
        }

        private static void ProcessRandomNumber(string[] messageParts, SocketMessage message)
        {
            var total = 0;
            var rnd = new Random();

            if(messageParts.Length == 2)
            {
                var result = 0;
                if(int.TryParse(messageParts[1], out result) && result >= 0)
                {
                    total = rnd.Next(result + 1);
                }
                else
                {
                    message.Channel.SendMessageAsync("Syntax Invalid: " + messageParts[0] + " " + messageParts[1]);
                }
            }

            message.Channel.SendMessageAsync("Total: " + total);
        }
    }
}

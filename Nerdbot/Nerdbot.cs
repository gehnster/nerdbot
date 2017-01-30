using Discord;
using Discord.WebSocket;
using Logging;
using Microsoft.Extensions.DependencyInjection;
using Nerdbot.Utilities.Fortuna;
using Nerdbot.Utilities.Fortuna.Extensions;
using NLogLogger;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdbot
{
    class Nerdbot
    {
        private static ILogger _log = new NLogConsoleLogger(typeof(Nerdbot).Name);
        public static Color OkColor { get; } = new Color(0x71cd40);
        public static Color ErrorColor { get; } = new Color(0xee281f);
        public static DiscordShardedClient Client { get; private set; }
        private static IServiceCollection Service { get; set; }
        public static BotConfiguration Configuration { get; private set; }

        static Nerdbot()
        {
            Configuration = new BotConfiguration();
            Service = new ServiceCollection();
            ConfigureServices();
        }

        private static void ConfigureServices()
        {
            
        }

        public async Task RunAsync(params string[] args)
        {

            _log.InfoLogging("Starting Nerdbot");

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

            _log.InfoLogging("Connected");
            // Hook into the MessageReceived event on DiscordSocketClient
            Client.MessageReceived += async (message) =>
            {
                await ProcessMessageAsync(message);
            };
        }

        public async Task RunAndBlockAsync(params string[] args)
        {
            await RunAsync(args).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static async Task ProcessMessageAsync(SocketMessage message)
        {
            if(!message.Author.IsBot)
            {
                var messageParts = message.Content.Split(' ');
                if(messageParts.Length > 0)
                {
                    if(messageParts[0] == "!help")
                    {
                        await ProcessHelpAsync(messageParts, message);
                    }
                    else if(messageParts[0] == "/r")
                    {
                        await ProcessRandomNumberAsync(messageParts, message);
                    }
                }
            }
            
        }

        private static async Task ProcessHelpAsync(string[] messageParts, SocketMessage message)
        {
            if(messageParts.Length == 1)
            {
                var returnMessage = "Nerdbot Version: " + Version.FullVersionString + Environment.NewLine;
                returnMessage += "Syntax:" + Environment.NewLine;
                returnMessage += "/r <number> - Roll a number between 0 and <number>" + Environment.NewLine;
                await message.Channel.SendMessageAsync(returnMessage);
            }
            else
            {
                await message.Channel.SendMessageAsync(ErrorMessages.Syntax);
            }
        }

        private static async Task ProcessRandomNumberAsync(string[] messageParts, SocketMessage message)
        {
            var total = 0;
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var rng = await PRNGFortunaProviderFactory.CreateAsync(token) as PRNGFortunaProvider;

            if (messageParts.Length == 2)
            {
                if(int.TryParse(messageParts[1], out var result) && result >= 0)
                {
                    total = IPRNGFortunaProviderExtensions.RandomNumber(rng, result + 1);
                }
                else
                {
                    await message.Channel.SendMessageAsync(ErrorMessages.Syntax);
                }
            }
            else
            {
                await message.Channel.SendMessageAsync(ErrorMessages.Syntax);
            }

            await message.Channel.SendMessageAsync("Total: " + total);
        }
    }
}

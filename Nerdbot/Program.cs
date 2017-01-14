using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

class Program
{
    // Convert our sync-main to an async main method
    static void Main(string[] args) => new Nerdbot.Nerdbot().RunAndBlockAsync().GetAwaiter().GetResult();
}
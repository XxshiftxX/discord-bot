using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using YoutubeExtractor;

namespace Nono_Bot_Dsharpplus
{
    class Program
    {
        static Random random = new Random();
        static string[] keys;

        static DiscordClient discord;
        static CommandsNextModule commands;
        static VoiceNextClient voice;
        static InteractivityModule interactivity;

        static void Main(string[] args) => MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            keys = File.ReadAllLines(@"D:\discordKey.txt");
            discord = new DiscordClient(new DiscordConfiguration
            {
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,
                Token = keys[0],
                TokenType = TokenType.Bot
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "노노봇 "
            });

            interactivity = discord.UseInteractivity(new InteractivityConfiguration{ });

            voice = discord.UseVoiceNext();

            commands.RegisterCommands<Voice>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}

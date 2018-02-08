using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Audio;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Nono_Bot
{
    class Program
    {
        private String _botToken
        {
            get => "NDA5MTk3NjY2NDQ3MTMwNjI3.DVbPGw.Bxzt1u1h0qQf-kQX4spE-yIP-3g";
        }
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        private static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            var clientConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 100
            };

            var commandConfig = new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async
            };

            _client = new DiscordSocketClient(clientConfig);
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            await InstallCommandsAsync();

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;
            _client.MessageUpdated += MessageUpdated;
            _client.Ready += () =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[노노봇]\t\t");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("가동 완료.");

                return Task.CompletedTask;
            };

            string token = _botToken;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if (message == null)
                return;

            int argPos = 0;

            if (!(message.HasStringPrefix("노노봇 ", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }
        
        private string RandomMessage(params string[] messages)
        {
            Random r = new Random();

            return messages[r.Next(0, messages.Length - 1)];
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if(message.Content == "핑")
            {
                var channel = _client.GetChannel(message.Channel.Id) as SocketTextChannel;
                Console.WriteLine(channel.Name);
                await message.Channel.SendMessageAsync("퐁!");
            }

            /*
            if (message.Content.Split()[0].IndexOf("노노") < 0)
                return;

            if (message.Content.IndexOf("노래") >= 0)
            {
                await JoinChannel(message);
                return;
            }
            else
            {
                await message.Channel.SendMessageAsync(RandomMessage(
                    "저기, 저 말고도 귀여운 아이들은 많다고 생각하는데요...",
                    "아우우... 돌아가고 싶은데요...",
                    "조용히 살고 싶은데요...",
                    "아이돌 같은 거... 무리이...",
                    "친척의 권유라서... 아이돌은 한번만이라고 이야기했었는데요..."
                    ));
            }
            */
        }

        public async Task JoinChannel(SocketMessage msg, IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (msg.Author as IGuildUser)?.VoiceChannel;
            if (channel == null) { await msg.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }
    }
}

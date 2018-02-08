using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nono_Bot
{
    public class Info : ModuleBase<SocketCommandContext>
    {
        [Command("에코")]
        public async Task SayAsync([Remainder] string echo)
        {
            await ReplyAsync($"{Context.User.Username} : {echo}");
        }

        [Command("따라하기")]
        public async Task SayPartAsync(int num, [Remainder] string echo)
        {
            await ReplyAsync($"{Context.User.Username} : {echo.Split()[num - 1]}");
        }

        [Command("노래틀기", RunMode = RunMode.Async)]
        public async Task MusicPlayAsync(IVoiceChannel channel  = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            if(channel == null)
            {
                await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            var audioClient = await channel.ConnectAsync();

            await SendAsync(audioClient, @"D:\Programing\CSharp\Nono Bot\Nono Bot\libs\ffmpeg.exe");
        }

        private Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg);
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            var ffmpeg = CreateStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Mixed);
            await output.CopyToAsync(discord);
            await discord.FlushAsync();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using HtmlAgilityPack;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using YoutubeExtractor;

namespace Nono_Bot_Dsharpplus
{
    [Group("보이스")]
    public class Voice
    {
        public Queue<string> musicQueue = new Queue<string>();
        public static bool isPlaying = false;

        [Command("참가")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                return;

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                return;

            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync("들어왔는데요...");
        }

        [Command("퇴장")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                return;

            vnc.Disconnect();
            await ctx.RespondAsync("안녕히계세요...");
        }

        [Command("정지")]
        public async Task Stop(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);

            if (vnc == null)
                return;
            if (!isPlaying)
                return;

            if (isPlaying)
            {
                isPlaying = false;
                await MusicStopped(ctx);
            }
            await ctx.RespondAsync("정지할게요...");
            await vnc.SendSpeakingAsync(false);
        }
        
        public async Task Play(CommandContext ctx, [RemainingText] string file)
        {
            var id = YoutubeClient.ParseVideoId(file);

            var client = new YoutubeClient();
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);


            var streamInfo = streamInfoSet.Audio.WithHighestBitrate();
            var ext = streamInfo.Container.GetFileExtension();
            await client.DownloadMediaStreamAsync(streamInfo, $"audio.{ext}");

            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("노래할 채널에 들어가있지 않다는 것 같은데요....");
                return;
            }

            if (!File.Exists($"audio.{ext}"))
            {
                await ctx.RespondAsync("아우우... 파일이 존재하지 않는다는 것 같은데요...");
                return;
            }

            if (isPlaying)
            {
                return;
            }

            await ctx.RespondAsync("재생할게요...");
            await vnc.SendSpeakingAsync(true);

            isPlaying = true;

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""audio.{ext}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            var ffout = ffmpeg.StandardOutput.BaseStream;

            var buff = new byte[3840];
            var br = 0;
            while ((br = ffout.Read(buff, 0, buff.Length)) > 0)
            {
                if (!isPlaying)
                    break;

                if (br < buff.Length)
                    for (var i = br; i < buff.Length; i++)
                        buff[i] = 0;
                await vnc.SendAsync(buff, 20);
            }

            if (isPlaying == true)
                await vnc.SendSpeakingAsync(false);

            isPlaying = false;
            Console.WriteLine("노래 끝");
            await MusicStopped(ctx);
        }

        [Command("유튜브")]
        public async Task Youtube(CommandContext ctx, [RemainingText] string text)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            string htmlCode;

            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage res = httpClient.GetAsync($"https://www.youtube.com/results?search_query={text}").Result)
                {
                    using (HttpContent content = res.Content)
                    {
                        htmlCode = content.ReadAsStringAsync().Result;
                    }
                }
            }
            htmlDoc.LoadHtml(htmlCode);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                await ctx.RespondAsync("아우우... HTML 파싱에 문제가 발생했다는 것 같은데요...");
                foreach (HtmlParseError error in htmlDoc.ParseErrors)
                {
                    await ctx.RespondAsync(error.Reason);
                }
                return;
            }

            if (htmlDoc.DocumentNode == null)
            {
                await ctx.RespondAsync("아우우... 유튜브 문서가 잘못됐다는 것 같은데요...");
                return;
            }

            var videoDivNode = htmlDoc.DocumentNode
               .Descendants("div")
               .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("yt-lockup-video"));

            int i = 0;
            string[] links = new string[5];
            string message = $"{text}의 검색 결과는 다음과 같다는데요... 명령어를 입력해주세요...\n```prolog\n";
            foreach (HtmlNode divNode in videoDivNode)
            {
                if (i++ > 4)
                    break;

                var titleNode = divNode
                    .Descendants("a")
                    .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("yt-uix-tile-link"))
                    .FirstOrDefault();

                message += $"{i}. {titleNode.InnerHtml}\n";
                links[i - 1] = $"https://www.youtube.com{titleNode.Attributes["href"].Value}";
            }
            message += $"\n0. 취소```";

            await ctx.RespondAsync(message);

            var ia = ctx.Client.GetInteractivityModule();
            int selectedNumber = 1;
            var iaMessage = await ia.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id && 
                (int.TryParse(xm.Content, out selectedNumber) && selectedNumber < 6), TimeSpan.FromSeconds(20));

            if (selectedNumber == 0)
                return;

            if (isPlaying)
                musicQueue.Enqueue(links[selectedNumber - 1]);
            else
                await Play(ctx, links[selectedNumber - 1]);
        }

        private async Task MusicStopped(CommandContext ctx)
        {
            if(musicQueue.Count <= 0)
                return;

            await Play(ctx, musicQueue.Dequeue());
        }
    }
}

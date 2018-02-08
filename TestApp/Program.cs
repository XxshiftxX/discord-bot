using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            asyncMainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task asyncMainAsync()
        {
            var client = new YoutubeClient();
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync("bnsUkE8i0tU");

            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
            var ext = streamInfo.Container.GetFileExtension();
            await client.DownloadMediaStreamAsync(streamInfo, $"downloaded_video.{ext}");
        }
    }
}

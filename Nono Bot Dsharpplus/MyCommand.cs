using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;

namespace Nono_Bot_Dsharpplus
{
    public class MyCommand
    {
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
    }
}

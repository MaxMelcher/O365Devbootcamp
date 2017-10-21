using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace O365DevBootcamp.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string replyText = activity.Text.Substring(activity.Text.LastIndexOf("</at>")+5).Trim();


            if (replyText == "Yes")
            {
                Activity reply = activity.CreateReply();
                reply.Text = "Thats totally accurate!";
                await context.PostAsync(reply);
            }
            else
            {
                Activity reply = activity.CreateReply();
                reply.AddHeroCard("Bots are awesome!", new List<string> { "Yes", "Yes" }, new List<string> { "Awesome", "More awesome!" });
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}
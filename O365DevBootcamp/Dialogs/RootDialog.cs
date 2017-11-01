using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            if (activity.Text.Contains("Yes!"))
            {
                Activity isTypingReply = activity.CreateReply("Thinking about it...");
                isTypingReply.Type = ActivityTypes.Typing;
                await context.PostAsync(isTypingReply);

                Thread.Sleep(2000);

                Activity reply = activity.CreateReply();
                reply.Text = "Thats totally accurate!";
                await context.PostAsync(reply);

                //await context.PostAsync(isTypingReply);

                //Thread.Sleep(2000);

                //Activity reply2 = activity.CreateReply();
                //reply2.Text = "... now I need to do important stuff!";
                //reply2.Type = ActivityTypes.EndOfConversation;
                //await context.PostAsync(reply2);
            }
            else
            {
                Activity reply = activity.CreateReply();
                reply.AddHeroCard("Bots are awesome!", new List<string> { "Yes!", "Yes!" }, new List<string> { "Awesome", "More awesome!" });
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}
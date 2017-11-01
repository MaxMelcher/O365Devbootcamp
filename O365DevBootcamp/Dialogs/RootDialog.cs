using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
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
                reply.AddHeroCard("Bots are awesome!", new List<string> { "Yes!", "Yes!" },
                    new List<string> { "Awesome", "More awesome!" });
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
        }
    }
    //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/88200c92-8d4d-442c-ad31-92113062da93?subscription-key=36ae2585cbaf405aa4529451261c216c&verbose=true&timezoneOffset=0&q=
    [LuisModel("88200c92-8d4d-442c-ad31-92113062da93", "36ae2585cbaf405aa4529451261c216c", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class SandwichDialog : LuisDialog<object>
    {
        /// <summary>
        /// Send a generic help message if an intent without an intent handler is detected.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            string message = $"I'm the sandwich bot. Ich kann Sandwiches erscheinen lassen! \n\n Detected intent: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("What")]
        public async Task SandwichOptions(IDialogContext context, LuisResult result)
        {
            var t = context.MakeMessage();

            t.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction { Title = "Ham", Type = ActionTypes.ImBack, Value = "Ham" },
                    new CardAction { Title = "Turkey", Type = ActionTypes.ImBack, Value = "Turkey" },
                    new CardAction { Title = "Veggy", Type = ActionTypes.ImBack, Value = "Veggy" }
                }
            };

            await context.PostAsync(t);
            PromptDialog.Text(context, AfterSelectSandwich, "Was für ein Sandwich möchtest du?");
        }


        [LuisIntent("Select")]
        public async Task SelectSandwich(IDialogContext context, LuisResult result)
        {
            EntityRecommendation ham;
            EntityRecommendation turkey;
            EntityRecommendation veggy;
            if (result.TryFindEntity("Ham", out ham))
            {
                await context.PostAsync($"Sandwich {ham.Entity} ausgewählt");
            }
            else if (result.TryFindEntity("Turkey", out turkey))
            {
                await context.PostAsync($"Sandwich {turkey.Entity} ausgewählt");
            }
            else if (result.TryFindEntity("Veggy", out veggy))
            {
                await context.PostAsync($"Sandwich {veggy.Entity} ausgewählt");
            }
            else
            {
                PromptDialog.Text(context, AfterSelectSandwich, "Was für ein Sandwich möchtest du?");
            }
        }

        [LuisIntent("Welches")]
        public async Task WhatSandwich(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Woher soll ich das wissen?!");
        }

        private async Task AfterSelectSandwich(IDialogContext context, IAwaitable<string> result)
        {
            string sandwich = await result;
            string selected = "";
            if (sandwich.Equals("Ham", StringComparison.InvariantCultureIgnoreCase))
            {
                selected = "Ham";
            }

            else if (sandwich.Equals("Turkey", StringComparison.InvariantCultureIgnoreCase))
            {
                selected = "Turkey";
            }
            else if (sandwich.Equals("Veggy", StringComparison.InvariantCultureIgnoreCase))
            {
                selected = "Veggy";
            }

            if (string.IsNullOrEmpty(selected))
            {
                PromptDialog.Choice(context, AfterSelectSandwich2,
                    new List<Sandwich> {new Sandwich("Ham"), new Sandwich("Turkey"), new Sandwich("Veggy")}, "WAS FÜR EIN SANDWICH DER DREI OPTIONEN MÖCHTEST DU?", "", 3, PromptStyle.Auto);
            }
            else
            {
                await context.PostAsync($"Sandwich {selected} ausgewählt");
                context.Wait(MessageReceived);
            }
        }

        private async Task AfterSelectSandwich2(IDialogContext context, IAwaitable<Sandwich> awaitable)
        {
            var r = await awaitable;
            await context.PostAsync($"Sandwich {r} ausgewählt");
            context.Wait(MessageReceived);
        }
    }

    [Serializable]
    public class Sandwich
    {
        public Sandwich(string style)
        {
            Style = style;
        }

        public string Style { get; set; }

        public override string ToString()
        {
            return Style;
        }
    }
}
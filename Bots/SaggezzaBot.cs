using CoreBot.Helpers;
using CoreBot.Models;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Bots
{
    public class SaggezzaBot<T> : ActivityHandler where T: Dialog
    {
        #region Variables
         protected readonly Dialog _dialog;
        protected readonly BotStateService _botStateService;
        protected readonly ILogger _logger;

        #endregion

        public SaggezzaBot(BotStateService botStateService, T dialog, ILogger<SaggezzaBot<T>> logger)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

         protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with message activity");

            //Run the new dialog with the message activity
            await _dialog.Run(turnContext, _botStateService.DialogStateAccessor, cancellationToken);
        }

         protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeCard = CreateAdaptiveCardAttachment();
                    var response = CreateResponse(turnContext.Activity, welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    //await GreetUser(turnContext, cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            //Save any state changes that might have occured during the turn
            await _botStateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _botStateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        
        // Create an attachment message response.
        private Activity CreateResponse(IActivity activity, Attachment attachment)
        {
            var response = ((Activity)activity).CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            string[] paths = { ".", "Cards", "welcomeCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        private async Task GreetUser(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            ConversationData conversationData = await _botStateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}, how can I help you today?", userProfile.Name)), cancellationToken);
            }
            else
            {
                if (conversationData.PromptedUserForName)
                {
                    //Set the name to what user entered
                    userProfile.Name = turnContext.Activity.Text.Trim();

                    //Acknowledge the user
                    await turnContext.SendActivityAsync(MessageFactory.Text(String.Format("Thanks {0}, How can I help you today?", userProfile.Name)), cancellationToken);

                    //Reset the flag for next cycle
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    //Prompt user for their name
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello! What is your name?"), cancellationToken);

                    //Set the flag to true
                    conversationData.PromptedUserForName = true;
                }
            }

        }
    }
}

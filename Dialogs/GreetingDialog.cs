using CoreBot.Models;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotBuilderSamples;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        public GreetingDialog(BotStateService botStateService) :  base(nameof(GreetingDialog))
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));

            InitializeWaterFallDialog();
        }

        private void InitializeWaterFallDialog()
        {
            //Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[] {
                InitilizeStepAsync                
            };

            //Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainflow", waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            //Set the starting dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainflow";
        }

        private async Task<DialogTurnResult> InitilizeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {   
            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Hi! What can I help you with today?\nI can help you in queries with \nIT, Admin or HR") }, cancellationToken);
            

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

      
    }
}

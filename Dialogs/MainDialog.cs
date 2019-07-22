// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Dialogs;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger _logger;
        protected readonly BotService _botServices;
        private readonly BotStateService _botStateService;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger, BotService botServices, BotStateService botStateService)
            : base(nameof(MainDialog))
        {
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));

            Configuration = configuration;
            _logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new BookingDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }



        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Configuration["LuisAppId"]) || string.IsNullOrEmpty(Configuration["LuisAPIKey"]) || string.IsNullOrEmpty(Configuration["LuisAPIHostName"]))
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file."), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                RecognizerResult recognizerResult = null;
                recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);


                //var result = stepContext.Context.Activity as Activity;
                //string data = result.ChannelData.ToString();
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text(data), cancellationToken);

                
                string msg = JsonConvert.SerializeObject(recognizerResult.Intents);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                
                // Top intent tell us which cognitive service to use.
                var topIntent = recognizerResult.GetTopScoringIntent();                
    
                // Next, we call the dispatcher with the top intent.
                await DispatchToTopIntentAsync(stepContext, topIntent.intent, recognizerResult, cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //// If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
            //if (stepContext.Result != null)
            //{
            //    var result = (BookingDetails)stepContext.Result;

            //    // Now we have all the booking details call the booking service.

            //    // If the call to the booking service was successful tell the user.

            //    var timeProperty = new TimexProperty(result.TravelDate);
            //    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //    var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            //}
            //else
            //{
            //    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            //}
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }



        private async Task DispatchToTopIntentAsync(WaterfallStepContext turnContext, string intent, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "q_SaggezzaKB":
                    await ProcessSampleQnAAsync(turnContext.Context, cancellationToken);
                    break;
                //case "l_HomeAutomation":
                //    //await ProcessHomeAutomationAsync(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
                //    break;
                //case "l_Weather":
                //    //await ProcessWeatherAsync(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
                //    break;
                //case "q_sample-qna":
                //    await ProcessSampleQnAAsync(turnContext.Context, cancellationToken);
                //    break;
                
                default:
                    //await InvokeTicketCreationDialog(turnContext.Context, cancellationToken);
                    return await turnContext.BeginDialogAsync(nameof(TicketCreationDialog), bookingDetails, cancellationToken);
                    //await turnContext.Context.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {intent}."), cancellationToken);
                    //break;
            }
        }

        private async Task ProcessSampleQnAAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessSampleQnAAsync");

            var results = await _botServices.SaggezzaQNAKB.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
            }
        }



          private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var bookingDetails = stepContext.Result != null
                    ?
                await LuisHelper.ExecuteLuisQuery(Configuration, _logger, stepContext.Context, cancellationToken)
                    :
                new BookingDetails();

            // In this sample we only have a single Intent we are concerned with. However, typically a scenario
            // will have multiple different Intents each corresponding to starting a different child Dialog.

            // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
            return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);
        }

      
         private async Task<DialogTurnResult> FinalStepAsync1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
            if (stepContext.Result != null)
            {
                var result = (BookingDetails)stepContext.Result;

                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.TravelDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var msg = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


    }
}

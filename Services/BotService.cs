﻿using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Services
{
    public class BotService
    {
        public BotService(IConfiguration configuration)
        {
            try
            {
                var applicationId = configuration["LuisAppId"];
                var endpointKey = configuration["LuisAPIKey"];
                var LuisAPIHostName = "westus";
                var endpoint = $"https://{LuisAPIHostName}.api.cognitive.microsoft.com";
                //var endpoint = "https://westus.api.cognitive.microsoft.com";
                Dispatch = new LuisRecognizer(new LuisApplication(
                applicationId,
                endpointKey,
                endpoint),
                new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true },
                true);

            SaggezzaQNAKB = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAEndpointKey"],
                Host = configuration["QnAEndpointHostName"]
            });
            }
            catch (Exception)
            {

                throw;
            }
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            
        }

        public LuisRecognizer Dispatch { get; private set; }
        public QnAMaker SaggezzaQNAKB { get; private set; }
    }
}

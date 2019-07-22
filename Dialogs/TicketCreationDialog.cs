using CoreBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class TicketCreationDialog : ComponentDialog
    {
        public TicketCreationDialog(string dialogId) : base(dialogId)
        {
             AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {               
                SendInitialMessageAndConfirmation,
                ActOnTheInitialMessageResponse,
                CloseTheDialogConversation
            }));
        }

        private Task<DialogTurnResult> CloseTheDialogConversation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ticketDataModel = (TicketDataModel)stepContext.Options;

           

            throw new NotImplementedException();
        }

        private Task<DialogTurnResult> ActOnTheInitialMessageResponse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<DialogTurnResult> SendInitialMessageAndConfirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

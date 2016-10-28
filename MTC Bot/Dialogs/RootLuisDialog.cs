using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using MTC_Bot.Helpers;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using System.Threading;
using Microsoft.Bot.Builder.FormFlow;
using System.Diagnostics;
using System.Web;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MTC_Bot.Dialogs
{
    [LuisModel(Constants.LuisId, Constants.LuisSubscriptionKey)]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private bool userWelcomed;

        public string textoEscrito;

        private async Task ResumeAfterPrompt(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var userName = await result;
                userWelcomed = true;

                await context.PostAsync($"Como posso te ajudar, {userName}?");

                context.UserData.SetValue(Constants.UsernameKey, userName);
            }
            catch (TooManyAttemptsException)
            {
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("Saudacao")]
        public async Task GreetingAsync(IDialogContext context, LuisResult result)
        {
            string username;

            if (!context.UserData.TryGetValue(Constants.UsernameKey, out username))
            {
                PromptDialog.Text(context, ResumeAfterPrompt, "Olá! Antes de iniciarmos, qual é o seu nome?");
                return;
            }

            if (!userWelcomed)
            {
                userWelcomed = true;
                await context.PostAsync($"Bem vindo, {username}! Você quer seu boleto?");

                context.Wait(MessageReceived);
                return;
            }

            //await MakeRequest("oi");

            await context.PostAsync($"Como posso te ajudar, {username}?");

            context.Wait(MessageReceived);
        }

        [LuisIntent("EmitirBoleto")]
        public async Task Atendimento(IDialogContext context, LuisResult result)
        {
            //var message = await activity;
            var replyMessage = context.MakeMessage();

            // await context.PostAsync($"Bem vindo ao atendimento da Porto Seguro, você está procurando pela sua 2 via de boleto..");

            // The Attachments property allows you to send and receive images and other content 
            replyMessage.Attachments = new List<Attachment>()
             {
                 new Attachment()
                 {
                     ContentUrl = "https://docs.botframework.com/en-us/images/faq-overview/botframework_overview_july.png",
                     ContentType = "image/png",
                     Name = "BotFrameworkOverview.png"
                 }
             };

            replyMessage.Text = "Bem vindo ao atendimento da Porto Seguro, segue a 2 via do seu boleto.";

            await context.PostAsync(replyMessage);

            context.Wait(this.MessageReceived);

        }

        [LuisIntent("ContratarSeguro")]
        public async Task FormViagem(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Count() == 0)
            {
                PromptDialog.Text(context, ResumeAfterPromptSeguro, "Eu entendi que você quer contratar um seguro. Mas qual tipo?");
                return;
            }
            else
            {
                var message = result;
                var SeguroForms = new FormDialog<FormSeguro>(new FormSeguro(), FormSeguro.SeguroBuildForm, FormOptions.PromptInStart);
                context.Call(SeguroForms, this.ResumeAfterFormDialog);
            }
        }

        private async Task ResumeAfterPromptSeguro(IDialogContext context, IAwaitable<object> result)
        {
            var message = result;
            var SeguroForms = new FormDialog<FormSeguro>(new FormSeguro(), FormSeguro.SeguroBuildForm, FormOptions.PromptInStart);
            context.Call(SeguroForms, this.ResumeAfterFormDialog);
        }

        private async Task ResumeAfterFormDialog(IDialogContext context, IAwaitable<object> result)
        {
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = GetCardsAttachments();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }


        private static IList<Attachment> GetCardsAttachments()
        {
            return new List<Attachment>()
            {
                GetHeroCard(
                    "Mundo 120 Ouro",
                    "Lore ipsum",
                    "Lore ipsum",
                    new CardImage(url: "http://www.portoseguro.com.br/static-files/Institucional/Layout/bg/logo_home.gif"),
                    new CardAction(ActionTypes.OpenUrl, "Contratar", value: "https://wwws.portoseguro.com.br/vendaonline/viagem/home.ns?cmph=113&utm_campaign=lds_segviagem&utm_source=interps_cotacao_vdo&utm_medium=own&utm_content=menu_btnCotacao")),
                GetHeroCard(
                    "Mundo 120 Prata",
                    "Lore ipsum",
                    "Lore ipsum",
                    new CardImage(url: "http://www.portoseguro.com.br/static-files/Institucional/Layout/bg/logo_home.gif"),
                    new CardAction(ActionTypes.OpenUrl, "Contratar", value: "https://wwws.portoseguro.com.br/vendaonline/viagem/home.ns?cmph=113&utm_campaign=lds_segviagem&utm_source=interps_cotacao_vdo&utm_medium=own&utm_content=menu_btnCotacao")),
                GetHeroCard(
                    "Mundo 120 Bronze",
                    "Lore ipsum",
                    "Lore ipsum",
                    new CardImage(url: "http://www.portoseguro.com.br/static-files/Institucional/Layout/bg/logo_home.gif"),
                    new CardAction(ActionTypes.OpenUrl, "Contratar", value: "https://wwws.portoseguro.com.br/vendaonline/viagem/home.ns?cmph=113&utm_campaign=lds_segviagem&utm_source=interps_cotacao_vdo&utm_medium=own&utm_content=menu_btnCotacao")),
                GetHeroCard(
                    "América",
                    "Lore ipsum",
                    "Lore ipsum",
                    new CardImage(url: "http://www.portoseguro.com.br/static-files/Institucional/Layout/bg/logo_home.gif"),
                    new CardAction(ActionTypes.OpenUrl, "Contratar", value: "https://wwws.portoseguro.com.br/vendaonline/viagem/home.ns?cmph=113&utm_campaign=lds_segviagem&utm_source=interps_cotacao_vdo&utm_medium=own&utm_content=menu_btnCotacao")),
            };
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };
            return heroCard.ToAttachment();
        }

        [LuisIntent(Constants.ListDetailedRoom)]
        public async Task DetailedRoomAsync(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Any())
            {
                var roomName = result.Entities
                    .OrderByDescending(x => x.Entity.Length)
                    .Select(x => x.Entity)
                    .FirstOrDefault();

                await RoomsHelper.DetailedRoomInfoAsync(context, roomName);
            }
            else
            {
                await context.PostAsync("Sala não encontrada");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent(Constants.ListAllRooms)]
        public async Task RoomsListAsync(IDialogContext context, LuisResult result)
        {
            var roomsList = GetAllRoomsList();
            await context.PostAsync(roomsList);

            context.Wait(MessageReceived);
        }

        public async Task<string> SentimenAnalysis(string text)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "344a8fc89c22474a8917958e044ee5bd");

            // Request parameters
            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";

            HttpResponseMessage response;

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes("{\"documents\": [{\"id\": \"1\",\"text\": \"" + text + "\"}]}");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }

            string stream = await response.Content.ReadAsStringAsync();
            var r = JsonConvert.DeserializeObject(stream);
            return r.ToString();
        }

        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Desculpe, eu não entendi...");

            context.Wait(MessageReceived);
        }

        [LuisIntent(Constants.ScheduleRooms)]
        public async Task ScheduleRoom(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Gerando formulário...");
            context.Call(new EngagementDialog(), null);
        }

        private static string GetAllRoomsList()
        {
            var replyMessage = string.Empty;

            try
            {
                var con = new SqlConnection(Constants.ConnectionString);
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select * from room",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var rooms = cmd.ExecuteReader();

                while (rooms.Read())
                {
                    replyMessage += $"* { rooms["name"] }\n\n";
                }

                con.Close();
            }
            catch (Exception e)
            {
                replyMessage = e.ToString();
            }

            return replyMessage;
        }


    }
}
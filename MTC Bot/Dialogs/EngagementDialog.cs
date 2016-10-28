using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;
using MTC_Bot.Helpers;
using MTC_Bot.Models;

namespace MTC_Bot.Dialogs
{
    [Serializable]
    public class EngagementDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Caso tenha dúvidas ao preencher o formulário, digite 'help'.");

            var engagementFormDialog =
                    FormDialog.FromForm(BuildEngagementForm, FormOptions.PromptInStart);

            context.Call(engagementFormDialog, ResumeAfterFormDialog);
        }
        
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var message = await result;

                if (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("ajuda"))
                {
                    await context.Forward(new SupportDialog(),
                        ResumeAfterSupportDialog, message, CancellationToken.None);
                }
                else
                {
                    //ShowOptions(context);
                    var engagementFormDialog =
                    FormDialog.FromForm(BuildEngagementForm, FormOptions.PromptInStart);

                    context.Call(engagementFormDialog, ResumeAfterFormDialog);
                }
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("Muitas tentativas inválidas, tente novamente.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Obrigado por contactar nosso time de suporte. O número do seu pedido é {ticketNumber}.");

            context.Wait(MessageReceivedAsync);
        }

        private IForm<Engagement> BuildEngagementForm()
        {
            var builder = new FormBuilder<Engagement>()
                .Field(new FieldReflector<Engagement>(nameof(Engagement.Category))
                    .SetType(null)
                    .SetAllowsMultiple(false)
                    .SetFieldDescription("Categoria")
                    .SetDefine(DefineCategory))
                .Field(new FieldReflector<Engagement>(nameof(Engagement.Location))
                    .SetType(null)
                    .SetAllowsMultiple(false)
                    .SetFieldDescription("Localização")
                    .SetDefine(DefineLocation))
                .Field(new FieldReflector<Engagement>(nameof(Engagement.Offer))
                    .SetType(null)
                    .SetAllowsMultiple(false)
                    .SetFieldDescription("Oferta")
                    .SetDefine(DefineOffer))
                .Field(nameof(Engagement.EngagementDescription))
                .Field(nameof(Engagement.Client))
                .Field(nameof(Engagement.Partner))
                .Field(new FieldReflector<Engagement>(nameof(Engagement.EngagementType))
                    .SetType(null)
                    .SetAllowsMultiple(false)
                    .SetFieldDescription("Tipo de engajamento")
                    .SetDefine(DefineEngagementType))
                .Field(nameof(Engagement.OpportunityId))
                .Field(new FieldReflector<Engagement>(nameof(Engagement.PartnerInDelivery))
                    .SetType(null)
                    .SetAllowsMultiple(false)
                    .SetFieldDescription("Parceiro envolvido na entrega")
                    .SetDefine(DefinePartnerInDelivery))
                .Field(new FieldReflector<Engagement>(nameof(Engagement.Speakers))
                    .SetType(null)
                    .SetAllowsMultiple()
                    .SetFieldDescription("Palestrantes Microsoft")
                    .SetDefine(DefineSpeakers))
                .Field(nameof(Engagement.OtherSpeakers))
                .Field(nameof(Engagement.StartDate), validate: ValidateStartDate)
                .Field(nameof(Engagement.EndDate), validate: ValidateEndDate)
                .Field(new FieldReflector<Engagement>(nameof(Engagement.Period))
                    .SetType(null)
                    .SetAllowsMultiple(false)
                    .SetFieldDescription("Período")
                    .SetDefine(DefinePeriod))
                .Field(nameof(Engagement.Participants))
                .Field(new FieldReflector<Engagement>(nameof(Engagement.Room))
                    .SetType(null)
                    .SetAllowsMultiple(false)
                    .SetFieldDescription("Sala")
                    .SetDefine(DefineRoomName))
                .Field(nameof(Engagement.AdditionalInfo))

                //.Confirm(ConfirmCalendar)
                //.AddRemainingFields()
                .OnCompletion(processScheduling);

            SetBuilderConfiguration(builder);

            return builder.Build();
        }

        private static void SetBuilderConfiguration(IFormBuilder<Engagement> builder)
        { 
            builder.Configuration.Yes = new[] {"sim", "s", "ok"};
            builder.Configuration.No = new[] {"não", "nao", "n"};
        }
        
        private static async Task ResumeAfterFormDialog(IDialogContext context, IAwaitable<Engagement> result)
        {
            await context.PostAsync("Engajamento agendado com sucesso!");
            var test = await result;

            // TODO: Scheduling logic

        }

        #region Engagement Form definition

        private static Task<bool> DefineCategory(
            Engagement state, Field<Engagement> field)
        {
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select id, CategoryDescription as 'category' from EngagementCategory",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    field.AddDescription(details["id"].ToString(), details["category"].ToString())
                        .AddTerms(details["id"].ToString(), details["category"].ToString());
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            finally
            {
                con.Close();
            }

            return Task.FromResult(true);
        }

        private static Task<bool> DefineLocation(
            Engagement state, Field<Engagement> field)
        {
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select id, LocationDescription as 'location' from EngagementLocation",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    field.AddDescription(details["id"].ToString(), details["location"].ToString())
                        .AddTerms(details["id"].ToString(), details["location"].ToString());
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            finally
            {
                con.Close();
            }

            return Task.FromResult(true);
        }

        private static Task<bool> DefineOffer(
            Engagement state, Field<Engagement> field)
        {
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select id, OfferDescription as 'offer' from EngagementOffer",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    field.AddDescription(details["id"].ToString(), details["offer"].ToString())
                        .AddTerms(details["id"].ToString(), details["offer"].ToString());
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            finally
            {
                con.Close();
            }

            return Task.FromResult(true);
        }

        private static Task<bool> DefineEngagementType(
            Engagement state, Field<Engagement> field)
        {
            var con = new SqlConnection(Constants.ConnectionString);
            
            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select id, TypeDescription as 'type' from EngagementType",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    field.AddDescription(details["id"].ToString(), details["type"].ToString())
                        .AddTerms(details["id"].ToString(), details["type"].ToString());
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            finally
            {
                con.Close();
            }

            return Task.FromResult(true);
        }

        private static Task<bool> DefinePartnerInDelivery(
            Engagement state, Field<Engagement> field)
        {
            field.AddDescription("Sim", "Sim").AddTerms("Sim", "Sim");
            field.AddDescription("Não", "Não").AddTerms("Não", "Não");

            return Task.FromResult(true);
        }

        private static Task<bool> DefineSpeakers(
            Engagement state, Field<Engagement> field)
        {
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select id, Name from Speaker where SpeakerStatus = 1",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    field.AddDescription(details["id"].ToString(), details["name"].ToString())
                        .AddTerms(details["id"].ToString(), details["name"].ToString());
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            finally
            {
                con.Close();
            }

            return Task.FromResult(true);
        }

        private static Task<bool> DefinePeriod(
            Engagement state, Field<Engagement> field)
        {
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select id, PeriodDescription as 'period' from EngagementPeriod",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    field.AddDescription(details["id"].ToString(), details["period"].ToString())
                        .AddTerms(details["id"].ToString(), details["period"].ToString());
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            finally
            {
                con.Close();
            }

            return Task.FromResult(true);
        }

        private static Task<bool> DefineRoomName(
            Engagement state, Field<Engagement> field)
        {
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select name from room",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    field.AddDescription(details["name"], details["name"].ToString())
                        .AddTerms(details["name"], details["name"].ToString());
                }
            }
            catch (SqlException)
            {
                return Task.FromResult(false);
            }
            finally
            {
                con.Close();
            }

            return Task.FromResult(true);
        }

        private static Task<bool> DefineRoomCalendar(
            Engagement state, Field<Engagement> field)
        {
            var availabilityList = new List<string>
            {
                "03/10 - Manhã",
                "04/10 - Manhã",
                "04/10 - Tarde",
                "05/10 - Tarde",
                "06/10 - Manhã",
                "07/10 - Tarde"
            };

            foreach (var item in availabilityList)
            {
                field.AddDescription(item, item).AddTerms(item, item);
            }

            return Task.FromResult(true);
        }

        #endregion

        #region Engagement Form validation

        readonly OnCompletionAsyncDelegate<Engagement> processScheduling = async (context, state) =>
        {
            await context.PostAsync("Realizando agendamento...");
        };

        private static Task<ValidateResult> ValidateStartDate(Engagement state, object value)
        {
            var result = new ValidateResult
            {
                IsValid = true,
                Value = value
            };
            
            var startDate = (DateTime) value;
            DateTime formattedDt;

            if (DateTime.TryParse(startDate.ToString(new CultureInfo("pt-BR")), out formattedDt))
            {
                startDate = formattedDt;
            }

            if (startDate < DateTime.Today)
            {
                result.Feedback = "Data inicial inválida";
                result.IsValid = false;
            }

            return Task.FromResult(result);
        }

        private static Task<ValidateResult> ValidateEndDate(Engagement state, object value)
        {
            var result = new ValidateResult
            {
                IsValid = true,
                Value = value
            };

            var startDate = state.StartDate;
            DateTime formattedDt;

            if (DateTime.TryParse(state.StartDate.ToString(new CultureInfo("pt-BR")), out formattedDt))
            {
                startDate = formattedDt;
            }
            
            var endDate = (DateTime) value;
            
            if (DateTime.TryParse(
                endDate.ToString(new CultureInfo("pt-BR")), out formattedDt))
            {
                endDate = formattedDt;
            }

            if (endDate < startDate)
            {
                result.Feedback = "Data final inválida";
                result.IsValid = false;
            }

            return Task.FromResult(result);
        }

        private static Task<PromptAttribute> ConfirmCalendar(Engagement state)
        {
            var scheduleMsg = string.Empty;

            if (state.Calendar.Count == 1)
            {
                scheduleMsg = state.Calendar[0];
            }
            else
            {
                scheduleMsg = state.Calendar
                    .Aggregate(scheduleMsg, (current, schedule) =>
                    current + $"{schedule}, ");

                // Replacing ", " by "."
                scheduleMsg = scheduleMsg.Substring(0, scheduleMsg.Length - 2);
            }

            var confirmMessage = $"Sala {state.Room} para {scheduleMsg}. Confirmar?";
            return Task.FromResult(new PromptAttribute(confirmMessage));
        }

        #endregion
    }
}
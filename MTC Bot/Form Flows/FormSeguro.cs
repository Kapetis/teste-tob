using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MTC_Bot.Dialogs
{
    [Serializable]
    public class FormSeguro
    {
        [Prompt("Qual a cidade origem da sua viagem? {||}")]
        public CidadesOrigem? Origem;
        [Prompt("Qual o país destino da sua viagem? {||}")]
        public PaisesDestino? Destino;
        [Prompt("Estará visitando algum país da Europa? {||}")]
        public VisitandoEuropa? VisitaEuropa;
        [Prompt("Qual a data da sua partida? Escreva no padrão dd/mm/aaaa")]
        [Template(TemplateUsage.NotUnderstood, "Eu não entendi \"{0}\".", "Tente novamente com o formato dd/mm/aaaa, eu não entendi \"{0}\".")]
        [Pattern(@"^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((19|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((19|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((19|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$")]
        public DateTime DataPartida { get; set; }
        [Prompt("Qual a data do seu retorno? Escreva no padrão dd/mm/aaaa")]
        [Template(TemplateUsage.NotUnderstood, "Eu não entendi \"{0}\".", "Tente novamente com o formato dd/mm/aaaa, eu não entendi \"{0}\".")]
        [Pattern(@"^(((0[1-9]|[12]\d|3[01])\/(0[13578]|1[02])\/((19|[2-9]\d)\d{2}))|((0[1-9]|[12]\d|30)\/(0[13456789]|1[012])\/((19|[2-9]\d)\d{2}))|((0[1-9]|1\d|2[0-8])\/02\/((19|[2-9]\d)\d{2}))|(29\/02\/((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))))$")]
        public DateTime DataRetorno { get; set; }
        [Prompt("Quantos passageiros menores de 70 anos? {||}")]
        public Menor70? passageirosMenorde70;
        [Prompt("Quantos passageiros maiores de 70 anos? {||}")]
        public Maior70? passageirosMaiorde70;
        [Prompt("Qual o motivo da sua viagem? {||}")]   
        public MotivoViagemOptions? MotivoDaViagem;

        public static IForm<FormSeguro> SeguroBuildForm()
        {
            OnCompletionAsyncDelegate<FormSeguro> processandoCalculo = async (context, state) =>
            {
                await context.PostAsync($"Ok. Calculando seguro para {state.Destino} de {state.Origem} indo no dia {state.DataPartida} e voltando no dia {state.DataRetorno}");
            };

            return new FormBuilder<FormSeguro>()
                .Field(nameof(Origem))
                .Field(nameof(Destino))
                .Field(nameof(VisitaEuropa))
                .Field(nameof(DataPartida), validate: ValidateStartDate)
                .Field(nameof(DataRetorno), validate: ValidateEndDate)
                .Field(nameof(passageirosMenorde70))
                .Field(nameof(passageirosMaiorde70))
                .Field(nameof(MotivoDaViagem))
                .OnCompletion(processandoCalculo)
                .Build();
        }

        private static Task<ValidateResult> ValidateStartDate(FormSeguro state, object value)
        {
            var result = new ValidateResult
            {
                IsValid = true,
                Value = value
            };
            var startDate = (DateTime)value;
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

        private static Task<ValidateResult> ValidateEndDate(FormSeguro state, object value)
        {
            var result = new ValidateResult
            {
                IsValid = true,
                Value = value
            };
            var startDate = state.DataPartida;
            DateTime formattedDt;
            if (DateTime.TryParse(state.DataPartida.ToString(new CultureInfo("pt-BR")), out formattedDt))
            {
                startDate = formattedDt;
            }
            var endDate = (DateTime)value;
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
    }

    public enum CidadesOrigem { SP, RJ, ES, MG, PE, SC, RS}

    public enum PaisesDestino { Brasil, EstadosUnidos, Canada}
    
    public enum MotivoViagemOptions { Lazer, Negocios, Estudos, VisitaAmigoOuParente}
    
    public enum VisitandoEuropa { Sim, Não }

    public enum Maior70 { Zero, Um, Dois, Três, Quatro, Cinco }

    public enum Menor70 { Zero, Um, Dois, Três, Quatro, Cinco }
}
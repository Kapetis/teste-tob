using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;

namespace MTC_Bot.Models
{
    [Template(TemplateUsage.NotUnderstood, 
        "Eu não entendi \"{0}\". Por favor, tente novamente.", 
        "Tente novamente, eu não entendi \"{0}\".", 
        "Desculpe, eu não entendi \"{0}\". Por favor, tente novamente.")]
    //[Template(TemplateUsage.EnumSelectOne, "Selecione {&}: {||}")]
    [Template(TemplateUsage.EnumSelectOne, "{&}: {||}", 
        ChoiceStyle = ChoiceStyleOptions.PerLine)]
    [Template(TemplateUsage.EnumSelectMany, "{&}: {||}",
        ChoiceStyle = ChoiceStyleOptions.PerLine)]

    // TODO: Help
    //[Template(TemplateUsage.Help, "ajuda1\n\n* ajuda2.")]

    [Serializable]
    public class Engagement
    {
        [Prompt("Qual a categoria do engajamento?")]
        public string Category { get; set; }

        [Prompt("Qual a localização?")]
        public string Location { get; set; }

        [Prompt("Qual a oferta (MTC)?")]
        public string Offer { get; set; }

        [Prompt("Forneça uma descrição para o engajamento/reunião")]
        public string EngagementDescription { get; set; }

        [Prompt("Qual o nome do cliente?")]
        public string Client { get; set; }

        [Optional]
        [Prompt("Qual o nome do parceiro? (opcional)")]
        public string Partner { get; set; }

        [Prompt("Qual o tipo do engajamento?")]
        public string EngagementType { get; set; }

        [Prompt("Qual é o Opportunity ID?")]
        public string OpportunityId { get; set; }

        [Prompt("Parceiro envolvido na entrega?")]
        public string PartnerInDelivery { get; set; }

        [Prompt("Plestrantes")]
        public List<string> Speakers { get; set; }

        [Optional]
        [Prompt("Outros palestrantes (opcional)")]
        public string OtherSpeakers { get; set; }

        [Prompt("Qual a data inicial do engajamento? (Ex.: 15/10/16)")]
        public DateTime StartDate { get; set; }

        [Prompt("Qual a data final do engajamento? (Ex.: 15/10/16)")]
        public DateTime EndDate { get; set; }

        [Prompt("Qual o período do engajamento?")]
        public string Period { get; set; }

        [Numeric(1, int.MaxValue)]
        [Prompt("Quantos participantes?")]
        public int Participants { get; set; }

        [Prompt("Selecione uma sala para o engajamento")]
        public string Room { get; set; }

        [Optional]
        [Prompt("Informações adicionais (opcional)")]
        public string AdditionalInfo { get; set; }

        [Prompt("Disponibilidade da sala")]
        public List<string> Calendar { get; set; }
    }
}
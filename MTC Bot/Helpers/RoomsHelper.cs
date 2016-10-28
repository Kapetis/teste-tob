using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace MTC_Bot.Helpers
{
    public class RoomsHelper
    {
        public static async Task<string> DetailedRoomInfoAsync(IBotToUser context, string roomName)
        {
            var replyMessage = string.Empty;
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select id, name, abbreviation from room " +
                                  $"where abbreviation like '{roomName}%' or name like '{roomName}%'",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var rooms = cmd.ExecuteReader();

                if (!rooms.HasRows)
                {
                    con.Close();
                    await context.PostAsync("Sala não encontrada");
                }
                else
                {
                    if (rooms.Read())
                    {
                        var resultMessage = context.MakeMessage();
                        resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        resultMessage.Attachments = new List<Attachment>();

                        // Only PNG images is supported
                        var cardImages = new List<CardImage>
                        {
                            // https://upload.wikimedia.org/wikipedia/commons/8/8b/Microsoft_logo_(2012)_modified.png
                            
                            new CardImage("https://upload.wikimedia.org/wikipedia/commons/8/8b/Microsoft_logo_(2012)_modified.png")
                        };

                        var plCard = new HeroCard()
                        {
                            Title = $"{rooms["name"]}",
                            Images = cardImages,
                            //Buttons = cardButtons
                        };

                        var plAttachment = plCard.ToAttachment();
                        resultMessage.Attachments.Add(plAttachment);

                        await context.PostAsync(resultMessage);
                        await RoomDetailsAsync(context, rooms["id"].ToString());
                        await RoomItemsAsync(context, rooms["id"].ToString());
                    }

                    //await context.PostAsync("Deseja realizar agendamentos? Acesse http://aka.ms/requestmtcsp");
                }
            }
            catch (Exception e)
            {
                replyMessage = e.ToString();
            }
            finally
            {
                con.Close();
            }

            return replyMessage;
        }

        private static async Task RoomDetailsAsync(IBotToUser context, string roomId)
        {
            var msg = string.Empty;
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select l.name, l.capacity, l.info " +
                                  "from Room r inner join Layouts l on l.room = r.Id " +
                                  $"where r.id = '{roomId}'",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                while (details.Read())
                {
                    var layoutInfo = details["info"].ToString();

                    if (!string.IsNullOrEmpty(layoutInfo))
                        layoutInfo += ".";

                    msg += $"* {details["name"]} para {details["capacity"]} pessoas. {layoutInfo}\n\n";
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
            }
            finally
            {
                con.Close();
            }

            await context.PostAsync(msg);
        }

        private static async Task RoomItemsAsync(IBotToUser context, string roomId)
        {
            var msg = string.Empty;
            var con = new SqlConnection(Constants.ConnectionString);

            try
            {
                con.Open();

                var cmd = new SqlCommand
                {
                    CommandText = "select item.name from Room r " +
                                  "inner join RoomInfo i on r.Id = i.room " +
                                  "inner join RoomItems item on item.id = i.Info " +
                                  $"where r.id = '{roomId}'",
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var details = cmd.ExecuteReader();

                if (details.HasRows)
                {
                    msg = "Infraestrutura: ";

                    while (details.Read())
                        msg += $"{details["name"]}, ";

                    // Replacing ", " by "."
                    msg = msg.Substring(0, msg.Length - 2) + ".";
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
            }
            finally
            {
                con.Close();
            }

            if (!string.IsNullOrEmpty(msg))
                await context.PostAsync(msg);
        }
    }
}
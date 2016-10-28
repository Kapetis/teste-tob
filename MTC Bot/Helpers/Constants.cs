namespace MTC_Bot.Helpers
{
    public class Constants
    {
        public const string ConnectionString = "Server=tcp:mtcspbotsqlserver.database.windows.net,1433;Initial Catalog=mtcspBotDB;Persist Security Info=False;User ID=botadm;Password=pass@word1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        //public const string LuisId = "5baa2f6d-75f8-452e-808b-dbfa0ef1a5fb";
        //public const string LuisId = "b8a3a627-bb4d-43ab-9ee0-1e2621dca956";
        //ORIGINAL
        //public const string LuisId = "9392e0e2-a01a-45c3-923c-12fde280e3ce";
        //Lucas
        public const string LuisId = "19727872-2b4f-457c-b6d1-f11bd77dd7ba";
        //public const string LuisSubscriptionKey = "030d3adb3e8c4c1897f087b59359ecdc";
        //public const string LuisSubscriptionKey = "1646b8824d31482a86262f5d9d33a2d8";
        //ORIGINAL
        //public const string LuisSubscriptionKey = "f8efc1a9b8934643a0d256e366608d9d";
        //Lucas
        public const string LuisSubscriptionKey = "754beacfb49749b19a45bb0507a1815f";
        public const string UsernameKey = "UserName";

        //Lucas Teste Porto Seguro - https://api.projectoxford.ai/luis/v1/application?id=19727872-2b4f-457c-b6d1-f11bd77dd7ba&subscription-key=754beacfb49749b19a45bb0507a1815f

        #region Intents

        public const string ListDetailedRoom = "ListRoom";
        public const string ListAllRooms = "ListAllRooms";
        public const string GreetMessage = "Greet";
        public const string ScheduleRooms = "Schedule";

        #endregion
    }
}
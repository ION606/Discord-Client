using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Discord_Client_Custom
{
    internal class user
    {
        private static string uid;
        private static string username;
        private static string discriminator;
        private static string avatar;
        private static string? nickname = null;

        public user(JsonNode specs)
        {
            uid = (string)specs["id"];
            username = (string)specs["username"];
            discriminator = (string)specs["discriminator"];
            avatar = (string)specs["avatar"];
        }

        public string getUserName()
        {
            return username;
        }


        public string getAvatar()
        {
            return avatar;
        }

        public string getId() { return uid; }


        public string toString()
        {
            return "{\n\tuid: " + uid + "\n\tusername: " + username + "\n\tdiscriminator: " + discriminator + "\n\tavatar: " + avatar + "\n}";
        }
    }
}

using Discord_Client_Custom.Channels;
using System.Configuration.Internal;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Discord_Client_Custom.client_internals
{
    public partial class Client
    {
        internal class userMain {
            JsonNode customStatus;
            string locale;
            string theme;

            bool verified;
            string username;
            string discriminator;
            int flags;
            string phone;
            string email;
            string id;
            string bio;
            string? banner; //Move these to config file maybe?
            string banner_color;
            Image avatar;
            string accent_color;

            JsonNode[] channels;
            user[] relationships;
            
            private async void getAvatarHelper(string? avatarUrl) {
                string iconUrl = "https://cdn.discordapp.com/avatars/" + id + "/" + avatarUrl + ".png?size=32";

                //null user
                if (avatarUrl == null)
                {
                    iconUrl = "https://discord.com/assets/1f0bfc0865d324c2587920a7d80c609b";
                }

                avatar = await ChannelObj.getIconStream(iconUrl);
                avatar.Tag = iconUrl;
            }


            public userMain(JsonNode uConfigs, JsonNode uObj, JsonNode[] chnls, user[] relshnshps) {
                customStatus = uConfigs["custom_status"];
                customStatus["status"] = JsonNode.Parse(uConfigs["status"].ToJsonString());

                locale = uConfigs["locale"].ToString();
                theme = uConfigs["theme"].ToString();

                verified = (bool)uObj["verified"];
                username = uObj["username"].ToString();
                discriminator = uObj["discriminator"].ToString();
                flags = (int)uObj["flags"];
                phone = uObj["phone"].ToString();
                email = uObj["email"].ToString();
                id = uObj["id"].ToString();
                bio = uObj["bio"].ToString();
                banner_color = uObj["banner_color"].ToString();
                //banner = uObj["banner"].ToString();

                accent_color = uObj["accent_color"].ToString();
                channels = chnls;
                relationships = relshnshps;
                getAvatarHelper(uObj["avatar"].ToString());
            }


            public Image getAvatar() { return avatar; }
            public string getId() { return id; }
            public JsonNode getStatus() { return this.customStatus; }
        }

        userMain uMain;

        internal userMain getUserMain()
        {
            return this.uMain;
        }

        private void printAll(JsonNode us, JsonNode um, user[] rel, JsonNode[] channels)
        {
            Console.WriteLine("===============User Settings================");
            Console.WriteLine(us);

            Console.WriteLine("================User Main===============");
            Console.WriteLine(um);

            Console.WriteLine("===============Relationships================");
            Console.WriteLine(rel.Length);

            Console.WriteLine("================Channels===============");
            Console.WriteLine(channels);
        }

        //configs is the "d" part of the READY event
        public Client(JsonNode configs) {
            var userSettings = configs["user_settings"];
            //var user_guild_settings = configs["user_guild_settings"];
            var uObj = configs["user"];
            var relationships = Array.ConvertAll<JsonNode, user>(configs["relationships"].AsArray().ToArray(), (item) => { return new user(item); });
            var channels = configs["private_channels"].AsArray().ToArray();
            Debug.WriteLine("\n\n\noh\n\n\n");


            uMain = new userMain(userSettings, uObj, channels, relationships);
            //printAll(userSettings, userMain, relationships, channels);
        }

        public static explicit operator Client(Control v)
        {
            throw new NotImplementedException();
        }
    }
}

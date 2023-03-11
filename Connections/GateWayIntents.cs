using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Client_Custom.Connections
{
    internal partial class GateWayIntents
    {
        public enum GatewayIntent
        {
             Guilds = 1 << 0,
             GuildMembers = 1 << 1,
             GuildModeration = 1 << 2,
             GuildEmojisStickers = 1 << 3,
             GuildIntegrations = 1 << 4,
             GuildWebhooks = 1 << 5,
             GuildInvites = 1 << 6,
             GuildVoiceStates = 1 << 7,
             GuildPresences = 1 << 8,
             GuildMessages = 1 << 9,
             GuildMessageReactions = 1 << 10,
             GuildMessageTyping = 1 << 11,
             DirectMessages = 1 << 12,
             DirectMessageReactions = 1 << 13,
             DirectMessageTyping = 1 << 14,
             MessageContent = 1 << 15,
             GuildScheduledEvents = 1 << 16,
             AutoModerationConfiguration = 1 << 20,
             AutoModerationExecution = 1 << 21,
        }

        public static int sum(GatewayIntent[] intents)
        {
            int sum = 0;
            foreach (var i in intents) sum += (int)i;
            return sum;
        }

        public int value;
        public GatewayIntent[] intents;

        //Initializes for DMs ONLY (so far...)
        public GateWayIntents(bool isDmOnly)
        {
            if (isDmOnly)
            {
                GatewayIntent[] intentsTemp = { GatewayIntent.DirectMessages, GatewayIntent.DirectMessageReactions, GatewayIntent.DirectMessageTyping };
                this.intents = intentsTemp;
                this.value = sum(intents);
            } else
            {
                throw new NotImplementedException();
            }
        }
    }
}

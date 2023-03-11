using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Websocket.Client;
using Discord_Client_Custom.client_internals;



namespace Discord_Client_Custom.Connections
{
    internal class Connection
    {
        private static Uri gateWayUrl = new Uri("wss://gateway.discord.gg/?v=10&encoding=json");
        private static ManualResetEvent exitEvent = new ManualResetEvent(false);
        private static int heartBeatSequence = 0;
        private int heartBeatInterval = 0;
        int heartBeatCounter = 0;
        private WebsocketClient WS;
        private GateWayIntents intents;
        public JsonNode uInfoRaw;


        private async void heartBeat(object o)
        {
            var obj = new { op = 1, d = heartBeatSequence };
            var toSend = System.Text.Json.JsonSerializer.Serialize(obj);

            WS.Send(toSend);
            heartBeatCounter++;
        }


        private void startHeartBeat(object o)
        {
            var confObj = JsonNode.Parse(o.ToString());
            heartBeatInterval = (int)confObj["d"]["heartbeat_interval"];
            Debug.WriteLine("INTERVAL SET TO: " + heartBeatInterval.ToString());

            var idObj = new
            {
                op = 2,
                d = new
                {
                    token = MsgRequests.userToken,
                    intents = intents.value, //61440,
                    properties = new
                    {
                        os = "linux",
                        browser = "my_library",
                        device = "my_library"
                    }
                }
            };

            WS.Send(System.Text.Json.JsonSerializer.Serialize(idObj));

            var autoEvent = new AutoResetEvent(false);
            var stateTimer = new System.Threading.Timer(heartBeat, autoEvent, 0, heartBeatInterval);
        }


        private static void statusUpdate(object statusObj)
        {
            //Console.WriteLine(statusObj.ToString() + "\n\n");
        }


        private static void messageEvent(object msgObj)
        {
            Console.WriteLine(msgObj.ToString() + "\n\n");
        }

        public async Task<JsonNode> connect(FlowLayoutPanel dmFlowPannel)
        {
            using (var client = new WebsocketClient(gateWayUrl))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info =>
                    Debug.WriteLine($"Reconnection happened, type: {info.Type}"));


                //"READY" info dump handling
                client.MessageReceived.Where((msg) =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    return (int)msgObj["op"] == 0 && msgObj["t"].ToString() == "READY";
                }).Subscribe((msg) =>
                {
                    var configs = JsonNode.Parse(msg.Text);
                    //var c = new Client(configs["d"], dmFlowPannel);
                    uInfoRaw = configs["d"];
                });


                client.MessageReceived.Where((msg) =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    return ((string)msgObj["t"] == "PRESENCE_UPDATE");
                }).Subscribe(statusUpdate);


                // client.MessageReceived.Subscribe(msg => Debug.WriteLine($"Message received: {msg}"));

                client.MessageReceived.Where((msg) =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    return ((int)msgObj["op"] == 10);
                }).Subscribe((object o) =>
                {
                    intents = new GateWayIntents(true);
                    //throw new Exception(intents.ToString());
                    startHeartBeat(o);
                });


                client.MessageReceived.Where((msg) =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    return ((string)msgObj["t"] == "PRESENCE_UPDATE");
                }).Subscribe(statusUpdate);

                client.MessageReceived.Where((msg) =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    return ((int)msgObj["op"] == 0) && (string)msgObj["t"] == "MESSAGE_CREATE";
                }).Subscribe(messageEvent);


                client.Start();

                Task.Run(() => client.Send("{ message }"));
                WS = client;

                //exitEvent.WaitOne();
                
                while (uInfoRaw == null) { }
                return uInfoRaw;
            }
        }

        public Connection() {
            
        }
    }
}

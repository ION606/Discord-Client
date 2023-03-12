using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using Websocket.Client;
using Discord_Client_Custom.client_internals;
using Discord_Client_Custom.Channels;
using System.ComponentModel;

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
        private object gateWayProperties;
        private readonly mainPage pageRef;


        private async void heartBeat(object o)
        {
            var obj = new { op = 1, d = heartBeatSequence };
            var toSend = System.Text.Json.JsonSerializer.Serialize(obj);

            WS.Send(toSend);
            heartBeatCounter++;
            Debug.WriteLine("PING");
        }


        private void startHeartBeat(object o)
        {
            var confObj = JsonNode.Parse(o.ToString());
            heartBeatInterval = (int)confObj["d"]["heartbeat_interval"];
            Debug.WriteLine("INTERVAL SET TO: " + heartBeatInterval.ToString());

            gateWayProperties = new
            {
                os = "linux",
                browser = "ion_",
                device = "my_library"
            };

            var idObj = new
            {
                op = 2,
                d = new
                {
                    token = MsgRequests.userToken,
                    intents = intents.value, //61440,
                    properties = gateWayProperties
                }
            };

            WS.Send(System.Text.Json.JsonSerializer.Serialize(idObj));

            var autoEvent = new AutoResetEvent(false);
            var stateTimer = new System.Threading.Timer(heartBeat, autoEvent, 0, heartBeatInterval);
        }


        private void getStatusUpdate(object statusObj)
        {
            //Console.WriteLine(statusObj.ToString() + "\n\n");
        }


        //{"t":"MESSAGE_CREATE","s":3,"op":0,"d":{"type":0,"tts":false,"timestamp":"2023-03-12T16:13:52.238000+00:00","referenced_message":null,"pinned":false,"nonce":"1084509575685603328","mentions":[],"mention_roles":[],"mention_everyone":false,"id":"1084509576445042838","flags":0,"embeds":[],"edited_timestamp":null,"content":"ping","components":[],"channel_id":"907088809169158164","author":{"username":"1.1.5","public_flags":0,"id":"720349017829015633","display_name":null,"discriminator":"4592","avatar_decoration":null,"avatar":null},"attachments":[]}}
        private void messageEvent(object msgObj)
        {
            pageRef.insertMessageObj(msgObj);
        }


        private bool isRunning(bool printToDebug = false)
        {
            if (printToDebug) Debug.WriteLine("WEBSOCKET SERVER IS RUNNING? " + WS.IsRunning.ToString());
            return WS.IsRunning;
        }


        // Setters

        // https://discord.com/developers/docs/topics/gateway-events#update-presence
        public async void setStatusUpdate(string status)
        {
            var idObj = new
            {
                op = 3,
                d = new
                {
                    since = 0,
                    activities = new object[0],
                    status = status,
                    afk = false
                }
            };

            var objToSend = System.Text.Json.JsonSerializer.Serialize(idObj);
            WS.Send(objToSend);
        }


        public async void connect(FlowLayoutPanel dmFlowPannel)
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
                    Debug.WriteLine("\n\nlhkdsfgjhdsgfhsjdgf\n\n");
                    uInfoRaw = configs["d"];
                });


                client.MessageReceived.Where((msg) =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    return ((string)msgObj["t"] == "PRESENCE_UPDATE");
                }).Subscribe(getStatusUpdate);


                //client.MessageReceived.Subscribe(msg => Console.WriteLine($"Message received: {msg}"));

                /*client.MessageReceived.Where(msg =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    if (msgObj["t"] != null) return msgObj["t"].ToString() != "READY";
                    else return true;
                }).Subscribe((msg) => Debug.WriteLine($"Message received: {msg}"));*/


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
                }).Subscribe(getStatusUpdate);


                client.MessageReceived.Where((msg) =>
                {
                    var msgObj = JsonNode.Parse(msg.Text);
                    return ((int)msgObj["op"] == 0) && msgObj["t"].ToString() == "MESSAGE_CREATE";
                }).Subscribe(messageEvent);


                client.DisconnectionHappened.Subscribe((info) => Debug.WriteLine(info.ToString()));
                client.Start();

                WS = client;

                await Task.Run(() => client.Send("{ message }"));

                exitEvent.WaitOne();

                //return this.uInfoRaw;
            }

        }

        public Connection(mainPage pageRefTemp)
        {
            this.pageRef = pageRefTemp;
        }
    }
}

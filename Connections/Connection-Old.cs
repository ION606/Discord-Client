using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace Discord_Client_Custom.Connections
{
    internal class ConnectionOld
    {
        private static string gateWayUrl = "wss://gateway.discord.gg/?v=10&encoding=json";
        private static string baseUrl = "discord.com/api/gateway";
        public int ReceiveBufferSize { get; set; } = 8192;

        private static int heartBeatSequence = 0;
        int heartBeatCounter = 0;

        private ClientWebSocket WS;
        private CancellationTokenSource CTS;

        private async void heartBeat(object o)
        {
            if (heartBeatCounter == 3)
            {
                Debug.WriteLine("\n\n\njDHsfKJdshfLK<DShflKJDSHflKJDSn\n\n\n");
                var idObj = new
                {
                    op = 2,
                    d = new
                    {
                        token = MsgRequests.userToken,
                        intents = 513, //61440,
                        properties = new {
                            os = "linux",
                            browser = "my_library",
                            device = "my_library"
                        }
                    }
                };

                byte[] idData = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(idObj));
                await WS.SendAsync(idData, WebSocketMessageType.Text, false, CancellationToken.None);

                var buffer = new byte[ReceiveBufferSize];
                await WS.ReceiveAsync(buffer, CancellationToken.None);

            }

            var obj = new { op = 1, d = heartBeatSequence };
            var toSend = System.Text.Json.JsonSerializer.Serialize(obj);

            byte[] data = Encoding.ASCII.GetBytes(toSend);
            await WS.SendAsync(data, WebSocketMessageType.Text, false, CancellationToken.None);
            Debug.WriteLine("PING");
            heartBeatCounter++;
        }


        public async Task ConnectAsync()
        {
            Debug.WriteLine("CONNECTING....");

            if (WS != null)
            {
                if (WS.State == WebSocketState.Open) return;
                else WS.Dispose();
            }

            WS = new ClientWebSocket();

            if (CTS != null) CTS.Dispose();
            CTS = new CancellationTokenSource();

            await WS.ConnectAsync(new Uri(gateWayUrl), CTS.Token);
            await Task.Factory.StartNew(ReceiveLoop, CTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }


        public async Task DisconnectAsync()
        {
            if (WS is null) return;
            // TODO: requests cleanup code, sub-protocol dependent.
            if (WS.State == WebSocketState.Open)
            {
                CTS.CancelAfter(TimeSpan.FromSeconds(2));
                await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            WS.Dispose();
            WS = null;
            CTS.Dispose();
            CTS = null;
        }


        private async Task ReceiveLoop()
        {
            var loopToken = CTS.Token;
            MemoryStream outputStream = null;
            WebSocketReceiveResult receiveResult = null;
            var buffer = new byte[ReceiveBufferSize];
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(ReceiveBufferSize);
                    do
                    {
                        receiveResult = await WS.ReceiveAsync(buffer, CTS.Token);
                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                            outputStream.Write(buffer, 0, receiveResult.Count);
                    }
                    while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                    outputStream.Position = 0;
                    ResponseReceived(outputStream);
                }
            }
            catch (TaskCanceledException e) { Debug.WriteLine(e); }
            finally
            {
                outputStream?.Dispose();
            }
        }

        /*
        private async Task<JsonNode> SendMessageAsync<RequestType>(RequestType message)
        {
            // TODO: handle serializing requests and deserializing responses, handle matching responses to the requests.
        }*/

        private async void ResponseReceived(Stream inputStream)
        {
            var responseJson = JsonNode.Parse(inputStream);
            int opCode = (int)responseJson["op"];

            Debug.WriteLine("=======================================================\nREADY\n");
            Debug.WriteLine(responseJson);
            Debug.WriteLine("=======================================================");

            //Heartbeat stuff
            if (opCode == 0) {
                Debug.WriteLine("=======================================================\nREADY\n");
                Debug.WriteLine(responseJson);
                Debug.WriteLine("=======================================================");
            }
            else if (opCode == 11)
            {
                int hinterval = (int)responseJson["d"];
                var toSend = "op: 1\r\nd: " + responseJson["d"];

                Debug.WriteLine(responseJson);
                byte[] data = Encoding.ASCII.GetBytes(toSend);
                await WS.SendAsync(data, WebSocketMessageType.Text, false, CancellationToken.None);
            }
            else if (opCode == 10) {
                Debug.WriteLine("=======================================================");
                Debug.WriteLine("CONNECTION ESTABLISHED");
                Debug.WriteLine("=======================================================");

                int heartBeatInterval = (int)responseJson["d"]["heartbeat_interval"];

                var autoEvent = new AutoResetEvent(false);
                var stateTimer = new System.Threading.Timer(heartBeat, autoEvent, 0, heartBeatInterval);
                autoEvent.WaitOne();
            }
            else
            {
                Debug.WriteLine("=======================================================");
                Debug.WriteLine(responseJson);
                Debug.WriteLine("=======================================================");
            }

            // TODO: handle deserializing responses and matching them to the requests.
            // IMPORTANT: DON'T FORGET TO DISPOSE THE inputStream!
            inputStream.DisposeAsync();
        }


        public ConnectionOld()
        {
            
        }


        public void Dispose() => DisconnectAsync().Wait();

    }
}

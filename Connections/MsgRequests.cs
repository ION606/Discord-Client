using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace Discord_Client_Custom.Connections
{
    internal class MsgRequests
    {
        private static string dmMePath = "https://discord.com/api/users/@me/channels";
        private static string dmGetMsgsBasepath = "https://discord.com/api/channels/{{id}}/messages?limit=10";
        public static string userToken = System.Environment.GetEnvironmentVariable("userToken");

        public MsgRequests(string url, string reqType)
        {
            //userToken = System.Environment.GetEnvironmentVariable("userToken");
        }



        public async static Task<JsonNode> sendMessage(string content, string ep)
        {
            Debug.WriteLine(ep);

            try
            {
                var client = new HttpClient();

                var toSend = new Dictionary<string, string>
                {
                    { "content", content },
                };

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ep),
                    Method = HttpMethod.Post,
                    Content = new FormUrlEncodedContent(toSend)
                };

                request.Headers.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //CHANGE THIS TO CONFIG LATER
                request.Headers.Add("Authorization", userToken);
                request.Headers.Add("User-Agent", ".NET Foundation Repository Reporter");


                var taskResponse = await client.SendAsync(request);
                var responseContent = await taskResponse.Content.ReadAsStringAsync();
                JsonNode responseJSON = JsonNode.Parse(responseContent);

                if (taskResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Debug.Write(responseJSON["message"]);
                    return null;
                }

                return responseJSON;
            } catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }


        public static async Task<JsonNode> getChannels()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(dmMePath),
                Method = HttpMethod.Get,
            };

            request.Headers.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //CHANGE THIS TO CONFIG LATER
            request.Headers.Add("Authorization", userToken);
            request.Headers.Add("User-Agent", ".NET Foundation Repository Reporter");

            var taskResponse = await client.SendAsync(request);
            var responseContent = await taskResponse.Content.ReadAsStringAsync();

            if (responseContent == null) { return null; }

            JsonNode responseJSON = JsonNode.Parse(responseContent);

            if (taskResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Debug.Write(responseJSON["message"]);
                return null;
            }


            return responseJSON;
        }

        //Returns the 
        public static async Task<JsonNode> getMessages(string cid, string? lastId = null)
        {
            string newUrl = dmGetMsgsBasepath.Replace("{{id}}", cid);
            if (lastId != null)
            {
                newUrl += "&before=" + lastId;
            }

            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(newUrl),
                Method = HttpMethod.Get,
            };

            request.Headers.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Headers.Add("Authorization", userToken);
            request.Headers.Add("User-Agent", ".NET Foundation Repository Reporter");

            var taskResponse = await client.SendAsync(request);
            var responseContent = await taskResponse.Content.ReadAsStringAsync();

            if (responseContent == null) { return null; }

            JsonNode responseJSON = JsonNode.Parse(responseContent);

            if (taskResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Debug.Write(responseJSON["message"]);
                return null;
            }


            return responseJSON;
        }
    }
}

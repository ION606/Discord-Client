using Discord_Client_Custom.client_internals;
using Discord_Client_Custom.Connections;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using static Discord_Client_Custom.client_internals.Client;

namespace Discord_Client_Custom.Channels
{
    public class ChannelObj
    {
        public static async Task<Image> getIconStream(string imageUrl)
        {
            Image image = null;

            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
                var response = await client.SendAsync(request);

                Stream stream = await response.Content.ReadAsStreamAsync();

                image = Image.FromStream(stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("=======================================================");
                Debug.Write(ex.StackTrace);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Using URL: " + imageUrl);
                Debug.WriteLine("=======================================================");
                return null;
            }

            return image;
        }


        private static List<user> users = new List<user>();
        private static List<ChannelMsgGroup> groupedMsgs = new List<ChannelMsgGroup>();
        private static string cid;
        private static int ctype = 1;
        private static string? cname;
        private static string? cicon;
        private static string? cownerId;
        private string lastSent;


        public ChannelObj() { }

        //For displaying information
        public ChannelObj(JsonNode contents)
        {
            cid = contents["id"].ToString();
            ctype = (int)contents["type"];

            if (ctype == 3)
            {
                cname = contents["name"].ToString();
                cicon = contents["icon"].ToString();
                cownerId = contents["owner_id"].ToString();
            }

            //Add the users to the DM
            Array recipients = contents["recipients"].AsArray().ToArray();

            for (int i = 0; i < recipients.Length; i++)
            {
                JsonNode uObj = (JsonNode)recipients.GetValue(i);
                users.Add(new user(uObj));
                //users.Append(new user(uObj));
            }

            //Console.WriteLine(recipients.GetValue(0).ToString() + " NUM: " + recipients.Length.ToString());
        }


        //Creating Messages
        internal ChannelObj(JsonNode contents, string cid2, TableLayoutPanel dmFlowContent, Image uicon, userMain uMain)
        {
            dmFlowContent.Controls.Clear();

            //Begin message section
            Image uMainIcon = uMain.getAvatar();
            string uMainId = uMain.getId();

            //There's gonna be a faster way
            var arr = contents.AsArray().ToArray().Reverse().ToArray();
            string id_current = (string)arr[0]["author"]["id"];
            int startInd = 0;

            //Make sure the first column is consitant
            dmFlowContent.ColumnStyles.Clear();
            dmFlowContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, uicon.Width));
            dmFlowContent.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            //dmFlowContent.ColumnStyles[1].Width = 500;

            //Add all content into one "message"
            string msgContent = ""; //(string)arr[0]["content"];
            int i;

            int skipCounter = 0;
            
            for (i = 0; i < arr.Length; i++)
            {
                if ((string)arr[i]["content"] == "")
                {
                    skipCounter++;
                    continue;
                }

                //Remove when attatcmenent integration is added
                if (msgContent.Length == 0 && skipCounter == i) id_current = (string)arr[i]["author"]["id"];

                string authorName = (string)arr[i]["author"]["id"];

                if (authorName != id_current)
                {
                    var msgObjTemp = (JsonNode)arr[startInd];
                    msgObjTemp["content"] = msgContent;

                    if (authorName != uMainId)
                    {
                        groupedMsgs.Add(new ChannelMsgGroup(msgObjTemp, dmFlowContent, uMainIcon, i));
                    }
                    else
                    {
                        groupedMsgs.Add(new ChannelMsgGroup(msgObjTemp, dmFlowContent, uicon, i));
                    }

                    startInd = i;
                    id_current = authorName;
                    msgContent = "";
                }

                msgContent += arr[i]["content"] + "\n";
            }

            
            if (i != startInd)
            {
                var msgObjTemp = (JsonNode)arr[startInd];
                msgObjTemp["content"] = msgContent;

                if ((string)arr[startInd]["author"]["id"] == uMainId)
                {
                    new ChannelMsgGroup(msgObjTemp, dmFlowContent, uMainIcon, i);
                }
                else
                {
                    new ChannelMsgGroup(msgObjTemp, dmFlowContent, uicon, i);
                }
            }

            lastSent = id_current;

            //Check if this is a system DM
            if (arr[0]["author"]["system"] != null)
            {
                var cantSendLabel = new Label();
                cantSendLabel.Text = "This is a system channel, so you can't send any messages!";

                dmFlowContent.Controls.Add(cantSendLabel, 0, i + 1);
                dmFlowContent.SetColumnSpan(cantSendLabel, 2);
                dmFlowContent.ScrollControlIntoView(cantSendLabel);

                return;
            }

            //Add the text box
            var txtbx = new RichTextBox();
            txtbx.AutoWordSelection = true;
            txtbx.Size = new Size(dmFlowContent.Width - 50, 50);
            txtbx.KeyDown += async (object o, KeyEventArgs k) =>
            {
                if (k.KeyCode == Keys.Enter)
                {
                    string ep;
                    if (ctype == 1)
                    {
                        ep = "https://discord.com/api/channels/" + cid2 + "/messages";
                    } else
                    {
                        //Deal with group message stuff here.....
                        Debug.WriteLine("Message Sending has not been implemented for group DMs (yet)");
                        return;
                    }

                    var response = await MsgRequests.sendMessage(txtbx.Text, ep);
                    //JsonNode[] arr = new JsonNode[1];
                    //arr[0] = response;

                    //Add the message to chat in the app
                    groupedMsgs.Add(new ChannelMsgGroup(response, dmFlowContent, uMainIcon, i + 1));
                    txtbx.Clear();

                    i++;
                    dmFlowContent.Controls.Remove(txtbx);
                    dmFlowContent.Controls.Add(txtbx, 1, i + 1);
                    dmFlowContent.SetColumnSpan(txtbx, 2);
                } else
                {
                    //Do typing intent stuff here
                    //Connection
                }
            };

            dmFlowContent.Controls.Add(txtbx, 0, i+1);
            dmFlowContent.SetColumnSpan(txtbx, 2);
            dmFlowContent.ScrollControlIntoView(txtbx);
        }


        public string getName()
        {
            if (cname != null)
            {
                return cname;
            }

            return users[0].getUserName();
        }


        public string getId()
        {
            return cid;
        }


        public async Task<Image> getIcon()
        {
            string iconUrl;

            if (ctype == 1)
            {
                iconUrl = "https://cdn.discordapp.com/avatars/" + users[0].getId() + "/";
                string uAvatar = users[0].getAvatar();

                //null user
                if (uAvatar != null)
                {
                    iconUrl += uAvatar;
                }
                else
                {
                    iconUrl = "https://discord.com/assets/1f0bfc0865d324c2587920a7d80c609b";
                }

            }
            else
            {
                iconUrl = "https://cdn.discordapp.com/channel-icons/" + cid + "/" + cicon;
            }

            iconUrl += ".png?size=32";


            var imgRaw = await getIconStream(iconUrl);
            imgRaw.Tag = iconUrl;
            return (Image)(new Bitmap(imgRaw, new Size(32, 32))); ;

            //string rootPath = @"C:\DownloadedImageFromUrl";
            //string fileName = System.IO.Path.Combine(rootPath, "test.gif");
            //image.Save(fileName);
        }


        public string toString()
        {
            string totalString = "cid: " + cid;

            if (cname != null) { totalString += ", cname: " + cname; }

            totalString += "\nusers: [";
            for (int i = 0; i < users.Count; i++)
            {
                totalString += users[i].toString();
            }

            return totalString + "]\n";
        }
    }
}

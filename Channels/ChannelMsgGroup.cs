using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.Json.Nodes;

namespace Discord_Client_Custom.Channels
{
    internal class ChannelMsgGroup
    {
        internal class message
        {
            internal class Author
            {
                private string id;
                private string username;
                private string discriminator;
                private string avatar;

                public string getId() { return id; }
                public string getTag() { return username + discriminator; }
                public string getAvatar() { return avatar; }



                public Author(JsonNode inp)
                {
                    id = inp["id"].ToString();
                    username = inp["username"].ToString();
                    discriminator = inp["discriminator"].ToString();

                    //Check for deleted user
                    if ((string)inp["avatar"] != "")
                    {
                        avatar = inp["avatar"].ToString();
                    }
                }

                public string toString()
                {
                    return "[\n\tid: " + id + "\n\ttag: " + username + "#" + discriminator + "\n\tavatar: " + getAvatar() + "\n]";
                }
            }


            private string id;
            private string content;
            private Author msgAuthor;
            private string url;
            private DateTime timestamp;

            public message(JsonNode inp)
            {
                id = (string)inp["id"];
                url = "https://discord.com/channels/@me/" + inp["channel_id"] + "/" + inp["id"];
                content = (string)inp["content"];
                msgAuthor = new Author(inp["author"]);
                timestamp = DateTime.Parse((string)inp["timestamp"]);


                if (id == null || content == null || msgAuthor == null)
                {
                    Debug.WriteLine("id, content, or author is null");
                }
            }


            public string getId() { return id; }
            public string getUrl() { return url; }
            public DateTime getTimestamp() { return timestamp; }

            public string toString()
            {
                return "id: " + id + "\ncontent: " + content + "\nauthor: " + msgAuthor.toString();
            }


            public string getContent() { return content; }
        }


        private ContextMenuStrip createContextMenu(bool isIcon, string iconUrl)
        {
            //see profile, see icon

            //delete message, copy link, edit message

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Text = "right-click";

            if (isIcon)
            {
                var item1 = new ToolStripMenuItem("see profile");
                var item2 = new ToolStripMenuItem("see icon");
                item2.Click += new EventHandler((sender, args) =>
                {

                    //Horrid resolution, try just opening the web page
                    var icon = (PictureBox)((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = iconUrl.Remove(iconUrl.IndexOf("?size=")),
                        UseShellExecute = true
                    });
                });

                menu.Items.Add(item1);
                menu.Items.Add(item2);

            }
            else
            {
                var item1 = new ToolStripMenuItem("delete message");
                var item2 = new ToolStripMenuItem("edit message");
                var item3 = new ToolStripMenuItem("copy link");
                item3.Click += new EventHandler(async (sender, args) =>
                {
                    Clipboard.SetText(iconUrl);
                    var parent = (Label)((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
                    var oldCol = parent.ForeColor;
                    parent.ForeColor = Color.Blue;
                    ManualResetEvent resetEvent = new ManualResetEvent(false);
                    await Task.Delay(2000);
                    parent.ForeColor = oldCol;
                });

                menu.Items.Add(item1);
                menu.Items.Add(item2);
                menu.Items.Add(item3);
            }


            return menu;
        }

        private message[] msgs;
        private Label[] msglabels;

        public ChannelMsgGroup(JsonNode inpMsgs, TableLayoutPanel dmFlowContent, Image uIcon, int rowNumber)
        {
            //msgs = new message[inpMsgs.Length];
            //msglabels = new Label[inpMsgs.Length];

            //dmFlowContent.SuspendLayout();
            //Add the user icon
            var uBtn = new PictureBox
            {
                Image = uIcon,
                Tag = uIcon.Tag,
            };

            uBtn.ContextMenuStrip = createContextMenu(true, (string)uIcon.Tag);
            uBtn.Size = uIcon.Size;
            uBtn.Padding = new Padding(0, 0, 0, 50);
            dmFlowContent.Controls.Add(uBtn, 0, rowNumber);


            var msg = new message(inpMsgs);

            Label txt = new Label();
            txt.Text = msg.getContent();
            txt.Tag = msg.getId();

            txt.MaximumSize = new Size(dmFlowContent.Width - 70, 100000000);
            txt.AutoSize = true;
            txt.Visible = true;

            txt.ContextMenuStrip = createContextMenu(false, msg.getUrl());
            dmFlowContent.Controls.Add(txt, 1, rowNumber);


            /* FOR MULTIPLE MESSAGES
            for (int i = 0; i < msgs.Length; i++)
            {
                //if ((string)inpMsgs[i]["content"] == "") continue;
                msgs[i] = new message(inpMsgs[i]);

                Label txt = new Label();
                txt.Text = msgs[i].getContent();
                txt.Tag = msgs[i].getId();

                txt.MaximumSize = new Size(dmFlowContent.Width - 70, 100000000);
                txt.AutoSize = true;
                txt.Visible = true;

                txt.ContextMenuStrip = createContextMenu(false, msgs[i].getUrl());

                dmFlowContent.Controls.Add(txt, 1, rowNumber + i - 1);

                //dmFlowContent.RowStyles.Add(new RowStyle(SizeType.Absolute));
                //dmFlowContent.RowStyles[rowNumber + i].Height = txt.Height;
                //dmFlowContent.SetFlowBreak(txt, true);
            }*/

            //dmFlowContent.ResumeLayout();
        }

        public Label[] getLabels()
        {
            return msglabels;
        }
    }
}

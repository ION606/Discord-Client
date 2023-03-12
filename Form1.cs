using System.Diagnostics;
using static Discord_Client_Custom.Connections.MsgRequests;
using System.Text.Json.Nodes;
using System.Drawing.Imaging;
using Discord_Client_Custom.Channels;
using Discord_Client_Custom.Connections;
using Discord_Client_Custom.client_internals;
using System.Runtime.CompilerServices;

namespace Discord_Client_Custom
{
    public partial class mainPage : Form
    {
        private Connection con;
        public Client clientMain;
        internal ChannelObj channelCurrent;

        public mainPage()
        {
            InitializeComponent();
        }


        private async static void dm_btn_click(object sender, EventArgs e)
        {
            string cid = (string)((Button)sender).Tag;

            //Format and present messages
            //MessageBox.Show(cid);
            var o = await MsgRequests.getMessages(cid);
            var p = (mainPage)((Button)sender).Parent.Parent;
            p.dmFlowContent.Tag = cid;

            p.channelCurrent = new ChannelObj(o, cid, p.dmFlowContent, ((Button)sender).Image, p.clientMain.getUserMain());
        }


        private ComboBox createStatusBar()
        {
            var cb = new ComboBox();

            cb.Items.Add("online");
            cb.Items.Add("idle");
            cb.Items.Add("dnd");
            cb.Items.Add("offline");

            cb.SelectedIndexChanged += (object o, EventArgs a) =>
            {
                Debug.WriteLine("Status updated to " + cb.Text);
                string s = cb.Text;
                if (s == "do not disturb") s = "dnd";
                else if (s == "offline") s = "invisible";
                con.setStatusUpdate(s);
            };

            return cb;
        }


        public void insertMessageObj(object msgObj)
        {
            // Threading fix
            this.Invoke(delegate
            {
                JsonNode response = JsonNode.Parse(msgObj.ToString());
                JsonNode msg = JsonNode.Parse(response["d"].ToString());

                if (dmFlowContent.Tag == null || msg["channel_id"].ToString() != dmFlowContent.Tag.ToString())
                {
                    //Debug.WriteLine(dmFlowContent.Tag + "\n" + msg["channel_id"].ToString());
                    //Add a notif icon on the dmFlowPannel or something
                    Debug.WriteLine(false);
                }
                else
                {
                    user u = new user(msg["author"]);
                    string avatarUrl = u.getAvatar();
                    string iconUrl = "https://cdn.discordapp.com/avatars/" + u.getId() + "/" + u.getAvatar() + ".png?size=32";

                    //null user
                    if (avatarUrl == null)
                    {
                        iconUrl = "https://discord.com/assets/1f0bfc0865d324c2587920a7d80c609b.png";
                    }

                    channelCurrent.insertMessage(dmFlowContent, iconUrl, msg);
                }
            });
        }

        public async void start(JsonNode uInfoRaw)
        {
            clientMain = new Client(uInfoRaw);
            var dmsRaw = con.uInfoRaw["private_channels"].AsArray().ToArray(); // (await getChannels()).AsArray().ToArray();
            if (dmsRaw == null) { throw new NotImplementedException(); }


            for (int i = 0; i < dmsRaw.Length; i++)
            {
                var o = dmsRaw[i];
                if ((int)o["type"] == 3) { continue; }


                var co = new ChannelObj(dmsRaw[i]);

                Label lab = new Label();
                lab.Text = co.getName();
                lab.Location = new Point(30, 30 * i);
                lab.Tag = co.getId();

                //Button
                Button btn = new Button();
                btn.Location = new Point(50, 50 + 30 * i);
                btn.Tag = co.getId();

                var s = new Size(175, 40);
                btn.Size = s;

                btn.Image = await co.getIcon();
                btn.Text = co.getName();
                btn.ImageAlign = ContentAlignment.MiddleLeft;
                btn.TextImageRelation = TextImageRelation.ImageBeforeText;
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.Click += dm_btn_click;

                //dmFlowPannel.Controls.Add(lab);
                dmFlowPannel.Controls.Add(btn);
            }

            // Add status update menu
            var cb = createStatusBar();
            cb.Location = new Point(5, dmFlowContent.Height + 5);
            cb.SelectedText = clientMain.getUserMain().getStatus()["status"].ToString();
            this.Controls.Add(cb);
        }


        private async void dmFlowPannel_Paint(object sender, EventArgs e)
        {

        }


        private async void dmFlowContent_Paint(object sender, EventArgs e)
        {

        }


        private async void mainPage_Load(object sender, EventArgs e)
        {
            /*if (Environment.GetEnvironmentVariable("userToken") == null)
            {
                string promptValue = Prompt.ShowDialog("Please enter token", "prompt");
                
                Environment.SetEnvironmentVariable("userToken", promptValue);
           }*/

            con = new Connection(this);
            Task.Run(() => { con.connect(dmFlowPannel); });
            while (con.uInfoRaw == null) { }

            start(con.uInfoRaw);
        }
    }
}
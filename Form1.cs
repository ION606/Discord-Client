using System.Diagnostics;
using static Discord_Client_Custom.Connections.MsgRequests;
using System.Text.Json.Nodes;
using System.Drawing.Imaging;
using Discord_Client_Custom.Channels;
using Discord_Client_Custom.Connections;
using Discord_Client_Custom.client_internals;

namespace Discord_Client_Custom
{
    public partial class mainPage : Form
    {
        private Connection con;
        public Client clientMain;

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
            
            ChannelObj c = new ChannelObj(o, cid, p.dmFlowContent, ((Button)sender).Image, p.clientMain.getUserMain());
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

            var c = new Connection();
            var uInfoRaw = await c.connect(dmFlowPannel);

            clientMain = new Client(uInfoRaw);
            var dmsRaw = c.uInfoRaw["private_channels"].AsArray().ToArray(); // (await getChannels()).AsArray().ToArray();
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

            //*/
        }
    }
}
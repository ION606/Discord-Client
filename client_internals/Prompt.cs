using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Client_Custom.client_internals
{
    internal static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };

            Label l2 = new Label();
            l2.Text = "https://discord.com/api/oauth2/authorize?client_id=1084141041403310130&redirect_uri=https%3A%2F%2Flocalhost%3A8690%2F&response_type=code&scope=identify";
            l2.Click += (object o, EventArgs a) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = l2.Text,
                    UseShellExecute = true
                });
            };

            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(l2);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            string path = @"C:\path-to-file"; // CHANGE THIS
            using (FileStream fs = File.Create(path))
            {
                // writing data in string
                string dataasstring = "data"; //your data
                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                fs.Write(info, 0, info.Length);

                // writing data in bytes already
                byte[] data = new byte[] { 0x0 };
                fs.Write(data, 0, data.Length);
            }

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}

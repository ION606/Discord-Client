namespace Discord_Client_Custom
{
    partial class mainPage
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //this.testbtn = new System.Windows.Forms.Button();
            this.dmFlowPannel = new System.Windows.Forms.FlowLayoutPanel();
            this.dmFlowContent = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // testbtn
            /*/
            this.testbtn.Location = new System.Drawing.Point(320, 147);
            this.testbtn.Name = "testbtn";
            this.testbtn.Size = new System.Drawing.Size(75, 23);
            this.testbtn.TabIndex = 0;
            this.testbtn.Text = "AUR NAUR";
            this.testbtn.UseVisualStyleBackColor = true;
            this.testbtn.Click += new System.EventHandler(this.testBtn_click);
            */
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            //this.Controls.Add(this.testbtn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.mainPage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();


            // 
            // TableLayoutPanel1
            //
            this.SuspendLayout();
            this.dmFlowPannel.AutoScroll = true;
            this.dmFlowPannel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.dmFlowPannel.Location = new System.Drawing.Point(1, 2);
            this.dmFlowPannel.Name = "dmFlowPannel";
            this.dmFlowPannel.Size = new System.Drawing.Size(200, 400);
            this.dmFlowPannel.TabIndex = 0;
            this.dmFlowPannel.Paint += new System.Windows.Forms.PaintEventHandler(this.dmFlowPannel_Paint);
            this.Controls.Add(this.dmFlowPannel);


            //
            // dmFlowContent
            //
            this.SuspendLayout();
            this.dmFlowContent.AutoScroll = true;
            //this.dmFlowContent.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.dmFlowContent.Location = new System.Drawing.Point(210, 5);
            this.dmFlowContent.Name = "dmFlowContent";
            this.dmFlowContent.Size = new System.Drawing.Size(575, 400);
            this.dmFlowContent.TabIndex = 0;
            this.dmFlowContent.BackColor = Color.DarkGray;
            this.dmFlowContent.Paint += new System.Windows.Forms.PaintEventHandler(this.dmFlowContent_Paint);
            
            //this.dmFlowContent.WrapContents = true;
            this.Controls.Add(this.dmFlowContent);

            this.statusbox = new ComboBox();
            this.statusbox.Location = new System.Drawing.Point(1, this.dmFlowContent.Bottom + 1);
        }

        #endregion

        private FlowLayoutPanel dmFlowPannel;

        //FFS find a way to fix this ASAP
        public TableLayoutPanel dmFlowContent;

        private ComboBox statusbox;
    }
}
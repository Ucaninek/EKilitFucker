namespace EKilitFucker
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            TB_Logs = new TextBox();
            GB_Logs = new GroupBox();
            L_Intro = new Label();
            B_Start = new Button();
            LL_SelfAd = new LinkLabel();
            PB_Boykisser = new PictureBox();
            GB_Params = new GroupBox();
            L_Deco = new Label();
            GB_Logs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PB_Boykisser).BeginInit();
            GB_Params.SuspendLayout();
            SuspendLayout();
            // 
            // TB_Logs
            // 
            TB_Logs.BackColor = SystemColors.ControlLightLight;
            TB_Logs.Dock = DockStyle.Fill;
            TB_Logs.Location = new Point(3, 19);
            TB_Logs.Multiline = true;
            TB_Logs.Name = "TB_Logs";
            TB_Logs.PlaceholderText = "This app is sponsored by Boykisser (O-O)";
            TB_Logs.ReadOnly = true;
            TB_Logs.ScrollBars = ScrollBars.Vertical;
            TB_Logs.Size = new Size(492, 344);
            TB_Logs.TabIndex = 0;
            // 
            // GB_Logs
            // 
            GB_Logs.Controls.Add(TB_Logs);
            GB_Logs.Location = new Point(290, 72);
            GB_Logs.Name = "GB_Logs";
            GB_Logs.Size = new Size(498, 366);
            GB_Logs.TabIndex = 1;
            GB_Logs.TabStop = false;
            GB_Logs.Text = "Logs";
            // 
            // L_Intro
            // 
            L_Intro.AutoSize = true;
            L_Intro.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            L_Intro.Location = new Point(12, 9);
            L_Intro.Name = "L_Intro";
            L_Intro.Size = new Size(628, 60);
            L_Intro.TabIndex = 2;
            L_Intro.Text = "this makeshift app will try to render e-kilit useless.\r\ngo ahead and press the start button and pray everything goes well\r\n";
            // 
            // B_Start
            // 
            B_Start.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            B_Start.Location = new Point(6, 322);
            B_Start.Name = "B_Start";
            B_Start.Size = new Size(260, 35);
            B_Start.TabIndex = 3;
            B_Start.Text = "Start";
            B_Start.UseVisualStyleBackColor = true;
            B_Start.Click += B_Start_Click;
            // 
            // LL_SelfAd
            // 
            LL_SelfAd.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LL_SelfAd.AutoSize = true;
            LL_SelfAd.Location = new Point(201, 301);
            LL_SelfAd.Name = "LL_SelfAd";
            LL_SelfAd.Size = new Size(65, 15);
            LL_SelfAd.TabIndex = 4;
            LL_SelfAd.TabStop = true;
            LL_SelfAd.Text = "zemi.space";
            LL_SelfAd.LinkClicked += LL_SelfAd_LinkClicked;
            // 
            // PB_Boykisser
            // 
            PB_Boykisser.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            PB_Boykisser.Image = (Image)resources.GetObject("PB_Boykisser.Image");
            PB_Boykisser.Location = new Point(6, 106);
            PB_Boykisser.Name = "PB_Boykisser";
            PB_Boykisser.Size = new Size(260, 210);
            PB_Boykisser.SizeMode = PictureBoxSizeMode.Zoom;
            PB_Boykisser.TabIndex = 5;
            PB_Boykisser.TabStop = false;
            // 
            // GB_Params
            // 
            GB_Params.Controls.Add(L_Deco);
            GB_Params.Controls.Add(B_Start);
            GB_Params.Controls.Add(LL_SelfAd);
            GB_Params.Controls.Add(PB_Boykisser);
            GB_Params.ImeMode = ImeMode.NoControl;
            GB_Params.Location = new Point(12, 72);
            GB_Params.Name = "GB_Params";
            GB_Params.Size = new Size(272, 363);
            GB_Params.TabIndex = 6;
            GB_Params.TabStop = false;
            GB_Params.Text = "Parameters";
            // 
            // L_Deco
            // 
            L_Deco.AutoSize = true;
            L_Deco.Font = new Font("Segoe UI", 48F, FontStyle.Regular, GraphicsUnit.Point, 0);
            L_Deco.ForeColor = SystemColors.ControlDarkDark;
            L_Deco.Location = new Point(59, 17);
            L_Deco.Name = "L_Deco";
            L_Deco.Size = new Size(160, 86);
            L_Deco.TabIndex = 7;
            L_Deco.Text = "'(._.)'";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(GB_Params);
            Controls.Add(L_Intro);
            Controls.Add(GB_Logs);
            Name = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Shown += Form1_Shown;
            GB_Logs.ResumeLayout(false);
            GB_Logs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PB_Boykisser).EndInit();
            GB_Params.ResumeLayout(false);
            GB_Params.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TB_Logs;
        private GroupBox GB_Logs;
        private Label L_Intro;
        private Button B_Start;
        private LinkLabel LL_SelfAd;
        private PictureBox PB_Boykisser;
        private GroupBox GB_Params;
        private Label L_Deco;
    }
}

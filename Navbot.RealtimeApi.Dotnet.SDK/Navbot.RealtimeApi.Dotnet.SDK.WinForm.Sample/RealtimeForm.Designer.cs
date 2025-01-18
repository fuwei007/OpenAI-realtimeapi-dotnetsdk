namespace Navbot.RealtimeApi.Dotnet.SDK.WinForm.Sample
{
    partial class RealtimeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            realtimeApiWinFormControl = new RealtimeApiWinFormControl();
            btnStart = new Button();
            btnEnd = new Button();
            rtxOutputInfo = new RichTextBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(realtimeApiWinFormControl, 0, 0);
            tableLayoutPanel1.Location = new Point(12, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(599, 276);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // realtimeApiWinFormControl
            // 
            realtimeApiWinFormControl.BackColor = SystemColors.ControlText;
            realtimeApiWinFormControl.Dock = DockStyle.Fill;
            realtimeApiWinFormControl.Location = new Point(3, 3);
            realtimeApiWinFormControl.Name = "realtimeApiWinFormControl";
            realtimeApiWinFormControl.OpenAiApiKey = "";
            realtimeApiWinFormControl.Size = new Size(593, 270);
            realtimeApiWinFormControl.TabIndex = 0;
            realtimeApiWinFormControl.VoiceVisualEffect = Core.VisualEffect.Cycle;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(80, 319);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(128, 42);
            btnStart.TabIndex = 1;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnEnd
            // 
            btnEnd.Location = new Point(364, 319);
            btnEnd.Name = "btnEnd";
            btnEnd.Size = new Size(128, 42);
            btnEnd.TabIndex = 2;
            btnEnd.Text = "End";
            btnEnd.UseVisualStyleBackColor = true;
            btnEnd.Click += btnEnd_Click;
            // 
            // rtxOutputInfo
            // 
            rtxOutputInfo.Location = new Point(614, 4);
            rtxOutputInfo.Name = "rtxOutputInfo";
            rtxOutputInfo.Size = new Size(184, 393);
            rtxOutputInfo.TabIndex = 3;
            rtxOutputInfo.Text = "";
            // 
            // RealtimeForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 397);
            Controls.Add(rtxOutputInfo);
            Controls.Add(btnEnd);
            Controls.Add(btnStart);
            Controls.Add(tableLayoutPanel1);
            Name = "RealtimeForm";
            Text = "Realtime";
            Load += RealtimeForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private RealtimeApiWinFormControl realtimeApiWinFormControl1;
        private Button btnStart;
        private Button btnEnd;
        private RichTextBox rtxOutputInfo;
        private RealtimeApiWinFormControl realtimeApiWinFormControl;
    }
}
namespace 白貓鍵盤操控
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btn_Start = new System.Windows.Forms.Button();
            this.lab_PID = new System.Windows.Forms.Label();
            this.lab_PTitle = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btn_Reset = new System.Windows.Forms.Button();
            this.cb_ControllerPlay = new System.Windows.Forms.CheckBox();
            this.cb_TopShow = new System.Windows.Forms.CheckBox();
            this.cb_Move = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(529, 76);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // btn_Start
            // 
            this.btn_Start.Enabled = false;
            this.btn_Start.Location = new System.Drawing.Point(466, 187);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(75, 23);
            this.btn_Start.TabIndex = 1;
            this.btn_Start.Text = "開始";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // lab_PID
            // 
            this.lab_PID.AutoSize = true;
            this.lab_PID.Location = new System.Drawing.Point(10, 95);
            this.lab_PID.Name = "lab_PID";
            this.lab_PID.Size = new System.Drawing.Size(95, 12);
            this.lab_PID.TabIndex = 2;
            this.lab_PID.Text = "選擇的程式PID: 0";
            // 
            // lab_PTitle
            // 
            this.lab_PTitle.AutoSize = true;
            this.lab_PTitle.Location = new System.Drawing.Point(10, 112);
            this.lab_PTitle.Name = "lab_PTitle";
            this.lab_PTitle.Size = new System.Drawing.Size(107, 12);
            this.lab_PTitle.TabIndex = 3;
            this.lab_PTitle.Text = "選擇的視窗標題: 無";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 216);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(529, 246);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // btn_Reset
            // 
            this.btn_Reset.Location = new System.Drawing.Point(385, 187);
            this.btn_Reset.Name = "btn_Reset";
            this.btn_Reset.Size = new System.Drawing.Size(75, 23);
            this.btn_Reset.TabIndex = 5;
            this.btn_Reset.Text = "重整";
            this.btn_Reset.UseVisualStyleBackColor = true;
            this.btn_Reset.Click += new System.EventHandler(this.btn_Reset_Click);
            // 
            // cb_ControllerPlay
            // 
            this.cb_ControllerPlay.AutoSize = true;
            this.cb_ControllerPlay.Enabled = false;
            this.cb_ControllerPlay.Location = new System.Drawing.Point(12, 150);
            this.cb_ControllerPlay.Name = "cb_ControllerPlay";
            this.cb_ControllerPlay.Size = new System.Drawing.Size(72, 16);
            this.cb_ControllerPlay.TabIndex = 6;
            this.cb_ControllerPlay.Text = "搖桿操作";
            this.cb_ControllerPlay.UseVisualStyleBackColor = true;
            this.cb_ControllerPlay.CheckedChanged += new System.EventHandler(this.cb_ControllerPlay_CheckedChanged);
            // 
            // cb_TopShow
            // 
            this.cb_TopShow.AutoSize = true;
            this.cb_TopShow.Location = new System.Drawing.Point(12, 129);
            this.cb_TopShow.Name = "cb_TopShow";
            this.cb_TopShow.Size = new System.Drawing.Size(72, 16);
            this.cb_TopShow.TabIndex = 7;
            this.cb_TopShow.Text = "置頂顯示";
            this.cb_TopShow.UseVisualStyleBackColor = true;
            this.cb_TopShow.CheckedChanged += new System.EventHandler(this.cb_TopShow_CheckedChanged);
            // 
            // cb_Move
            // 
            this.cb_Move.AutoSize = true;
            this.cb_Move.Checked = true;
            this.cb_Move.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_Move.Location = new System.Drawing.Point(12, 171);
            this.cb_Move.Name = "cb_Move";
            this.cb_Move.Size = new System.Drawing.Size(72, 16);
            this.cb_Move.TabIndex = 8;
            this.cb_Move.Text = "移動控制";
            this.cb_Move.UseVisualStyleBackColor = true;
            this.cb_Move.CheckedChanged += new System.EventHandler(this.cb_Move_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 474);
            this.Controls.Add(this.cb_Move);
            this.Controls.Add(this.cb_TopShow);
            this.Controls.Add(this.cb_ControllerPlay);
            this.Controls.Add(this.btn_Reset);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.lab_PTitle);
            this.Controls.Add(this.lab_PID);
            this.Controls.Add(this.btn_Start);
            this.Controls.Add(this.listBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "白貓鍵盤操作";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.Label lab_PID;
        private System.Windows.Forms.Label lab_PTitle;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btn_Reset;
        private System.Windows.Forms.CheckBox cb_ControllerPlay;
        private System.Windows.Forms.CheckBox cb_TopShow;
        private System.Windows.Forms.CheckBox cb_Move;
    }
}


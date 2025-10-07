namespace Project_Armored_Car
{
    partial class Client
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Client));
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            LOG_TEXTBOX = new RichTextBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            PORT_TEXTBOX = new TextBox();
            label2 = new Label();
            label1 = new Label();
            IP_TEXTBOX = new TextBox();
            CONNENT_BUTTON = new Button();
            tableLayoutPanel4 = new TableLayoutPanel();
            label3 = new Label();
            SEND_BUTTON = new Button();
            INPUT_TEXTBOX = new TextBox();
            NAME_TEXTBOX = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(484, 461);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(LOG_TEXTBOX, 0, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel4, 0, 2);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel2.Size = new Size(478, 455);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // LOG_TEXTBOX
            // 
            LOG_TEXTBOX.Dock = DockStyle.Fill;
            LOG_TEXTBOX.Location = new Point(3, 3);
            LOG_TEXTBOX.Name = "LOG_TEXTBOX";
            LOG_TEXTBOX.ReadOnly = true;
            LOG_TEXTBOX.Size = new Size(472, 379);
            LOG_TEXTBOX.TabIndex = 2;
            LOG_TEXTBOX.Text = "";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 6;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tableLayoutPanel3.Controls.Add(PORT_TEXTBOX, 3, 0);
            tableLayoutPanel3.Controls.Add(label2, 2, 0);
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(IP_TEXTBOX, 1, 0);
            tableLayoutPanel3.Controls.Add(CONNENT_BUTTON, 5, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 388);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(472, 29);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // PORT_TEXTBOX
            // 
            PORT_TEXTBOX.Dock = DockStyle.Fill;
            PORT_TEXTBOX.Location = new Point(283, 3);
            PORT_TEXTBOX.MaxLength = 5;
            PORT_TEXTBOX.Name = "PORT_TEXTBOX";
            PORT_TEXTBOX.Size = new Size(94, 23);
            PORT_TEXTBOX.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(203, 0);
            label2.Name = "label2";
            label2.Size = new Size(74, 29);
            label2.TabIndex = 2;
            label2.Text = "PORT";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(44, 29);
            label1.TabIndex = 0;
            label1.Text = "IP";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // IP_TEXTBOX
            // 
            IP_TEXTBOX.Dock = DockStyle.Fill;
            IP_TEXTBOX.Location = new Point(53, 3);
            IP_TEXTBOX.MaxLength = 15;
            IP_TEXTBOX.Name = "IP_TEXTBOX";
            IP_TEXTBOX.Size = new Size(144, 23);
            IP_TEXTBOX.TabIndex = 1;
            // 
            // CONNENT_BUTTON
            // 
            CONNENT_BUTTON.Dock = DockStyle.Fill;
            CONNENT_BUTTON.Location = new Point(425, 3);
            CONNENT_BUTTON.Name = "CONNENT_BUTTON";
            CONNENT_BUTTON.Size = new Size(44, 23);
            CONNENT_BUTTON.TabIndex = 4;
            CONNENT_BUTTON.Text = "연결";
            CONNENT_BUTTON.UseVisualStyleBackColor = true;
            CONNENT_BUTTON.Click += CONNENT_BUTTON_Click;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 6;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tableLayoutPanel4.Controls.Add(label3, 0, 0);
            tableLayoutPanel4.Controls.Add(SEND_BUTTON, 5, 0);
            tableLayoutPanel4.Controls.Add(INPUT_TEXTBOX, 3, 0);
            tableLayoutPanel4.Controls.Add(NAME_TEXTBOX, 1, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 423);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(472, 29);
            tableLayoutPanel4.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(3, 0);
            label3.Name = "label3";
            label3.Size = new Size(44, 29);
            label3.TabIndex = 3;
            label3.Text = "이름";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SEND_BUTTON
            // 
            SEND_BUTTON.Location = new Point(425, 3);
            SEND_BUTTON.Name = "SEND_BUTTON";
            SEND_BUTTON.Size = new Size(44, 23);
            SEND_BUTTON.TabIndex = 0;
            SEND_BUTTON.Text = "전송";
            SEND_BUTTON.UseVisualStyleBackColor = true;
            SEND_BUTTON.Click += SEND_BUTTON_Click;
            // 
            // INPUT_TEXTBOX
            // 
            INPUT_TEXTBOX.Dock = DockStyle.Fill;
            INPUT_TEXTBOX.Location = new Point(153, 3);
            INPUT_TEXTBOX.Name = "INPUT_TEXTBOX";
            INPUT_TEXTBOX.Size = new Size(236, 23);
            INPUT_TEXTBOX.TabIndex = 1;
            INPUT_TEXTBOX.KeyDown += INPUT_TEXTBOX_KeyDown;
            // 
            // NAME_TEXTBOX
            // 
            NAME_TEXTBOX.Location = new Point(53, 3);
            NAME_TEXTBOX.Name = "NAME_TEXTBOX";
            NAME_TEXTBOX.Size = new Size(64, 23);
            NAME_TEXTBOX.TabIndex = 4;
            NAME_TEXTBOX.Text = "ㅇㅇ";
            // 
            // Client
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(484, 461);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(488, 120);
            Name = "Client";
            Text = "Client";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private RichTextBox LOG_TEXTBOX;
        private TableLayoutPanel tableLayoutPanel3;
        private TextBox PORT_TEXTBOX;
        private Label label2;
        private Label label1;
        private TextBox IP_TEXTBOX;
        private Button CONNENT_BUTTON;
        private TableLayoutPanel tableLayoutPanel4;
        private Button SEND_BUTTON;
        private TextBox INPUT_TEXTBOX;
        private Label label3;
        private TextBox NAME_TEXTBOX;
    }
}
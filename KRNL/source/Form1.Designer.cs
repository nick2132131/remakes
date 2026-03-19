namespace KRNL
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
            panel1 = new Panel();
            button13 = new Button();
            button6 = new Button();
            pictureBox1 = new PictureBox();
            label1 = new Label();
            flowLayoutPanel2 = new FlowLayoutPanel();
            panel2 = new Panel();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            button7 = new Button();
            inj = new Button();
            sve = new Button();
            opn = new Button();
            clr = new Button();
            exec = new Button();
            listBox2 = new ListBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(29, 29, 29);
            panel1.Controls.Add(button13);
            panel1.Controls.Add(button6);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(flowLayoutPanel2);
            panel1.Location = new Point(-22, -4);
            panel1.Name = "panel1";
            panel1.Size = new Size(720, 31);
            panel1.TabIndex = 9;
            panel1.Paint += panel1_Paint;
            panel1.DoubleClick += panel1_DoubleClick;
            // 
            // button13
            // 
            button13.FlatAppearance.BorderSize = 0;
            button13.FlatStyle = FlatStyle.Flat;
            button13.ForeColor = Color.White;
            button13.Location = new Point(658, 7);
            button13.Name = "button13";
            button13.Size = new Size(25, 22);
            button13.TabIndex = 4;
            button13.Text = "➖";
            button13.UseVisualStyleBackColor = true;
            button13.Click += button13_Click;
            // 
            // button6
            // 
            button6.FlatAppearance.BorderSize = 0;
            button6.FlatStyle = FlatStyle.Flat;
            button6.ForeColor = Color.White;
            button6.Location = new Point(686, 7);
            button6.Name = "button6";
            button6.Size = new Size(25, 22);
            button6.TabIndex = 3;
            button6.Text = "✖";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.krnl_logo;
            pictureBox1.Location = new Point(25, 7);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(21, 21);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.White;
            label1.Location = new Point(346, 10);
            label1.Name = "label1";
            label1.Size = new Size(36, 15);
            label1.TabIndex = 1;
            label1.Text = "KRNL";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.BackColor = Color.RoyalBlue;
            flowLayoutPanel2.Location = new Point(0, -4);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(713, 10);
            flowLayoutPanel2.TabIndex = 0;
            flowLayoutPanel2.Paint += flowLayoutPanel2_Paint;
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(17, 18, 17);
            panel2.Controls.Add(webView21);
            panel2.Controls.Add(button7);
            panel2.Controls.Add(inj);
            panel2.Controls.Add(sve);
            panel2.Controls.Add(opn);
            panel2.Controls.Add(clr);
            panel2.Controls.Add(exec);
            panel2.Controls.Add(listBox2);
            panel2.Location = new Point(-22, 48);
            panel2.Name = "panel2";
            panel2.Size = new Size(753, 306);
            panel2.TabIndex = 10;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Location = new Point(25, 4);
            webView21.Name = "webView21";
            webView21.Size = new Size(560, 267);
            webView21.TabIndex = 16;
            webView21.ZoomFactor = 1D;
            // 
            // button7
            // 
            button7.BackColor = Color.FromArgb(40, 40, 40);
            button7.FlatAppearance.BorderSize = 0;
            button7.FlatStyle = FlatStyle.Flat;
            button7.Font = new Font("Microsoft YaHei UI", 9F);
            button7.ForeColor = Color.White;
            button7.Location = new Point(609, 274);
            button7.Name = "button7";
            button7.Size = new Size(100, 25);
            button7.TabIndex = 13;
            button7.Text = "OPTIONS";
            button7.UseVisualStyleBackColor = false;
            button7.Click += button5_Click;
            // 
            // inj
            // 
            inj.BackColor = Color.FromArgb(40, 40, 40);
            inj.FlatAppearance.BorderSize = 0;
            inj.FlatStyle = FlatStyle.Flat;
            inj.Font = new Font("Microsoft YaHei UI", 9F);
            inj.ForeColor = Color.White;
            inj.Location = new Point(441, 274);
            inj.Name = "inj";
            inj.Size = new Size(100, 25);
            inj.TabIndex = 12;
            inj.Text = "INJECT";
            inj.UseVisualStyleBackColor = false;
            inj.Click += inj_Click;
            // 
            // sve
            // 
            sve.BackColor = Color.FromArgb(40, 40, 40);
            sve.FlatAppearance.BorderSize = 0;
            sve.FlatStyle = FlatStyle.Flat;
            sve.Font = new Font("Microsoft YaHei UI", 9F);
            sve.ForeColor = Color.White;
            sve.Location = new Point(337, 274);
            sve.Name = "sve";
            sve.Size = new Size(100, 25);
            sve.TabIndex = 11;
            sve.Text = "SAVE FILE";
            sve.UseVisualStyleBackColor = false;
            sve.Click += sve_Click;
            // 
            // opn
            // 
            opn.BackColor = Color.FromArgb(40, 40, 40);
            opn.FlatAppearance.BorderSize = 0;
            opn.FlatStyle = FlatStyle.Flat;
            opn.Font = new Font("Microsoft YaHei UI", 9F);
            opn.ForeColor = Color.White;
            opn.Location = new Point(233, 274);
            opn.Name = "opn";
            opn.Size = new Size(100, 25);
            opn.TabIndex = 10;
            opn.Text = "OPEN FILE";
            opn.UseVisualStyleBackColor = false;
            opn.Click += opn_Click;
            // 
            // clr
            // 
            clr.BackColor = Color.FromArgb(40, 40, 40);
            clr.FlatAppearance.BorderSize = 0;
            clr.FlatStyle = FlatStyle.Flat;
            clr.Font = new Font("Microsoft YaHei UI", 9F);
            clr.ForeColor = Color.White;
            clr.Location = new Point(129, 274);
            clr.Name = "clr";
            clr.Size = new Size(100, 25);
            clr.TabIndex = 9;
            clr.Text = "CLEAR";
            clr.UseVisualStyleBackColor = false;
            clr.Click += clr_Click;
            // 
            // exec
            // 
            exec.BackColor = Color.FromArgb(40, 40, 40);
            exec.FlatAppearance.BorderSize = 0;
            exec.FlatStyle = FlatStyle.Flat;
            exec.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            exec.ForeColor = Color.White;
            exec.Location = new Point(25, 274);
            exec.Name = "exec";
            exec.Size = new Size(100, 25);
            exec.TabIndex = 8;
            exec.Text = "EXECUTE";
            exec.UseVisualStyleBackColor = false;
            exec.Click += exec_Click;
            // 
            // listBox2
            // 
            listBox2.BackColor = Color.FromArgb(29, 29, 29);
            listBox2.BorderStyle = BorderStyle.None;
            listBox2.ForeColor = Color.White;
            listBox2.FormattingEnabled = true;
            listBox2.IntegralHeight = false;
            listBox2.Location = new Point(588, 4);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(121, 267);
            listBox2.TabIndex = 15;
            listBox2.SelectedIndexChanged += listBox2_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.White;
            button1.Location = new Point(1, 27);
            button1.Name = "button1";
            button1.Size = new Size(42, 20);
            button1.TabIndex = 11;
            button1.Text = "File";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.ForeColor = Color.White;
            button2.Location = new Point(46, 27);
            button2.Name = "button2";
            button2.Size = new Size(57, 20);
            button2.TabIndex = 12;
            button2.Text = "Credits";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.ForeColor = Color.White;
            button3.Location = new Point(171, 27);
            button3.Name = "button3";
            button3.Size = new Size(77, 20);
            button3.TabIndex = 14;
            button3.Text = "Hot-scripts";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.ForeColor = Color.White;
            button4.Location = new Point(107, 27);
            button4.Name = "button4";
            button4.Size = new Size(60, 20);
            button4.TabIndex = 13;
            button4.Text = "Games";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.ForeColor = Color.White;
            button5.Location = new Point(253, 27);
            button5.Name = "button5";
            button5.Size = new Size(57, 20);
            button5.TabIndex = 15;
            button5.Text = "Others";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(33, 33, 33);
            ClientSize = new Size(690, 350);
            Controls.Add(button5);
            Controls.Add(button3);
            Controls.Add(button4);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KRNL";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private Panel panel2;
        private Button button7;
        private Button inj;
        private Button sve;
        private Button opn;
        private Button clr;
        private Button exec;
        private ListBox listBox2;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Label label1;
        private PictureBox pictureBox1;
        private Button button6;
        private Button button13;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
    }
}
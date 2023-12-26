namespace AJTOOL
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
            buttonStartCheck = new Button();
            treeView1 = new TreeView();
            dateTimePicker1 = new DateTimePicker();
            dateTimePicker2 = new DateTimePicker();
            label1 = new Label();
            buttonAddDir = new Button();
            buttonStop = new Button();
            SuspendLayout();
            // 
            // buttonStartCheck
            // 
            buttonStartCheck.Location = new Point(12, 128);
            buttonStartCheck.Name = "buttonStartCheck";
            buttonStartCheck.Size = new Size(192, 35);
            buttonStartCheck.TabIndex = 0;
            buttonStartCheck.Text = "开始搜索";
            buttonStartCheck.UseVisualStyleBackColor = true;
            buttonStartCheck.Click += buttonStartCheck_Click;
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.Location = new Point(210, 0);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(590, 450);
            treeView1.TabIndex = 1;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(12, 29);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(192, 23);
            dateTimePicker1.TabIndex = 2;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Location = new Point(12, 58);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(192, 23);
            dateTimePicker2.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 3;
            label1.Text = "时间范围";
            // 
            // buttonAddDir
            // 
            buttonAddDir.Location = new Point(12, 87);
            buttonAddDir.Name = "buttonAddDir";
            buttonAddDir.Size = new Size(192, 35);
            buttonAddDir.TabIndex = 0;
            buttonAddDir.Text = "添加目录";
            buttonAddDir.UseVisualStyleBackColor = true;
            buttonAddDir.Click += buttonAddDir_Click;
            // 
            // buttonStop
            // 
            buttonStop.Location = new Point(12, 169);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(192, 35);
            buttonStop.TabIndex = 0;
            buttonStop.Text = "停止搜索";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += buttonStop_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(dateTimePicker2);
            Controls.Add(dateTimePicker1);
            Controls.Add(treeView1);
            Controls.Add(buttonAddDir);
            Controls.Add(buttonStop);
            Controls.Add(buttonStartCheck);
            Name = "Form1";
            Text = "搜索磁盘每天新增修改文件";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonStartCheck;
        private TreeView treeView1;
        private DateTimePicker dateTimePicker1;
        private DateTimePicker dateTimePicker2;
        private Label label1;
        private Button buttonAddDir;
        private Button buttonStop;
    }
}
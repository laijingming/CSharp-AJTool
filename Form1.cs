using AJTOOL.Class;


namespace AJTOOL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region �߼�
        private Dictionary<TreeNode, CancellationTokenSource> runningTasks = new Dictionary<TreeNode, CancellationTokenSource>();
        /// <summary>
        /// ��ʼ���
        /// </summary>
        private void StartCheck(TreeView treeView, DateTime sTime, DateTime eTime)
        {
            runningTasks.Clear();
            foreach (TreeNode node in treeView.Nodes)
            {
                node.Nodes.Clear();
                CancellationTokenSource source = new CancellationTokenSource();
                Task.Run(() =>
                {
                    LoadDirectory((CustomTreeNode)node, sTime, eTime, source);
                    source.Cancel();
                });
                runningTasks.Add(node, source);
            }
        }

        /// <summary>
        /// ֹͣ���
        /// </summary>
        private void StopCheck()
        {
            foreach (var item in runningTasks)
            {
                CancellationTokenSource source = item.Value;
                source.Cancel();
            }
        }

        /// <summary>
        /// ���Ŀ¼
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="node"></param>
        private DateTime LoadDirectory(CustomTreeNode node, DateTime sTime, DateTime eTime, CancellationTokenSource source)
        {
            if (source.IsCancellationRequested)
            {
                return default;//�յ�ֹͣ����ʶ������Ĭ��ֵ
            }
            DateTime lastModifiedDateTime = new DateTime();
            try
            {
                // ��ȡ��ǰĿ¼����Ŀ¼
                string[] subDirectories = Directory.GetDirectories(node.ResourePath);
                // ����ǰĿ¼�µ���Ŀ¼
                foreach (string subDirectory in subDirectories)
                {
                    //�Խڵ�Ŀ¼���м��
                    CustomTreeNode subNode = new CustomTreeNode(subDirectory);
                    lastModifiedDateTime = LoadDirectory(subNode, sTime, eTime, source);
                    if (subNode.Nodes.Count > 0)
                    {
                        subNode.LastDateTime = lastModifiedDateTime;
                        subNode.CustomText();
                        node.InsertNode(subNode);
                        //InsertNode(node, treeNode);
                    }
                }
                node.CustomText();

                //����ļ�
                string[] files = Directory.GetFiles(node.ResourePath);
                foreach (string file in files)
                {
                    // ������ʱ������޸ĵ��ļ�
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= sTime && lastModified <= eTime)
                    {
                        if (node.Tag == null || (DateTime)node.Tag <= lastModified)
                        {
                            lastModifiedDateTime = lastModified;//��¼Ŀ¼����޸�ʱ��
                        }
                        CustomTreeNode fileNode = new CustomTreeNode(file) { ForeColor = Color.Red, LastDateTime = lastModified, Count = 1 };
                        fileNode.CustomText();
                        node.InsertNode(fileNode);
                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                node.InsertNode(new CustomTreeNode($"�޷�����Ŀ¼ {ex.Message}"));
            }
            return lastModifiedDateTime;

        }

        /// <summary>
        /// ��ȡ�ļ������ļ�������޸�ʱ��
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        private static DateTime GetResourceLastWriteTime(string resourcePath)
        {
            if (File.Exists(resourcePath))
            {
                return File.GetLastWriteTime(resourcePath);
            }
            return Directory.GetLastWriteTime(resourcePath);
        }



        /// <summary>
        /// ��Ӽ���Ŀ¼
        /// </summary>
        private static void AddDir(TreeView treeView)
        {
            //using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()) 
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    // �û�ѡ���Ŀ¼·��
                    CustomTreeNode customTreeNode = new CustomTreeNode(folderBrowserDialog.SelectedPath);
                    // ��Ŀ¼����ڵ�
                    treeView.Nodes.Add(customTreeNode);
                }
            }
        }

        /// <summary>
        /// ���� ContextMenuStrip
        /// </summary>
        /// <param name="treeView"></param>
        private static void MakeContextMenuStrip(TreeView treeView)
        {
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            // ���ɾ��ѡ��
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("ɾ��");
            //ɾ��ѡ��ĵ���¼�����
            deleteMenuItem.Click += (sender, e) =>
            {
                // ��ȡѡ�еĽڵ�
                TreeNode selectedNode = treeView.SelectedNode;

                if (selectedNode != null)
                {
                    // ɾ��ѡ�еĽڵ�
                    treeView.Nodes.Remove(selectedNode);
                }
            };
            contextMenuStrip.Items.Add(deleteMenuItem);

            // �� ContextMenuStrip ����� TreeView
            treeView.ContextMenuStrip = contextMenuStrip;
        }

        /// <summary>
        /// ɾ��ѡ��ĵ���¼�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            // ��ȡѡ�еĽڵ�
            TreeNode selectedNode = treeView1.SelectedNode;

            if (selectedNode != null)
            {
                // ɾ��ѡ�еĽڵ�
                treeView1.Nodes.Remove(selectedNode);
                // ���ߣ���������ڸ���ҵ���߼�ɾ������ʹ�� selectedNode.Remove() ����
            }
        }

        /// <summary>
        /// ��ʼ��ʱ��
        /// </summary>
        private static void InitDateTimePicker(DateTimePicker p1, DateTimePicker p2)
        {
            p1.Format = DateTimePickerFormat.Custom;
            p1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            p2.Format = DateTimePickerFormat.Custom;
            p2.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            p1.Text = DateTime.Today.ToString();
            p2.Text = DateTime.Today.AddDays(1).ToLongDateString();
        }

        #endregion


        private void Form1_Load(object sender, EventArgs e)
        {
            InitDateTimePicker(dateTimePicker1, dateTimePicker2);
            MakeContextMenuStrip(treeView1);
        }


        //��ʼ���
        private void buttonStartCheck_Click(object sender, EventArgs e)
        {
            StartCheck(treeView1, dateTimePicker1.Value, dateTimePicker2.Value);
        }

        //���Ŀ¼
        private void buttonAddDir_Click(object sender, EventArgs e)
        {
            AddDir(treeView1);
        }

        //ֹͣ���
        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopCheck();
        }
    }
}
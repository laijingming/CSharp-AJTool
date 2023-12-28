using AJTOOL.Class;
using System.Diagnostics;

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
        /// ��ʼ�����ڵ�Ŀ¼
        /// </summary>
        private void StartCheck(TreeView treeView, DateTime sTime, DateTime eTime)
        {
            runningTasks.Clear();// �����������
            foreach (TreeNode node in treeView.Nodes)
            {
                node.Nodes.Clear();// ��սڵ��µ��ӽڵ�
                CancellationTokenSource source = new CancellationTokenSource();
                // �첽ִ�м���Ŀ¼����
                Task.Run(() =>
                {
                    LoadDirectory((CustomTreeNode)node, sTime, eTime, source.Token);
                });
                runningTasks.Add(node, source);// ���ڵ�Ͷ�Ӧ��ȡ�� Token ���������б�
            }
        }

        /// <summary>
        /// ֹͣ������ڽ��е�����
        /// </summary>
        private void StopCheck()
        {
            foreach (var item in runningTasks)
            {
                CancellationTokenSource source = item.Value;// ��ȡ�����Ӧ��ȡ�� Token
                source.Cancel();// ȡ������ִ��
            }
        }

        /// <summary>
        /// ���Ŀ¼
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="node"></param>
        private void LoadDirectory(CustomTreeNode node, DateTime sTime, DateTime eTime, CancellationToken source)
        {
            if (source.IsCancellationRequested)
            {
                return;//��ȡ�������������˳�����
            }
            try
            {
                // ��ȡ��ǰĿ¼����Ŀ¼
                string[] subDirectories = Directory.GetDirectories(node.ResourePath);
                // ����ǰĿ¼�µ���Ŀ¼
                foreach (string subDirectory in subDirectories)
                {
                    //�Խڵ�Ŀ¼���м��
                    CustomTreeNode subNode = new CustomTreeNode(subDirectory);
                    LoadDirectory(subNode, sTime, eTime, source);
                    if (subNode.Nodes.Count > 0)
                    {
                        node.InsertNode(subNode);
                    }
                }
                node.UpdateNodeUI();

                //����ļ�
                string[] files = Directory.GetFiles(node.ResourePath);
                foreach (string file in files)
                {
                    // ������ʱ������޸ĵ��ļ�
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= sTime && lastModified <= eTime)
                    {
                        node.InsertNode(new CustomTreeNode(file) { ForeColor = Color.Red, LastDateTime = lastModified, Count = 1 });
                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                node.InsertNode(new CustomTreeNode($"�޷�����Ŀ¼ {ex.Message}"));
            }
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
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                // �û�ѡ���Ŀ¼·��
                CustomTreeNode customTreeNode = new CustomTreeNode(folderBrowserDialog.SelectedPath);
                // ��Ŀ¼����ڵ�
                treeView.Nodes.Add(customTreeNode);
            }
        }

        /// <summary>
        /// ΪTreeView�����Ҽ��˵��������ɾ���ʹ�Ŀ¼ѡ��
        /// </summary>
        /// <param name="treeView">Ҫ����Ҽ��˵���TreeView</param>
        private static void MakeContextMenuStrip(TreeView treeView)
        {
            // �����Ҽ��˵�
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            // ���ɾ��ѡ��
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("ɾ��");
            //ɾ��ѡ��ĵ���¼�����
            deleteMenuItem.Click += (sender, e) =>
            {
                // ��ȡѡ�еĽڵ�
                if (treeView.SelectedNode is CustomTreeNode selectedNode)
                {
                    // ɾ��ѡ�еĽڵ�
                    treeView.Nodes.Remove(selectedNode);
                }
            };
            contextMenuStrip.Items.Add(deleteMenuItem);

            // ���ɾ��ѡ��
            ToolStripMenuItem openFolder = new ToolStripMenuItem("��Ŀ¼");
            //ɾ��ѡ��ĵ���¼�����
            openFolder.Click += (sender, e) =>
            {
                // ��ȡѡ�еĽڵ�
                if (treeView.SelectedNode is CustomTreeNode selectedNode)
                {
                    Process.Start("explorer.exe", selectedNode.ResourePath);
                }
            };
            contextMenuStrip.Items.Add(openFolder);

            // �� ContextMenuStrip ����� TreeView
            treeView.ContextMenuStrip = contextMenuStrip;
        }

        /// <summary>
        /// ��ʼ������DateTimePicker�ؼ����������ʽΪ�Զ�������ʱ���ʽ�������ó�ʼֵΪ����͵ڶ���
        /// </summary>
        /// <param name="p1">��һ��DateTimePicker�ؼ�</param>
        /// <param name="p2">�ڶ���DateTimePicker�ؼ�</param>
        private static void InitDateTimePicker(DateTimePicker p1, DateTimePicker p2)
        {
            // ��������ʱ���ʽΪ�Զ����ʽ
            p1.Format = DateTimePickerFormat.Custom;
            p1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            p2.Format = DateTimePickerFormat.Custom;
            p2.CustomFormat = "yyyy-MM-dd HH:mm:ss";

            // ���ó�ʼֵΪ����͵ڶ���
            p1.Text = DateTime.Today.ToString();
            p2.Text = DateTime.Today.AddDays(1).ToLongDateString();
        }

        #endregion


        // �� Form1_Load �г�ʼ��ʱ��κ��Ҽ��˵�
        private void Form1_Load(object sender, EventArgs e)
        {
            InitDateTimePicker(dateTimePicker1, dateTimePicker2); // ��ʼ��ʱ���
            MakeContextMenuStrip(treeView1); // �����Ҽ��˵�
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
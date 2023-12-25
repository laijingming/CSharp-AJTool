using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace AJTOOL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region
        class CustomTreeNode : TreeNode
        {   
            public CustomTreeNode(string path) 
            {
                Path = path;
                Text = Path;
            }
            public string Path { get; set; }
            public int Count { get; set; }

            public DateTime LastDateTime { get; set; }

            public void CustomText()
            {
                if (Nodes.Count>0)
                {
                    Count = 0;
                    foreach (CustomTreeNode item in Nodes)
                    {
                        if (LastDateTime < item.LastDateTime)
                        {
                            LastDateTime = item.LastDateTime;
                        }
                        Count += item.Count;
                    }
                }
                
                if (TreeView!=null)
                {
                    TreeView.Invoke(new Action(() =>
                    {
                        _customText();
                    }));
                }
                else
                {
                    _customText();
                }
            }

            private void _customText()
            {
                if (File.Exists(Path))
                {
                    Text = $"{Path} ---�޸�ʱ��{LastDateTime}";

                }
                else
                {
                    if (Nodes.Count>0)
                    {
                        Text = $"{Path} ---����{Count} �޸�ʱ��{LastDateTime}";
                    }
                    else
                    {
                        Text = Path;
                    }
                }
            }

            public void InsertNode(CustomTreeNode subNode) 
            {
                // ���ڵ�����������ͼ�Ƿ����
                if (TreeView != null)
                {
                    // ʹ�� Invoke ����ȷ���� UI �߳���ִ�в������
                    TreeView.Invoke(new Action(() =>
                    {
                        Nodes.Add(subNode);
                        Expand();
                    }));
                }
                else
                {
                    // �����ǰ���� UI �߳��ϣ�ֱ����ڵ�
                    Nodes.Add(subNode);
                }
            }
        }
        #endregion

        #region �߼�

        /// <summary>
        /// ��ʼ��ʱ��
        /// </summary>
        private void initDateTimePicker(DateTimePicker p1,DateTimePicker p2)
        {
            p1.Format = DateTimePickerFormat.Custom;
            p1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            p2.Format = DateTimePickerFormat.Custom;
            p2.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            p1.Text = DateTime.Today.ToString();
            p2.Text = DateTime.Today.AddDays(1).ToLongDateString();
        }

        /// <summary>
        /// ���Ŀ¼
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="node"></param>
        private DateTime LoadDirectory(CustomTreeNode node)
        {
            DateTime lastModifiedDateTime = new();
            try
            {   
                // ��ȡ��ǰĿ¼����Ŀ¼
                string[] subDirectories = Directory.GetDirectories(node.Path);
                // ����ǰĿ¼�µ���Ŀ¼
                foreach (string subDirectory in subDirectories)
                {
                    //�Խڵ�Ŀ¼���м��
                    CustomTreeNode subNode = new(subDirectory);
                    lastModifiedDateTime = LoadDirectory(subNode);
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
                string[] files = Directory.GetFiles(node.Path);
                foreach (string file in files)
                {
                    // ������ʱ������޸ĵ��ļ�
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= dateTimePicker1.Value && lastModified <= dateTimePicker2.Value)
                    {
                        if (node.Tag == null || (DateTime)node.Tag <= lastModified)
                        {
                            lastModifiedDateTime = lastModified;//��¼Ŀ¼����޸�ʱ��
                        }
                        CustomTreeNode fileNode = new(file) { ForeColor = Color.Red, LastDateTime = lastModified, Count=1 };
                        fileNode.CustomText();
                        node.InsertNode(fileNode);
                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                UpdateNode(node, $"�޷�����Ŀ¼ {ex.Message}");
            }
            return lastModifiedDateTime;

        }

        /// <summary>
        /// ����ڵ�
        /// </summary>
        /// <param name="node"></param>
        /// <param name="aNode"></param>
        private void InsertNode(TreeNode node, TreeNode aNode) 
        {
            // ���ڵ�����������ͼ�Ƿ����
            if (node.TreeView != null)
            {
                // ʹ�� Invoke ����ȷ���� UI �߳���ִ�в������
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Nodes.Add(aNode);
                    node.Expand();
                }));
            }
            else
            {
                // �����ǰ���� UI �߳��ϣ�ֱ����ڵ�
                node.Nodes.Add(aNode);
            }


        }

        /// <summary>
        /// ���½ڵ�����
        /// </summary>
        /// <param name="node"></param>
        /// <param name="text"></param>
        private void UpdateNode(TreeNode node, string text) 
        {
            // ���ڵ�����������ͼ�Ƿ����
            if (node.TreeView != null)
            {
                // ʹ�� Invoke ����ȷ���� UI �߳���ִ�в������
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Text = text;
                }));
            }
            else
            {
                // �����ǰ���� UI �߳��ϣ�ֱ����ڵ�
                node.Text = text;

            }
        }


        /// <summary>
        /// ��ȡ�ļ������ļ�������޸�ʱ��
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        private DateTime GetResourceLastWriteTime(string resourcePath)
        {
            if (File.Exists(resourcePath))
            {
                return File.GetLastWriteTime(resourcePath);
            }
            return Directory.GetLastWriteTime(resourcePath);
        }

        /// <summary>
        /// ��ʼ���
        /// </summary>
        private void StartCheck(TreeView treeView)
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                node.Nodes.Clear();
                Task.Run(() =>
                {
                    LoadDirectory((CustomTreeNode)node);
                });
            }
        }

        /// <summary>
        /// ��Ӽ���Ŀ¼
        /// </summary>
        private void AddDir(TreeView treeView)
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                // �û�ѡ���Ŀ¼·��
                CustomTreeNode customTreeNode = new(folderBrowserDialog.SelectedPath);
                // ��Ŀ¼����ڵ�
                treeView.Nodes.Add(customTreeNode);
            }
        }



        #endregion


        private void Form1_Load(object sender, EventArgs e)
        {
            initDateTimePicker(dateTimePicker1,dateTimePicker2);
        }

        

        private void buttonStartCheck_Click(object sender, EventArgs e)
        {
            StartCheck(treeView1);
        }

        private void buttonAddDir_Click(object sender, EventArgs e)
        {
            AddDir(treeView1);
        }
    }
}
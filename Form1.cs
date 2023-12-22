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
        private bool LoadDirectory(string directoryPath, TreeNode node)
        {

            try
            {
                bool hasModifiedFiles = false;

                // ��ȡ��ǰĿ¼����Ŀ¼
                string[] subDirectories = Directory.GetDirectories(directoryPath);
                // ����ǰĿ¼�µ���Ŀ¼
                foreach (string subDirectory in subDirectories)
                {
                    //�Խڵ�Ŀ¼���м��
                    TreeNode treeNode = new (Path.GetFileName(subDirectory));
                    bool hasModified = LoadDirectory(subDirectory, treeNode);
                    if (hasModified)
                    {
                        InsertNode(node, treeNode);
                    }
                }

                //����ļ�
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    // ������ʱ������޸ĵ��ļ�
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= dateTimePicker1.Value && lastModified <= dateTimePicker2.Value)//¼���ڼ�����Χ�ļ�
                    {
                        string str = $"{Path.GetFileName(file)} ---------����޸�ʱ�䣺{lastModified}";
                        InsertNode(node, new TreeNode() { Text = str, ForeColor = Color.Red, Tag = lastModified });
                        hasModifiedFiles = true;
                    }
                    
                }
                return hasModifiedFiles;

            }
            catch (UnauthorizedAccessException ex)
            {
                UpdateNode(node, $"�޷�����Ŀ¼ {ex.Message}");
                return false;
            }

        }

        /// <summary>
        /// ����ڵ�
        /// </summary>
        /// <param name="resourcePath">��Դ·��</param>
        /// <param name="node">��ǰ�ڵ�</param>
        /// <param name="isDir">��ǰ·���Ƿ�ΪĿ¼</param>
        /// <returns></returns>
        private void AddResourceNode(string resourcePath, TreeNode node, bool isDir = true)
        {

            

            string str = $"{Path.GetFileName(resourcePath)} ---------����޸�ʱ�䣺{GetResourceLastWriteTime(resourcePath)}";
            if (!isDir)//�������Ŀ¼��ֱ�����Ҷ�ӽڵ㣬�����ص�ǰ�ڵ�
            {
                DateTime lastModified = GetResourceLastWriteTime(resourcePath);
                if (lastModified >= dateTimePicker1.Value && lastModified <= dateTimePicker2.Value)//¼���ڼ�����Χ�ļ�
                {
                    InsertNode(node, new TreeNode() { Text = str, ForeColor = Color.Red, Tag = GetResourceLastWriteTime(resourcePath) });
                }
            }
            else
            {

                //�����Ŀ¼������֧�ڵ�
                TreeNode treeNode = new TreeNode() { Text = str, ForeColor = Color.Blue, Tag = GetResourceLastWriteTime(resourcePath) };
                InsertNode(node, treeNode);


                string path = node.Text;
                UpdateNode(node, node.Text + "-----�����:" + resourcePath);
                //�Խڵ�Ŀ¼���м��
                //Check(resourcePath, treeNode);
                UpdateNode(node, path);
            }
        }

        /// <summary>
        /// ����ڵ�
        /// </summary>
        /// <param name="node"></param>
        /// <param name="aNode"></param>
        private void InsertNode(TreeNode node, TreeNode aNode) 
        {
            if (node.TreeView != null)
            {
                // �����Ҫ��ʹ�� Invoke ����ȷ���� UI �߳���ִ�в������
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Nodes.Add(aNode);
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
            if (node == null || node.TreeView == null)
            {
                return;
            }
            // ����Ƿ���Ҫ���̷߳��� UI Ԫ��
            if (node.TreeView.InvokeRequired)
            {
                // �����Ҫ��ʹ�� Invoke ����ȷ���� UI �߳���ִ�и��²���
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Text = text; // ���½ڵ���ı�����
                }));
            }
            else
            {
                // �����ǰ���� UI �߳��ϣ�ֱ�Ӹ��½ڵ���ı�����
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
                    LoadDirectory(node.Text, node);
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
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                // ��Ŀ¼����ڵ�
                treeView.Nodes.Add(selectedFolderPath);
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
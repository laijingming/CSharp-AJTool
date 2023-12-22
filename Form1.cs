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
        
        #region 逻辑

        /// <summary>
        /// 初始化时间
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
        /// 检测目录
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="node"></param>
        private bool LoadDirectory(string directoryPath, TreeNode node)
        {

            try
            {
                bool hasModifiedFiles = false;

                // 获取当前目录的子目录
                string[] subDirectories = Directory.GetDirectories(directoryPath);
                // 处理当前目录下的子目录
                foreach (string subDirectory in subDirectories)
                {
                    //对节点目录进行检测
                    TreeNode treeNode = new (Path.GetFileName(subDirectory));
                    bool hasModified = LoadDirectory(subDirectory, treeNode);
                    if (hasModified)
                    {
                        InsertNode(node, treeNode);
                    }
                }

                //检测文件
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    // 处理在时间段内修改的文件
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= dateTimePicker1.Value && lastModified <= dateTimePicker2.Value)//录入在检索范围文件
                    {
                        string str = $"{Path.GetFileName(file)} ---------最后修改时间：{lastModified}";
                        InsertNode(node, new TreeNode() { Text = str, ForeColor = Color.Red, Tag = lastModified });
                        hasModifiedFiles = true;
                    }
                    
                }
                return hasModifiedFiles;

            }
            catch (UnauthorizedAccessException ex)
            {
                UpdateNode(node, $"无法访问目录 {ex.Message}");
                return false;
            }

        }

        /// <summary>
        /// 加入节点
        /// </summary>
        /// <param name="resourcePath">资源路径</param>
        /// <param name="node">当前节点</param>
        /// <param name="isDir">当前路径是否为目录</param>
        /// <returns></returns>
        private void AddResourceNode(string resourcePath, TreeNode node, bool isDir = true)
        {

            

            string str = $"{Path.GetFileName(resourcePath)} ---------最后修改时间：{GetResourceLastWriteTime(resourcePath)}";
            if (!isDir)//如果不是目录，直接添加叶子节点，并返回当前节点
            {
                DateTime lastModified = GetResourceLastWriteTime(resourcePath);
                if (lastModified >= dateTimePicker1.Value && lastModified <= dateTimePicker2.Value)//录入在检索范围文件
                {
                    InsertNode(node, new TreeNode() { Text = str, ForeColor = Color.Red, Tag = GetResourceLastWriteTime(resourcePath) });
                }
            }
            else
            {

                //如果是目录新增分支节点
                TreeNode treeNode = new TreeNode() { Text = str, ForeColor = Color.Blue, Tag = GetResourceLastWriteTime(resourcePath) };
                InsertNode(node, treeNode);


                string path = node.Text;
                UpdateNode(node, node.Text + "-----检测中:" + resourcePath);
                //对节点目录进行检测
                //Check(resourcePath, treeNode);
                UpdateNode(node, path);
            }
        }

        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="aNode"></param>
        private void InsertNode(TreeNode node, TreeNode aNode) 
        {
            if (node.TreeView != null)
            {
                // 如果需要，使用 Invoke 方法确保在 UI 线程上执行插入操作
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Nodes.Add(aNode);
                }));
            }
            else
            {
                // 如果当前已在 UI 线程上，直插入节点
                node.Nodes.Add(aNode);
            }

 
        }

        /// <summary>
        /// 更新节点内容
        /// </summary>
        /// <param name="node"></param>
        /// <param name="text"></param>
        private void UpdateNode(TreeNode node, string text) 
        {
            if (node == null || node.TreeView == null)
            {
                return;
            }
            // 检查是否需要跨线程访问 UI 元素
            if (node.TreeView.InvokeRequired)
            {
                // 如果需要，使用 Invoke 方法确保在 UI 线程上执行更新操作
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Text = text; // 更新节点的文本内容
                }));
            }
            else
            {
                // 如果当前已在 UI 线程上，直接更新节点的文本内容
                node.Text = text;
            }
        }


        /// <summary>
        /// 获取文件或者文件夹最后修改时间
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
        /// 开始检测
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
        /// 添加检索目录
        /// </summary>
        private void AddDir(TreeView treeView)
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                // 用户选择的目录路径
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                // 将目录加入节点
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
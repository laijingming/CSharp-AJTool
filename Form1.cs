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
                    Text = $"{Path} ---修改时间{LastDateTime}";

                }
                else
                {
                    if (Nodes.Count>0)
                    {
                        Text = $"{Path} ---数量{Count} 修改时间{LastDateTime}";
                    }
                    else
                    {
                        Text = Path;
                    }
                }
            }

            public void InsertNode(CustomTreeNode subNode) 
            {
                // 检查节点所属的树视图是否存在
                if (TreeView != null)
                {
                    // 使用 Invoke 方法确保在 UI 线程上执行插入操作
                    TreeView.Invoke(new Action(() =>
                    {
                        Nodes.Add(subNode);
                        Expand();
                    }));
                }
                else
                {
                    // 如果当前已在 UI 线程上，直插入节点
                    Nodes.Add(subNode);
                }
            }
        }
        #endregion

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
        private DateTime LoadDirectory(CustomTreeNode node)
        {
            DateTime lastModifiedDateTime = new();
            try
            {   
                // 获取当前目录的子目录
                string[] subDirectories = Directory.GetDirectories(node.Path);
                // 处理当前目录下的子目录
                foreach (string subDirectory in subDirectories)
                {
                    //对节点目录进行检测
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

                //检测文件
                string[] files = Directory.GetFiles(node.Path);
                foreach (string file in files)
                {
                    // 处理在时间段内修改的文件
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= dateTimePicker1.Value && lastModified <= dateTimePicker2.Value)
                    {
                        if (node.Tag == null || (DateTime)node.Tag <= lastModified)
                        {
                            lastModifiedDateTime = lastModified;//记录目录最后修改时间
                        }
                        CustomTreeNode fileNode = new(file) { ForeColor = Color.Red, LastDateTime = lastModified, Count=1 };
                        fileNode.CustomText();
                        node.InsertNode(fileNode);
                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                UpdateNode(node, $"无法访问目录 {ex.Message}");
            }
            return lastModifiedDateTime;

        }

        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="aNode"></param>
        private void InsertNode(TreeNode node, TreeNode aNode) 
        {
            // 检查节点所属的树视图是否存在
            if (node.TreeView != null)
            {
                // 使用 Invoke 方法确保在 UI 线程上执行插入操作
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Nodes.Add(aNode);
                    node.Expand();
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
            // 检查节点所属的树视图是否存在
            if (node.TreeView != null)
            {
                // 使用 Invoke 方法确保在 UI 线程上执行插入操作
                node.TreeView.Invoke(new Action(() =>
                {
                    node.Text = text;
                }));
            }
            else
            {
                // 如果当前已在 UI 线程上，直插入节点
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
                    LoadDirectory((CustomTreeNode)node);
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
                CustomTreeNode customTreeNode = new(folderBrowserDialog.SelectedPath);
                // 将目录加入节点
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
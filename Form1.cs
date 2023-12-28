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

        #region 逻辑
        private Dictionary<TreeNode, CancellationTokenSource> runningTasks = new Dictionary<TreeNode, CancellationTokenSource>();
        /// <summary>
        /// 开始检测各节点目录
        /// </summary>
        private void StartCheck(TreeView treeView, DateTime sTime, DateTime eTime)
        {
            runningTasks.Clear();// 清除现有任务
            foreach (TreeNode node in treeView.Nodes)
            {
                node.Nodes.Clear();// 清空节点下的子节点
                CancellationTokenSource source = new CancellationTokenSource();
                // 异步执行加载目录操作
                Task.Run(() =>
                {
                    LoadDirectory((CustomTreeNode)node, sTime, eTime, source.Token);
                });
                runningTasks.Add(node, source);// 将节点和对应的取消 Token 加入任务列表
            }
        }

        /// <summary>
        /// 停止检测正在进行的任务
        /// </summary>
        private void StopCheck()
        {
            foreach (var item in runningTasks)
            {
                CancellationTokenSource source = item.Value;// 获取任务对应的取消 Token
                source.Cancel();// 取消任务执行
            }
        }

        /// <summary>
        /// 检测目录
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="node"></param>
        private void LoadDirectory(CustomTreeNode node, DateTime sTime, DateTime eTime, CancellationToken source)
        {
            if (source.IsCancellationRequested)
            {
                return;//在取消请求的情况下退出任务
            }
            try
            {
                // 获取当前目录的子目录
                string[] subDirectories = Directory.GetDirectories(node.ResourePath);
                // 处理当前目录下的子目录
                foreach (string subDirectory in subDirectories)
                {
                    //对节点目录进行检测
                    CustomTreeNode subNode = new CustomTreeNode(subDirectory);
                    LoadDirectory(subNode, sTime, eTime, source);
                    if (subNode.Nodes.Count > 0)
                    {
                        node.InsertNode(subNode);
                    }
                }
                node.UpdateNodeUI();

                //检测文件
                string[] files = Directory.GetFiles(node.ResourePath);
                foreach (string file in files)
                {
                    // 处理在时间段内修改的文件
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= sTime && lastModified <= eTime)
                    {
                        node.InsertNode(new CustomTreeNode(file) { ForeColor = Color.Red, LastDateTime = lastModified, Count = 1 });
                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                node.InsertNode(new CustomTreeNode($"无法访问目录 {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取文件或者文件夹最后修改时间
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
        /// 添加检索目录
        /// </summary>
        private static void AddDir(TreeView treeView)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                // 用户选择的目录路径
                CustomTreeNode customTreeNode = new CustomTreeNode(folderBrowserDialog.SelectedPath);
                // 将目录加入节点
                treeView.Nodes.Add(customTreeNode);
            }
        }

        /// <summary>
        /// 为TreeView创建右键菜单栏，添加删除和打开目录选项
        /// </summary>
        /// <param name="treeView">要添加右键菜单的TreeView</param>
        private static void MakeContextMenuStrip(TreeView treeView)
        {
            // 创建右键菜单
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            // 添加删除选项
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("删除");
            //删除选项的点击事件处理
            deleteMenuItem.Click += (sender, e) =>
            {
                // 获取选中的节点
                if (treeView.SelectedNode is CustomTreeNode selectedNode)
                {
                    // 删除选中的节点
                    treeView.Nodes.Remove(selectedNode);
                }
            };
            contextMenuStrip.Items.Add(deleteMenuItem);

            // 添加删除选项
            ToolStripMenuItem openFolder = new ToolStripMenuItem("打开目录");
            //删除选项的点击事件处理
            openFolder.Click += (sender, e) =>
            {
                // 获取选中的节点
                if (treeView.SelectedNode is CustomTreeNode selectedNode)
                {
                    Process.Start("explorer.exe", selectedNode.ResourePath);
                }
            };
            contextMenuStrip.Items.Add(openFolder);

            // 将 ContextMenuStrip 分配给 TreeView
            treeView.ContextMenuStrip = contextMenuStrip;
        }

        /// <summary>
        /// 初始化两个DateTimePicker控件，设置其格式为自定义日期时间格式，并设置初始值为当天和第二天
        /// </summary>
        /// <param name="p1">第一个DateTimePicker控件</param>
        /// <param name="p2">第二个DateTimePicker控件</param>
        private static void InitDateTimePicker(DateTimePicker p1, DateTimePicker p2)
        {
            // 设置日期时间格式为自定义格式
            p1.Format = DateTimePickerFormat.Custom;
            p1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            p2.Format = DateTimePickerFormat.Custom;
            p2.CustomFormat = "yyyy-MM-dd HH:mm:ss";

            // 设置初始值为当天和第二天
            p1.Text = DateTime.Today.ToString();
            p2.Text = DateTime.Today.AddDays(1).ToLongDateString();
        }

        #endregion


        // 在 Form1_Load 中初始化时间段和右键菜单
        private void Form1_Load(object sender, EventArgs e)
        {
            InitDateTimePicker(dateTimePicker1, dateTimePicker2); // 初始化时间段
            MakeContextMenuStrip(treeView1); // 创建右键菜单
        }


        //开始检测
        private void buttonStartCheck_Click(object sender, EventArgs e)
        {
            StartCheck(treeView1, dateTimePicker1.Value, dateTimePicker2.Value);
        }

        //添加目录
        private void buttonAddDir_Click(object sender, EventArgs e)
        {
            AddDir(treeView1);
        }

        //停止检测
        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopCheck();
        }
    }
}
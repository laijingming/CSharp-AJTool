using AJTOOL.Class;


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
        /// 开始检测
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
        /// 停止检测
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
        /// 检测目录
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="node"></param>
        private DateTime LoadDirectory(CustomTreeNode node, DateTime sTime, DateTime eTime, CancellationTokenSource source)
        {
            if (source.IsCancellationRequested)
            {
                return default;//收到停止检测标识，返回默认值
            }
            DateTime lastModifiedDateTime = new DateTime();
            try
            {
                // 获取当前目录的子目录
                string[] subDirectories = Directory.GetDirectories(node.ResourePath);
                // 处理当前目录下的子目录
                foreach (string subDirectory in subDirectories)
                {
                    //对节点目录进行检测
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

                //检测文件
                string[] files = Directory.GetFiles(node.ResourePath);
                foreach (string file in files)
                {
                    // 处理在时间段内修改的文件
                    DateTime lastModified = GetResourceLastWriteTime(file);
                    if (lastModified >= sTime && lastModified <= eTime)
                    {
                        if (node.Tag == null || (DateTime)node.Tag <= lastModified)
                        {
                            lastModifiedDateTime = lastModified;//记录目录最后修改时间
                        }
                        CustomTreeNode fileNode = new CustomTreeNode(file) { ForeColor = Color.Red, LastDateTime = lastModified, Count = 1 };
                        fileNode.CustomText();
                        node.InsertNode(fileNode);
                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                node.InsertNode(new CustomTreeNode($"无法访问目录 {ex.Message}"));
            }
            return lastModifiedDateTime;

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
            //using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()) 
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    // 用户选择的目录路径
                    CustomTreeNode customTreeNode = new CustomTreeNode(folderBrowserDialog.SelectedPath);
                    // 将目录加入节点
                    treeView.Nodes.Add(customTreeNode);
                }
            }
        }

        /// <summary>
        /// 创建 ContextMenuStrip
        /// </summary>
        /// <param name="treeView"></param>
        private static void MakeContextMenuStrip(TreeView treeView)
        {
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            // 添加删除选项
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("删除");
            //删除选项的点击事件处理
            deleteMenuItem.Click += (sender, e) =>
            {
                // 获取选中的节点
                TreeNode selectedNode = treeView.SelectedNode;

                if (selectedNode != null)
                {
                    // 删除选中的节点
                    treeView.Nodes.Remove(selectedNode);
                }
            };
            contextMenuStrip.Items.Add(deleteMenuItem);

            // 将 ContextMenuStrip 分配给 TreeView
            treeView.ContextMenuStrip = contextMenuStrip;
        }

        /// <summary>
        /// 删除选项的点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            // 获取选中的节点
            TreeNode selectedNode = treeView1.SelectedNode;

            if (selectedNode != null)
            {
                // 删除选中的节点
                treeView1.Nodes.Remove(selectedNode);
                // 或者，如果你想在根据业务逻辑删除，请使用 selectedNode.Remove() 方法
            }
        }

        /// <summary>
        /// 初始化时间
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
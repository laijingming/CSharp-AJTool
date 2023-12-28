### C# WinForms 实现文件变化跟踪

在实际应用中，有时会想要知道特定目录或磁盘上文件的变化情况，尤其是每天新增的文件。以下是一种使用C# WinForms的方法来轻松实现这一目标。

#### 功能概述

1.  **添加目录**：通过浏览功能选择要监控的目录，并将其添加到树形视图中。
2.  **按钮触发**：通过按钮触发执行目录检测，获取最新的文件变动情况。
3.  **简易操作界面**：简洁的用户界面包括添加目录、手动开始/停止检测、设置时间段等功能，方便灵活地进行操作。
4.  **文件信息显示**：界面清晰展示每个目录的文件数量、最后修改时间等信息，帮助了解文件的状态。

#### 初始化界面和事件处理

首先在WinForms中创建了一个简单的用户界面。通过界面中的按钮来添加要监控的目录，开始和停止检测以及设置时间段。右键菜单项可以方便地删除或者打开选定的目录。

     // 在 Form1_Load 中初始化时间段和右键菜单
     private void Form1_Load(object sender, EventArgs e)
     {
         InitDateTimePicker(dateTimePicker1, dateTimePicker2); // 初始化时间段
         MakeContextMenuStrip(treeView1); // 创建右键菜单
     }


     // 开始检测按钮点击事件
     private void buttonStartCheck_Click(object sender, EventArgs e)
     {
         StartCheck(treeView1, dateTimePicker1.Value, dateTimePicker2.Value);
     }

     //停止检测按钮点击事件
     private void buttonStop_Click(object sender, EventArgs e)
     {
         StopCheck();
     }

     //添加目录按钮点击事件
     private void buttonAddDir_Click(object sender, EventArgs e)
     {
         AddDir(treeView1);
     }

#### 搜索目录文件变化

使用了异步任务来搜索每个目录中新增的文件。通过`StartCheck`方法，遍历每个节点，为其启动一个异步任务来执行`LoadDirectory`方法。这个函数负责遍历目录、检测新增或修改文件，是应用程序核心的一部分。

     //开始检测各节点目录
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

    //检测目录
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

#### 更新UI显示

为了保证界面的实时响应，`CustomTreeNode`类(继承TreeNode)中实现了更新节点信息的方法。

    // 更新显示文本
    public void UpdateNodeUI()
    {
        SetTimeAndCount();
        if (TreeView != null)
        {
            TreeView.Invoke(new Action(() =>
            {
                SetText();
            }));
        }
        else
        {
            SetText();
        }
    }

### 全部代码

Form1.cs
```C#
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


        // 开始检测按钮点击事件
        private void buttonStartCheck_Click(object sender, EventArgs e)
        {
            StartCheck(treeView1, dateTimePicker1.Value, dateTimePicker2.Value);
        }

        //停止检测按钮点击事件
        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopCheck();
        }

        //添加目录按钮点击事件
        private void buttonAddDir_Click(object sender, EventArgs e)
        {
            AddDir(treeView1);
        }
    }
```
Class.CustomTreeNode.cs
```C#
    internal class CustomTreeNode : TreeNode
    {
        /// <summary>
        /// 资源类型 1:文件 2:文件夹 3:头节点
        /// </summary>
        private int pathType = 0;
        public CustomTreeNode(string path)
        {
            ResourePath = path;
            Text = path;
        }
        /// <summary>
        /// 资源地址
        /// </summary>
        public string ResourePath { get; set; }
        /// <summary>
        /// 拥有文件数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastDateTime { get; set; }
        
        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="subNode"></param>
        public void InsertNode(CustomTreeNode subNode)
        {
            // 检查节点所属的树视图是否存在
            if (TreeView != null)
            {   
                // 使用 Invoke 方法确保在 UI 线程上执行插入操作
                TreeView.Invoke(new Action(() =>
                {
                    Nodes.Add(subNode);
                    Expand();//展开节点
                }));
            }
            else
            {
                // 如果当前已在 UI 线程上，直插入节点
                Nodes.Add(subNode);
            }
            subNode.UpdateNodeUI();
        }

        /// <summary>
        /// 更新显示文本
        /// </summary>
        public void UpdateNodeUI()
        {
            SetTimeAndCount();
            if (TreeView != null)
            {
                TreeView.Invoke(new Action(() =>
                {
                    SetText();
                }));
            }
            else
            {
                SetText();
            }
        }
        #region 私有
        /// <summary>
        /// 设置最后修改时间和节点文件数量
        /// </summary>
        private void SetTimeAndCount()
        {   
            if (Nodes.Count <= 0) return;//过滤叶子节点
            Count = 0;
            foreach (CustomTreeNode item in Nodes)
            {
                if (LastDateTime < item.LastDateTime)
                {
                    LastDateTime = item.LastDateTime;//记录最后修改时间
                }
                Count += item.Count;//统计每个节点文件数量
            }
        }

        /// <summary>
        /// 设置显示文本
        /// </summary>
        private void SetText()
        {
            pathType = File.Exists(ResourePath) ? 1 : Parent==null ? 3 : 2;
            switch (pathType)
            {   
                case 1://文件
                    Text = $"{Path.GetFileName(ResourePath)} ---时间：{LastDateTime}";
                    break;
                case 2://文件夹
                    Text = $"{Path.GetFileName(ResourePath)} ---{Count} 时间:{LastDateTime}";
                    break;
                case 3://头节点
                    Text = $"{ResourePath} ---{Count} 时间:{LastDateTime}";
                    break; 
                default:
                    break;
            }
        }
        #endregion
    }
```
#### 总结

本文展示了如何使用C#和WinForms来实现一个简单的目录文件监控应用。通过监控目录中每日新增的文件变化，可以随时了解到目录内文件的变动情况。

这个应用程序是一个起点，你可以根据自己的需求和想法进行扩展和优化。感谢阅读！



GitHub地址：<https://github.com/laijingming/CSharp-AJTool>

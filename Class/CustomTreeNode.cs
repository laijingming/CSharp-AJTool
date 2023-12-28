
namespace AJTOOL.Class
{
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
}


namespace AJTOOL.Class
{
    internal class CustomTreeNode : TreeNode
    {
        public CustomTreeNode(string path)
        {
            ResourePath = path;
            Text = path;
        }
        public string ResourePath { get; set; }
        public int Count { get; set; }

        public DateTime LastDateTime { get; set; }

        public void CustomText()
        {
            if (Nodes.Count > 0)
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

            if (TreeView != null)
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
            string path = Path.GetFileName(ResourePath);
            if (Parent == null)
            {
                path = ResourePath;
            }
            if (File.Exists(ResourePath))
            {
                Text = $"{path} ---时间：{LastDateTime}";

            }
            else
            {
                if (Nodes.Count > 0)
                {
                    Text = $"{path} ---{Count} 时间:{LastDateTime}";
                }
                else
                {
                    Text = path;
                }
            }
        }
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
        }

        /// <summary>
        /// 更新节点内容
        /// </summary>
        /// <param name="node"></param>
        /// <param name="text"></param>
        public void UpdateNode(string text)
        {
            // 检查节点所属的树视图是否存在
            if (TreeView != null)
            {
                // 使用 Invoke 方法确保在 UI 线程上执行插入操作
                TreeView.Invoke(new Action(() =>
                {
                    Text = text;
                }));
            }
            else
            {
                // 如果当前已在 UI 线程上，直插入节点
                Text = text;
            }
        }
    }
}

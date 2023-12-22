

namespace AJTOOL.Class
{
    internal class CustomTreeNode:TreeNode
    {
        public Color NodeColor { get; set; }
        public Color TimeColor { get; set; }
        public DateTime LastModified { get; set; }

        public CustomTreeNode(string text, Color nodeColor, Color timeColor, DateTime lastModified) : base(text)
        {
            NodeColor = nodeColor;
            TimeColor = timeColor;
            LastModified = lastModified;
        }
    }
}

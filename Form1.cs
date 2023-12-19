using System.Collections;

namespace AJTOOL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 监听目录
        /// </summary>
        /// <param name="Directory"></param>
        private void Listen(string directoryPath, DateTime startTime, DateTime endTime)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            //string[] files = Directory.GetFiles(directoryPath,"*",SearchOption.TopDirectoryOnly);
            string[] files = Directory.GetFiles(directoryPath);
            foreach (string file in files)
            {
                DateTime lastModified = File.GetLastWriteTime(file);
                if (lastModified >= startTime && lastModified <= endTime)
                {
                    // 处理在时间段内修改的文件
                    Console.WriteLine($"文件 {file} 最后修改时间：{lastModified}");
                    //MessageBox.Show($"文件 {file} 最后修改时间：{lastModified}");
                    treeView1.Nodes.Add(file);
                }
            }
        }

        private static ArrayList GetDirectoryPaths()
        {
            return new ArrayList()
            {
                "D:\\CSharp\\AJTOOL",
                "D:\\CSharp\\DXTEXT",
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime startTime = new(2023, 1, 1); // 设置起始时间
            DateTime endTime = new(2023, 12, 31); // 设置结束时间
            ArrayList directoryPaths = GetDirectoryPaths();
            foreach (string directoryPath in directoryPaths)
            {
                Listen(directoryPath, startTime, endTime);
            }
        }
    }
}
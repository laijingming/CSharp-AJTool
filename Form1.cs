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
        /// ����Ŀ¼
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
                    // ������ʱ������޸ĵ��ļ�
                    Console.WriteLine($"�ļ� {file} ����޸�ʱ�䣺{lastModified}");
                    //MessageBox.Show($"�ļ� {file} ����޸�ʱ�䣺{lastModified}");
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
            DateTime startTime = new(2023, 1, 1); // ������ʼʱ��
            DateTime endTime = new(2023, 12, 31); // ���ý���ʱ��
            ArrayList directoryPaths = GetDirectoryPaths();
            foreach (string directoryPath in directoryPaths)
            {
                Listen(directoryPath, startTime, endTime);
            }
        }
    }
}
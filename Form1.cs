using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace duplicatedFilesScanner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<MyFile> fileList;
        Dictionary<long, List<MyFile>> sizeMap;
        Dictionary<string, List<MyFile>> md5Map;
        List<List<MyFile>> filesWithSameNameAndSize;
        public string GetFileSize(long size)
        {
            double len = size;
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            string filesize = String.Format("{0:0.##} {1}", len, sizes[order]);
            return filesize;
        }
        private void start(List<string> startFolderList)
        {
            fileList = new List<MyFile>();
            sizeMap = new Dictionary<long, List<MyFile>>();
            md5Map = new Dictionary<string, List<MyFile>>();
            filesWithSameNameAndSize = new List<List<MyFile>>();
            treeView1.Nodes.Clear();
            Thread backThread = new Thread(() =>
            {
                ScanFiles(startFolderList);
                fillSizeMap();
                filterDiffName();
                showTreeMap();
            });
            backThread.Start();
        }

        private void filterDiffName()
        {
            List<List<MyFile>> filesHasSameSize_List = sizeMap.Values.ToList();
            foreach (var filesWithSameSize in filesHasSameSize_List)
            {
                Dictionary<string, List<MyFile>> fileNameMap = new Dictionary<string, List<MyFile>>();
                foreach (var file in filesWithSameSize)
                {
                    if (fileNameMap.TryGetValue(file.name, out var list) == false)
                    {
                        list = new List<MyFile>();
                        fileNameMap.Add(file.name, list);
                    }
                    list.Add(file);
                }
                foreach (var fileNameList in fileNameMap.Values)
                {
                    if (fileNameList.Count > 1)
                    {
                        filesWithSameNameAndSize.Add(fileNameList);
                    }
                }
            }
        }

        private void showTreeMap()
        {
            //List<List<MyFile>> filesHasSameSize_List = sizeMap.Values.ToList();
            //filesHasSameSize_List = filesHasSameSize_List.FindAll(item => item.Count > 1);
            filesWithSameNameAndSize.Sort((x, y) => { return x[0].size == y[0].size ? 0 : (x[0].size > y[0].size ? -1 : 1); });
            //int i = 0;
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (var item in filesWithSameNameAndSize)
            {
                //if (i++ > 100)
                //{
                //    return;
                //}
                string str = ("size:" + GetFileSize(item[0].size)).PadRight(15) + ("count:" + item.Count).PadRight(12) + "path:"+item[0].path;
                TreeNode node = new TreeNode(str);

                node.Tag = item;
                foreach (var file in item)
                {
                    var fileNode = node.Nodes.Add(file.path);
                    fileNode.Tag = file;
                }
                nodes.Add(node);
            }
            this.Invoke(new MethodInvoker(() =>
            {
                treeView1.Nodes.AddRange(nodes.ToArray());
            }));
        }

        private void fillMd5Map()
        {

        }

        private void fillSizeMap()
        {
            foreach (var item in fileList)
            {
                if (sizeMap.TryGetValue(item.size, out var list) == false)
                {
                    list = new List<MyFile>();
                    sizeMap.Add(item.size, list);
                }
                list.Add(item);
            }
        }
        private void ScanFiles(List<string> startFolders)
        {
            foreach (var folderPath in startFolders)
            {
                DirectoryInfo info = new DirectoryInfo(folderPath);
                ScanFilesRecursively(info);
            }           
        } 
        private void ScanFilesRecursively(DirectoryInfo folder)
        {
            try
            {
                FileInfo[] files = folder.GetFiles();
                foreach (FileInfo file in files)
                {
                    fileList.Add(new MyFile(file));
                }
                DirectoryInfo[] subFolders = folder.GetDirectories();
                logStatus(fileList.Count + " files scanned");
                foreach (DirectoryInfo subFolder in subFolders)
                {
                    ScanFilesRecursively(subFolder);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void logStatus(string str)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                this.toolStripStatusLabel1.Text = str;
            }));
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.contextMenuStripTreeNode.Items.Clear();
            if (e.Button == MouseButtons.Right && e.Node != null)
            {
                var selectedNode = e.Node;
                treeView1.SelectedNode = selectedNode;
                if (selectedNode.Parent != null) //file node selected
                {
                    ToolStripMenuItem menu1 = new ToolStripMenuItem();
                    menu1.Text = "open path";
                    menu1.Click += new System.EventHandler((sender1, e1) =>
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = "explorer.exe";
                        p.StartInfo.Arguments = @"/e,/select," + selectedNode.Text;
                        p.Start();
                    });

                    ToolStripMenuItem menu2= new ToolStripMenuItem();
                    menu2.Text = "open file";
                    menu2.Click += new System.EventHandler((sender1, e1) =>
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = "explorer.exe";
                        p.StartInfo.Arguments = selectedNode.Text;
                        p.Start();
                    });

                    ToolStripMenuItem menu3= new ToolStripMenuItem();
                    menu3.Text = "delete";
                    menu3.Click += new System.EventHandler((sender1, e1) =>
                    {
      
                    });
                    this.contextMenuStripTreeNode.Items.Clear();
                    this.contextMenuStripTreeNode.Items.AddRange(new ToolStripItem[]{ menu1,menu2,menu3});
                    this.contextMenuStripTreeNode.Show(MousePosition.X, MousePosition.Y);
                }
                else //fileGroup node selected
                {
                    ToolStripMenuItem menu1 = new ToolStripMenuItem();
                    menu1.Text = "compute hash";
                    menu1.Click += new System.EventHandler((sender1, e1) =>
                    {
                        //todo
                    });
                    ToolStripMenuItem menu2 = new ToolStripMenuItem();
                    menu2.Text = "compare content";
                    menu2.Click += new System.EventHandler((sender1, e1) =>
                    {
                        //todo
                    });

                    this.contextMenuStripTreeNode.Items.Clear();
                    this.contextMenuStripTreeNode.Items.AddRange(new ToolStripItem[] { menu1,menu2 });
                    this.contextMenuStripTreeNode.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void start2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectPath selectPathDialog = new SelectPath((paths)=> { start(paths); });
            selectPathDialog.ShowDialog();
        }
    }
}

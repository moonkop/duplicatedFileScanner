using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace duplicatedFilesScanner
{
    public partial class SelectPath : Form
    {
        public delegate void SelectPathCallback(List<string> paths);

        public SelectPath(SelectPathCallback callback)
        {
            InitializeComponent();
            this.callback = callback;
        }
        List<string> paths = new List<string>();
        SelectPathCallback callback;
        private void button_add_Click(object sender, EventArgs e)
        {
            string newPath = getPathFromDialog();
            paths.Add(newPath);
            ShowPathsInListBox();  
        }
        private void ShowPathsInListBox()
        {
            listBox1.Items.Clear();
            foreach (var path in paths)
            {
                listBox1.Items.Add(path);
            }
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                paths.RemoveAt(listBox1.SelectedIndex);
            }
            ShowPathsInListBox();
        }
        private string getPathFromDialog()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            return fbd.SelectedPath;
        }

        private void SelectPath_Load(object sender, EventArgs e)
        {

        }

        private void button_start_Click(object sender, EventArgs e)
        {
            if (paths.Count==0)
            {
                MessageBox.Show("please select a folder at least");
            }
            callback(paths);
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//bug after select
//generate more sub dir
namespace File_explorer
{
    public partial class Form1 : Form
    {
        Stack<string> stackPath = new Stack<string>();
        List<string> sourceFile = new List<string>();
        List<string> targetFile = new List<string>();
        List<string> fileName = new List<string>();
        enum currentStatus
        {
            copy,
            cut
        }
        currentStatus currentSTT;
        public Form1()
        {
            InitializeComponent();
        }
        private void InvalidateTreeView()
        {
            //clear tree view on the left
            if (treeView1 != null)
                treeView1.Nodes.Clear();

            //add computer node
            TreeNode tnMycomputer = new TreeNode();
            treeView1.Nodes.Add(tnMycomputer);
            tnMycomputer.Text = "My Computer";

            //get list of disk
            ManagementObjectSearcher getDiskQuery = new ManagementObjectSearcher("select * from win32_logicaldisk");
            ManagementObjectCollection disks = getDiskQuery.Get();

            //add list of disk node to my computer node
            foreach (var disk in disks)
            {
                tnMycomputer.Nodes.Add(disk.GetPropertyValue("Name").ToString() + "\\");
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InvalidateTreeView();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode seletedNode = e.Node;
                if (seletedNode.Text.CompareTo("My Computer") != 0)
                {

                    string newPath = GetCurrentPath(seletedNode);
                    //check path of selected node
                    if (!Directory.Exists(newPath))
                        throw new Exception("The selected node is invalid");
                    //get folders of selected node
                    //discover disks on the my computer
                    string[] strFolders = Directory.GetDirectories(newPath);

                    foreach (var item in strFolders)
                    {
                        string[] splittedFrag = item.Split('\\');
                        seletedNode.Nodes.Add(splittedFrag[splittedFrag.Length - 1]);
                    }
                    //show on right
                    ShowDetail(seletedNode);
                    //show path in txtbox
                    txtbox_path.Text = newPath;
                    //push path to stack 
                    stackPath.Push(newPath);
                    //webBrowser1.Navigate(new Uri(GetCurrentPath(seletedNode)));

                }


            }
            catch
            {

            }
        }
        string GetCurrentPath(TreeNode selectedNode)
        {
            //remove my computer from the path
            //create new path
            string[] fragPath = selectedNode.FullPath.Split('\\');
            string newPath = fragPath[1];
            for (int i = 2; i < fragPath.Length; i++)
            {
                newPath += fragPath[i] + "\\";
            }
            return newPath;
        }
        private void ShowDetail(string path)
        {
            //clear
            listView1.Items.Clear();

            // get path info to show in the right
            DirectoryInfo currentDir = new DirectoryInfo(path);
            //show some information of subs directories
            foreach (var folder in currentDir.GetDirectories())
            {
                string[] tempItem = new string[5];
                tempItem[0] = folder.Name;
                tempItem[1] = "Folder";
                tempItem[2] = "...";
                tempItem[3] = folder.CreationTime.ToString();
                tempItem[4] = folder.LastWriteTime.ToString();
                ListViewItem item = new ListViewItem(tempItem);
                listView1.Items.Add(item);
            }
            //show files info of directory
            foreach (var file in currentDir.GetFiles())
            {
                string[] tempFile = new string[5];
                tempFile[0] = file.Name;
                tempFile[1] = file.Extension;
                tempFile[2] = (file.Length / 1024).ToString() + "KB";
                tempFile[3] = file.CreationTime.ToString();
                tempFile[4] = file.LastWriteTime.ToString();

                ListViewItem item = new ListViewItem(tempFile);
                listView1.Items.Add(item);
            }
        }
        private void ShowDetail(TreeNode treeNode)
        {
            //clear
            listView1.Items.Clear();
            string newPath = GetCurrentPath(treeNode);
            // get path info to show in the right
            DirectoryInfo currentDir = new DirectoryInfo(newPath);
            //show some information of subs directories
            foreach (var folder in currentDir.GetDirectories())
            {
                string[] tempItem = new string[5];
                tempItem[0] = folder.Name;
                tempItem[1] = "Folder";
                tempItem[2] = "...";
                tempItem[3] = folder.CreationTime.ToString();
                tempItem[4] = folder.LastWriteTime.ToString();
                ListViewItem item = new ListViewItem(tempItem);
                listView1.Items.Add(item);
            }
            //show files info of directory
            foreach (var file in currentDir.GetFiles())
            {
                string[] tempFile = new string[5];
                tempFile[0] = file.Name;
                tempFile[1] = file.Extension;
                tempFile[2] = (file.Length / 1024).ToString() + "KB";
                tempFile[3] = file.CreationTime.ToString();
                tempFile[4] = file.LastWriteTime.ToString();

                ListViewItem item = new ListViewItem(tempFile);
                listView1.Items.Add(item);
            }
        }
        //delete function
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {

                while (listView1.SelectedItems.Count > 0)
                {
                    //delete file
                    try
                    {
                        string path = stackPath.Peek() + listView1.SelectedItems[0].Text;
                        File.Delete(stackPath.Peek() + listView1.SelectedItems[0].Text);
                    }
                    catch { }
                    //delete dir
                    try
                    { 
                        Directory.Delete(stackPath.Peek() + "\\" + listView1.SelectedItems[0].Text, true);
                    }
                    catch { }
                    //delete in list view
                    listView1.Items.Remove(listView1.SelectedItems[0]);
                }
                //reset treeview
                InvalidateTreeView();
            }
        }

        //up 
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("clicked");
        }
        //up
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            //go to path in stack
            //remove it from stack 
            string path = "";
            if (stackPath.Count > 0)
            {
                stackPath.Pop();
                if (stackPath.Count > 0)
                {
                    path = stackPath.Peek();
                    //show in right
                    ShowDetail(path);
                    //show in text box
                    txtbox_path.Text = path;
                }
            }
            

        }

        //copy
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count>0)
            {
                //set current status
                currentSTT = currentStatus.copy;

                //reset sourceFile list
                //reset fileName list
                fileName.Clear();
                sourceFile.Clear();

                //init sourceFile
                //init fileName
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    fileName.Add(listView1.SelectedItems[i].Text);
                    sourceFile.Add(txtbox_path.Text +  fileName[i]);
                }

            }
        }

        //paste
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            //init targetFile
            for (int i = 0; i < sourceFile.Count; i++)
            {
                targetFile.Add(txtbox_path.Text + fileName[i]);
            }
            if (currentSTT == currentStatus.copy)
            {
                //loop to copy
                for (int i = 0; i < sourceFile.Count; i++)
                {
                    //copy file
                    try
                    {
                        File.Copy(sourceFile[i], targetFile[i], true);
                    }
                    //copy folder
                    catch
                    {
                        string[] files = System.IO.Directory.GetFiles(sourceFile[i]);

                        // Copy the files and overwrite destination files if they already exist.
                        foreach (string s in files)
                        {

                            //create folder if it is not exist
                            if (!Directory.Exists(targetFile[i]))
                                Directory.CreateDirectory(targetFile[i]);

                            // Use static Path methods to extract only the file name from the path.
                            string fileName = System.IO.Path.GetFileName(s);
                            string destFile = System.IO.Path.Combine(targetFile[i], fileName);
                            System.IO.File.Copy(s, destFile, true);
                        }
                    }
                }
            }
            else if(currentSTT == currentStatus.cut)
            {
                //loop to move
                for (int i = 0; i < sourceFile.Count; i++)
                {
                    
                    try
                    {
                        Directory.Move(sourceFile[i], targetFile[i]);
                    }
                    catch
                    {
                        DialogResult result= MessageBox.Show("File already exist, do you want to replace it ?","Warning !!!", MessageBoxButtons.YesNo);
                        if(result==DialogResult.Yes)
                        {
                            File.Delete(targetFile[i]);
                            File.Move(sourceFile[i], targetFile[i]);
                        }
                    }

                    
                }
            }
            //reset the list view
            ShowDetail(stackPath.Peek());
        }

        //cut
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                //set current status
                currentSTT = currentStatus.cut;

                //reset sourceFile
                //reset targetFile
                //reset fileName
                sourceFile.Clear();
                targetFile.Clear();
                fileName.Clear();

                //init sourceFile
                //init fileName
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    fileName.Add(listView1.SelectedItems[i].Text);
                    sourceFile.Add(txtbox_path.Text + fileName[i]);
                }
            }
        }

        //view large icon
        private void largeIconToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            //reset list view
            ShowDetail(stackPath.Peek());
        }
        //view small icon
        private void smallIconToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            //reset list view
            ShowDetail(stackPath.Peek());
        }
        //view list
        private void listToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            //reset list view
            ShowDetail(stackPath.Peek());
        }
        //view detail
        private void detailToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
            //reset list view
            ShowDetail(stackPath.Peek());
        }
    }
}

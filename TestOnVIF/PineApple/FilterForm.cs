using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamView
{
    public partial class FilterForm : Form
    {
        public Form1 parent;
        internal static Station st = new Station();
        public FilterForm()
        {
            InitializeComponent();
            InitTreeView();
            AddChildren();
        }

        private void InitTreeView1()
        {
            TreeNode node;
            node = new TreeNode("ทั้งหมด");
            node.Checked = true;
            treeView1.Nodes.Add(node);
            node = new TreeNode("สภ.นครขอนแก่น");
            treeView1.Nodes.Add(node);
            node = new TreeNode("สภ.มหาวิทยาลัยขอนแก่น");
            treeView1.Nodes.Add(node);
            node = new TreeNode("สภ.บ้านไผ่");
            treeView1.Nodes.Add(node);
            node = new TreeNode("ภจว.มุกดาหาร");
            treeView1.Nodes.Add(node);
        }

        private void InitTreeView()
        {
            TreeNode treeNode;

            //treeNode = new TreeNode("ทั้งหมด");
            //treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[401] ขอนแก่น");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[402] อุดรธานี");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[403] กาฬสินธุ์");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[404] ร้อยเอ็ด");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[405] หนองคาย");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[406] สกลนคร");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[407] มหาสารคาม");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[408] เลย");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[409] นครพนม");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[410] มุกดาหาร");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[411] หนองบัวลำภู");
            treeView1.Nodes.Add(treeNode);
            treeNode = new TreeNode("[412] บึงกาฬ");
            treeView1.Nodes.Add(treeNode);

        }
        private void AddChildren()
        {
            DataRow[] rows;
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                string s = string.Format("{0:D2}", i + 1);
                string sfilter = string.Format("ID >= 4{0}000 AND ID <= 4{0}999", s);
                rows = st.dtStation.Select(sfilter);
                foreach (DataRow row in rows)
                {
                    TreeNode node = new TreeNode(string.Format("[{0}] {1}", row["ID"].ToString(), row["Name"].ToString()));
                    node.Tag = row["ID"];
                    treeView1.Nodes[i].Nodes.Add(node);
                    childrenCount++;
                }
            }
        }

        List<TreeNode> children = new List<TreeNode>();
        int childrenCount = 0;

        private void FilterForm_Load(object sender, EventArgs e)
        {
            //for(int i = 0; i < treeView1.Nodes.Count; i++)
            //    treeView1.Nodes[i].Checked = true;
            if (checkBoxAll.Checked)
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                    treeView1.Nodes[i].Checked = true;
            else
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                    treeView1.Nodes[i].Checked = false;

            int count = parent.GetMarkerCount();
            textBox1.Text = string.Format("จำนวนกล้อง {0} ตัว", count);
        }

        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //children.Clear();
            //for (int i = 0; i < treeView1.Nodes.Count; i++)
            //{
            //    foreach (TreeNode node in treeView1.Nodes[i].Nodes)
            //    {
            //        if (node.Checked) children.Add(node);
            //    }
            //}
        }

        private void checkBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAll.CheckState == CheckState.Indeterminate) return;
            for (int i = 0; i < treeView1.Nodes.Count; i++)
                treeView1.Nodes[i].Checked = checkBoxAll.Checked;
            children.Clear();
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                foreach (TreeNode node in treeView1.Nodes[i].Nodes)
                {
                    if (node.Checked) children.Add(node);
                }
            }
            //MessageBox.Show(children.Count.ToString());
            treeView1.CollapseAll();
            textBox1.Text = string.Format("จำนวนกล้อง {0} ตัว", parent.GetMarkerCount(children));
            parent.Focus();
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
            children.Clear();
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                foreach (TreeNode node in treeView1.Nodes[i].Nodes)
                {
                    if (node.Checked) children.Add(node);
                }
            }
            if (children.Count == 0)
                checkBoxAll.CheckState = CheckState.Unchecked;
            else if (children.Count == childrenCount)
                checkBoxAll.CheckState = CheckState.Checked;
            else
                checkBoxAll.CheckState = CheckState.Indeterminate;
            textBox1.Text = string.Format("จำนวนกล้อง {0} ตัว", parent.GetMarkerCount(children));
            parent.Focus();
        }
    }
}

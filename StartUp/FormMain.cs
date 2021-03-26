using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace StartUp
{
    public partial class FormMain : Form
    {
        private int m_WindowMode = 0;
        private Int64 m_CheckDelay = 10000;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Close MU Server Start Up?", "Question", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showNormalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem target = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem item in target.Owner.Items)
            {
                item.Checked = (item == target);

                if (item.Checked)
                {
                    this.m_WindowMode = target.Owner.Items.IndexOf(item);
                }
            }
        }

        private void secondsToolStripMenuItem10_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem target = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem item in target.Owner.Items)
            {
                item.Checked = (item == target);

                if (item.Checked)
                {
                    this.m_CheckDelay = target.Owner.Items.IndexOf(item) * 1000;
                }
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Filter = "Executable Files (*.exe)|*.exe";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show(openFileDialog.FileName);
                }
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}

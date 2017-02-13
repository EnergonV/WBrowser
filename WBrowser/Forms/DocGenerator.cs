using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Diagnostics;

using BestCS.DocNet.Documenters;
using BestCS.DocNet.Documenters.HTMLDocumenter;

namespace WBrowser
{
    public partial class DocGenerator : Form
    {
        string projectName,
            chmOutFile,
            excludeFileName,
            excludeNameSpace,
            outPath;



        public DocGenerator()
        {
            InitializeComponent();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            projectName = textBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            generateDocumentation();
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Вывод диалога выбора папки
            vyborPapkiDlg = new FolderBrowserDialog();
            vyborPapkiDlg.ShowDialog();
            textBox5.Text = vyborPapkiDlg.SelectedPath;
            outPath = textBox5.Text;
        }

        private void textBox4_Enter(object sender, EventArgs e)
        {
            excludeNameSpace = textBox4.Text;
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            chmOutFile = textBox2.Text;
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            excludeFileName = textBox3.Text;
        }

        void generateDocumentation()
        {


        }
    }
}

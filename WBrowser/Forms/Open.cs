// Goga Claudia
//WBrowser 2009
//Email : goga.claudia@gmail.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonMark;
using CommonMark.Syntax;
using System.IO;

namespace WBrowser
{
    public partial class Open : Form
    {
        WebBrowser wb;

        public Open(WebBrowser wb)
        {
            this.wb = wb;
             InitializeComponent();
            //sources = new List<System.IO.TextReader>();			
        }
		
		private string loadMDFile(string file)
		{
			CommonMark.CommonMarkSettings settings = CommonMark.CommonMarkSettings.Default.Clone();
			string itog;
			
                // by using a in-memory source, the disparity of results is reduced.
            if(File.Exists(file))
            {
                using (var reader = new System.IO.StreamReader(file))
                	using (var writer = new System.IO.StringWriter())
                {
                    try
                    {						
						CommonMark.CommonMarkConverter.Convert(reader, writer, settings);				
						itog = writer.ToString();
                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }
                }
				return itog;
                    
             }
            else MessageBox.Show(string.Format("Файл {0} не обнаружен.", file));
            return null;
		}

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.EndsWith(".md"))
            {           
            	wb.DocumentText = loadMDFile(textBox1.Text);
            	this.Close();
            }
            else
            {
            	if(File.Exists(textBox1.Text)) wb.Navigate(textBox1.Text);
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Текстовые файлы(*.txt)|*.txt|Файлы Html(*.html *.htm)|*.html;*.htm|Файлы Md(*.md)|*.md|Все файлы|*.*";
            if (o.ShowDialog() == DialogResult.OK)
                textBox1.Text = o.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text.EndsWith(".md"))
                {
                	wb.DocumentText = loadMDFile(textBox1.Text);
                    this.Close();            
                }
                else
                { 
                	wb.Navigate(string.Format("file:///{0}", textBox1.Text));
                this.Close();
                }
           }
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using Development.Controls;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;


namespace WBrowser
{
    [ComVisibleAttribute(false)]
    [ClassInterfaceAttribute(ClassInterfaceType.AutoDispatch)]
    [DockingAttribute(DockingBehavior.AutoDock)]
    [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public partial class WBrowser : Form
    {

    	public static String favXml = Application.StartupPath+@"\data\favorits.xml", linksXml = Application.StartupPath+@"\data\links.xml";
        String settingsXml=Application.StartupPath+@"\data\settings.xml", historyXml=Application.StartupPath+@"\data\history.xml";
        List<String> urls = new List<String>();
        XmlDocument settings = new XmlDocument();
        String homePage;
        CultureInfo currentCulture;
       public static HtmlEditorForm hedit;
        string  name;
		string[] args;
        string theAddress = "about:blank";
        DocServer docServer;
		bool pageIsLoaded;
        
		public string Address
        {
            set
            {
                theAddress = value;
            }
            get
            {
                return theAddress;
            }
        }
		
		public string DocumentText
		{
		  get
		  {
		  	Stream documentStream = getCurrentBrowser().DocumentStream;
			if (documentStream == null)
			  return "";
			StreamReader streamReader = new StreamReader(documentStream);
			documentStream.Position = 0L;
			return streamReader.ReadToEnd();
		  }
		}

	   
	   
        public  WBrowser()
        {
            InitializeComponent();
            currentCulture = CultureInfo.CurrentCulture;					
			
        }
        
 		
		 public WBrowser(String url)
        {
            InitializeComponent();
            currentCulture = CultureInfo.CurrentCulture;
            this.Address = url;
        }

		public WBrowser(string[] args)
		{
			InitializeComponent();
			this.args = args;
            currentCulture = CultureInfo.CurrentCulture;
			string filePath = Application.StartupPath+@"\data";				
		
				//Проверка наличия папки для данных			
				try
				   {
					 if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);		 
				   }
				catch (IOException ex)
				{
					MessageBox.Show("В процессе преобразования произошла ошибка: "+ex.Message,"Неудачная Операция",MessageBoxButtons.OK ,MessageBoxIcon.Warning);
			    } 

			}
		
			
        #region Form load/Closing/Closed

        //visible items
        private void setVisibility()
        {
            if (!File.Exists(settingsXml))
            {
                XmlElement r = settings.CreateElement("settings");
                settings.AppendChild(r);
                XmlElement el ;
                
                el=settings.CreateElement("menuBar");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("adrBar");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("linkBar");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("favoritesPanel");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("SplashScreen");
                el.SetAttribute("checked", "True");
                r.AppendChild(el);

                 el = settings.CreateElement("homepage");
                el.InnerText="http://localhost:81";
                r.AppendChild(el);

                el = settings.CreateElement("dropdown");
                el.InnerText = "15";
                r.AppendChild(el);
            }
            else
            {
                settings.Load(settingsXml);
                XmlElement r = settings.DocumentElement;
                menuBar.Visible = (r.ChildNodes[0].Attributes[0].Value.Equals("True"));
                adrBar.Visible = (r.ChildNodes[1].Attributes[0].Value.Equals("True"));
                linkBar.Visible=(r.ChildNodes[2].Attributes[0].Value.Equals("True"));
                favoritesPanel.Visible = (r.ChildNodes[3].Attributes[0].Value.Equals("True"));
               // splashScreenToolStripMenuItem.Checked = (r.ChildNodes[4].Attributes[0].Value.Equals("True"));
                homePage=r.ChildNodes[4].InnerText;
            }

            this.linksBarToolStripMenuItem.Checked = linkBar.Visible;
            this.menuBarToolStripMenuItem.Checked = menuBar.Visible;
            this.commandBarToolStripMenuItem.Checked = adrBar.Visible;
            //splashScreenToolStripMenuItem.Checked = (settings.DocumentElement.ChildNodes[4].Attributes[0].Value.Equals("True"));
            homePage = settings.DocumentElement.ChildNodes[4].InnerText;
        }
		
		
		
		 // загрузка главной формы
        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            comboBox1.SelectedItem = comboBox1.Items[0];
            setVisibility();
			docServer = new DocServer();
            this.toolStripStatusLabel1.Text = docServer.Start();


            //	Проверка наличия аргументов и их обработка  		
            if (args != null && args.Length > 1)
            {
                for (int i = 0; i < this.args.Length; i++)
                {
                    string arg = this.args[i];
                    if (File.Exists(arg) && Application.ExecutablePath != arg)
                    {
                        if (arg.EndsWith(".md"))
                        {
                            var text = loadMDFile(arg);
                            Address = string.Format("file:///{0}", arg);
                            addNewTab(Address);
                            pageDocName.Visible = true;
                            pageDocName.Text = "Файл: " + string.Format("file:///{0}", arg);
                            this.getCurrentBrowser().DocumentText = text;
                        }
                        else
                        {
                            Address = string.Format("file:///{0}", arg);
                            addNewTab(Address);
                        }

                    }
                }
            }
            else { addNewTab(Address); }

           // this.toolStripStatusLabel1.Text = "Готово";
			/*
		 //
	    // The simplest overload of MessageBox.Show. [1]
	    //
	    MessageBox.Show("Dot Net Perls is awesome.");
	    //
	    // Dialog box with text and a title. [2]
	    //
	    MessageBox.Show("Dot Net Perls is awesome.",
		"Important Message");
	    //
	    // Dialog box with two buttons: yes and no. [3]
	    //
	    DialogResult result1 = MessageBox.Show("Is Dot Net Perls awesome?",
		"Important Question",
		MessageBoxButtons.YesNo);
	    //
	    // Dialog box with question icon. [4]
	    //
	    DialogResult result2 = MessageBox.Show("Is Dot Net Perls awesome?",
		"Important Query",
		MessageBoxButtons.YesNoCancel,
		MessageBoxIcon.Question);
	    //
	    // Dialog box with question icon and default button. [5]
	    //
	    DialogResult result3 = MessageBox.Show("Is Visual Basic awesome?",
		"The Question",
		MessageBoxButtons.YesNoCancel,
		MessageBoxIcon.Question,
		MessageBoxDefaultButton.Button2);
	    //
	    // Test the results of the previous three dialogs. [6]
	    //
	    if (result1 == DialogResult.Yes &&
		result2 == DialogResult.Yes &&
		result3 == DialogResult.No)
	    {
		MessageBox.Show("You answered yes, yes and no.");
	    }
	    //
	    // Dialog box that is right-aligned (not useful). [7]
	    //
	    MessageBox.Show("Dot Net Perls is the best.",
		"Critical Warning",
		MessageBoxButtons.OKCancel,
		MessageBoxIcon.Warning,
		MessageBoxDefaultButton.Button1,
		MessageBoxOptions.RightAlign,
		true);
	    //
	    // Dialog box with exclamation icon. [8]
	    //
	    MessageBox.Show("Dot Net Perls is super.",
		"Important Note",
		MessageBoxButtons.OK,
		MessageBoxIcon.Exclamation,
		MessageBoxDefaultButton.Button1);
		*/
        }		

        //form closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
			docServer.Stop();
            if (browserTabControl.TabCount != 2)
            {
                DialogResult dlg_res = (new Close()).ShowDialog();

                if (dlg_res == DialogResult.No) { e.Cancel = true; closeTab(); }
                else if (dlg_res == DialogResult.Cancel) e.Cancel = true;
                else Application.ExitThread();
            }
        }
        //form closed
        private void WBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            settings.Save(settingsXml);			
           
        }

         #endregion

        #region FAVORITES,LINKS,HISTORY METHODS 

        //addFavorit method
        private void addFavorit(String url, string name)
        {
            XmlDocument myXml = new XmlDocument();
            XmlElement el = myXml.CreateElement("favorit");
            el.SetAttribute("url", url);
          //  el.SetAttribute("favicon", favicon);
            el.InnerText = name;
            if (!File.Exists(favXml))
            {
                XmlElement root = myXml.CreateElement("favorites");
                myXml.AppendChild(root);
                root.AppendChild(el);
            }
            else
            {
                myXml.Load(favXml);
                myXml.DocumentElement.AppendChild(el);
            }
            if (favoritesPanel.Visible == true)
            {
                TreeNode node = new TreeNode(el.InnerText, faviconIndex(el.GetAttribute("url")), faviconIndex(el.GetAttribute("url")));
                node.ToolTipText = el.GetAttribute("url");
                node.Name = el.GetAttribute("url");
                node.ContextMenuStrip = favContextMenu;
                favTreeView.Nodes.Add(node);
            }
            myXml.Save(favXml);
        }
        //addLink method
        private void addLink(String url, string name)
        {
            XmlDocument myXml = new XmlDocument();
            XmlElement el = myXml.CreateElement("link");
            el.SetAttribute("url", url);
            el.InnerText = name;

            if (!File.Exists(linksXml))
            {
                XmlElement root = myXml.CreateElement("links");
                myXml.AppendChild(root);
                root.AppendChild(el);
            }
            else
            {
                myXml.Load(linksXml);
                myXml.DocumentElement.AppendChild(el);
            }
            if (linkBar.Visible == true)
            {
                ToolStripButton b =
                          new ToolStripButton(el.InnerText, getFavicon(url), items_Click, el.GetAttribute("url"));
                b.ToolTipText = el.GetAttribute("url");
                b.MouseUp += new MouseEventHandler(b_MouseUp);
                linkBar.Items.Add(b);
            }

            if (favoritesPanel.Visible == true)
            {
                TreeNode node = new TreeNode(el.InnerText, faviconIndex(url), faviconIndex(el.GetAttribute("url")));
                node.Name = el.GetAttribute("url");
                node.ToolTipText = el.GetAttribute("url");
                node.ContextMenuStrip = linkContextMenu;
                favTreeView.Nodes[0].Nodes.Add(node);
            }
            myXml.Save(linksXml);
        }
        //delete link method
        private void deleteLink()
        {
             if (favoritesPanel.Visible == true)
                favTreeView.Nodes[0].Nodes[Address].Remove();
             if (linkBar.Visible == true)
                 linkBar.Items.RemoveByKey(Address);
            XmlDocument myXml = new XmlDocument();
            myXml.Load(linksXml);
            XmlElement root = myXml.DocumentElement;
            foreach (XmlElement x in root.ChildNodes)
            {
                if (x.GetAttribute("url").Equals(Address))
                {
                    root.RemoveChild(x);
                    break;
                }
            }

            myXml.Save(linksXml);
        }
        //renameLink method
        private void renameLink()
        {
            RenameLink rl = new RenameLink(name);
            if (rl.ShowDialog() == DialogResult.OK)
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(linksXml);
                foreach (XmlElement x in myXml.DocumentElement.ChildNodes)
                {
                    if (x.InnerText.Equals(name))
                    {
                        x.InnerText = rl.newName.Text;
                        break;
                    }
                }
                if(linkBar.Visible==true)
                  linkBar.Items[Address].Text = rl.newName.Text;
                if(favoritesPanel.Visible==true)
                favTreeView.Nodes[0].Nodes[Address].Text = rl.newName.Text;
                myXml.Save(linksXml);
            }
            rl.Close();
        }
        //delete favorit method
        private void deleteFavorit()
        {
            favTreeView.SelectedNode.Remove();

            XmlDocument myXml = new XmlDocument();
            myXml.Load(favXml);
            XmlElement root = myXml.DocumentElement;
            foreach (XmlElement x in root.ChildNodes)
            {
                if (x.GetAttribute("url").Equals(Address))
                {
                    root.RemoveChild(x);
                    break;
                }
            }

            myXml.Save(favXml);

        }
        //renameFavorit method
        private void renameFavorit()
        {
            RenameLink rl = new RenameLink(name);
            if (rl.ShowDialog() == DialogResult.OK)
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(favXml);
                foreach (XmlElement x in myXml.DocumentElement.ChildNodes)
                {
                    if (x.InnerText.Equals(name))
                    {
                        x.InnerText = rl.newName.Text;
                        break;
                    }
                }
                favTreeView.Nodes[Address].Text = rl.newName.Text;
                myXml.Save(favXml);
            }
            rl.Close();
        }
		
		 /// <summary>
        /// Loads a md file and converts it into html.
        /// </summary>
		public string loadMDFile(string file)
		{
			CommonMark.CommonMarkSettings settings = CommonMark.CommonMarkSettings.Default.Clone();
			string itog = string.Empty;
			
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
                    catch (CommonMark.CommonMarkException ex)
                    {                        
                       MessageBox.Show("В процессе преобразования произошла ошибка: "+ex.Message,"Неудачная Операция",MessageBoxButtons.OK ,MessageBoxIcon.Warning);
					   Application.ExitThread();
                    }
                }
				return itog;
                    
             }
            else MessageBox.Show(string.Format("Файл {0} не обнаружен.", file), "Неудачная Операция",MessageBoxButtons.OK ,MessageBoxIcon.Warning );
            return null;
		}

        /// <summary>
        /// Метод для записи истории посещений веб-страниц.
        /// </summary>
        
        private void addHistory(Uri url,string data)
        {
            XmlDocument myXml = new XmlDocument();
            int i=1;
            XmlElement el = myXml.CreateElement("item");
            el.SetAttribute("url", url.ToString());
            el.SetAttribute("lastVisited", data);

            if((url.ToString()).StartsWith("http://localhost:81/"))
            {
                return;
            }

            if (!File.Exists(historyXml))
            {
                XmlElement root = myXml.CreateElement("history");
                myXml.AppendChild(root);
                el.SetAttribute("times", "1");
                root.AppendChild(el);
            }
            else
            {
                myXml.Load(historyXml);

                foreach (XmlElement x in myXml.DocumentElement.ChildNodes)
                {
                    if (x.GetAttribute("url").Equals(url.ToString()))
                    {
                        i = int.Parse(x.GetAttribute("times")) + 1;
                        myXml.DocumentElement.RemoveChild(x);
                        break;
                    }
                }

                el.SetAttribute("times", i.ToString());
                myXml.DocumentElement.InsertBefore(el, myXml.DocumentElement.FirstChild);

                if (favoritesPanel.Visible == true)
                {
                    /*ordered visited today*/
                    if (comboBox1.Text.Equals("Посещённые Сегодня Упорядоченно"))
                    {
                        if (!historyTreeView.Nodes.ContainsKey(url.ToString()))
                        {
                            TreeNode node =
                                 new TreeNode(url.ToString(), 3, 3);
                            node.ToolTipText = url.ToString() + "\nПоследний Визит: " + data + "\nВремя Посещения :" + i.ToString();
                            node.Name = url.ToString();
                            node.ContextMenuStrip = histContextMenu;
                            historyTreeView.Nodes.Insert(0, node);
                        }
                        else
                            historyTreeView.Nodes[url.ToString()].ToolTipText
                              = url.ToString() + "\nПоследний Визит: " + data + "\nВремя посещения: " + i.ToString();
                    }
                    /*view by site*/
                    if (comboBox1.Text.Equals("Обзор по Сайту"))
                    {
                        if (!historyTreeView.Nodes.ContainsKey(url.Host.ToString()))
                        {
                            historyTreeView.Nodes.Add(url.Host.ToString(), url.Host.ToString(), 0, 0);

                            TreeNode node =
                                   new TreeNode(url.ToString(), 3, 3);
                            node.ToolTipText = url.ToString() + "\nПоследний Визит: " + data + "\nВремя Посещения: " + i.ToString();
                            node.Name = url.ToString();
                            node.ContextMenuStrip = histContextMenu;
                            historyTreeView.Nodes[url.Host.ToString()].Nodes.Add(node);
                        }

                        else
                            if (!historyTreeView.Nodes[url.Host.ToString()].Nodes.ContainsKey(url.ToString()))
                            {
                                TreeNode node =
                                    new TreeNode(url.ToString(), 3, 3);
                                node.ToolTipText = url.ToString() + "\nПоследний Визит: " + data + "\nВремя Посещения: " + i.ToString();
                                node.Name = url.ToString();
                                node.ContextMenuStrip = histContextMenu;
                                historyTreeView.Nodes[url.Host.ToString()].Nodes.Add(node);
                            }
                            else
                                historyTreeView.Nodes[url.Host.ToString()].Nodes[url.ToString()].ToolTipText
                                        = url.ToString() + "\nПоследний Визит: " + data + "\nВремя Посещения" + i.ToString();

                    }
                    /* view by date*/
                    if (comboBox1.Text.Equals("Обзор по Дате"))
                    {
                        if (historyTreeView.Nodes[4].Nodes.ContainsKey(url.ToString()))
                            historyTreeView.Nodes[url.ToString()].ToolTipText
                                    = url.ToString() + "\nПоследний Визит: " + data + "\nВремя Посещения: " + i.ToString();
                        else
                        {
                            TreeNode node =
                                new TreeNode(url.ToString(), 3, 3);
                            node.ToolTipText = url.ToString() + "\nПоследний Визит: " + data + "\nВремя Посещения :" + i.ToString();
                            node.Name = url.ToString();
                            node.ContextMenuStrip = histContextMenu;
                            historyTreeView.Nodes[4].Nodes.Add(node);
                        }
                    }
                }

            } 
            myXml.Save(historyXml);
        }

//удалить историю
        private void deleteHistory()
        {
            XmlDocument myXml = new XmlDocument();
            myXml.Load(historyXml);
            XmlElement root = myXml.DocumentElement;
            foreach (XmlElement x in root.ChildNodes)
            {
                if (x.GetAttribute("url").Equals(Address))
                {
                    root.RemoveChild(x);
                    break;
                }
            }
            historyTreeView.SelectedNode.Remove();
            myXml.Save(historyXml);
        }

        #endregion

        #region TABURI
        /*TAB-uri*/


//дополнительно обрабатываемый переход браузера
		private void Navigate(String address)
		{
			if (!address.StartsWith("file:///") &&!address.StartsWith("http://") &&
				!address.StartsWith("https://"))
			{
				address = "http://" + address;
			}
            
            try
			{
				getCurrentBrowser().Navigate(new Uri(address), false);

            }
			catch (System.UriFormatException)
			{
				throw ;
			}

                if (getCurrentBrowser().DocumentTitle != "")
                {
                    pageDocName.Visible = true;
                    pageDocName.Text = "Веб-страница: " + getCurrentBrowser().DocumentTitle;
					this.toolStripStatusLabel1.Text = pageDocName.Text;
                }
				else pageDocName.Visible = false;	
			while (getCurrentBrowser().ReadyState != WebBrowserReadyState.Complete)
			{
				Application.DoEvents();
			}				
				
		}

        //добавление новой вкладки
        public void addNewTab(string addr = "http://localhost:81")
        {
			
			if(addr == "http://localhost:81"||addr ==  "about:blank")
            {
            this.Address = "http://localhost:81";
			}
			
            TabPage tpage = new TabPage();
            tpage.BorderStyle = BorderStyle.Fixed3D;
            browserTabControl.TabPages.Insert(browserTabControl.TabCount - 1, tpage);
            WebBrowser browser =  new WebBrowser();
            browser.IsWebBrowserContextMenuEnabled = true;
            browser.ScriptErrorsSuppressed = true;
           // browser.Document.ContextMenuShowing += new HtmlElementEventHandler(wb_showContextMenu);
            tpage.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
            browserTabControl.SelectTab(tpage);
			browser.ScriptErrorsSuppressed = true;
            browser.ProgressChanged += new WebBrowserProgressChangedEventHandler(Form1_ProgressChanged);
            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Form1_DocumentCompleted);
            browser.Navigating += new WebBrowserNavigatingEventHandler(Form1_Navigating);
            browser.CanGoBackChanged += new EventHandler(browser_CanGoBackChanged);
            browser.CanGoForwardChanged += new EventHandler(browser_CanGoForwardChanged);		

			
			try
			{
				browser.Navigate(new Uri(this.Address));

            }
			catch (System.UriFormatException ex)
			{
				//throw ex;
			}
			    if (browser.DocumentTitle != "")
                {
					pageDocName.Visible = true;
					pageDocName.Text = "Сайт: " + browser.DocumentTitle;			

                }
				else pageDocName.Visible = false;
           
        }
/*
        private void wb_showContextMenu(object sender, HtmlElementEventArgs e)
        {
            wbTabContextMenu.Show(Cursor.Position);
            e.ReturnValue = false;
        }
*/
        //DocumentCompleted
        private void Form1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser currentBrowser = (WebBrowser)sender;//getCurrentBrowser();
			currentBrowser.AllowNavigation = true;
            this.pageIsLoaded = true;
            String text = "Пустая Страница";

            if (!currentBrowser.Url.ToString().Equals("about:blank"))
            {
                text = currentBrowser.Url.Host.ToString();
				if (currentBrowser.DocumentTitle != "")
                {
                    pageDocName.Visible = true;
                    pageDocName.Text = "Сайт: " + currentBrowser.DocumentTitle;
                }
				else pageDocName.Visible = false;
            }

            this.adrBarTextBox.Text = currentBrowser.Url.ToString();
            browserTabControl.SelectedTab.Text = text;

            img.Image = favicon(currentBrowser.Url.ToString(), "net.png");

            if (!urls.Contains(currentBrowser.Url.Host.ToString()))
                urls.Add(currentBrowser.Url.Host.ToString());

            if (!currentBrowser.Url.ToString().Equals("about:blank") && this.pageIsLoaded == true)
                addHistory(currentBrowser.Url,DateTime.Now.ToString(currentCulture));
            
              if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath) 
    return;  

        }
        //ProgressChanged    
        private void Form1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress < e.MaximumProgress&& e.CurrentProgress != (-1))
			{				
                toolStripProgressBar1.Value=(int)e.CurrentProgress;
				toolStripProgressBar1.Visible = true;
			}
            else 
			{
				toolStripProgressBar1.Value = toolStripProgressBar1.Minimum;
				toolStripProgressBar1.Visible = false;
			}
			
		var webBrowser = (WebBrowser)sender;
		if (webBrowser.Document != null)
		{
			foreach (HtmlElement tag in webBrowser.Document.All)
			{
				if (tag.Id == null)
				{
					tag.Id = String.Empty;
					switch (tag.TagName.ToUpper())
					{
						case "A":
						{
							tag.MouseUp += new HtmlElementEventHandler(link_MouseUp);
							break;
						}
					}
				}
			}
		}

	 }
 
 private void link_MouseUp(object sender, HtmlElementEventArgs e)
{
    var link = (HtmlElement)sender;
	bool isOK = false;
	
    switch (e.MouseButtonsPressed)
    {
        case MouseButtons.Left:
        {

                        try
                        {
                            if (isOK != true)
                            {
                                if ((link.GetAttribute("target") != null) && (link.GetAttribute("target").ToLower() == "_blank") && !this.Address.StartsWith("http://localhost/") || e.ShiftKeyPressed || (e.MouseButtonsPressed == MouseButtons.Middle))
                                {
                                    isOK = true;
                                    link.SetAttribute("target", "_self");
                                    addNewTab(link.GetAttribute("href"));

                                    break;
                                }

                                else
                                {
                                    isOK = true;
                                    Navigate(link.GetAttribute("href"));
                                    break;
                                }
                            }
                        }
                        catch(Exception ex) { }
		break;
           
        }
        case MouseButtons.Right:
        {
                        //В этом месте происходит накладка двух контекстных меню.
                        //Нужно лишнее из них сделать невидимым.

          //linkContextMenu.Show(MousePosition);
            break;
        }
    }
}
        //Navigating
        private void Form1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
			this.pageIsLoaded = false;
            this.toolStripStatusLabel1.Text = getCurrentBrowser().StatusText;		

        }
        //closeTab method
        private void closeTab()
        {
            if (browserTabControl.TabCount != 2)
            {
                browserTabControl.TabPages.RemoveAt(browserTabControl.SelectedIndex);
            }

        }
        //selected index changed
        private void browserTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (browserTabControl.SelectedIndex == browserTabControl.TabPages.Count - 1)
			{
				addNewTab();
				//MessageBox.Show("browserTabControl_SelectedIndexChanged");
			}
            else
            {
                if (getCurrentBrowser().Url != null)
                    adrBarTextBox.Text = getCurrentBrowser().Url.ToString();
                else adrBarTextBox.Text = "about:blank";

                if (getCurrentBrowser().CanGoBack) toolStripButton1.Enabled = true;
                else toolStripButton1.Enabled = false;

                if (getCurrentBrowser().CanGoForward) toolStripButton2.Enabled = true;
                else toolStripButton2.Enabled = false;
            }
        }

        /* tab context menu */

        private void closeTabToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            closeTab();
        }
        private void duplicateTabToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {              
                addNewTab(getCurrentBrowser().Url.ToString());   
			//MessageBox.Show("closeTabToolStripMenuItem1_Click1");				
            }
            else addNewTab(Address);
			//MessageBox.Show("closeTabToolStripMenuItem1_Click2");	
        }
        #endregion

        #region FAVICON
		
		private static bool netConnected()
		{	
		
			if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
			{
			MessageBox.Show("Отсутствует или ограниченно физическое подключение к сети\nПроверьте настройки вашего сетевого подключения","Неудачная Операция",MessageBoxButtons.OK ,MessageBoxIcon.Warning );
			return false;
			}			
            try
            {
                HttpWebRequest reqFP = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com");
                HttpWebResponse rspFP = (HttpWebResponse)reqFP.GetResponse();
                if (HttpStatusCode.OK == rspFP.StatusCode)
                {
                    // HTTP = 200 - Интернет безусловно есть!
                    rspFP.Close();
                    return true;
                }
                else
                {
                    // сервер вернул отрицательный ответ, возможно что инета нет

                    rspFP.Close();
				MessageBox.Show("Нет подключения к интернету\nПроверьте ваш фаервол или настройки сетевого подключения","Неудачная Операция",MessageBoxButtons.OK ,MessageBoxIcon.Warning);
                    return false;

                }

            }

            catch (WebException)

            {

                // Ошибка, значит интернета у нас нет. Плачем :'(

                return false;

            }
			
	}
       
        // favicon
        public static Image favicon(String u, string file)
        {
	/*Здесь мы осуществим проверку подключения, чтобы
	предотвратить попытку загрузки фавиконов из сети.
	Код не совсем правильный (унаследован от Клавдии Гога)
	По настоящему фавикон нужно искать в этой строке
	<head>
	<link rel="shortcut icon" href="./favicon.ico"/>
	</head>
	
	*/
	/* Здесь необходимо реализовать сохранение фавиконов в xml-файл,
	чтобы устранить задержку на запуске...
	
		if(netConnected())
		{			
            Uri url = new Uri(u);
            //string iconurl = "http://" + url.Host + "/favicon.ico";     
            WebRequest request = WebRequest.Create(String.Format("http://" + url.Host + "/favicon.ico"));
            WebResponse response = request.GetResponse();

            if (response != null)
            {

                try
                {                   

                    Stream s = response.GetResponseStream();
					 //  Bitmap bitmap = new Bitmap(Image.FromStream(response.GetResponseStream()));
					 //  this.Icon = Icon.FromHandle(bitmap.GetHicon());
                    return Image.FromStream(s);
                }
                catch (Exception ex)
                {
                  if(ex.InnerException  != null)
                    return Image.FromFile(file);
                }

           }
		}
		*/
		//поскольку пока нет кода для локального сохранения фавиконов,
		//в случае отсутствия подключения будем отображать это:
	     return Properties.Res.ToolbarWebLink;

       }
        //favicon index
        private int faviconIndex(string url)
        {
            Uri key = new Uri(url);
            if (!imgList.Images.ContainsKey(key.Host.ToString()))
                imgList.Images.Add(key.Host.ToString(), favicon(url, imgList.Images.Keys[3].ToString()));
            return imgList.Images.IndexOfKey(key.Host.ToString());
        }
        //getFavicon from key
        private Image getFavicon(string key)
        {
            Uri url = new Uri(key);
            if (!imgList.Images.ContainsKey(url.Host.ToString()))
                imgList.Images.Add(url.Host.ToString(), favicon(key
                    , "link.png"));
            return imgList.Images[url.Host.ToString()];
        }
        #endregion

        #region     TOOL CONTEXT MENU
        /* TOOL CONTEXT MENU*/

        //link bar
        private void linksBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            linkBar.Visible = !linkBar.Visible;
            this.linksBarToolStripMenuItem.Checked = linkBar.Visible;
            settings.DocumentElement.ChildNodes[2].Attributes[0].Value = linkBar.Visible.ToString();
        }
        //menu bar
        private void menuBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            menuBar.Visible = !menuBar.Visible;
            this.menuBarToolStripMenuItem.Checked = menuBar.Visible;
            settings.DocumentElement.ChildNodes[0].Attributes[0].Value = menuBar.Visible.ToString();
        }
        //address bar
        private void commandBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adrBar.Visible = !adrBar.Visible;
            this.commandBarToolStripMenuItem.Checked = adrBar.Visible;
            settings.DocumentElement.ChildNodes[1].Attributes[0].Value = adrBar.Visible.ToString();
        }
        #endregion

        #region ADDRESS BAR
        /*ADDRESS BAR*/

        private WebBrowser getCurrentBrowser()
        {
            return (WebBrowser)browserTabControl.SelectedTab.Controls[0];
        }
        //ENTER
        private void adrBarTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(adrBarTextBox.Text);

            }
        }
        //select all from adr bar
        private void adrBarTextBox_Click(object sender, EventArgs e)
        {
          //  adrBarTextBox.SelectAll();
        }
        //show urls

        private void showUrl()
        {
            if (File.Exists(historyXml))
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(historyXml);
                int i = 0;
                int num=int.Parse(settings.DocumentElement.ChildNodes[5].InnerText.ToString());
                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    if (num <= i++ ) break;
                    else  adrBarTextBox.Items.Add(el.GetAttribute("url").ToString());
                           
                }
            }
        }

        private void adrBarTextBox_DropDown(object sender, EventArgs e)
        {
            adrBarTextBox.Items.Clear();
            showUrl();
        }
        //navigate on selected url 
        private void adrBarTextBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Navigate(adrBarTextBox.SelectedItem.ToString());
        }
     //canGoForwardChanged
        void browser_CanGoForwardChanged(object sender, EventArgs e)
        {
            toolStripButton2.Enabled = !toolStripButton2.Enabled;
        }
        //canGoBackChanged
        void browser_CanGoBackChanged(object sender, EventArgs e)
        {
            toolStripButton1.Enabled = !toolStripButton1.Enabled;
        }
        //back  
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoBack();
        }
        //forward
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoForward();
        }
        //go
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Navigate(adrBarTextBox.Text);

        }
        //refresh
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Refresh();
        }
        //stop
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Stop();
        }
        //favorits
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            favoritesPanel.Visible = !favoritesPanel.Visible;
            settings.DocumentElement.ChildNodes[3].Attributes[0].Value = favoritesPanel.Visible.ToString();
        }
        //add to favorits
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {
                AddFavorites dlg = new AddFavorites(getCurrentBrowser().Url.ToString());
                DialogResult res = dlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    if (dlg.favFile == "Favorites")
                        addFavorit(getCurrentBrowser().Url.ToString(), dlg.favName);
                    else addLink(getCurrentBrowser().Url.ToString(), dlg.favName);
                }
                dlg.Close();
            }

        }
        //search
        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                if (googleSearch.Checked == true)
                    Navigate("http://google.com/search?q=" + searchTextBox.Text);
                else
                    Navigate("http://search.live.com/results.aspx?q="+searchTextBox.Text);                       
        }

        private void googleSearch_Click(object sender, EventArgs e)
        {
            liveSearch.Checked =!googleSearch.Checked;
        }

        private void liveSearch_Click(object sender, EventArgs e)
        {
            googleSearch.Checked = !liveSearch.Checked;
        }

        #endregion

        #region LINKS BAR

        /*LINKS BAR*/

        

        //favorits button
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            favoritesPanel.Visible = !favoritesPanel.Visible;
            settings.DocumentElement.ChildNodes[3].Attributes[0].Value = favoritesPanel.Visible.ToString();
        }
        //add to favorits bar button
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
                addLink(getCurrentBrowser().Url.ToString(), getCurrentBrowser().Url.ToString());
        }

        //showLinks on link bar
        private void showLinks()
        {
            if (File.Exists(linksXml))
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(linksXml);
                XmlElement root = myXml.DocumentElement;
                foreach (XmlElement el in root.ChildNodes)
                {
                    ToolStripButton b =
                        new ToolStripButton(el.InnerText, getFavicon(el.GetAttribute("url")), items_Click, el.GetAttribute("url"));

                    b.ToolTipText = el.GetAttribute("url");
                    b.MouseUp += new MouseEventHandler(b_MouseUp);
                    linkBar.Items.Add(b);
                }
            }
        }
        //click link button
        private void items_Click(object sender, EventArgs e)
        {
            ToolStripButton b = (ToolStripButton)sender;
            Navigate(b.ToolTipText);
        }
        //show context menu on button
        private void b_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton b = (ToolStripButton)sender;
            Address = b.ToolTipText;
            name = b.Text;

            if (e.Button == MouseButtons.Right)
                linkContextMenu.Show(MousePosition);
        }
//visible change
        private void linkBar_VisibleChanged(object sender, EventArgs e)
        {
            if (linkBar.Visible == true) showLinks();
            else while (linkBar.Items.Count > 3) linkBar.Items[linkBar.Items.Count - 1].Dispose();
        }

        #endregion

        #region LINK, FAVORITES, HISTORY CONTEXT MENU
        /*GENERAL*/

        //открыть
        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Navigate(Address);
        }
        //открыть в новой вкладке
        private void openInNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
		   addNewTab(Address);
           //Navigate(address);

        }
        //открыть в новом окне
        private void openInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WBrowser new_form = new WBrowser();
            new_form.Show();
            new_form.Navigate(Address);
        }
                     /*LINK CONTEXT MENU*/
        //удалить ссылку
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteLink();
        }
        //переименовать ссылку
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            renameLink();
        }
                          /*FAVORITES CONTEXT MENU*/
        //удалить фаворит
        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            deleteFavorit();
        }
        //переименовать фаворит
        private void renameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            renameFavorit();
        }
           
              /*HISTORY CONTEXT MENU */

        private void openToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Navigate(historyTreeView.SelectedNode.Text);
        }

//удалить историю
        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            deleteHistory();
        }
 //добавить в избранное
        private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {   
            AddFavorites dlg = new AddFavorites(historyTreeView.SelectedNode.Text);
             DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (dlg.favFile == "Избранное")
                        addFavorit(getCurrentBrowser().Url.ToString(), dlg.favName);
                    else addLink(getCurrentBrowser().Url.ToString(), dlg.favName);
                   
                    deleteHistory();
                }
                dlg.Close();

                
        }

        #endregion

        #region FAVORITES WINDOW

        private void showFavorites()
        {
            XmlDocument myXml = new XmlDocument();
            TreeNode link = new TreeNode("Ссылки",0,0);
            link.NodeFont =new  Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            favTreeView.Nodes.Add(link);

            if (File.Exists(favXml))
            {
                myXml.Load(favXml);

                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    TreeNode node = 
                        new TreeNode(el.InnerText,faviconIndex(el.GetAttribute("url")), faviconIndex(el.GetAttribute("url")));
                    node.ToolTipText = el.GetAttribute("url");
                    node.Name = el.GetAttribute("url");
                    node.ContextMenuStrip = favContextMenu;
                    favTreeView.Nodes.Add(node);
                }

            }

            if (File.Exists(linksXml))
            {
                myXml.Load(linksXml);

                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    TreeNode node = 
                        new TreeNode(el.InnerText, faviconIndex(el.GetAttribute("url")), faviconIndex(el.GetAttribute("url")));
                    node.ToolTipText = el.GetAttribute("url");
                    node.Name = el.GetAttribute("url");
                    node.ContextMenuStrip = linkContextMenu;
                    favTreeView.Nodes[0].Nodes.Add(node);
                }

            }

        }
//node click
        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                favTreeView.SelectedNode = e.Node;
                Address = e.Node.ToolTipText;
                name = e.Node.Text;
            }
            else
                if (e.Node != favTreeView.Nodes[0])
                    Navigate(e.Node.ToolTipText);

        }
//show history in tree wiew
        private void showHistory()
        {
            historyTreeView.Nodes.Clear();
            XmlDocument myXml = new XmlDocument();

            if (File.Exists(historyXml))
            {
                myXml.Load(historyXml);
                DateTime now=DateTime.Now;
                if (comboBox1.Text.Equals("Посещённые Сегодня Упорядоченно"))
                {
                    historyTreeView.ShowRootLines = false;
                    foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                    {
                         DateTime d=DateTime.Parse(el.GetAttribute("lastVisited"),currentCulture);
                        
                        if (!(d.Date==now.Date)) return;

                        TreeNode node =
                            new TreeNode(el.GetAttribute("url"), 3, 3);
                        node.ToolTipText = el.GetAttribute("url") + "\nПоследнее Посещение: " + el.GetAttribute("lastVisited") + "\nВремя Посещения: " + el.GetAttribute("times");
                        node.Name = el.GetAttribute("url");
                        node.ContextMenuStrip = histContextMenu;
                        historyTreeView.Nodes.Add(node);
                    }

                }
               
           if (comboBox1.Text.Equals("Обзор по Сайту"))
           {
              historyTreeView.ShowRootLines = true;
              foreach(XmlElement el in myXml.DocumentElement.ChildNodes)
              { Uri site=new Uri(el.GetAttribute("url"));
                  
                  if(!historyTreeView.Nodes.ContainsKey(site.Host.ToString()))
                      historyTreeView.Nodes.Add(site.Host.ToString(),site.Host.ToString(),0,0);
                   TreeNode node =new TreeNode(el.GetAttribute("url"), 3, 3);
                    node.ToolTipText = el.GetAttribute("url") + "\nПоследнее Посещение: " + el.GetAttribute("lastVisited") + "\nВремя Посещения: " + el.GetAttribute("times");
                    node.Name = el.GetAttribute("url");
                    node.ContextMenuStrip = histContextMenu;
                    historyTreeView.Nodes[site.Host.ToString()].Nodes.Add(node);
               }

           }

           if (comboBox1.Text.Equals("Обзор по Дате"))
           {  
               historyTreeView.ShowRootLines = true;
               historyTreeView.Nodes.Add("2 Недели Назад","2 Недели Назад",2,2);
               historyTreeView.Nodes.Add("Прошлая Неделя","Прошлая Неделя",2,2);
               historyTreeView.Nodes.Add("Эта Неделя","Эта Неделя",2,2);
               historyTreeView.Nodes.Add("Вчера","Вчера",2,2);
               historyTreeView.Nodes.Add("Сегодня","Сегодня",2,2);
               foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
               {
                  DateTime d=DateTime.Parse(el.GetAttribute("lastVisited"),currentCulture);
                  
                   TreeNode node = new TreeNode(el.GetAttribute("url"), 3, 3);
                   node.ToolTipText = el.GetAttribute("url") + "\nПоследнее Посещение: " + el.GetAttribute("lastVisited") + "\nВремя Посещения: " + el.GetAttribute("times");
                   node.Name = el.GetAttribute("url");
                   node.ContextMenuStrip = histContextMenu;

                   if (d.Date==now.Date)
                       historyTreeView.Nodes[4].Nodes.Add(node);
                   else
                       if (d.AddDays(1).ToShortDateString().Equals(now.ToShortDateString()))
                           historyTreeView.Nodes[3].Nodes.Add(node);
                       else
                           if (d.AddDays(7) > now)
                               historyTreeView.Nodes[2].Nodes.Add(node);
                           else
                               if (d.AddDays(14) > now)
                                   historyTreeView.Nodes[1].Nodes.Add(node);
                               else
                                   if (d.AddDays(21) > now)
                                       historyTreeView.Nodes[0].Nodes.Add(node);
                                      else
                                       if (d.AddDays(22) > now)
                                           myXml.DocumentElement.RemoveChild(el);
               }
           }
         }
            
            
        }
//history nodes click
        private void historyTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                historyTreeView.SelectedNode = e.Node;
                Address = e.Node.Text;
            }
            else
                if (!comboBox1.Text.Equals("Посещённые Сегодня Упорядоченно"))
                {
                    if (!historyTreeView.Nodes.Contains(e.Node))
                        Navigate(e.Node.Text);
                }
                else 
                    Navigate(e.Node.Text);
        }

//fav panel visible change
        private void favoritesPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (favoritesPanel.Visible == true)
            {
                showFavorites();
                showHistory();
            }
            else
            {
                favTreeView.Nodes.Clear();
                historyTreeView.Nodes.Clear();
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            showHistory();
        }

        #endregion

        #region FAVORITS
        /*FAVORITES*/

        //add to favorits
        private void addToFavoritsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {
                AddFavorites dlg = new AddFavorites(getCurrentBrowser().Url.ToString());
                DialogResult res = dlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    if (dlg.favFile == "Избранное")
                        addFavorit(getCurrentBrowser().Url.ToString(), dlg.favName);
                    else addLink(getCurrentBrowser().Url.ToString(), dlg.favName);
                }
                dlg.Close();
            }
        }
        //add to favorits bar
        private void addToFavoritsBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addLink(getCurrentBrowser().Url.ToString(), getCurrentBrowser().Url.ToString());
        }
        //organize favorites
        private void organizeFavoritsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new OrganizeFavorites(favTreeView, linkBar,linkContextMenu,favContextMenu)).ShowDialog();
        }

        //show favorites in menu
        private void favoritesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            XmlDocument myXml = new XmlDocument();
            if (File.Exists(favXml))
            {
                myXml.Load(favXml);

                for (int i = favoritesToolStripMenuItem.DropDownItems.Count - 1; i > 5; i--)
                {
                    favoritesToolStripMenuItem.DropDownItems.RemoveAt(i);
                }
                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(el.InnerText, getFavicon(el.GetAttribute("url")), fav_Click);
                    item.ToolTipText = el.GetAttribute("url");
                    favoritesToolStripMenuItem.DropDownItems.Add(item);
                }
            }
        }
        //show links in menu
        private void linksMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            XmlDocument myXml = new XmlDocument();
            if (File.Exists(linksXml))
            {
                myXml.Load(linksXml);
                linksMenuItem.DropDownItems.Clear();
                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(el.InnerText, getFavicon(el.GetAttribute("url")), fav_Click);
                    item.ToolTipText = el.GetAttribute("url");
                    linksMenuItem.DropDownItems.Add(item);
                }
            }
        }
        private void fav_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem m = (ToolStripMenuItem)sender;
            Navigate(m.ToolTipText);
        }
		

        #endregion

        #region FILE
        /*FILE*/

        //new tab
        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addNewTab();
			//MessageBox.Show("newTabToolStripMenuItem_Click");
        }
        //duplicate tab
        private void duplicateTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {              
                addNewTab(getCurrentBrowser().Url.ToString());
				//MessageBox.Show("duplicateTabToolStripMenuItem_Click");
            }
        }
        //new window
        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new WBrowser()).Show();

        }
        //close tab
        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeTab();
        }
        //open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new Open(getCurrentBrowser())).Show();
        }
        //page setup
        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPageSetupDialog();
        }
        //save as
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowSaveAsDialog();
        }
        //print
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPrintDialog();

        }
        //print preview
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPrintPreviewDialog();
        }
        //свойства
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPropertiesDialog();
        }
        //отправить страницу по почте
        private void pageByEmailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Navigate("https://login.yahoo.com/config/login_verify2?&.src=ym");
            Process.Start("msimn.exe");
        }
        //отправить ссылку по почте
        private void linkByEmailToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // Navigate("https://login.yahoo.com/config/login_verify2?&.src=ym");
            Process.Start("msimn.exe");
        }


        #endregion

        #region EDIT
        /*EDIT*/
        //вырезать
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("Cut", false, null);

        }
        //копировать
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("Copy", false, null);

        }
        //вставить
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("Paste", false, null);
        }
        //выбрать все
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("SelectAll", true, null);
        }
        #endregion

        #region VIEW
       
        /* VIEW */

//explorer bars
        private void favoritsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            favoritesPanel.Visible = true;
            favoritesTabControl.SelectedTab = favTabPage;

        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            favoritesPanel.Visible = true;
            favoritesTabControl.SelectedTab = historyTabPage;
        }
//favorites,history checked
        private void explorerBarsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            favoritesViewMenuItem.Checked =
                (favoritesPanel.Visible == true && favoritesTabControl.SelectedTab == favTabPage);

            historyViewMenuItem.Checked =
                (favoritesPanel.Visible == true && favoritesTabControl.SelectedTab == historyTabPage);
        }

        /*Go to*/
//drop down opening
        private void goToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            backToolStripMenuItem.Enabled = getCurrentBrowser().CanGoBack;
            forwardToolStripMenuItem.Enabled = getCurrentBrowser().CanGoForward;

            while (goToMenuItem.DropDownItems.Count > 5)
                goToMenuItem.DropDownItems.RemoveAt(goToMenuItem.DropDownItems.Count-1);
            
            foreach (string a in urls)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(a, null, goto_click);

                item.Checked = (getCurrentBrowser().Url.Host.ToString().Equals(a));

                goToMenuItem.DropDownItems.Add(item);
            }
        }
        private void goto_click(object sender, EventArgs e)
        {
            Navigate(sender.ToString());
        }
        //назад
        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoBack();
        }
        //вперёд
        private void forwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoForward();
        }
        //домой
        private void homePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Navigate(homePage);
        }
           //остановить
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Stop();
        }
       //обновить
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Refresh();
        }
        //просмотр исходного кода
        private void sourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String source=("source.html");
            StreamWriter writer =File.CreateText(source);
            writer.Write(getCurrentBrowser().DocumentText);
            writer.Close();
           hedit = new HtmlEditorForm(source);
                  hedit.Show();
            File.Delete(source);
            
           // Process.Start("c:\\Program Files\\Notepad++\\notepad++.exe", source);            
        }
        //text size 
        private void textSizeToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string level = e.ClickedItem.ToString();
            smallerToolStripMenuItem.Checked = false;
            smallestToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = false;
            largerToolStripMenuItem.Checked = false;
            largestToolStripMenuItem.Checked = false;
            switch (level)
            {
                case "Smallest": getCurrentBrowser().Document.ExecCommand("FontSize", true, "0");
                                 smallestToolStripMenuItem.Checked = true;
                                 break;
                case "Smaller": getCurrentBrowser().Document.ExecCommand("FontSize", true, "1");
                                 smallerToolStripMenuItem.Checked = true;
                                 break;
                case "Medium": getCurrentBrowser().Document.ExecCommand("FontSize",true,"2");
                                 mediumToolStripMenuItem.Checked = true; 
                                break;
                case "Larger": getCurrentBrowser().Document.ExecCommand("FontSize",true,"3");
                                largerToolStripMenuItem.Checked = true; 
                                break;
                case "Largest": getCurrentBrowser().Document.ExecCommand("FontSize",true,"4");
                                largestToolStripMenuItem.Checked = true;
                                 break;
            }
        }
        //full screen
        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(this.FormBorderStyle == FormBorderStyle.None && this.WindowState == FormWindowState.Maximized))
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;
                menuBar.Visible = false;
                linkBar.Visible = false;
                adrBar.Visible = false;
                favoritesPanel.Visible = false;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.TopMost = false;
                menuBar.Visible = (settings.DocumentElement.ChildNodes[0].Attributes[0].Value.Equals("True"));
                adrBar.Visible = (settings.DocumentElement.ChildNodes[1].Attributes[0].Value.Equals("True"));
                linkBar.Visible = (settings.DocumentElement.ChildNodes[2].Attributes[0].Value.Equals("True"));
                favoritesPanel.Visible = (settings.DocumentElement.ChildNodes[3].Attributes[0].Value.Equals("True"));
            }
        }

        //exit full screen
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
				this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = false;
                menuBar.Visible = true;
                linkBar.Visible = true;
                adrBar.Visible = true;
                favoritesPanel.Visible = true;
            }
        }
/*

        //splash screen
        private void splashScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings.DocumentElement.ChildNodes[4].Attributes[0].Value
                = splashScreenToolStripMenuItem.Checked.ToString();
        }
*/
        #endregion

        #region TOOLS

//delete browsing history
        private void deleteBrowserHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteBrowsingHistory b = new DeleteBrowsingHistory();
            if (b.ShowDialog() == DialogResult.OK)
            {
                if (b.History.Checked == true)
                {
                    File.Delete(historyXml);
                    historyTreeView.Nodes.Clear();
                }
                if (b.TempFiles.Checked == true)
                {
                    urls.Clear();
                    while (imgList.Images.Count > 4)
                        imgList.Images.RemoveAt(imgList.Images.Count-1);
                    File.Delete("source.txt");

                }
            }
        }
//internet options
        private void internetOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InternetOption intOp = new InternetOption(getCurrentBrowser().Url.ToString());
            if (intOp.ShowDialog() == DialogResult.OK)
            {
                if (!intOp.homepage.Text.Equals(""))
                {
                    homePage = intOp.homepage.Text;
                    settings.DocumentElement.ChildNodes[4].InnerText = intOp.homepage.Text;
                }
                    if (intOp.deleteHistory.Checked == true)
                {
                    File.Delete(historyXml);
                    historyTreeView.Nodes.Clear();
                }
                settings.DocumentElement.ChildNodes[5].InnerText = intOp.num.Value.ToString();
                ActiveForm.ForeColor = intOp.forecolor;
                ActiveForm.BackColor = intOp.backcolor;
                linkBar.BackColor = intOp.backcolor;
                adrBar.BackColor = intOp.backcolor;
                ActiveForm.Font = intOp.font;
                linkBar.Font = intOp.font;
                menuBar.Font = intOp.font;
            }


        }

//calculator
        private void calcTSMItem_Click(object sender, EventArgs e)
        {
            Process.Start("calc.exe");
        }

        //calendar
        private void calendarTSMItem_Click(object sender, EventArgs e)
        {
            (new Calendar()).Show();
        }

        private void ieSettingsTSMI_Click(object sender, EventArgs e)
        {
            Process.Start("inetcpl.cpl");
        }

        private void powerShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("powershell_ise.exe");
        }

        private void редакторHtmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hedit = new HtmlEditorForm();
            hedit.Show();
        }

        #endregion

        #region HELP
        //about
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new About(false)).Show();
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            if (googleSearch.Checked == true)
                Navigate("http://google.com/search?q=" + searchTextBox.Text);
            else
                Navigate("http://search.live.com/results.aspx?q=" + searchTextBox.Text);
        }

        private void img_Click(object sender, EventArgs e)
        {
            Navigate("http://localhost:81");
        }

        private void докГенераторToolStripMenuItem_Click(object sender, EventArgs e)
        {
           var dg = new DocGenerator();
            dg.Show();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("mailto:dinruspro@mail.ru");
        }
		void AdrBarTextBoxClick(object sender, EventArgs e)
		{

		}
 #endregion   
 
 #region NEWTABOPENING
/*
private void Browser_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
{
    var webBrowser = (WebBrowser)sender;
    if (webBrowser.Document != null)
    {
        foreach (HtmlElement tag in webBrowser.Document.All)
        {
            if (tag.Id == null)
            {
                tag.Id = String.Empty;
                switch (tag.TagName.ToUpper())
                {
                    case "A":
                    {
                        tag.MouseUp += new HtmlElementEventHandler(link_MouseUp);
                        break;
                    }
                }
            }
        }
    }
}


private void link_MouseUp(object sender, HtmlElementEventArgs e)
{
    var link = (HtmlElement)sender;
    mshtml.HTMLAnchorElementClass a = (mshtml.HTMLAnchorElementClass)link.DomElement;
    switch (e.MouseButtonsPressed)
    {
        case MouseButtons.Left:
        {
            if ((a.target != null && a.target.ToLower() == "_blank") || e.ShiftKeyPressed || e.MouseButtonsPressed == MouseButtons.Middle)
            {
                AddTab(a.href);
            }
            else
            {
                CurrentBrowser.TryNavigate(a.href);
            }
            break;
        }
        case MouseButtons.Right:
        {
            CurrentBrowser.ContextMenuStrip = null;
            var contextTag = new ContextTag();
            contextTag.Element = a;
            contextHtmlLink.Tag = contextTag;
            contextHtmlLink.Show(Cursor.Position);
            break;
        }
    }
}
//See more at dotBro



void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
{
    getCurrentBrowser().Document.Click += new HtmlElementEventHandler(Document_Click);
}

void Document_Click(object sender, HtmlElementEventArgs e)
{
    HtmlElement ele = getCurrentBrowser().Document.GetElementFromPoint(e.MousePosition);
    while (ele != null)
    {
        if (ele.TagName.ToLower() == "a")
        {
            // METHOD-1
            // Use the url to open a new tab
            string url = ele.GetAttribute("href");
            // TODO: open the new tab
			addNewTab(url);
            e.ReturnValue = false;

            // METHOD-2
            // Use this to make it navigate to the new URL on the current browser/tab
           // ele.SetAttribute("target", "_self");
        }
        ele = ele.Parent;
    }
}
*/
 #endregion
       

    }
}

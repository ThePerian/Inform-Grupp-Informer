using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using IGCInformer.Workers;
using IGCInformer.Helpers;

namespace IGCInformer
{
    public partial class MainWindow : Window
	{
		WebBrowser browser;
		public Loader loader;
		Refresher refresher;
        public List<RssFeed> feedList { get; set; }
        public List<SyndicationFeed> channelList { get; set; }
        public RssChannelStackPanel rightClickedRssPanel { get; set; }
        //иконка в трее
        System.Windows.Forms.NotifyIcon notifyIcon;
        public DispatcherTimer updater;

        public MainWindow()
		{
			InitializeComponent();
			
			//запускать окно свернутым, доступ только через трей
			//this.WindowState = WindowState.Minimized;
            this.Closing += new CancelEventHandler(MainWindow_Closing);
            this.StateChanged += new EventHandler(MainWindow_StateChanged);
                        
            string iconPath = Environment.CurrentDirectory+"\\Icon.ico";
            if (File.Exists(iconPath))
            {
            	this.Icon =(System.Windows.Media.ImageSource) 
            		new System.Windows.Media.ImageSourceConverter().ConvertFromString(iconPath);
            }
            
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Visible = true;
            if (File.Exists(iconPath))
            {
            	notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            }
            notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(NotifyIcon_DoubleClick);
            notifyIcon.Text = "Информ-Групп Информер";
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIcon.ContextMenu.MenuItems.Add("Информ-Групп Информер", NotifyIcon_DoubleClick);
            notifyIcon.ContextMenu.MenuItems.Add("Закрыть", NotifyIconClose_Click);
            
			browser = new WebBrowser();
            pnl_feedList.Children.Add(browser);
            browser.NavigateToString(HtmlBuilder.ConstructEmptyHtml());
            
            btn_refresh.Click += new RoutedEventHandler(btn_refresh_Click);
            btn_addFeed.Click += new RoutedEventHandler(btn_addFeed_Click);
            btn_login.Click += new RoutedEventHandler(btn_login_Click);
            
            img_igc.MouseLeftButtonDown += new MouseButtonEventHandler(img_igc_MouseDown);
            img_igc.MouseEnter += new MouseEventHandler(img_igc_MouseEnter);
            img_igc.MouseLeave += new MouseEventHandler(img_igc_MouseLeave);
            
            refresher = new Refresher(this);
            loader = new Loader(this);
            
            updater = new DispatcherTimer();
            updater.Tick += new EventHandler(updater_Tick);
            updater.Interval = new TimeSpan(0,0,30);
		}
        
        private void img_igc_MouseDown(object sender, EventArgs e)
        {
        	Process.Start("http://www.igc.ru");
        }
        
        private void img_igc_MouseEnter(object sender, EventArgs e)
        {
        	this.Cursor = Cursors.Hand;
        	lbl_statusMessage.Content = "http://www.igc.ru";
        }
        
        private void img_igc_MouseLeave(object sender, EventArgs e)
        {
        	this.Cursor = Cursors.Arrow;
        	lbl_statusMessage.Content = "";
        }
        
        private void btn_login_Click(object sender, EventArgs e)
        {
        	Authorizer auth = new Authorizer(this);
        	auth.StartAuthorization();
        }

        private void btn_addFeed_Click(object sender, EventArgs e)
		{
        	if (this.feedList == null)
        	{
        		lbl_statusMessage.Content = "Не найден список RSS каналов";
        		return;
        	}
        	//вывести диалоговое окно для добавления канала в список, пользователь не сможет перенести фокус на основное окно
			var dlg = new NewFeed(this);
            dlg.ShowDialog();
		}
		
		private void NotifyIconClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}
		
		private void updater_Tick(object sender, EventArgs e)
		{
			//каждые %updater.Interval% секунд проверяет наличие новых сообщений
			//фактически нажимает кнопку "Обновить"
			//при наличии новых сообщений выводит всплывающее окно с ПОСЛЕДНИМ сообщением
			//возможно стоит добавить вывод нескольких окон если со времени последнего
			//обновления появилось несколько сообщений
			refresher.RefreshAllFeeds(true);
        	/*SyndicationFeed rssFeedChannel;
        	
        	if (this.feedList == null) return;
        	
			for (int i = 0; i < this.feedList.Count; i++)
            {
                Thread.Sleep(200);
                try
                {
	                using (var xmlReader = XmlReader.Create(this.feedList[i].Url))
	                {
	                    rssFeedChannel = SyndicationFeed.Load(xmlReader);
	                }
                }
                catch
                {
                	return;
                }
                this.channelList[i] = rssFeedChannel;
                this.feedList[i].Refresh(this.channelList[i], false);
                RssChannelStackPanel pan = (RssChannelStackPanel) this.pnl_channelList.Children[i + 1];
                TextBlock tb = (TextBlock) pan.Children[0];
                tb.Text = "(" + this.feedList[i].NewItemsCount + ") " + this.channelList[i].Title.Text;
            }
			for (int i = 0; i < this.feedList.Count; i++)
			{
				
                if ((this.WindowState == WindowState.Minimized)
                    && (this.feedList[i].hasNewItems))
                	showUpdate(this.channelList[i]);
			}*/
		}
		
		private void NotifyIcon_DoubleClick(Object sender, EventArgs e)
		{
			this.Show();
			this.Activate();
			if (this.WindowState == WindowState.Minimized)
				this.WindowState = WindowState.Normal;
		}
		
		private void MainWindow_StateChanged(Object sender, EventArgs e)
		{
			if (this.WindowState == WindowState.Minimized)
				this.Hide();//не отображать главное окно в панели задач
		}
		
        private void MainWindow_Closing(Object sender, CancelEventArgs e)
        {
        	if (this.feedList != null) FeedHelper.SaveFeeds(this.feedList);
            this.notifyIcon.Dispose();
            foreach (Window window in Application.Current.Windows)
            {
            	if (window.GetType() == typeof(Notification)) window.Close();//если не закрыть вручную, оповещения остаются висеть
            }
        }
		
		void btn_refresh_Click(object sender, RoutedEventArgs e)
		{
			refresher.RefreshAllFeeds(false);
		}
		
		/// <summary>
		/// Добавить RSS-канал в список в главном окне.
		/// </summary>
		/// <param name="channelTitle">Заголовок канала.</param>
		/// <param name="id">Порядковый номер канала.</param>
		public void AddChannelPanel(string channelTitle, int id)
        {
            RssChannelStackPanel rssStackPanel = new RssChannelStackPanel(id);
            rssStackPanel.Orientation = Orientation.Horizontal;
            rssStackPanel.Margin = new Thickness(10, 2, 0, 2);
            rssStackPanel.MouseEnter += new MouseEventHandler(RssChannelItem_MouseEnter);
            rssStackPanel.MouseLeave += new MouseEventHandler(RssChannelItem_MouseLeave);
            rssStackPanel.MouseLeftButtonDown += new MouseButtonEventHandler(RssChannelItem_MouseLeftButtonDown);
            rssStackPanel.MouseRightButtonDown += new MouseButtonEventHandler(RssChannelItem_MouseRightButtonDown);

            TextBlock textBlock = new TextBlock();
            textBlock.Name = "channelItem";
            textBlock.Foreground = Brushes.White;
            textBlock.FontSize = 14.0;
            textBlock.Text = channelTitle;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            rssStackPanel.Children.Add(textBlock);

            ContextMenu contextMenu = new ContextMenu();
            rssStackPanel.ContextMenu = contextMenu;

            MenuItem removeChannelMenuItem = new MenuItem();
            removeChannelMenuItem.Click += new RoutedEventHandler(removeChannelMenuItem_Click);
            removeChannelMenuItem.Header = "Удалить из списка";
            contextMenu.Items.Add(removeChannelMenuItem);

            pnl_channelList.Children.Add(rssStackPanel);
        }
		
		private void RssChannelItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.rightClickedRssPanel = (RssChannelStackPanel) sender;
        }

        private void RssChannelItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        	//обнулить счетчик новых сообщений и снять флаг новых сообщений
        	//если пользователь открыл данный RSS-канал в главном окне
            var rssChannelStackPanel = (RssChannelStackPanel) sender;
            this.feedList[rssChannelStackPanel.Id].hasNewItems = false;
            this.feedList[rssChannelStackPanel.Id].NewItemsCount = 0;
            browser.NavigateToString(HtmlBuilder.ConstructItemsHtml(this.channelList[rssChannelStackPanel.Id]));
            TextBlock tb = (TextBlock) rssChannelStackPanel.Children[0];
            tb.Text = "(" + this.feedList[rssChannelStackPanel.Id].NewItemsCount + ") "
            	+ this.channelList[rssChannelStackPanel.Id].Title.Text;
        }

        private void RssChannelItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var stackPanel = (RssChannelStackPanel) sender;
            stackPanel.Background = Brushes.Orange;//"#FFFFA500"
            stackPanel.Cursor = Cursors.Hand;
            try
            {
            	this.lbl_statusMessage.Content = feedList[stackPanel.Id].Url.ToString();
            }
            catch 
            {
            	//скорее всего сюда никогда не попадем, т.к. URL проверяется при добавлении канала
            	this.lbl_statusMessage.Content = "Не удалось получить URL";
            }
        }

        private void RssChannelItem_MouseLeave(object sender, MouseEventArgs e)
        {
            var stackPanel = (StackPanel) sender;
            var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x7E, 0x8B, 0xD0));
            stackPanel.Background = brush;
            stackPanel.Cursor = Cursors.Arrow;
            this.lbl_statusMessage.Content = "";
        }
        
        private void removeChannelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            pnl_channelList.Children.Remove(this.rightClickedRssPanel);
            this.feedList.RemoveAt(this.rightClickedRssPanel.Id);
            RssChannelStackPanel panel;
            for(int i=1; i<this.pnl_channelList.Children.Count; i++)
            {
            	panel = (RssChannelStackPanel)pnl_channelList.Children[i];
            	panel.Id = i-1;
            }
        }
        
        //кнопка для тестирования всплывающего сообщения
        void button1_Click(object sender, RoutedEventArgs e)
		{
        	Notification notWindow = new Notification("Консультант Плюс в Ростове. Мы оказываем информационную и техническую поддержку по вопросам использования справочно правовой системы Консультант Плюс Ростов-на-Дону.",
        	                                          "кадровые семинары, консультант плюс демо версия, консультант плюс документы, консультант плюс законы, консультант плюс программа, консультант плюс ростов",
        	                                          new Uri("http://igc.ru"));
			notWindow.Show();
		}
	}
}
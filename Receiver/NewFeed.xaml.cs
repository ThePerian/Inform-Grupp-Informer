using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace IGCInformer
{
	/// <summary>
	/// Окно добавления нового RSS-канала.
	/// </summary>
    public partial class NewFeed : Window
	{
		private MainWindow mainWindow;

        public NewFeed(MainWindow window)
        {
            InitializeComponent();
            this.mainWindow = window;
            this.btn_addFeed.Click += new RoutedEventHandler(NewFeed_Click);
        }

        private void NewFeed_Click(object sender, EventArgs e)
        {
        	var feedUrl = tb_url.Text;
            SyndicationFeed rssFeedChannel;
            
            try
            {
                using (var xmlReader = XmlReader.Create(feedUrl))
                {
                    rssFeedChannel = SyndicationFeed.Load(xmlReader);
                }
                if (!mainWindow.feedList.Any(x => feedUrl == x.Url))
                {
                    this.mainWindow.channelList.Add(rssFeedChannel);
                    var rssFeed = new RssFeed(feedUrl, rssFeedChannel.Items.ElementAt(0).PublishDate.ToString());
                    this.mainWindow.feedList.Add(rssFeed);
                    rssFeed.Refresh(rssFeedChannel);
                    this.mainWindow.AddChannelPanel("(" + rssFeed.NewItemsCount + ")" + rssFeedChannel.Title.Text,
                                                    this.mainWindow.pnl_channelList.Children.Count-1);
                    this.tb_validation.Background = Brushes.Green;
                    this.tb_validation.Text = "Новый канал успешно добавлен в список.";
                }
                else
                {
                    this.tb_validation.Background = Brushes.Orange;
                    this.tb_validation.Text = "Канал с указанным URL уже есть в списке.";
                }
            }
            catch (Exception)
            {
                this.tb_validation.Background = Brushes.Red;
                this.tb_validation.Text = "Не удалось найти канал по указанному адресу.";
            }
        }
	}
}
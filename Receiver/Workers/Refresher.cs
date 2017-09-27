using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace IGCInformer.Workers
{
	/// <summary>
	/// Проверяет наличие новых сообщений в RSS-каналах.
	/// </summary>
    public class Refresher
	{
		private MainWindow mainWindow;
        private SyndicationFeed rssFeedChannel;
        bool isAuto;

        public Refresher(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
			isAuto = true;            
        }
        
        /// <summary>
        /// Проверить наличие новых сообщений во всех каналах.
        /// </summary>
        /// <param name="isAuto">Указывает была ли запущена автоматически. При автоматическом запуске не выводится информация в статусную строку.</param>
	    public void RefreshAllFeeds(bool isAuto)
	    {
	    	this.isAuto = isAuto;
			if ((this.mainWindow.feedList == null) || (this.mainWindow.feedList.Count == 0)) return;
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += this.worker_DoWork;
			worker.RunWorkerCompleted += this.worker_RunWorkerCompleted;
			if (!isAuto)
			{
				worker.WorkerReportsProgress = true;
				worker.ProgressChanged += this.worker_ProgressChanged;
				this.mainWindow.lbl_statusMessage.Content = "Обновление...";
				this.mainWindow.progressBar.Visibility = Visibility.Visible;
			}
			worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            for (int i = 0; i < this.mainWindow.feedList.Count; i++)
            {
                Thread.Sleep(200);
                using (var xmlReader = XmlReader.Create(this.mainWindow.feedList[i].Url))
                {
                    this.rssFeedChannel = SyndicationFeed.Load(xmlReader);
                }
                this.mainWindow.channelList[i] = rssFeedChannel;
                this.mainWindow.feedList[i].Refresh(this.mainWindow.channelList[i]);
                if (!isAuto)
                {
                	worker.ReportProgress((int) (((double) (i + 1) / (double) this.mainWindow.feedList.Count) * 100));
                }
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			if (this.mainWindow.feedList.Count==0) return;
            for (int i = 0; i < this.mainWindow.feedList.Count; i++)
            {
                RssChannelStackPanel pan = (RssChannelStackPanel) this.mainWindow.pnl_channelList.Children[i + 1];
                TextBlock tb = (TextBlock) pan.Children[0];
                tb.Text = "(" + this.mainWindow.feedList[i].NewItemsCount + ") "
                	+ this.mainWindow.channelList[i].Title.Text;
                if ((this.mainWindow.WindowState == WindowState.Minimized)
                    && (this.mainWindow.feedList[i].hasNewItems))
                	showUpdate(this.mainWindow.channelList[i]);
            }
            if (!isAuto)
            {
	            this.mainWindow.lbl_statusMessage.Content = "Обновление завершено.";
	            this.mainWindow.progressBar.Value = 0;
	            this.mainWindow.progressBar.Visibility = Visibility.Hidden;
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	this.mainWindow.lbl_statusMessage.Content = 
        		String.Concat("Обновление... " + e.ProgressPercentage.ToString() + "%");
            this.mainWindow.progressBar.Value = e.ProgressPercentage;
        }
		
        /// <summary>
        /// Показывает в отдельном окне первое сообщение в указанном фиде.
        /// </summary>
        /// <param name="feed">Фид, сообщение из которого нужно показать.</param>
		private void showUpdate(SyndicationFeed feed)
		{
			string header = feed.Title.Text;
			string message;
			Uri link;
			IEnumerator<SyndicationItem> enumerator = feed.Items.GetEnumerator();
			enumerator.MoveNext();
			SyndicationItem last = enumerator.Current;
			message = last.Title.Text;
			link = last.Links.ElementAt(0).Uri;
        	Notification window = new Notification(header, message, link);
        	window.Show();
        }
	}
}

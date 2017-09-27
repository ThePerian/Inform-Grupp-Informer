using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using IGCInformer.Helpers;

namespace IGCInformer.Workers
{
	/// <summary>
	/// Выполняет начальную загрузку RSS-каналов в фоновом режиме.
	/// </summary>
    public class Loader
	{
		MainWindow mainWindow;
        SyndicationFeed rssFeedChannel;
		
		public Loader(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
		}
		
		/// <summary>
		/// Начинает загрузку каналов.
		/// </summary>
		public void LoadChannels()
		{
			BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += this.worker_DoWork;
            worker.RunWorkerCompleted += this.worker_RunWorkerCompleted;
            worker.ProgressChanged += this.worker_ProgressChanged;
            this.mainWindow.lbl_statusMessage.Content = "Загрузка...";
            this.mainWindow.progressBar.Visibility = Visibility.Visible;
            worker.RunWorkerAsync();
		}
		
		private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (FeedHelper.LocateFeedList() == string.Empty)
            {
            	e.Cancel = true;
            	return;
            }
            this.mainWindow.feedList = FeedHelper.LoadFeeds(FeedHelper.LocateFeedList()).ToList();
            this.mainWindow.channelList = new List<SyndicationFeed>();
            for (int i = 0; i < this.mainWindow.feedList.Count; i++)
            {
            	try
            	{
                	var xmlReader = XmlReader.Create(mainWindow.feedList[i].Url);
                	this.rssFeedChannel = SyndicationFeed.Load(xmlReader);
                }
            	catch
            	{
            		MessageBox.Show("RSS канал " + mainWindow.feedList[i].Url + " не найден.",
            		                "Ошибка", MessageBoxButton.OK);
            		mainWindow.feedList.RemoveAt(i--);
            		continue;
            	}
                this.mainWindow.feedList[i].Refresh(this.rssFeedChannel);
                this.mainWindow.channelList.Add(rssFeedChannel);
                worker.ReportProgress((int) (((double) (i + 1) / (double) this.mainWindow.feedList.Count) * 100));
            }
        }
		
		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			if ((this.mainWindow.feedList != null) && (this.mainWindow.feedList.Count!=0)) 
			{
				for (int i = 1; i < this.mainWindow.pnl_channelList.Children.Count; i++)
				{
					this.mainWindow.pnl_channelList.Children.RemoveAt(i);
				}
	            for (int i = 0; i < this.mainWindow.feedList.Count; i++)
	            {
	                this.mainWindow.AddChannelPanel("(" + this.mainWindow.feedList[i].NewItemsCount + ") "
	            	                             + this.mainWindow.channelList[i].Title.Text, i);
	            }
            	this.mainWindow.lbl_statusMessage.Content = "Загрузка завершена.";
			}
			else
			{
				this.mainWindow.lbl_statusMessage.Content = "Не найден список RSS каналов";
			}
            this.mainWindow.progressBar.Value = 0;
            this.mainWindow.progressBar.Visibility = Visibility.Hidden;
            this.mainWindow.updater.Start();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	this.mainWindow.lbl_statusMessage.Content = 
        		String.Concat("Загрузка... " + e.ProgressPercentage.ToString() + "%");
            this.mainWindow.progressBar.Value = e.ProgressPercentage;
        }
	}
}

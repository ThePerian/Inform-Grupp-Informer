using System;
using System.Linq;
using System.ServiceModel.Syndication;

namespace IGCInformer
{
	/// <summary>
	/// RSS-канал.
	/// </summary>
    public class RssFeed
	{
		public string Url { get; set; }
        public string LastUpdated { get; set; }
        public int NewItemsCount { get; set; }
        public bool hasNewItems { get; set; }

        public RssFeed() {}

        /// <summary>
        /// Создает новый RSS-канал.
        /// </summary>
        /// <param name="url">Путь к каналу.</param>
        /// <param name="lastUpdated">Время последнего обновления.</param>
        public RssFeed(string url, string lastUpdated)
        {
            this.Url = url;
            this.LastUpdated = lastUpdated;
            this.NewItemsCount = 0;
        }

        /// <summary>
        /// Обновить RSS-канал. Обновляет счетчик новых сообщений.
        /// </summary>
        /// <param name="feed">RSS-канал.</param>
        public void Refresh(SyndicationFeed feed)
        {
            DateTime lastUpdated = DateTime.Parse(this.LastUpdated);
            int count = 0;
            
            feed.Items = feed.Items.OrderByDescending(feedItem => feedItem.PublishDate);
            foreach (var feedItem in feed.Items)
            {
                if (feedItem.PublishDate > lastUpdated)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            this.LastUpdated = feed.Items.ElementAt(0).PublishDate.ToString();
            if (count == 0)
            {
            	this.hasNewItems = false;
            }
            else
            {
            	this.NewItemsCount = count;
            	this.hasNewItems = true;
            }
        }
        
        /// <summary>
        /// Обновить RSS-канал.
        /// </summary>
        /// <param name="feed">RSS-канал.</param>
        /// <param name="resetNewItems">Определяет обновлять ли счетчик новых сообщений.
        /// Если счетчик не обновляется, флаг, сообщающий о наличии новых сообщений, все равно устанавливается.</param>
        public void Refresh(SyndicationFeed feed, bool resetNewItems)
        {
            DateTime lastUpdated = DateTime.Parse(this.LastUpdated);
            int count = 0;
            
            feed.Items = feed.Items.OrderByDescending(feedItem => feedItem.PublishDate);
            foreach (var feedItem in feed.Items)
            {
                if (feedItem.PublishDate > lastUpdated)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            this.LastUpdated = feed.Items.ElementAt(0).PublishDate.ToString();
            if (count == 0)
            {
            	this.hasNewItems = false;
            }
            else
            {
            	if (resetNewItems) 
            	{
            		this.NewItemsCount = count;
            	}
            	else
            	{
            		this.NewItemsCount += count;
            	}
            	this.hasNewItems = true;
            }
        }
	}
}

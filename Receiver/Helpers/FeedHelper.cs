using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;

namespace IGCInformer.Helpers
{
	/// <summary>
	/// Предоставляет набор функций для работы с RSS-каналами.
	/// </summary>
	public class FeedHelper
	{
		//путь к файлу со списком RSS-каналов, должен находиться там же, где .exe
		private static string RssListPath = LocateFeedList();

		/// <summary>
		/// Загружает список RSS-каналов из документа XML.
		/// </summary>
		/// <param name="xmlUri">Путь к документу XML.</param>
		/// <returns>Список (List) элементов типа RssFeed</returns>
        public static IEnumerable<RssFeed> LoadFeeds(string xmlUri)
        {
            XDocument xml = XDocument.Load(xmlUri);
            try
            {
	            var feed = from item in xml.Descendants("Feed")
	                    select new RssFeed
	                    {
	                        Url = (string)item.Element("Url"),
	                        LastUpdated = (string)item.Element("LastUpdated")
	                    };
            	return feed;
            }
            catch (Exception e)
            {
            	MessageBox.Show(e.Message, "Произошла ошибка при загрузке RSS каналов");
            	var feed = new List<RssFeed>(0);
            	return feed;
            }
        }

        /// <summary>
        /// Сохраняет время последнего обновления канала.
        /// </summary>
        /// <param name="feed">RSS-канал.</param>
        public static void UpdateTimes(RssFeed feed)
        {
            XDocument xml = XDocument.Load(RssListPath);
            var feedItem = xml.Descendants("Feed").FirstOrDefault(x => x.Element("Url").Value == feed.Url);
            if (feedItem != null)
            {
                feedItem.Element("LastUpdated").Value = feed.LastUpdated;
            }
            xml.Save(RssListPath);
        }

        /// <summary>
        /// Сохраняет список каналов в файл XML.
        /// </summary>
        /// <param name="feedList">Список каналов.</param>
        public static void SaveFeeds(List<RssFeed> feedList)
        {
        	try
        	{
        		if (!File.Exists(RssListPath)) File.Create(Environment.CurrentDirectory + "\\RssList.xml");
        	}
        	catch
        	{
        		MessageBox.Show("Не удалось сохранить список каналов", "Ошибка", MessageBoxButtons.OK);
        		return;
        	}
            //XDocument xml = XDocument.Load(RssListPath);
            //XElement element = new XElement("Feeds");
            //element.RemoveAll();
            XElement newXml = new XElement("Feeds",
                                        from f in feedList
                                        select new XElement("Feed",
                                               new XElement("Url", f.Url),
                                               new XElement("LastUpdated", f.LastUpdated))
                                       );
            newXml.Save(RssListPath);
        }
        
        /// <summary>
        /// Определяет наличие списка каналов в рабочей папке программы.
        /// </summary>
        /// <returns>Путь к XML-файлу со списком каналов.</returns>
        public static string LocateFeedList()
        {
        	string feedListPath = Environment.CurrentDirectory + "\\RssList.xml";
        	if (!File.Exists(feedListPath))
        	{
        		return string.Empty;
        	}
        	return feedListPath;
        }
	}
}

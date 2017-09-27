using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;

namespace IGCInformer.Helpers
{
	/// <summary>
	/// Предоставляет функции для создания HTML-документов в виде строк.
	/// </summary>
    public static class HtmlBuilder
    {
    	/// <summary>
    	/// Создает HTML-страницу на основе RSS-канала.
    	/// </summary>
    	/// <param name="feed">Канал, данные которого необходимо отобразить в виде HTML-страницы.</param>
    	/// <returns>HTML-страница в виде строки.</returns>
        public static string ConstructItemsHtml(SyndicationFeed feed)
        {
            StringBuilder result = new StringBuilder(
                    @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">" +
                         "<html>" +
                           @"<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">" +
                            @"<head></head>" +
                            @"<body style=""font-family:Calibri;"">");
        	feed.Items = feed.Items.OrderByDescending(item => item.PublishDate);
            foreach (var item in feed.Items)
            {
            	result.Append(@"<h6 style=""text-align:right;"">" 
            	              + item.PublishDate.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss")
            	              + @"</h6>");
                if (item.Categories.Count != 0) result.Append(@"<h4 style=""text-align:center;"">КАТЕГОРИЯ: "
            	                                              +item.Categories.ElementAt(0).Name
            	                                              +@"</h4>");
                result.Append(@"<h3 style=""text-align:center;"">");
                result.Append(@"<a href="""
                              + item.Links.ElementAt(0).Uri.ToString()
                              + '"' + " target=" + '"' + "_blank" + '"' + ">");
                result.Append(item.Title.Text);
                result.Append(@"</a></h3>");
                if (item.Summary != null) result.Append(item.Summary.Text);
                result.Append(@"<hr/>");
            }
            result.Append(@"</body>");
            result.Append(@"</html>");
            return result.ToString();
        }

        /// <summary>
        /// Создает HTML-страницу для отображения после успешной авторизации на сервере OAuth.
        /// </summary>
        /// <returns>Страница HTML в виде строки.</returns>
        public static string ConstructResponseHtml()
        {
            StringBuilder result = new StringBuilder();

            result.Append(@"<!DOCTYPE html><html>" +
                            @"<head></head>" +
                            @"<body onload=""closeThis();"">" +
                            @"<h1>Authorization Successfull</h1>" +
                            @"<p>You can now close this window</p>" +
                            @"<script type=""text/javascript"">" +
                                @"function closeMe() { window.close(); }" +
                                @"function closeThis() { window.close(); }" +
                            @"</script>" +
                            @"</body>" +
                            @"</html>");
            return result.ToString();
        }
        
        /// <summary>
        /// Создает пустой HTML документ.
        /// </summary>
        /// <returns>Страница HTML в виде строки.</returns>
        public static string ConstructEmptyHtml()
        {
        	StringBuilder result = new StringBuilder();

            result.Append(@"<!DOCTYPE html><html>" +
                            @"<head></head>" +
                            @"<body></body>" +
                            @"</html>");
            return result.ToString();
        }
    }
}


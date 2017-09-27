using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IGCInformer.Helpers
{
	/// <summary>
	/// Предоставляет набор функций для авторизации на Яндексе с помощью OAuth 2.0.
	/// </summary>
	public class OAuthForYandex
	{
		//ID и секрет получаются на https://oauth.yandex.ru/client/new и изменить их невозможно
        private const string oauthClientId = "54d4553d5eac4ab887c3d03f7b5064f6";
        private const string oauthClientSecret = "bddaae676b5f4cc1b96fee1793e8b189";
        //адрес, на который Яндекс перенаправляет пользователя после авторизации
        public string listenAddress {get; set;}
        //путь, по которому нужно обращаться для получения кода авторизации
		public string baseUrl {get; set;}
		public string authorizationUrl {get; set;}
		//путь для запроса Access Token
		public string oauthTokenRequestUrl {get; set;}
		//путь для запроса информации о пользователе
		public string userInfoRequestUrl {get; set;}
        
		public OAuthForYandex()
		{
            listenAddress = "http://localhost:4444/Authorization/";
            baseUrl = "https://oauth.yandex.ru";
            authorizationUrl = string.Format(
            	"{0}/authorize?response_type=code&client_id={1}",
            	baseUrl,
            	oauthClientId);
            oauthTokenRequestUrl = "https://oauth.yandex.ru/token";
            userInfoRequestUrl = "https://login.yandex.ru/info";
		}
		
		/// <summary>
		/// Получает код для запроса Access Token.
		/// </summary>
		/// <returns>Строка с кодом.</returns>
		public string GetOauthAccessCode()
        {
            string CloseWindowResponse = HtmlBuilder.ConstructResponseHtml();
            string code = "";
            
            using (var listener = new HttpListener())
            {
                string localHostUrl = string.Format(listenAddress);

                listener.Prefixes.Add(localHostUrl);
                try
                {
                	listener.Start(); 
                	//может быть закрыт доступ из-за недостатка прав или использования порта;
					//в таком случае сменить порт
                }
                catch (Exception ex)
                {
                	MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK);
                	return string.Empty;
                }

                using (Process.Start(authorizationUrl))
                {
                	while (true)
		            {
		                var context = listener.GetContext();
		
		                if (context.Request.Url.OriginalString.Contains(listenAddress))
		                {//TODO: обработать случай когда пользователь отказал в доступе к данным и случай когда связь прервана
		                    code = context.Request.QueryString["code"]; 
		                    //получить информацию можно только если код или токен передаются после ?,
							//информацию из фрагмента не получить никак
		                    if (code == null)
		                    {
		                        return string.Empty;
		                    }
		                    var writer = new StreamWriter(context.Response.OutputStream);
		                    writer.WriteLine(CloseWindowResponse);
		                    writer.Flush();
		
		                    Thread.Sleep(1000);
		
		                    context.Response.Close();
		                    break;
		                }
		                context.Response.StatusCode = 404;
		                context.Response.Close();
		            }
                }
                listener.Close();
            }
            return code;
        }
		
		/// <summary>
		/// Получает строку пар "параметр-значение" для передачи запроса на получение Access Token, встраивается в Url.
		/// </summary>
		/// <param name="oauthAccessCode">код для получения Access Token</param>
		/// <returns>Строка параметров.</returns>
		private string GetOauthTokenPostData(string oauthAccessCode)
		{
            string oauthTokenPostData = "grant_type=authorization_code&code="
                + oauthAccessCode
                + "&client_id="
                + oauthClientId
                + "&client_secret="
                + oauthClientSecret;
			return oauthTokenPostData;
		}
		
		/// <summary>
		/// Получает строку пар "параметр-значение" для передачи запроса на получение
		/// информации о пользователе, встраивается в Url.
		/// </summary>
		/// <param name="oauthAccessToken">Токен для доступа к информации о пользователе.</param>
		/// <returns>Строка параметров.</returns>
		private string GetUserInfoPostData(string oauthAccessToken)
		{
            string postData = "format=xml"
                + "&oauth_token=" 
                + oauthAccessToken;
			return postData;
		}

		/// <summary>
		/// Запрашивает на сервере авторизации Access Token.
		/// </summary>
		/// <param name="oauthAccessCode">Код для получения токена.</param>
		/// <returns>Вся информация, переданная сервером в ответ.</returns>
        public string GetOauthData(string oauthAccessCode)
        {
            string postData = GetOauthTokenPostData(oauthAccessCode);
            string responseFromServer = MakeWebRequest(oauthTokenRequestUrl, postData);
            return responseFromServer;
        }
        
        /// <summary>
        /// Запрашивает на сервере авторизации Access Token.
        /// </summary>
        /// <param name="oauthAccessCode">Код для получения токена.</param>
        /// <returns>Access Token.</returns>
        public string GetOauthAccessToken(string oauthAccessCode)
        {
        	string oauthData = GetOauthData(oauthAccessCode);
        	//вместе с токеном возвращается еще несколько в данном случае не нужных параметров
        	//вырезаем токен из ответной строки
            string access_token = oauthData.Split(':', ',')[3].Trim('"', ' ');
        	return access_token;
        }

        /// <summary>
        /// Запрашивает на сервере информацию о пользователе.
        /// </summary>
        /// <param name="oauthAccessToken">Код для доступа к информации.</param>
        /// <returns>Ответ сервера - вся информация, к которой открыт доступ.</returns>
        public string GetUserInfo(string oauthAccessToken)
        {
            string postData = GetUserInfoPostData(oauthAccessToken);
            string responseFromServer = MakeWebRequest(userInfoRequestUrl, postData);
            return responseFromServer;
        }
		
        /// <summary>
        /// Формирует и отправляет POST запрос.
        /// </summary>
        /// <param name="requestUrl">Путь, по которому отправляется запрос.</param>
        /// <param name="postData">Набор пар "параметр=значение".</param>
        /// <returns>Весь ответ сервера.</returns>
        private string MakeWebRequest(string requestUrl, string postData)
        {
        	WebRequest request = WebRequest.Create(requestUrl);
        	//при необходимости можно вынести тип метода в параметры функции
        	//и превратить функцию в универсальный формирователь запросов
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response;
            try 
            {
            	response = request.GetResponse();
            }
            catch
            {
            	return string.Empty;
            }
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }
	}
}

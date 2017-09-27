using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using IGCInformer.Helpers;

namespace IGCInformer.Workers
{
	/// <summary>
	/// Выполняет запрос авторизации и, в случае положительного ответа, информацию о пользователе. 
	/// </summary>
	public class Authorizer
	{
		MainWindow mainWindow;
		string oauthAccessToken;
		
		public Authorizer(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			oauthAccessToken = string.Empty;
		}
		
		public void StartAuthorization()
		{
			BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += this.worker_DoWork;
            worker.RunWorkerCompleted += this.worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
		}
		
		private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
			//получить имя, фамилию и аватарку пользователя на Яндексе и вывести их над списком RSS
        	OAuthForYandex oauth = new OAuthForYandex();
        	string oauthAccessCode = oauth.GetOauthAccessCode();
        	if (oauthAccessCode == string.Empty)
        	{
        		mainWindow.lbl_statusMessage.Content = "Не удалось получить доступ к Яндексу";
        		e.Cancel = true;
        		return;
        	}
	        oauthAccessToken = oauth.GetOauthAccessToken(oauthAccessCode);
		}
		
		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			if (oauthAccessToken == string.Empty) return;
        	OAuthForYandex oauth = new OAuthForYandex();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(oauth.GetUserInfo(oauthAccessToken));//возвращает всю доступную информацию в виде XML
            string first_name = xmlDoc.SelectSingleNode("user//first_name").InnerText;//по-хорошему стоит создать класс для юзера
            string last_name = xmlDoc.SelectSingleNode("user//last_name").InnerText;
            string avatar_id = xmlDoc.SelectSingleNode("user//default_avatar_id").InnerText;
            mainWindow.tb_name.Text = first_name + " " + last_name;
            mainWindow.lbl_statusMessage.Content = "Вы вошли как " + first_name + " " + last_name;

            string fullFilePath = @"https://avatars.yandex.net/get-yapic/" + avatar_id + "/islands-small";//islands-small - размер аватарки, 28х28

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();

            mainWindow.img_avatar.Source = bitmap;
            
            mainWindow.Activate();

            mainWindow.btn_login.Visibility = Visibility.Collapsed;
            mainWindow.tb_name.Visibility = Visibility.Visible;
            mainWindow.img_avatar.Visibility = Visibility.Visible;
            mainWindow.btn_refresh.Visibility = Visibility.Visible;
            
            mainWindow.tb_prompt.Text = "Уведомления";
            
            mainWindow.loader.LoadChannels();
		}
	}
}

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace IGCInformer
{
	/// <summary>
	/// Всплывающее окно с новым сообщением.
	/// </summary>
	public partial class Notification : Window
	{
		Uri uri;
		
		public Notification()
		{
			Initialize();
		}
		
		/// <summary>
		/// Создает новый экземпляр всплывающего окна с сообщением.
		/// </summary>
		/// <param name="header">Заголовок, название RSS-канала.</param>
		/// <param name="message">Сообщение, заголовок новости.</param>
		/// <param name="link">Адрес, по которому перейдет пользователь при нажатии на сообщение.</param>
		public Notification(string header, string message, Uri link)
		{
			Initialize();
			this.tb_header.Text = header;
			this.tb_message.Text = message;
			this.uri = link;
		}
		
		public void Initialize()
		{
			InitializeComponent();
			this.tb_message.MouseLeftButtonDown += new MouseButtonEventHandler(Message_MouseLeftButtonDown);
            this.tb_message.MouseEnter += new MouseEventHandler(Message_MouseEnter);
            this.tb_message.MouseLeave += new MouseEventHandler(Message_MouseLeave);
            this.img_close.MouseLeftButtonDown += new MouseButtonEventHandler(Close_MouseLeftButtonDown);
            this.img_close.MouseEnter += new MouseEventHandler(Close_MouseEnter);
            this.img_close.MouseLeave += new MouseEventHandler(Close_MouseLeave);
		}
		
		public new void Show()
		{
			base.Show();
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - this.Width;
			DoubleAnimation animation = new DoubleAnimation(this.Top, this.Top - this.Height, 
			                                                TimeSpan.FromSeconds(0.5), FillBehavior.Stop);
			this.BeginAnimation(Notification.TopProperty, animation);//TODO: анимация застревает если попадает на ежепятисекундное обновление
		}

        private void animation_Completed(object sender, EventArgs e)
        {
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - this.Height;
        }
		
		private void Message_MouseLeftButtonDown(Object sender, MouseEventArgs e)
		{
			this.Close();
			Process.Start(this.uri.ToString());
		}
		
		private void Close_MouseLeftButtonDown(Object sender, MouseEventArgs e)
		{
			this.Close();
		}
		
		private void Message_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.tb_message.TextDecorations = TextDecorations.Underline;
        }

        private void Message_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            this.tb_message.TextDecorations = null;
        }
        
		private void Close_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.img_close.Source = new BitmapImage(
            	new Uri(@"/IGCInformer;component/bin/Debug/closeactive.png",
            	        UriKind.Relative));
        }

        private void Close_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            this.img_close.Source = new BitmapImage(
            	new Uri(@"/IGCInformer;component/bin/Debug/close.png",
            	        UriKind.Relative));
        }
	}
}
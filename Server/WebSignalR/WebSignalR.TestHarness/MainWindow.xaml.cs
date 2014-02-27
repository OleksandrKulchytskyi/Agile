using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebSignalR.TestHarness
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private CookieContainer cookieTrunk;
		private WebClientEx webClient;
		private Cookie regCookie;
		HubConnection hubConnection;
		IHubProxy hubProxy;
		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += MainWindow_Loaded;
			this.Closing += MainWindow_Closing;
		}

		void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (hubConnection != null && hubConnection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
			{
				hubConnection.Stop();
			}
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			txtUrl.Text = "http://localhost/websignalr/handlers/loginhandler.ashx";
		}

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(txtPass.Text) && !string.IsNullOrEmpty(txtUser.Text) && !string.IsNullOrEmpty(txtUrl.Text))
			{
				try
				{
					regCookie = null;
					cookieTrunk = new CookieContainer();
					webClient = new WebClientEx(cookieTrunk);
					webClient.Headers.Add("Authorization", "Basic " + toBase64Utf8(txtUser.Text + ":" + toBase64Utf8(txtPass.Text)));
					string result = webClient.DownloadString(txtUrl.Text);
					if (result.Contains("User has been authorized."))
					{
						var val = cookieTrunk.GetCookies(new Uri(txtUrl.Text));
						if (val != null)
							regCookie = val[1];
						MessageBox.Show(this, "Authorized");
					}
					else
						MessageBox.Show(this, "Not authorized");
				}
				catch (Exception ex)
				{
					MessageBox.Show(this, ex.Message);
				}
				finally
				{
					webClient.Dispose();
				}
			}

			if (regCookie != null)
			{
				Uri url = new Uri(txtUrl.Text);
				hubConnection = new HubConnection(url.Scheme + "://" + url.Authority + "/");
				hubConnection.CookieContainer = cookieTrunk;
				hubProxy = hubConnection.CreateHubProxy("agileHub");
				try
				{
					hubConnection.Start().Wait();

					hubProxy.On("onJoinRoom", new Action<dynamic>((d) =>
					{
						Dispatcher.Invoke(new Action(() =>
						{
							txtLog.AppendText("User joined" + Environment.NewLine);
							txtLog.AppendText(string.Concat(d.Name, " ", d.Active));
						}));
					}));

					hubProxy.Invoke("JoinRoom", "Project", hubConnection.ConnectionId).Wait();
				}
				catch (AggregateException ex)
				{
					MessageBox.Show(ex.InnerException.Message);
				}
			}
		}

		private string toBase64Utf8(string strOriginal)
		{
			byte[] byt = System.Text.Encoding.UTF8.GetBytes(strOriginal);
			// convert the byte array to a Base64 string
			return Convert.ToBase64String(byt);
		}
	}
}

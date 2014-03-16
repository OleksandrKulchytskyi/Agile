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
	public partial class MainWindow : Window, ILog
	{
		private CookieContainer cookieTrunk;
		private WebClientEx webClient;
		private Cookie regCookie;
		private HubConnection hubConnection;
		private dynamic userState;
		private IHubProxy hubProxy;

		private Dictionary<string, dynamic> onCalbackContainer;

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
			txtUser.Text = "Admin";
			txtUrl.Text = "http://localhost:6404/handlers/loginhandler.ashx";
			txtHubname.Text = "agileHub";
			btnIvoke.IsEnabled = false;
			btnLogout.IsEnabled = false;
			btnStoptHub.IsEnabled = false;
			btnRegisterCallback.IsEnabled = false;
		}

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (regCookie != null)
			{
				MessageBox.Show(this, "You are currently logged in." + Environment.NewLine + "Press logout button and relogin again.");
				return;
			}

			txtLog.Clear();

			if (!string.IsNullOrEmpty(txtPass.Password) && !string.IsNullOrEmpty(txtUser.Text) && !string.IsNullOrEmpty(txtUrl.Text))
			{
				try
				{
					regCookie = null;
					cookieTrunk = new CookieContainer();
					webClient = new WebClientEx(cookieTrunk);
					webClient.Headers.Add("Authorization", "Basic " + toBase64Utf8(txtUser.Text + ":" + toBase64Utf8(txtPass.Password)));
					string result = webClient.DownloadString(txtUrl.Text);
					if (result.Contains("User has been authorized."))
					{

						var val = cookieTrunk.GetCookies(new Uri(txtUrl.Text));
						if (val != null)
						{
							btnLogin.IsEnabled = false;
							btnLogout.IsEnabled = true;
							regCookie = val[1];
						}
						MessageBox.Show(this, "Authorized");
					}
					else
						MessageBox.Show(this, "Not authorized");
				}
				catch (Exception ex)
				{
					txtLog.AppendText(ex.ToString());
					MessageBox.Show(this, ex.Message);
				}
				finally
				{
					webClient.Dispose();
				}
			}
		}

		private void btnLogout_Click(object sender, RoutedEventArgs e)
		{
			if (regCookie == null)
			{
				MessageBox.Show(this, "You are not logged in." + Environment.NewLine + "Please perform login operation first.");
				return;
			}

			using (WebClientEx client = new WebClientEx(cookieTrunk))
			{
				Uri Old = new Uri(txtUrl.Text);
				Uri logoutUri = new Uri(Old.Scheme + Uri.SchemeDelimiter + Old.Authority + "/handlers/logouthandler.ashx");
				try
				{
					String result = webClient.DownloadString(logoutUri);
					if (result.Contains("Successfully"))
					{
						regCookie = null;
						btnLogin.IsEnabled = true;
					}
				}
				catch (WebException ex)
				{
					MessageBox.Show(this, ex.Message);
				}
				finally
				{
					btnLogout.IsEnabled = false;
				}
			}
		}

		private string toBase64Utf8(string strOriginal)
		{
			byte[] byt = System.Text.Encoding.UTF8.GetBytes(strOriginal);
			// convert the byte array to a Base64 string
			return Convert.ToBase64String(byt);
		}

		private void btnIvoke_Click(object sender, RoutedEventArgs e)
		{
			if (regCookie == null)
			{
				MessageBox.Show(this, "Not logged in.");
				return;
			}

			MethodInvokeView view = new MethodInvokeView(hubProxy);
			view.Owner = this;
			view.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
			view.ShowDialog();
		}

		private void btnStartHub_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(txtHubname.Text))
			{
				MessageBox.Show(this, "Hub name cannot be empty. Fill Hub name text field.");
				return;
			}

			if (regCookie != null)
			{
				Uri url = new Uri(txtUrl.Text);
				string http = url.Scheme + Uri.SchemeDelimiter + url.Authority + "/";
				if (!url.Authority.Contains(":") && url.LocalPath.Length > 2)
					http = http + url.LocalPath.Substring(0, url.LocalPath.IndexOf('/', 1));

				hubConnection = new HubConnection(http);
				hubConnection.StateChanged += hubConnection_StateChanged;
				hubConnection.CookieContainer = cookieTrunk;
				try
				{
					hubProxy = hubConnection.CreateHubProxy(txtHubname.Text);
					hubConnection.Start().Wait();
					btnStartHub.IsEnabled = false;
					btnIvoke.IsEnabled = true;
					btnRegisterCallback.IsEnabled = true;
					btnStoptHub.IsEnabled = true;
					onCalbackContainer = new Dictionary<string, dynamic>();
					hubProxy.On("onJoinRoom", new Action<dynamic>((d) =>
					{
						Dispatcher.Invoke(new Action(() =>
						{
							txtLog.AppendText("User joined" + Environment.NewLine);
							txtLog.AppendText(d.ToString());
							txtLog.AppendText(string.Concat(d.Name, " ", d.Active, Environment.NewLine));
							onCalbackContainer["onJoinRoom"] = d;
						}));
					}));

					hubProxy.On("onState", new Action<dynamic>((d) =>
					{
						Dispatcher.Invoke(new Action(() =>
						{
							txtLog.AppendText("OnState received" + Environment.NewLine);
							if (d != null)
							{
								txtLog.AppendText(d.ToString() + Environment.NewLine);
								userState = d;
								onCalbackContainer["onState"] = d;
							}
						}));
					}));
				}
				catch (AggregateException ex)
				{
					txtLog.AppendText(GetStringFromAggregateException(ex));
					hubProxy = null;
					hubConnection = null;
					btnStoptHub.IsEnabled = false;
					btnStartHub.IsEnabled = true;
					hubConnection.StateChanged -= hubConnection_StateChanged;
				}
			}
		}

		void hubConnection_StateChanged(Microsoft.AspNet.SignalR.Client.StateChange obj)
		{

		}

		private void btnStoptHub_Click(object sender, RoutedEventArgs e)
		{
			if (hubConnection != null && hubConnection.State != Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
			{
				try
				{
					hubConnection.Stop();
				}
				catch (Exception ex)
				{
					txtLog.AppendText(ex.Message + Environment.NewLine);
				}
				finally
				{
					btnIvoke.IsEnabled = false;
					btnRegisterCallback.IsEnabled = false;
					btnStartHub.IsEnabled = true;
					(sender as Button).IsEnabled = false;
					hubConnection.StateChanged -= hubConnection_StateChanged;
					hubConnection = null;
				}
			}
		}

		internal string GetStringFromAggregateException(AggregateException ex)
		{
			if (ex == null)
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("ERROR:");
			sb.AppendLine(ex.InnerException.Message);
			if (ex.InnerException.InnerException != null)
				sb.AppendLine(ex.InnerException.InnerException.Message);

			return sb.ToString();
		}

		public void LogMessage(string msg)
		{
			if (txtLog != null)
				txtLog.AppendText(msg);
		}

		private void btnRegisterCallback_Click(object sender, RoutedEventArgs e)
		{
			SubscribeCallbackView view = new SubscribeCallbackView(hubProxy);
			view.Owner = this;
			view.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
			view.ShowDialog();
		}
	}
}

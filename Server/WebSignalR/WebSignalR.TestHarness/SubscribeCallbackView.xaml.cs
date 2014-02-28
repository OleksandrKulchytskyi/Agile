using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebSignalR.TestHarness
{
	/// <summary>
	/// Interaction logic for SubscribeCallbackView.xaml
	/// </summary>
	public partial class SubscribeCallbackView : Window
	{
		private WeakReference _weak;
		private TaskScheduler scheduler;

		public SubscribeCallbackView(IHubProxy proxy)
		{
			InitializeComponent();
			_weak = new WeakReference(proxy);
			//SyncronizationContext
			scheduler = TaskScheduler.FromCurrentSynchronizationContext();
		}

		private void btn_OnRegister(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(txtCallbackName.Text))
			{
				MessageBox.Show(this, "Callback name cannot be empty.");
				return;
			}


			if (_weak.Target != null && _weak.IsAlive)
			{
				ILog logger = (this.Owner as ILog);
				IHubProxy proxy = (_weak.Target as IHubProxy);
				try
				{
					proxy.On(txtCallbackName.Text, new Action<dynamic>((d) =>
					{
						Task.Factory.StartNew(() =>
						{
							logger.LogMessage("On " + txtCallbackName.Text + Environment.NewLine);
							if (d != null)
								logger.LogMessage(d.ToString() + Environment.NewLine);
						}, CancellationToken.None, TaskCreationOptions.None, scheduler);
					}));
				}
				catch (AggregateException ex)
				{
					logger.LogMessage((Owner as MainWindow).GetStringFromAggregateException(ex));
				}
			}
		}
	}
}

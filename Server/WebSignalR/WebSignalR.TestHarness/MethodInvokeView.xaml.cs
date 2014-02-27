using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	internal class ValueContainer
	{
		public Object Value { get; set; }
	}

	/// <summary>
	/// Interaction logic for MethodInvokeView.xaml
	/// </summary>
	public partial class MethodInvokeView : Window
	{
		WeakReference _weak;
		ObservableCollection<ValueContainer> parameters;

		public MethodInvokeView(IHubProxy proxy)
		{
			InitializeComponent();
			_weak = new WeakReference(proxy);
			this.Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			parameters = new ObservableCollection<ValueContainer>();
			dataGridParams.ItemsSource = parameters;
		}

		private void btnInvoke_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(txtMethod.Text))
			{
				MessageBox.Show(this, "Method name cannot be empty.");
				return;
			}
			else if (!_weak.IsAlive)
				return;
			IHubProxy proxy = _weak.Target as IHubProxy;
			if (proxy == null) return;

			proxy.Invoke(txtMethod.Text, parameters.Select(x => x.Value).ToArray());
		}
	}
}

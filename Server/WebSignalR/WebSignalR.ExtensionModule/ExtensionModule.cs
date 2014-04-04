using System;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.ExtensionModule
{
	public class MyExtensionModule : IModule
	{
		public string Title
		{
			get { return "Simple Web API Plugin"; }
		}

		public string Name
		{
			get { return System.Reflection.Assembly.GetAssembly(GetType()).GetName().Name; }
		}

		public Version Version
		{
			get { return new Version(1, 0, 0, 0); }
		}

		public string EntryControllerName
		{
			get { return "DataController"; }
		}
	}
}
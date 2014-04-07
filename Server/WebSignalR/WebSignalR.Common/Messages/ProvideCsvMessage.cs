using System;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Messages
{
	public class ProvideCsvMessage : IBroadcastMessage
	{
		public int RoomId
		{
			get;
			set;
		}

		public Guid OutputId
		{
			get;
			set;
		}

		public string Issuer { get; set; }

		public CsvReadyState State
		{
			get;
			set;
		}

		public int UserId
		{
			get;
			set;
		}
	}

	public enum CsvReadyState : int
	{
		Init = 0,
		Processing,
		Collecting,
		Ready,

		Faulted = 10
	}
}
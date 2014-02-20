using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Interfaces
{
	public interface IKeyProvider
	{
		byte[] EncryptionKey { get; }
		byte[] VerificationKey { get; }
	}
}
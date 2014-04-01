using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Test
{
	[TestClass]
	public class CryptoTest
	{
		[TestMethod]
		public void TestICrypto()
		{
			ICrypto crypto = new WebSignalR.Infrastructure.CommonCryptoHelper();
			string data = "<Constr data=\"key1\" val=\"12\" />";
			string encrypted = crypto.Encrypt(data);
			var decrypted = crypto.Decrypt(encrypted);
			StringAssert.StartsWith(data, decrypted);
		}
	}
}

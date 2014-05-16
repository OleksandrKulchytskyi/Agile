using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSignalR.Common.Interfaces;
using System.Text.RegularExpressions;
using System.IO;

namespace WebSignalR.Test
{
	[TestClass]
	public class CryptoTest
	{
		[TestMethod]
		public void TestICrypto()
		{
			string enc = "j1L1z5DjJ6oRNJq4TWPueuggucH8NYqnMaBdzmCpVZdwbB33R9szOBI/Udjz0WQDTymP8Z4MsBRG7TeXnJLk1vC2Fj8z5LYc+NXh0iZ2YeUiEfzGoy3XKiZsTKdgSTNBp5Eo9y8AHslT80IFTyVPsmJclD+YBEXD";
			ICrypto crypto = new WebSignalR.Infrastructure.CommonCryptoHelper();
			string data = "<Constr data=\"key1\" val=\"12\" />";
			string encrypted = crypto.Encrypt(data);
			var decrypted = crypto.Decrypt(encrypted);
			StringAssert.StartsWith(data, decrypted);
		}

		[TestMethod]
		public void DeleteInvalidChars()
		{
			Regex illegalInFileName = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);
			string myString = @"A\\B/C:D?E*F""G<H>I|";
			myString = illegalInFileName.Replace(myString, "");
		}
	}
}

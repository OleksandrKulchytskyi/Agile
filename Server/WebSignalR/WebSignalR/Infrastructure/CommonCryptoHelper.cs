using System;
using System.IO;
using System.Security.Cryptography;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Infrastructure
{
	public sealed class CommonCryptoHelper : ICrypto
	{
		private static Byte[] KEY_64 = System.Text.ASCIIEncoding.ASCII.GetBytes("39393939");
		private static Byte[] IV_64 = System.Text.ASCIIEncoding.ASCII.GetBytes("38383838");

		// returns DES encrypted string
		public string Encrypt(string palinText)
		{
			using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
			{
				using (MemoryStream ms = new MemoryStream())
				{
					CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
					StreamWriter sw = new StreamWriter(cs);

					sw.Write(palinText);
					sw.Flush();
					cs.FlushFinalBlock();
					ms.Flush();

					// convert back to a string
					return Convert.ToBase64String(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
				}
			}
		}

		// returns DES decrypted string
		public string Decrypt(string payload)
		{
			payload = payload.Replace(" ", "+");

			while (payload.Length % 4 != 0)
			{
				payload += "=";
			}
			using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
			{
				Byte[] buffer = Convert.FromBase64String(payload);
				using (MemoryStream ms = new MemoryStream(buffer))
				{
					CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read);
					StreamReader sr = new StreamReader(cs);

					return sr.ReadToEnd();
				}
			}
		}
	}
}
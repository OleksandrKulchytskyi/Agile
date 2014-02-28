using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Hosting;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Infrastructure
{
	public class FileBasedKeyProvider : IKeyProvider
	{
		private readonly Lazy<KeyCache> _keyCache;

		public FileBasedKeyProvider()
			: this(GetDefaultPath())
		{
		}

		public FileBasedKeyProvider(string path)
		{
			_keyCache = new Lazy<KeyCache>(() => new KeyCache(path));
		}

		public byte[] EncryptionKey
		{
			get { return _keyCache.Value.EncryptionKey; }
		}

		public byte[] VerificationKey
		{
			get { return _keyCache.Value.ValidationKey; }
		}

		private static string GetDefaultPath()
		{
			string path = string.Empty;
			try
			{
				path = HostingEnvironment.MapPath("App_Data");
				if (!string.IsNullOrEmpty(path))
					return path;

				path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				path = Path.Combine(path, "Secure");
				Directory.CreateDirectory(path);
			}
			catch { }
			return path;
		}

		private class KeyCache
		{
			public KeyCache(string path)
			{
				Initialize(path);
			}

			private void Initialize(string path)
			{
				string keyFile = Path.Combine(path, "keyfile");

				if (File.Exists(keyFile))
				{
					string[] lines = File.ReadAllLines(keyFile);

					if (lines.Length == 2 &&
						!string.IsNullOrEmpty(lines[0]) &&
						!string.IsNullOrEmpty(lines[1]))
					{
						try
						{
							EncryptionKey = CryptoHelper.FromHex(lines[0]);
							ValidationKey = CryptoHelper.FromHex(lines[1]);
						}
						catch
						{
							// If we failed to read the file for some reason just swallow the exception
						}
					}
				}

				if (EncryptionKey == null || ValidationKey == null)
				{
					EncryptionKey = GenerateRandomKey();
					ValidationKey = GenerateRandomKey();

					File.WriteAllLines(keyFile, new[] { CryptoHelper.ToHex(EncryptionKey), CryptoHelper.ToHex(ValidationKey) });
				}
			}

			public byte[] EncryptionKey { get; set; }

			public byte[] ValidationKey { get; set; }

			/// <summary>
			/// Generates a 256 bit random key
			/// </summary>
			private static byte[] GenerateRandomKey()
			{
				using (var crypto = new RNGCryptoServiceProvider())
				{
					var buffer = new byte[32];
					crypto.GetBytes(buffer);
					return buffer;
				}
			}
		}
	}
}
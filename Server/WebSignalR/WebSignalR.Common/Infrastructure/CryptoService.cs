using System;
using System.Security.Cryptography;
using WebSignalR.Common.Interfaces;

namespace WebSignalR.Common.Infrastructure
{
	public sealed class CryptoService : ICryptoService
	{
		private IKeyProvider _provider;

		public CryptoService(IKeyProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider parameter cannot ba a null.");
			_provider = provider;
		}

		public string CreateSalt()
		{
			var data = new byte[0x10];

			using (var crypto = new RNGCryptoServiceProvider())
			{
				crypto.GetBytes(data);
				return Convert.ToBase64String(data);
			}
		}

		public byte[] Protect(byte[] plainText)
		{
			var initializationVector = new byte[16];
			using (var crypto = new RNGCryptoServiceProvider())
			{
				crypto.GetBytes(initializationVector);
				return CryptoHelper.Protect(_provider.EncryptionKey, _provider.VerificationKey, initializationVector, plainText);
			}
		}

		public byte[] Unprotect(byte[] payload)
		{
			return CryptoHelper.Unprotect(_provider.EncryptionKey, _provider.VerificationKey, payload);
		}
	}
}
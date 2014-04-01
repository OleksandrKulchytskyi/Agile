namespace WebSignalR.Common.Interfaces
{
	public interface ICryptoService
	{
		string CreateSalt();

		//was extended
		byte[] Protect(byte[] plainText);

		byte[] Unprotect(byte[] payload);
	}

	public interface ICrypto
	{
		string Encrypt(string plainText);
		string Decrypt(string payload);
	}
}
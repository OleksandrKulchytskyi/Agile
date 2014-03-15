namespace WebSignalR.Common.Interfaces
{
	public interface ICryptoService
	{
		string CreateSalt();

		//was extended
		byte[] Protect(byte[] plainText);

		byte[] Unprotect(byte[] payload);
	}
}
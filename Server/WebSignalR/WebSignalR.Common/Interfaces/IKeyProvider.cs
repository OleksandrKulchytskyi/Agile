namespace WebSignalR.Common.Interfaces
{
	public interface IKeyProvider
	{
		byte[] EncryptionKey { get; }

		byte[] VerificationKey { get; }
	}
}
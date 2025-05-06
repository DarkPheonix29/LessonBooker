namespace LBCore.Interfaces
{
	public interface IFirebaseKeyRepos
	{
		// Method to generate a registration key asynchronously
		Task<string> GenerateRegistrationKeyAsync();

		// Method to use a registration key asynchronously
		Task<bool> UseRegistrationKeyAsync(string key);

		// Method to fetch all keys from Firestore asynchronously
		Task<List<KeyData>> GetAllKeysAsync();

		// Method to generate a new key (could be a more generic key generation)
		Task<string> GenerateKeyAsync();

		// Method to fetch all keys (optional for administration)
		Task<List<KeyData>> FetchKeysAsync();

		// Method to mark a key as used
		Task<bool> MarkKeyAsUsedAsync(string key);
	}
}

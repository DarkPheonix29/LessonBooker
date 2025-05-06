
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public static class FirebaseConfig
{
	private static bool _isInitialized = false;

	public static void InitializeFirebase()
	{
		if (_isInitialized)
			return;

		var firebaseCredentialPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIAL_PATH");

		if (string.IsNullOrWhiteSpace(firebaseCredentialPath) || !File.Exists(firebaseCredentialPath))
			throw new FileNotFoundException("Firebase service account key file not found.");

		FirebaseApp.Create(new AppOptions()
		{
			Credential = GoogleCredential.FromFile(firebaseCredentialPath)
		});

		_isInitialized = true;
	}
}

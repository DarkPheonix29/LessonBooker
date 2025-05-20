using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCore
{
	public static class FirestoreFactory
	{
		private static FirestoreDb _firestoreDb;

		public static FirestoreDb GetFirestoreDb()
		{
			if (_firestoreDb != null)
			{
				return _firestoreDb;
			}

			var path = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIAL_PATH");
			var credential = GoogleCredential.FromFile(path);

			_firestoreDb = FirestoreDb.Create("lessonbooker-8664a", new FirestoreClientBuilder
			{
				Credential = credential
			}.Build());

			return _firestoreDb;
		}
	}
}

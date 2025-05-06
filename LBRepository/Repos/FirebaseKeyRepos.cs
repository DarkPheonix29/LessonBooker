using Google.Cloud.Firestore;
using LBCore;
using LBCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Firebase
{
	public class FirebaseKeyRepos : IFirebaseKeyRepos
	{
		private readonly FirestoreDb _db;

		public FirebaseKeyRepos()
		{
			_db = FirestoreFactory.GetFirestoreDb();
		}

		public async Task<string> GenerateRegistrationKeyAsync()
		{
			var key = Guid.NewGuid().ToString();
			var keyRef = _db.Collection("registrationKeys").Document(key);

			var keyDoc = new Dictionary<string, object>
			{
				{ "key", key },
				{ "used", false },
				{ "createdAt", Timestamp.GetCurrentTimestamp() }
			};

			await keyRef.SetAsync(keyDoc);
			return key;
		}

		public async Task<bool> UseRegistrationKeyAsync(string key)
		{
			var keyRef = _db.Collection("registrationKeys").Document(key);
			var keyDoc = await keyRef.GetSnapshotAsync();

			if (keyDoc.Exists && !keyDoc.GetValue<bool>("used"))
			{
				await keyRef.UpdateAsync(new Dictionary<string, object> { { "used", true } });
				return true;
			}

			return false;
		}

		public async Task<List<KeyData>> GetAllKeysAsync()
		{
			var keys = new List<KeyData>();
			var snapshot = await _db.Collection("registrationKeys").GetSnapshotAsync();

			foreach (var document in snapshot.Documents)
			{
				var keyData = document.ConvertTo<KeyData>();
				keyData.Id = document.Id;
				keys.Add(keyData);
			}

			return keys;
		}

		public async Task<string> GenerateKeyAsync()
		{
			var newKey = new
			{
				Key = Guid.NewGuid().ToString(),
				Used = false,
				CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
			};

			var docRef = _db.Collection("registrationKeys").Document();
			await docRef.SetAsync(newKey);

			return newKey.Key;
		}

		public async Task<List<KeyData>> FetchKeysAsync()
		{
			var keys = new List<KeyData>();
			var snapshot = await _db.Collection("registrationKeys").GetSnapshotAsync();

			foreach (var document in snapshot.Documents)
			{
				var keyData = document.ConvertTo<KeyData>();
				keyData.Id = document.Id;
				keys.Add(keyData);
			}

			return keys;
		}

		public async Task<bool> MarkKeyAsUsedAsync(string key)
		{
			var query = _db.Collection("registrationKeys").WhereEqualTo("Key", key);
			var snapshot = await query.GetSnapshotAsync();

			if (snapshot.Documents.Count == 0)
				return false;

			var document = snapshot.Documents.First();
			await document.Reference.UpdateAsync("Used", true);

			return true;
		}
	}
}

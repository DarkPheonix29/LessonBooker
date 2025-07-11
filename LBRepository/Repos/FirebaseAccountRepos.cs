﻿using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using LBCore;
using LBCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LBRepository.Repos
{
	// Custom exception for Firebase account operations
	public class FirebaseAccountException : Exception
	{
		public FirebaseAccountException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class FirebaseAccountRepos : IFirebaseAccountRepos
	{
		private readonly FirebaseAuth _auth;
		private readonly FirestoreDb _firestoreDb;

		public FirebaseAccountRepos(FirebaseAuth auth)
		{
			_auth = auth ?? throw new ArgumentNullException(nameof(auth));
			_firestoreDb = FirestoreFactory.GetFirestoreDb();
		}

		public async Task LogoutUserAsync(string uid)
		{
			try
			{
				await _auth.RevokeRefreshTokensAsync(uid);
			}
			catch (Exception ex)
			{
				throw new FirebaseAccountException("Error revoking tokens.", ex);
			}
		}

		public async Task AssignRoleAsync(string userId, string role)
		{
			var customClaims = new Dictionary<string, object> { { "role", role } };
			await _auth.SetCustomUserClaimsAsync(userId, customClaims);
		}

		public async Task<UserRecord> SignUpAsync(string email, string password, string role)
		{
			try
			{
				var user = await _auth.CreateUserAsync(new UserRecordArgs
				{
					Email = email,
					Password = password
				});

				await AssignRoleAsync(user.Uid, role);

				var userDoc = _firestoreDb.Collection("users").Document(user.Uid);
				await userDoc.SetAsync(new Dictionary<string, object>
				{
					{ "email", email },
					{ "role", role }
				});

				return user;
			}
			catch (Exception ex)
			{
				throw new FirebaseAccountException("Error during sign-up.", ex);
			}
		}

		public async Task<string> LoginAsync(string idToken)
		{
			try
			{
				var decodedToken = await _auth.VerifyIdTokenAsync(idToken);
				return decodedToken.Uid;
			}
			catch (Exception ex)
			{
				throw new FirebaseAccountException("Error during log-in.", ex);
			}
		}
		public async Task<string?> GetUserRoleAsync(string uid)
		{
			try
			{
				var userDoc = await _firestoreDb.Collection("users").Document(uid).GetSnapshotAsync();
				if (!userDoc.Exists)
					return null;

				if (userDoc.ContainsField("role"))
					return userDoc.GetValue<string>("role");

				return null;
			}
			catch (Exception ex)
			{
				throw new FirebaseAccountException("Error fetching user role.", ex);
			}
		}
		public async Task DeleteUserAsync(string uid)
		{
			try
			{
				// Delete from Firebase Authentication
				await _auth.DeleteUserAsync(uid);

				// Delete from Firestore "users" collection
				await _firestoreDb.Collection("users").Document(uid).DeleteAsync();
			}
			catch (Exception ex)
			{
				throw new FirebaseAccountException("Error deleting user.", ex);
			}
		}
	}
}

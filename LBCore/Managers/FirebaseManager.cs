using FirebaseAdmin.Auth;
using LBCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCore.Managers
{
	public class FirebaseManager
	{
		private readonly IFirebaseAccountRepos _fireAccount;

		public FirebaseManager(IFirebaseAccountRepos fireAccount)
		{
			_fireAccount = fireAccount;
		}
		
		public async Task<UserRecord> SignUpAsync(string email, string password, string role)
		{
			// Create the user and assign the role.
			return await _fireAccount.SignUpAsync(email, password, role);
		}

		public async Task<string> LoginAsync(string idToken)
		{
			// This method verifies the token and returns the UID.
			return await _fireAccount.LoginAsync(idToken);
		}

		public async Task LogoutAsync(string uid)
		{
			await _fireAccount.LogoutUserAsync(uid);
		}

		public async Task<bool> VerifyTokenAsync(string idToken)
		{
			try
			{
				// Re-use the login method as token verification
				var uid = await _fireAccount.LoginAsync(idToken);
				return !string.IsNullOrEmpty(uid);
			}
			catch (Exception)
			{
				return false;
			}
		} 
	}
}

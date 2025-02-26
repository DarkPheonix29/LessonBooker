using FirebaseAdmin.Auth;
using LBCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBRepository.Repos
{
    public class FirebaseAccountRepos : IFirebaseAccountRepos
    {
		private readonly FirebaseAuth _auth;

		public FirebaseAccountRepos(FirebaseAuth auth)
		{
			_auth = auth;
		}

		public async Task LogoutUserAsync(string uid)
		{
			try
			{
				await _auth.RevokeRefreshTokensAsync(uid);
			}
			catch (Exception ex)
			{
				throw new Exception("Error revoking tokens: " + ex.Message);
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
				var userRecordArgs = new UserRecordArgs
				{
					Email = email,
					Password = password
				};
				var user = await _auth.CreateUserAsync(userRecordArgs);
				await AssignRoleAsync(user.Uid, role);
				return user;
			}
			catch (Exception ex)
			{
				throw new Exception("Error during sign-up: " + ex.Message);
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
				throw new Exception("Error during log-in: " + ex.Message);
			}
		}
	}
}

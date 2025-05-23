using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCore.Interfaces
{
    public interface IFirebaseAccountRepos
    {
		Task LogoutUserAsync(string uid);
		Task AssignRoleAsync(string userId, string role);
		Task<UserRecord> SignUpAsync(string email, string password, string role);
		Task<string> LoginAsync(string idToken);
		Task<string?> GetUserRoleAsync(string uid);
		Task DeleteUserAsync(string uid);

	}
}

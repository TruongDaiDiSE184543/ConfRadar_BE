using FirebaseAdmin.Auth;

namespace ConfRadar.Services.Services
{
    public interface IFirebaseAuthService
    {
        Task<FirebaseToken?> VerifyIdTokenAsync(string idToken);
    }

    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuth _firebaseAuth;
        public FirebaseAuthService()
        {
            _firebaseAuth = FirebaseAuth.DefaultInstance;
        }
        public async Task<FirebaseToken?> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                return await _firebaseAuth.VerifyIdTokenAsync(idToken);
            }
            catch (FirebaseAuthException ex)
            {
                throw;
            }
        }
    }
}

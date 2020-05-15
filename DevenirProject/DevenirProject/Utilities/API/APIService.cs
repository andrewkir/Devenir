using System.Threading.Tasks;
using Refit;

namespace DevenirProject.Utilities.API
{
    public interface ApiService
    {
        [Post("/latex")]
        [Headers("Content-Type: application/json")]
        Task<ApiResponse<string>> GetImageProcess([Body]string src, [Header("Authorization")] string access_token, [Header("Device-id")] string deviceId);

        [Post("/createtoken")]
        [Headers("Content-Type: application/json")]
        Task<ApiResponse<string>> CreateAccessToken([Body]string body);

        [Post("/getnonce")]
        [Headers("Content-Type: application/json")]
        Task<ApiResponse<string>> GetNonce([Body]string body);

        [Post("/refresh")]
        [Headers("Content-Type: application/json")]
        Task<ApiResponse<string>> RefreshToken([Header("Authorization")] string refresh_token, [Header("Device-id")] string deviceId);
    }
}
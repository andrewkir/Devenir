using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Json;
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
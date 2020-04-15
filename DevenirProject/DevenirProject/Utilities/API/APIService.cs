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
    public interface APIService
    {
        [Post("/v3/text")]
        [Headers("Content-Type: application/json")]
        Task<ApiResponse<string>> GetImageProcess([Body]string src, [Header("app_key")] string appKey, [Header("app_id")] string appId);
    }
}
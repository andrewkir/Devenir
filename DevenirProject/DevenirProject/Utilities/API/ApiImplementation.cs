using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Apache.Http;
using Org.Json;
using Refit;
using static Android.Provider.Settings;

namespace DevenirProject.Utilities.API
{
    public class ApiImplementation
    {
        const string PATH = "https://devenir.andrewkir.ru";

        public delegate void RequestResult(bool result, ApiResponse<string> response);
        event RequestResult RequestResultEvent;

        string deviceId;

        public async void LatexRecogniseRequestAsync(JSONObject body, Activity activity)
        {
            try
            {
                deviceId = Secure.GetString(activity.ContentResolver, Secure.AndroidId);

                ApiResponse<string> response = null;
                var api = RestService.For<APIService>(PATH);
                string access_token = SharedPrefsManager.GetTokens().access_token;
                Toast.MakeText(Application.Context, $"Токен не пустой? {access_token == ""}", ToastLength.Short).Show();
                if (access_token != null && access_token != "")
                {
                    Toast.MakeText(Application.Context, access_token, ToastLength.Short);
                    response = await api.GetImageProcess(body.ToString(), "Bearer " + access_token, deviceId);
                    if (response == null)
                    {
                        Toast.MakeText(Application.Context, "Отсутствует подключение к интернету!", ToastLength.Short).Show();
                        RequestResultEvent?.Invoke(false, null);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        Toast.MakeText(Application.Context, "Обновление токенов", ToastLength.Short).Show();
                        await RefreshTokenAsync(activity);
                    }
                    else
                    {
                        RequestResultEvent?.Invoke(true, response);
                    }
                }
                else
                {
                    await RefreshTokenAsync(activity);
                }
            }
            catch (HttpRequestException)
            {
                Toast.MakeText(Application.Context, $"Отсутствует подключение к сервису", ToastLength.Short).Show();
                RequestResultEvent?.Invoke(false, null);
                return;
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, $"Произошла непредвиденная ошибка " + ex.Message, ToastLength.Short).Show();
                RequestResultEvent?.Invoke(false, null);
                return;
            }
        }

        private async System.Threading.Tasks.Task RefreshTokenAsync(Activity activity)
        {
            try
            {
                var api = RestService.For<APIService>(PATH);
                string refresh_token = SharedPrefsManager.GetTokens().refresh_token;
                bool success = false;
                if (refresh_token != null && refresh_token != "")
                {
                    var response = await api.RefreshToken("Bearer " + refresh_token, deviceId);
                    if (response == null)
                    {
                        Toast.MakeText(Application.Context, "Отсутствует подключение к интернету!", ToastLength.Short).Show();
                        return;
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        JSONObject resp = new JSONObject(response.Content);
                        try
                        {
                            string access_token = resp.GetString("access_token");
                            string refresh_token_new = resp.GetString("refresh_token");
                            SharedPrefsManager.SaveTokens(access_token, null);
                            SharedPrefsManager.SaveTokens(refresh_token_new, null);
                            success = true;
                            Toast.MakeText(Application.Context, "Успешное обновление access_token и refresh_token", ToastLength.Short).Show();
                        }
                        catch (JSONException)
                        {
                            Toast.MakeText(Application.Context, "Ошибка сервера! Попробуйте повторить позже", ToastLength.Short).Show();
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        Toast.MakeText(Application.Context, "Переаттестация! Истёк refresh token", ToastLength.Short).Show();
                        var attest = new ApiAttestation();
                        attest.AddOnResultListener(delegate (bool res)
                        {
                            if (res) RequestResultEvent?.Invoke(res, null);
                        });
                        await attest.AttestateAsync(activity);
                    }
                }
                else
                {
                    Toast.MakeText(Application.Context, "Переаттестация! Пустой refresh token", ToastLength.Short).Show();
                    var attest = new ApiAttestation();
                    attest.AddOnResultListener(delegate (bool res)
                    {
                        if (res) RequestResultEvent?.Invoke(res, null);
                    });
                    await attest.AttestateAsync(activity);
                }
            }
            catch (HttpRequestException)
            {
                Toast.MakeText(Application.Context, $"Отсутствует подключение к сервису", ToastLength.Short).Show();
                RequestResultEvent?.Invoke(false, null);
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, $"Произошла непредвиденная ошибка " + ex.Message, ToastLength.Short).Show();
                RequestResultEvent?.Invoke(false, null);
            }
        }

        public void AddOnRequestResultListener(RequestResult requestResult)
        {
            RequestResultEvent += requestResult;
        }
    }
}
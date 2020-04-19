using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.SafetyNet;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Json;
using Refit;
using static Android.Provider.Settings;

namespace DevenirProject.Utilities.API
{
    class ApiAttestation
    {

        string timestamp;
        string device_id;

        public delegate void AttestationResult(bool res);
        event AttestationResult AttestationResultEvent;

        public async System.Threading.Tasks.Task AttestateAsync(Activity activity)
        {
            try
            {
                var api = RestService.For<APIService>("https://devenir.andrewkir.ru");
                JSONObject obj = new JSONObject();

                timestamp = DateTime.Now.Ticks.ToString();
                device_id = Secure.GetString(activity.ContentResolver, Secure.AndroidId);

                obj.Put("timestamp", timestamp);
                obj.Put("device_id", device_id);


                var response = await api.GetNonce(obj.ToString());
                if (response == null)
                {
                    Toast.MakeText(Application.Context, "Отсутствует подключение к интернету!", ToastLength.Short).Show();
                    return;
                }


                if (response.IsSuccessStatusCode)
                {
                    string nonce = response.Content;

                    if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context) == ConnectionResult.Success)
                    {
                        // The SafetyNet Attestation API is available.
                        SafetyNetClient client = SafetyNetClass.GetClient(activity);
                        var task = client.Attest(Encoding.ASCII.GetBytes(nonce), activity.Resources.GetString(Resource.String.api_safetyNetKey))
                                         .AddOnSuccessListener(activity, new OnSuccessListener(activity, timestamp, this))
                                         .AddOnFailureListener(activity, new OnFailureListener(this));
                    }
                    else
                    {
                        // Prompt user to update Google Play services.
                        Toast.MakeText(Application.Context, "Вы должны обновить Google Play сервисы!", ToastLength.Short).Show();
                        AttestationResultEvent?.Invoke(false);
                    }
                    return;
                }
                else
                {
                    Toast.MakeText(activity, "att " + response.Error.Content, ToastLength.Long).Show();
                    AttestationResultEvent?.Invoke(false);
                    return;
                }
            }
            catch (HttpRequestException)
            {
                Toast.MakeText(Application.Context, $"Отсутствует подключение к сервису", ToastLength.Short).Show();
                AttestationResultEvent?.Invoke(false);
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, $"Произошла непредвиденная ошибка " + ex.Message, ToastLength.Short).Show();
                AttestationResultEvent?.Invoke(false);
            }
        }

        public void AddOnResultListener(AttestationResult result)
        {
            if (result != null) AttestationResultEvent += result;
        }

        private async System.Threading.Tasks.Task CheckResultsAsync(string res, string timestamp, Activity activity)
        {
            try
            {
                var api = RestService.For<APIService>("https://devenir.andrewkir.ru");
                JSONObject obj = new JSONObject();
                obj.Put("safetynet", res);
                obj.Put("timestamp", timestamp);
                obj.Put("device_id", Secure.GetString(activity.ContentResolver, Secure.AndroidId));

                var response = await api.CreateAccessToken(obj.ToString());
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
                        string refrest_token = resp.GetString("refresh_token");
                        SharedPrefsManager.SaveTokens(access_token, refrest_token);
                        AttestationResultEvent?.Invoke(true);
                        Toast.MakeText(Application.Context, "Attestated", ToastLength.Short).Show();
                    }
                    catch (JSONException)
                    {
                        Toast.MakeText(Application.Context, "Ошибка сервера! Попробуйте повторить позже", ToastLength.Short).Show();
                        AttestationResultEvent?.Invoke(false);
                    }
                }
                else
                {
                    Toast.MakeText(Application.Context, $"Error response {response.Content.ToString()}", ToastLength.Long).Show();
                    AttestationResultEvent?.Invoke(false);
                }
            }
            catch (HttpRequestException)
            {
                Toast.MakeText(Application.Context, $"Отсутствует подключение к сервису", ToastLength.Short).Show();
                AttestationResultEvent?.Invoke(false);
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, $"Произошла непредвиденная ошибка " + ex.Message, ToastLength.Short).Show();
                AttestationResultEvent?.Invoke(false);
            }
        }


        public class OnFailureListener : Java.Lang.Object, IOnFailureListener
        {
            ApiAttestation sender;
            public OnFailureListener(ApiAttestation sender) { this.sender = sender; }

            public void OnFailure(Java.Lang.Exception e)
            {
                sender.AttestationResultEvent?.Invoke(false);
                Toast.MakeText(Application.Context, $"Во время проверки приложения возникла ошибка", ToastLength.Long).Show();
            }
        }

        public class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            Activity activity;
            string timestamp;
            ApiAttestation sender;

            public OnSuccessListener(Activity activity, string timestamp, ApiAttestation sender)
            {
                this.activity = activity;
                this.timestamp = timestamp;
                this.sender = sender;
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                var res = (result as SafetyNetApiAttestationResponse).JwsResult.ToString();
                System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(() => sender.CheckResultsAsync(res, timestamp, activity).RunSynchronously());
                task.RunSynchronously();
            }
        }
    }
}
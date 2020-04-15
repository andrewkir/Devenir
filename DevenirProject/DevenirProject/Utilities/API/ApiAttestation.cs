using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DevenirProject.Utilities.API
{
    static class ApiAttestation {
        public static void Attestate(Activity activity)
        {
            if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context) == ConnectionResult.Success)
            {
                // The SafetyNet Attestation API is available.
                Toast.MakeText(Application.Context, "Yep, all right", ToastLength.Short).Show();
                SafetyNetClient client = SafetyNetClass.GetClient(activity);
                var task = client.Attest(new byte[] { 0, 1, 2 }, activity.Resources.GetString(Resource.String.api_safetyNetKey))
                                 .AddOnSuccessListener(activity, new OnSuccessListener(activity))
                                 .AddOnFailureListener(activity, new OnFailureListener(activity));
            }
            else
            {
                // Prompt user to update Google Play services.
            }
        }
        public class OnFailureListener : Java.Lang.Object, IOnFailureListener
        {
            Activity activity;
            public OnFailureListener(Activity activity) { this.activity = activity; }

            public void OnFailure(Java.Lang.Exception e)
            {
                Toast.MakeText(activity, $"Nah! {e.Message}", ToastLength.Long).Show();
            }
        }

        public class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            Activity activity;
            public OnSuccessListener(Activity activity) { this.activity = activity; }

            public void OnSuccess(Java.Lang.Object result)
            {
                Toast.MakeText(activity, "Yesss!", ToastLength.Long).Show();
            }
        }
    }
}
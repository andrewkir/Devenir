using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Firebase.ML.Vision;
using Firebase.ML.Vision.Common;
using Firebase;
using Firebase.ML.Vision.Text;
using Android;
using Plugin.Media;
using Android.Graphics;
using Android.Gms.Tasks;
using System;
using Android.Util;
using System.Collections.Generic;
using Firebase.ML.Vision.Document;
using System.Threading.Tasks;
using DevenirProject.WebService;
using DevenirProject.ImageUtils;
using DevenirProject.Utilities.API;
using Refit;
using Org.Json;
using Android.Gms.Common;
using Android.Gms.SafetyNet;
using Android.Support.V4.Widget;

namespace DevenirProject
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button TakePhotoButton;
        Button ProcessImageButton;
        Button PickImageButton;
        Button AttestateButton;
        ImageView imageview;
        Bitmap photoResult;
        TextView text;
        SwipeRefreshLayout swipeLayout;

        FirebaseImageService firebaseImageService = new FirebaseImageService();
        ImageManager imageManager = new ImageManager();
        LatexService latexService;

        readonly string[] permissions =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            RequestPermissions(permissions, 0);

            latexService = new LatexService(this);

            TakePhotoButton = FindViewById<Button>(Resource.Id.TakePhotoButton);
            ProcessImageButton = FindViewById<Button>(Resource.Id.ProcessImageButton);
            PickImageButton = FindViewById<Button>(Resource.Id.PickImageButton);
            AttestateButton = FindViewById<Button>(Resource.Id.AttestateButton);

            swipeLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.refreshLayout);
            swipeLayout.Enabled = false;

            imageview = FindViewById<ImageView>(Resource.Id.imageView1);
            text = FindViewById<TextView>(Resource.Id.textView1);


            imageManager.AddOnImageResultListener(delegate (Bitmap bitmap, Exception ex)
            {
                if (ex == null)
                {
                    imageview.SetImageBitmap(bitmap);
                    photoResult = bitmap;
                }
                else Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            });

            firebaseImageService.AddImageResultListener(delegate (string text, string ex)
            {
                if (ex == null) Toast.MakeText(Application.Context, text, ToastLength.Short).Show();
                else Toast.MakeText(Application.Context, ex, ToastLength.Short).Show();
            });


            latexService.AddOnLatexResultListener(delegate (string res, string ex)
            {
                swipeLayout.Refreshing = false;
                if (res != null)
                {
                    var jsonRes = new JSONObject(res);
                    FindViewById<EditText>(Resource.Id.textInputEditText1).Text = jsonRes.Get("latex_styled").ToString();
                }
                else Toast.MakeText(Application.Context, ex, ToastLength.Long).Show();
            });




            TakePhotoButton.Click += delegate
            {
                imageManager.TakePhoto();
            };

            PickImageButton.Click += delegate
            {
                imageManager.PickPhoto();
            };

            AttestateButton.Click += delegate
            {
                var attest = new ApiAttestation();
                attest.AttestateAsync(this);
            };

            ProcessImageButton.Click += delegate
            {
                if (photoResult != null)
                {
                    swipeLayout.Refreshing = true;
                    firebaseImageService.ProcessImage(photoResult);
                    latexService.ProcessImage(photoResult);
                }
                else
                {
                    Toast.MakeText(Application.Context, "Сначала необходимо сделать фотографию", ToastLength.Short).Show();
                }
            };

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
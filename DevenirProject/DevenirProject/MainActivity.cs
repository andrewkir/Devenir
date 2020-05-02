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
using DevenirProject.Views;
using Android.Content;
using Com.Google.Android.Cameraview;

namespace DevenirProject
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Activity
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
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.AppTheme);
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            if (Build.VERSION.SdkInt > Build.VERSION_CODES.LollipopMr1) RequestPermissions(permissions, 0);

            latexService = new LatexService(this);

            TakePhotoButton = FindViewById<Button>(Resource.Id.TakePhotoButton);
            ProcessImageButton = FindViewById<Button>(Resource.Id.ProcessImageButton);
            PickImageButton = FindViewById<Button>(Resource.Id.PickImageButton);
            AttestateButton = FindViewById<Button>(Resource.Id.AttestateButton);

            swipeLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.refreshLayout);
            swipeLayout.Enabled = false;

            imageview = FindViewById<ImageView>(Resource.Id.imageView1);
            text = FindViewById<TextView>(Resource.Id.textView1);


            imageManager.AddOnImageResultListener(delegate (Bitmap bitmap, string path, Exception ex)
            {
                if (ex == null)
                {
                    imageview.SetImageBitmap(bitmap);
                    photoResult = bitmap;
                    Intent intent = new Intent(this, typeof(MainViewActivity));
                    intent.PutExtra("image", path);
                    StartActivity(intent);
                }
                else Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            });

            firebaseImageService.AddImageResultListener(delegate (string[] text, string[] ex)
            {
                swipeLayout.Refreshing = false;
                if (ex == null)
                {
                    foreach (var detectionResult in text)
                    {
                        Toast.MakeText(Application.Context, detectionResult, ToastLength.Short).Show();
                    }
                }
                else if (text != null) Toast.MakeText(Application.Context, ex[0], ToastLength.Short).Show();
                else Toast.MakeText(Application.Context, "null", ToastLength.Short).Show();
            });


            latexService.AddOnLatexResultListener(delegate (string[] res, string[] ex)
            {
                swipeLayout.Refreshing = false;
                if (res != null)
                {
                    foreach (var detectionResult in res)
                    {
                        var jsonRes = new JSONObject(detectionResult);
                        if (jsonRes.Has("latex_styled")) FindViewById<EditText>(Resource.Id.textInputEditText1).Text = jsonRes.Get("latex_styled").ToString();
                        if (jsonRes.Has("text")) FindViewById<EditText>(Resource.Id.textInputEditText1).Text = jsonRes.Get("text").ToString();
                    }
                }
                else Toast.MakeText(Application.Context, ex[0], ToastLength.Long).Show();
            });




            TakePhotoButton.Click += delegate
            {
                //imageManager.TakePhoto();
                Intent intent = new Intent(this, typeof(CameraLayout));
                StartActivity(intent);
            };

            PickImageButton.Click += delegate
            {
                imageManager.PickPhoto();
            };

            AttestateButton.Click += delegate
            {
                FindViewById<CameraView>(Resource.Id.camera).Start();
                var attest = new ApiAttestation();
                attest.AttestateAsync(this);
            };

            ProcessImageButton.Click += delegate
            {
                if (photoResult != null)
                {
                    swipeLayout.Refreshing = true;
                    firebaseImageService.ProcessImages(new Bitmap[] { photoResult });
                    //latexService.ProcessImages(new Bitmap[] { photoResult });
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
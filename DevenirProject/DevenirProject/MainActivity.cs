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

namespace DevenirProject
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button TakePhotoButton;
        Button ProcessImageButton;
        Button PickImageButton;
        ImageView imageview;
        Bitmap photoResult;
        TextView text;

        FirebaseImageService firebaseImageService = new FirebaseImageService(Application.Context);
        ImageManager imageManager = new ImageManager();
        LatexService latexService = new LatexService();

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


            TakePhotoButton = FindViewById<Button>(Resource.Id.TakePhotoButton);
            ProcessImageButton = FindViewById<Button>(Resource.Id.ProcessImageButton);
            PickImageButton = FindViewById<Button>(Resource.Id.PickImageButton);

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

            firebaseImageService.AddImageResultListener(delegate (FirebaseVisionDocumentText text, Exception ex)
            {
                if (ex == null) Toast.MakeText(Application.Context, text.Text.ToString(), ToastLength.Short).Show();
                else Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            });


            latexService.AddOnLatexResultListener(delegate (string res, string ex)
            {
                if (res != null)
                {
                    var jsonRes = new JSONObject(res);
                    Toast.MakeText(Application.Context, jsonRes.ToString(), ToastLength.Long).Show();
                    Toast.MakeText(Application.Context, jsonRes.Get("latex_styled").ToString(), ToastLength.Long).Show();
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

            ProcessImageButton.Click += delegate
            {

                var attest = new ApiAttestation();
                attest.ReturnJwtEvent += delegate (string res)
                {
                    FindViewById<EditText>(Resource.Id.textInputEditText1).Text = res;
                };
                attest.AttestateAsync(this);
                if (false)
                {
                    if (photoResult != null)
                    {
                        firebaseImageService.ProcessImage(photoResult);

                        using (var ms = new System.IO.MemoryStream())
                        {
                            photoResult.Compress(Bitmap.CompressFormat.Jpeg, 0, ms);
                            var res = Base64.EncodeToString(ms.ToArray(), Base64Flags.Default);
                            latexService.ProcessImageAsync("data:image/jpeg;base64," + res, this);
                        }
                    }
                    else
                    {
                        Toast.MakeText(Application.Context, "Сначала необходимо сделать фотографию", ToastLength.Short).Show();
                    }
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
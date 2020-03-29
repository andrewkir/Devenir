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

namespace DevenirProject
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button TakePhotoButton;
        Button ProcessImageButton;
        ImageView imageview;
        Bitmap photoResult;
        TextView text;

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


            imageview = FindViewById<ImageView>(Resource.Id.imageView1);
            text = FindViewById<TextView>(Resource.Id.textView1);

            TakePhotoButton.Click += delegate
            {
                ImageManager imageManager = new ImageManager();
                imageManager.AddOnImageResultListener(delegate (Bitmap bitmap) {
                    imageview.SetImageBitmap(bitmap);
                    photoResult = bitmap;
                });
                imageManager.TakePhoto();
            };

            ProcessImageButton.Click += delegate
            {
                FirebaseImageService firebaseImageService = new FirebaseImageService(Application.Context);
                firebaseImageService.AddImageResultListener(delegate (FirebaseVisionDocumentText text)
                {
                    Toast.MakeText(Application.Context, text.Text.ToString(), ToastLength.Short).Show();
                });

                if (photoResult != null)
                {
                    firebaseImageService.ProcessImage(photoResult);
                } else
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
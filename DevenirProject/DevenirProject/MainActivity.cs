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

namespace DevenirProject
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button btn;
        ImageView imageview;
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
            btn = FindViewById<Button>(Resource.Id.button1);
            imageview = FindViewById<ImageView>(Resource.Id.imageView1);
            text = FindViewById<TextView>(Resource.Id.textView1);

            btn.Click += delegate
            {
                TakePhoto();
            };
            Log.Debug("TESTS", "HELLO THERE");
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void ProcessImage(Bitmap bitmap)
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseVisionTextRecognizer detector = FirebaseVision.GetInstance(app).OnDeviceTextRecognizer;


            FirebaseVisionCloudDocumentRecognizerOptions options = new FirebaseVisionCloudDocumentRecognizerOptions.Builder()
                .SetLanguageHints(new List<String>{ "en", "ru" })
                .Build();
            FirebaseVisionDocumentTextRecognizer det = FirebaseVision.GetInstance(app).GetCloudDocumentTextRecognizer(options);

            FirebaseVisionImage image = FirebaseVisionImage.FromBitmap(bitmap);
            var result = det
                .ProcessImage(image)
                .AddOnCompleteListener(new SigninCompleteListener(text));
        }

        class SigninCompleteListener : Java.Lang.Object, IOnCompleteListener
        {
            TextView text;
            public SigninCompleteListener(TextView text)
            {
                this.text = text;
            }
            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                if (!task.IsSuccessful)
                {
                  
                }
                text.Text = ((FirebaseVisionDocumentText)task.Result).Text;
            }
        }

        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 40,
                Name = "myimage.jpg",
                Directory = "sample"

            });

            if (file == null)
            {
                return;
            }

            // Convert file to byte array and set the resulting bitmap to imageview
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            imageview.SetImageBitmap(bitmap);
            ProcessImage(bitmap);
        }

        
    }
}
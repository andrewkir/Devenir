using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Cameraview;
using Java.IO;

namespace DevenirProject
{
    [Activity(Label = "CameraLayout")]
    public class CameraLayout : Activity
    {
        CameraView camera;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_camera);

            // In Activity's onCreate() for instance
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
            {
                Window w = Window;
                w.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }

            FindViewById<Button>(Resource.Id.takepic).Click += delegate
            {
                camera.TakePicture();
            };
            camera = FindViewById<CameraView>(Resource.Id.cameraView);
            camera.AddCallback(new CameraViewCallback(camera, this));

            // Create your application here
        }

        protected override void OnResume()
        {
            base.OnResume();
            camera.Flash = CameraView.FlashAuto;
            camera.Start();
        }

        protected override void OnPause()
        {
            base.OnPause();
            camera.Stop();
        }

        class CameraViewCallback : CameraView.Callback
        {

            private CameraView mCameraView;
            private Activity activity;

            public CameraViewCallback(CameraView mCameraView, Activity activity)
            {
                this.mCameraView = mCameraView;
                this.activity = activity;
            }

            public override async void OnPictureTaken(CameraView camera, byte[] image)
            {
                var path = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, "Devenir");

                var filename = System.IO.Path.Combine(path, $"DEVENIR_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss.ff")}.jpg");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }

                try
                {
                    FileOutputStream outStream = new FileOutputStream(filename);
                    outStream.Write(image);
                    outStream.Close();

                    var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                    mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(filename)));
                    activity.SendBroadcast(mediaScanIntent);
                    Toast.MakeText(mCameraView.Context, "Pic taken", ToastLength.Short).Show();
                }
                catch (Exception e)
                {
                    Log.Debug("ERROR", e.Message);
                    Toast.MakeText(mCameraView.Context, "Error: "+e.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}
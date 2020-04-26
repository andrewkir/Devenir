using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Cameraview;
using DevenirProject.ImageUtils;
using Java.IO;

namespace DevenirProject
{
    [Activity(Label = "CameraLayout")]
    public class CameraLayout : Activity
    {
        CameraView camera;

        ImageButton toggleFlashButton;
        Button takePictureButton;
        ImageButton openGalleryButton;

        ImageManager imageManager;

        int flashCurrent = 0;
        string path;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.AppThemeClearStatusBar);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_camera);

            toggleFlashButton = FindViewById<ImageButton>(Resource.Id.toggleFlashButton);
            takePictureButton = FindViewById<Button>(Resource.Id.takePictureButton);
            openGalleryButton = FindViewById<ImageButton>(Resource.Id.openGalleryButton);

            camera = FindViewById<CameraView>(Resource.Id.cameraView);
            camera.AddCallback(new CameraViewCallback(camera, this, delegate (string path)
            {
                if (path != null && path != "")
                {
                    Intent intent = new Intent(this, typeof(PhotoCropActivity));
                    intent.PutExtra("image", path);
                    StartActivity(intent);

                    this.path = path;
                }
                else Toast.MakeText(Application.Context, "Ошибка во время сохранения фотографии", ToastLength.Short).Show();
            }));

            imageManager = new ImageManager();
            imageManager.AddOnImageResultListener(delegate (Bitmap bitmap, string path, Exception ex)
            {
                if (ex == null)
                {
                    Intent intent = new Intent(this, typeof(PhotoCropActivity));
                    intent.PutExtra("image", path);
                    StartActivity(intent);
                }
                else Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            });


            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            {
                Window w = Window;
                w.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }

            takePictureButton.Click += delegate
            {
                camera.TakePicture();
            };

            toggleFlashButton.Click += delegate
            {
                ToggleFlash();
            };

            openGalleryButton.Click += delegate
            {
                imageManager.PickPhoto();
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            camera.Start();
            try
            {
                camera.Flash = CameraView.FlashAuto;
                toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_auto_white));
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, "Ошибка во время работы со вспышкой", ToastLength.Short).Show();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            camera.Stop();
        }

        private void ToggleFlash()
        {
            try
            {
                flashCurrent = (flashCurrent + 1) % 3;
                switch (flashCurrent)
                {
                    case 0:
                        camera.Flash = CameraView.FlashAuto;
                        toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_auto_white));
                        break;
                    case 1:
                        camera.Flash = CameraView.FlashOn;
                        toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_on_white));
                        break;
                    case 2:
                        camera.Flash = CameraView.FlashOff;
                        toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_off_white));
                        break;
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, "Ошибка во время работы со вспышкой", ToastLength.Short).Show();
            }
        }
        class CameraViewCallback : CameraView.Callback
        {

            private CameraView mCameraView;
            private Activity activity;
            private Action<string> imageResult;

            public CameraViewCallback(CameraView mCameraView, Activity activity, Action<string> imageResult)
            {
                this.mCameraView = mCameraView;
                this.activity = activity;
                this.imageResult = imageResult;
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

                    imageResult?.Invoke(filename);
                    Toast.MakeText(mCameraView.Context, "Pic taken", ToastLength.Short).Show();
                }
                catch (Exception e)
                {
                    Log.Debug("ERROR", e.Message);
                    Toast.MakeText(mCameraView.Context, "Error: " + e.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Google.Android.Cameraview;
using DevenirProject.ImageUtils;
using Java.IO;

namespace DevenirProject
{
    [Activity(Label = "CameraLayout", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CameraLayout : Activity
    {
        OrientationListener orientationListener;

        CameraView camera;

        public ImageButton toggleFlashButton;
        Button takePictureButton;
        ImageButton openDefaultCameraButton;
        ImageButton openGalleryButton;
        TextView aspectRatioView;

        ImageManager imageManager;

        int flashCurrent = CameraView.FlashAuto;
        int currentAspectRatio = -1;
        int lastAngle = 0;
        bool isCameraTurnedOff = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.AppThemeClearStatusBar);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_camera);

            toggleFlashButton = FindViewById<ImageButton>(Resource.Id.toggleFlashButton);
            takePictureButton = FindViewById<Button>(Resource.Id.takePictureButton);
            openGalleryButton = FindViewById<ImageButton>(Resource.Id.openGalleryButton);
            aspectRatioView = FindViewById<TextView>(Resource.Id.aspectRatioView);
            openDefaultCameraButton = FindViewById<ImageButton>(Resource.Id.openDefaultCamera);

            InitializeCamera();

            imageManager = new ImageManager();
            imageManager.AddOnImageResultListener(delegate (Bitmap bitmap, string path, Exception ex)
            {
                if (path != null)
                {
                    Intent intent = new Intent(this, typeof(MainViewActivity));
                    intent.PutExtra("image", path);
                    StartActivity(intent);
                    isCameraTurnedOff = false;
                }
                else if (ex != null) Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                else
                {
                    isCameraTurnedOff = false;
                    camera.Start();
                }
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
                camera.Stop();
                isCameraTurnedOff = true;
                imageManager.PickPhoto();
            };

            aspectRatioView.Click += delegate
            {
                ToggleAspectRatio();
            };

            openDefaultCameraButton.Click += delegate
            {
                camera.Stop();
                isCameraTurnedOff = true;
                imageManager.TakePhoto();
            };



            orientationListener = new OrientationListener(this, delegate (int angle)
            {
                if (lastAngle != angle)
                {
                    if (lastAngle == -90 && angle == 180)
                    {
                        angle = -180;
                    }
                    toggleFlashButton.Animate().Rotation(angle).Start();
                    openGalleryButton.Animate().Rotation(angle).Start();
                    openDefaultCameraButton.Animate().Rotation(angle).Start();
                    aspectRatioView.Animate().Rotation(angle).Start();
                    if (angle == -180)
                        lastAngle = 180;
                    else
                        lastAngle = angle;
                }
            });

            camera.Start();
        }



        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("CURRENT_FLASH_MODE", flashCurrent);
            outState.PutInt("CURRENT_ASPECT_RATIO", currentAspectRatio);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            flashCurrent = savedInstanceState.GetInt("CURRENT_FLASH_MODE", CameraView.FlashAuto);
            currentAspectRatio = savedInstanceState.GetInt("CURRENT_ASPECT_RATIO", -1);
        }

        protected override void OnResume()
        {
            try
            {
                if (!isCameraTurnedOff) camera.Start();
                if (currentAspectRatio == -1) CalculateAndSetAspectRatio(camera);
                else
                {
                    currentAspectRatio--;
                    ToggleAspectRatio();
                }

                flashCurrent--;
                ToggleFlash();
            }
            catch (Exception ex)
            {
                Log.Debug("Camera error", ex.StackTrace);
                //InitializeCamera();
                //camera.Start();
            }
            base.OnResume();
        }

        protected override void OnStart()
        {
            orientationListener.Enable();
            base.OnStart();
        }

        protected override void OnStop()
        {
            orientationListener.Disable();
            base.OnStop();
        }

        private void InitializeCamera()
        {
            if (!isCameraTurnedOff)
            {
                camera = FindViewById<CameraView>(Resource.Id.cameraView);
                camera.AddCallback(new CameraViewCallback(camera, this, delegate (string path)
                {
                    if (path != null && path != "")
                    {
                        Intent intent = new Intent(this, typeof(MainViewActivity));
                        intent.PutExtra("image", path);
                        StartActivity(intent);
                    }
                    else Toast.MakeText(Application.Context, "Ошибка во время сохранения фотографии", ToastLength.Short).Show();
                }));
            }
        }

        protected override void OnPause()
        {
            try
            {
                if (camera != null)
                {
                    camera.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Camera error", ex.StackTrace);
            }
            base.OnPause();
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

        private void ToggleAspectRatio()
        {
            if (camera != null)
            {
                AspectRatio[] ratios = camera.SupportedAspectRatios.ToArray();
                camera.AspectRatio = ratios[(++currentAspectRatio) % ratios.Length];
                aspectRatioView.Text = camera.AspectRatio.ToString();
            }
        }

        private void CalculateAndSetAspectRatio(CameraView camera)
        {
            if (camera != null)
            {
                Display display = WindowManager.DefaultDisplay;
                int width = display.Width;
                int height = display.Height;

                double screenRatio;


                var orientation = base.Resources.Configuration.Orientation;
                if (orientation == Android.Content.Res.Orientation.Portrait)
                {
                    screenRatio = (double)height / width;
                }
                else
                {
                    screenRatio = (double)width / height;
                }

                AspectRatio[] ratios = camera.SupportedAspectRatios.ToArray();
                List<double> ratiosValues = new List<double>();
                foreach (var ratio in ratios)
                {
                    ratiosValues.Add(Math.Abs((double)ratio.GetX() / ratio.GetY() - screenRatio));
                }

                int index = ratiosValues
                    .Select((n, i) => new { index = i, value = n })
                    .OrderBy(item => item.value)
                    .First().index;
                camera.AspectRatio = ratios[index];
                currentAspectRatio = index;
                aspectRatioView.Text = camera.AspectRatio.ToString();
            }
        }

        private class OrientationListener : OrientationEventListener
        {
            int ROTATION_O = 0;
            int ROTATION_90 = 90;
            int ROTATION_180 = 180;
            int ROTATION_270 = -90;
            CameraLayout baseClass;

            int rotation = 0;
            Action<int> UpdateOrientationEvent;
            public OrientationListener(CameraLayout activity, Action<int> UpdateOrientation) : base(activity.ApplicationContext)
            {
                this.baseClass = activity;
                this.UpdateOrientationEvent = UpdateOrientation;
            }
            public override void OnOrientationChanged(int orientation)
            {
                if ((orientation < 35 || orientation > 325) && rotation != ROTATION_O)
                { // PORTRAIT
                    rotation = ROTATION_O;
                }
                else if (orientation > 145 && orientation < 215 && rotation != ROTATION_180)
                { // REVERSE PORTRAIT
                    rotation = ROTATION_180;
                }
                else if (orientation > 55 && orientation < 125 && rotation != ROTATION_270)
                { // REVERSE LANDSCAPE
                    rotation = ROTATION_270;
                }
                else if (orientation > 235 && orientation < 305 && rotation != ROTATION_90)
                { //LANDSCAPE
                    rotation = ROTATION_90;
                }

                UpdateOrientationEvent(rotation);
            }
        }


        class CameraViewCallback : CameraView.Callback
        {

            private CameraView mCameraView;
            private CameraLayout activity;
            private Action<string> imageResult;

            public CameraViewCallback(CameraView mCameraView, CameraLayout activity, Action<string> imageResult)
            {
                this.mCameraView = mCameraView;
                this.activity = activity;
                this.imageResult = imageResult;
            }

            public override void OnPictureTaken(CameraView camera, byte[] image)
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
                    //Handling screen rotation
                    Matrix matrix = new Matrix();
                    switch (activity.lastAngle)
                    {
                        case 0:
                            matrix.PostRotate(90);
                            break;
                        case -90:
                            matrix.PostRotate(180);
                            break;
                        case 180:
                            matrix.PostRotate(270);
                            break;
                        case 90:
                            matrix.PostRotate(0);
                            break;
                    }
                    Bitmap bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
                    Bitmap resultImage = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);



                    FileStream outStream = new FileStream(filename, FileMode.Create);
                    resultImage.Compress(Bitmap.CompressFormat.Jpeg, 90, outStream);
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
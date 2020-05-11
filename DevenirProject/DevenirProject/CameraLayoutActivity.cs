using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using DevenirProject.Utilities.CameraUtilities;
using DevenirProject.Utilities.Helpers;
using DevenirProject.Views;
using Java.Interop;
using Java.IO;

namespace DevenirProject
{
    [Activity(Label = "Devenir", Icon = "@mipmap/ic_launcher", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, MainLauncher = true)]
    public class CameraLayoutActivity : Activity
    {
        OrientationListener orientationListener;

        Android.Hardware.Camera camera;
        CameraPreview preview;
        FrameLayout cameraPreview;

        ImageButton toggleFlashButton;
        Button takePictureButton;
        ImageButton openDefaultCameraButton;
        ImageButton openGalleryButton;

        ImageManager imageManager;

        int flashCurrent = -1;
        int lastAngle = 0;

        readonly string[] permissions =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.AppThemeClearStatusBar);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_camera);

            //CrashLytics
            Fabric.Fabric.With(this, new Crashlytics.Crashlytics());
            Crashlytics.Crashlytics.HandleManagedExceptions();

            RequestPermissions(permissions, 0);

            if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)
            {
                RequestedOrientation = ScreenOrientation.Portrait;
            }

            toggleFlashButton = FindViewById<ImageButton>(Resource.Id.toggleFlashButton);
            takePictureButton = FindViewById<Button>(Resource.Id.takePictureButton);
            openGalleryButton = FindViewById<ImageButton>(Resource.Id.openGalleryButton);
            openDefaultCameraButton = FindViewById<ImageButton>(Resource.Id.openDefaultCamera);

            cameraPreview = FindViewById<FrameLayout>(Resource.Id.cameraView);

            imageManager = new ImageManager();
            imageManager.AddOnImageResultListener(delegate (Bitmap bitmap, string path, Exception ex)
            {
                if (path != null)
                {
                    StopCamera();
                    Intent intent = new Intent(this, typeof(MainViewActivity));
                    intent.PutExtra("image", path);
                    StartActivity(intent);
                }
                else if (ex != null)
                {

                    Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                    StartCamera();
                }
                else
                {
                    StartCamera();
                }
            });


            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            {
                Window w = Window;
                w.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }

            takePictureButton.Click += delegate
            {
                TakePicture();
            };

            toggleFlashButton.Click += delegate
            {
                ToggleFlash();
            };

            openGalleryButton.Click += delegate
            {
                imageManager.PickPhoto();
            };

            openDefaultCameraButton.Click += delegate
            {
                StopCamera();
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
                    if (angle == -180)
                        lastAngle = 180;
                    else
                        lastAngle = angle;
                }
            });
        }



        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("CURRENT_FLASH_MODE", flashCurrent);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            flashCurrent = savedInstanceState.GetInt("CURRENT_FLASH_MODE", -1);
        }

        protected override void OnResume()
        {
            try
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
                {
                    StartCamera();
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Camera error", ex.StackTrace);
            }

            //Ожидание открытия/закрытия камеры
            openDefaultCameraButton.Enabled = false;
            toggleFlashButton.Enabled = false;
            takePictureButton.Enabled = false;

            openDefaultCameraButton.PostDelayed(() => { openDefaultCameraButton.Enabled = true; }, 500);
            toggleFlashButton.PostDelayed(() => { toggleFlashButton.Enabled = true; }, 500);
            takePictureButton.PostDelayed(() => { takePictureButton.Enabled = true; }, 500);

            base.OnResume();
        }

        protected override void OnPause()
        {
            StopCamera();
            base.OnPause();
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

        private void StartCamera()
        {
            if (camera != null)
            {
                try
                {
                    camera.StartPreview();
                    return;
                }
                catch (Java.Lang.Exception e)
                {
                }
            }

            try
            {
                camera = CameraHelpers.GetCameraInstance();
                flashCurrent--;
                ToggleFlash();

                if (camera == null)
                {
                    //showAlert("Can not connect to camera.");
                }
                else
                {
                    preview = new CameraPreview(this, camera);
                    cameraPreview.RemoveAllViews();
                    cameraPreview.AddView(preview);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Camera activity", e.StackTrace);
            }
        }
        private void StopCamera()
        {
            if (camera != null)
            {
                try
                {
                    camera.SetPreviewCallback(null);
                    preview.Holder.RemoveCallback(preview);
                    camera.Release();
                }
                catch (Exception e)
                {
                    Log.Debug("Camera activity", e.StackTrace);
                }
            }
        }
        private void TakePicture()
        {
            if (camera == null) return;
            camera.TakePicture(null, null, new CameraPictureCallback(this, delegate (string path)
            {
                if (path != null && path != "")
                {
                    Intent intent = new Intent(this, typeof(MainViewActivity));
                    intent.PutExtra("image", path);
                    StartActivity(intent);
                }
                else Toast.MakeText(Application.Context, "Ошибка во время сохранения фотографии", ToastLength.Short).Show();
                StartCamera();
            }));
        }

        private void ToggleFlash()
        {
            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
            {
                var cameraParams = camera.GetParameters();
                List<string> supportedParams = cameraParams.SupportedFlashModes.ToList();
                if (supportedParams.Count > 0)
                {
                    if (flashCurrent < 0)
                    {
                        int index = supportedParams.ToList().FindLastIndex(flash => flash == Android.Hardware.Camera.Parameters.FlashModeAuto);
                        if (index == -1)
                        {
                            cameraParams.FlashMode = supportedParams[0];
                            flashCurrent = 0;
                        }
                        else
                        {
                            cameraParams.FlashMode = supportedParams[index];
                            flashCurrent = index;
                        }
                    }
                    else
                    {
                        int index = (flashCurrent + 1) % supportedParams.Count;
                        cameraParams.FlashMode = supportedParams[index];
                        flashCurrent = index;
                    }
                    if (cameraParams.FlashMode == Android.Hardware.Camera.Parameters.FlashModeOn ||
                        cameraParams.FlashMode == Android.Hardware.Camera.Parameters.FlashModeAuto ||
                        cameraParams.FlashMode == Android.Hardware.Camera.Parameters.FlashModeOff)
                    {
                        camera.SetParameters(cameraParams);
                    }
                    else
                    {
                        ToggleFlash();
                        return;
                    }
                }

                switch (cameraParams.FlashMode)
                {
                    case Android.Hardware.Camera.Parameters.FlashModeAuto:
                        toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_auto_white));
                        break;
                    case Android.Hardware.Camera.Parameters.FlashModeOn:
                        toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_on_white));
                        break;
                    case Android.Hardware.Camera.Parameters.FlashModeOff:
                        toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_off_white));
                        break;
                    default:
                        toggleFlashButton.SetImageDrawable(GetDrawable(Resource.Drawable.ic_flash_off_white));
                        break;
                }
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private class OrientationListener : OrientationEventListener
        {
            int ROTATION_O = 0;
            int ROTATION_90 = 90;
            int ROTATION_180 = 180;
            int ROTATION_270 = -90;
            CameraLayoutActivity baseClass;

            int rotation = 0;
            Action<int> UpdateOrientationEvent;
            public OrientationListener(CameraLayoutActivity activity, Action<int> UpdateOrientation) : base(activity.ApplicationContext)
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


        class CameraPictureCallback : Java.Lang.Object, Android.Hardware.Camera.IPictureCallback
        {
            private CameraLayoutActivity activity;
            private Action<string> imageResult;

            public CameraPictureCallback(CameraLayoutActivity activity, Action<string> imageResult)
            {
                this.activity = activity;
                this.imageResult = imageResult;
            }

            public void OnPictureTaken(byte[] image, Android.Hardware.Camera camera)
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
                    Bitmap bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);

                    //У некоторых телефонов по умолчанию фото повёрнуто на 90 градусов
                    if (bitmap.Width >= bitmap.Height) { matrix.PostRotate(90); }

                    switch (activity.lastAngle)
                    {
                        case 0:
                            matrix.PostRotate(0);
                            break;
                        case -90:
                            matrix.PostRotate(90);
                            break;
                        case 180:
                            matrix.PostRotate(180);
                            break;
                        case 90:
                            matrix.PostRotate(270);
                            break;
                    }
                    Bitmap resultImage = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);

                    FileStream outStream = new FileStream(filename, FileMode.Create);
                    resultImage.Compress(Bitmap.CompressFormat.Jpeg, 90, outStream);
                    outStream.Close();

                    var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                    mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(filename)));
                    activity.SendBroadcast(mediaScanIntent);

                    imageResult?.Invoke(filename);
                }
                catch (Exception e)
                {
                    Log.Debug("ERROR", e.Message);
                    imageResult?.Invoke(null);
                }
            }
        }
    }
}
using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using DevenirProject.Utilities.CameraUtilities;
using Java.IO;

namespace DevenirProject.Views
{
    class CameraPreview : SurfaceView, ISurfaceHolderCallback
    {
        Context context;
        Android.Hardware.Camera cameraInstance;
        ISurfaceHolder surfaceHolder;

        public CameraPreview(Context context, Android.Hardware.Camera camera) : base(context)
        {
            cameraInstance = camera;
            this.context = context;
            surfaceHolder = Holder;
            surfaceHolder.AddCallback(this);

            Android.Hardware.Camera.Parameters parameters = cameraInstance.GetParameters();
            parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeAuto;
            cameraInstance.SetParameters(parameters);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            if (surfaceHolder.Surface == null)
            {
                return;
            }
            try
            {
                cameraInstance.StopPreview();
                cameraInstance.SetPreviewDisplay(surfaceHolder);

                Android.Hardware.Camera.Parameters parameters = cameraInstance.GetParameters();
                Android.Hardware.Camera.Size optimalSize = CameraHelpers.GetOptimalPreviewSize(context, cameraInstance, width, height);
                parameters.SetPreviewSize(optimalSize.Width, optimalSize.Height);
                parameters.SetPictureSize(optimalSize.Width, optimalSize.Height);
                parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
                cameraInstance.SetParameters(parameters);
                cameraInstance.SetDisplayOrientation(CameraHelpers.GetCameraOrientation(context));
                cameraInstance.StartPreview();
            }
            catch (Exception e)
            {
                Log.Debug("Camera activity", "Error starting camera preview: " + e.Message);
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                cameraInstance.SetPreviewDisplay(holder);
                cameraInstance.StartPreview();
            }
            catch (IOException e)
            {
                Log.Debug("Camera activity", "Error setting camera preview: " + e.Message);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            //Камеру закрывает Activity
        }
    }
}
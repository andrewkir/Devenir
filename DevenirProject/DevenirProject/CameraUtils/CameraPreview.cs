using System;
using System.Collections.Generic;
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
using Java.IO;

namespace DevenirProject.CameraUtils
{
    class CameraPreview : SurfaceView, ISurfaceHolderCallback
    {
        bool isFocusing = false;
        Context mContext;
        Android.Hardware.Camera mCamera;
        ISurfaceHolder mHolder;

        public CameraPreview(Context context, Android.Hardware.Camera camera) : base(context)
        {
            mCamera = camera;
            mContext = context;
            mHolder = Holder;
            mHolder.AddCallback(this);

            Android.Hardware.Camera.Parameters parameters = mCamera.GetParameters();
            parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeAuto;
            mCamera.SetParameters(parameters);

            isFocusing = false;
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            if (mHolder.Surface == null)
            {
                return;
            }
            try
            {
                mCamera.StopPreview();
                mCamera.SetPreviewDisplay(mHolder);

                Android.Hardware.Camera.Parameters parameters = mCamera.GetParameters();
                Android.Hardware.Camera.Size optimalSize = CameraHelpers.getOptimalPreviewSize(mContext, mCamera, width, height);
                parameters.SetPreviewSize(optimalSize.Width, optimalSize.Height);
                parameters.SetPictureSize(optimalSize.Width, optimalSize.Height);
                parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
                mCamera.SetParameters(parameters);
                mCamera.SetDisplayOrientation(CameraHelpers.getCameraOrientation(mContext));
                mCamera.StartPreview();

                isFocusing = false;
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
                mCamera.SetPreviewDisplay(holder);
                mCamera.StartPreview();
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
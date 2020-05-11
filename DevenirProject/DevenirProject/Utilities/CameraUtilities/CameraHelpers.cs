using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Hardware;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace DevenirProject.Utilities.CameraUtilities
{
    public class CameraHelpers
    {
        public static Camera GetCameraInstance()
        {
            Camera camera = null;
            int numCams = Camera.NumberOfCameras;
            if (numCams > 0)
            {
                Camera.CameraInfo info = new Camera.CameraInfo();
                try
                {
                    for (int i = 0; i < numCams; i++)
                    {
                        Camera.GetCameraInfo(i, info);
                        if (info.Facing == Camera.CameraInfo.CameraFacingBack)
                        {
                            camera = Camera.Open(i);
                            camera.SetDisplayOrientation(90);
                            // also set the camera's output orientation
                            Camera.Parameters param = camera.GetParameters();
                            param.SetRotation(90);
                            camera.SetParameters(param);
                            break;
                        }
                    }
                }
                catch (RuntimeException ex)
                {
                    ex.PrintStackTrace();
                }
            }

            return camera;
        }


        public static int GetCameraOrientation(Context context)
        {
            Camera.CameraInfo info = new Camera.CameraInfo();
            //0 - camera facing back
            Camera.GetCameraInfo(0, info);
            IWindowManager wm = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            Display display = wm.DefaultDisplay;

            var rotation = display.Rotation;
            int degrees = 0;
            switch (rotation)
            {
                case SurfaceOrientation.Rotation0:
                    degrees = 0;
                    break;
                case SurfaceOrientation.Rotation90:
                    degrees = 90;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 270;
                    break;
            }

            return (info.Orientation - degrees + 360) % 360;
        }

        public static Camera.Size GetOptimalPreviewSize(Context context, Camera camera, int w, int h)
        {
            if (camera == null)
            {
                return null;
            }

            List<Camera.Size> sizes = camera.GetParameters().SupportedPreviewSizes.ToList();
            if (DisplayHelpers.GetScreenOrientation(context) == Android.Content.Res.Orientation.Portrait)
            {
                int portraitWidth = h;
                h = w;
                w = portraitWidth;
            }

            double ASPECT_TOLERANCE = 0.1;
            double targetRatio = (double)w / h;
            if (sizes == null) return null;

            Camera.Size optimalSize = null;
            double minDiff = double.MaxValue;

            int targetHeight = h;

            foreach (Camera.Size size in sizes)
            {
                double ratio = (double)size.Width / size.Height;
                if (System.Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE) continue;
                if (System.Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = System.Math.Abs(size.Height - targetHeight);
                }
            }

            if (optimalSize == null)
            {
                minDiff = double.MaxValue;
                foreach (Camera.Size size in sizes)
                {
                    if (System.Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = System.Math.Abs(size.Height - targetHeight);
                    }
                }
            }
            return optimalSize;
        }
    }
}
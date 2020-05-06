using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DevenirProject.CameraUtils
{
    class DisplayHelpers
    {
        public static Android.Content.Res.Orientation GetScreenOrientation(Context context)
        {
            IWindowManager wm = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            Display display = wm.DefaultDisplay;
            var orientation = Android.Content.Res.Orientation.Portrait;
            if (display.Width == display.Height)
            {
                orientation = Android.Content.Res.Orientation.Square;
            }
            else
            {
                if (display.Width < display.Height)
                {
                    orientation = Android.Content.Res.Orientation.Portrait;
                }
                else
                {
                    orientation = Android.Content.Res.Orientation.Landscape;
                }
            }
            return orientation;
        }
    }
}
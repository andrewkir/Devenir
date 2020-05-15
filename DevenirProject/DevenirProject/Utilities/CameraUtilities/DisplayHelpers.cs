using Android.Content;
using Android.Runtime;
using Android.Views;

namespace DevenirProject.Utilities.CameraUtilities
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
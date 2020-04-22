using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DevenirProject.Views
{
    public class MultiCropView : ViewGroup
    {
        Point point;
        public MultiCropView(Context context) : base(context)
        {
        }

        public MultiCropView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context, attrs);
        }

        public MultiCropView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize(context, attrs);
        }

        public MultiCropView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize(context, attrs);
        }

        private void Initialize(Context context, IAttributeSet attrs = null)
        {
            point = new Point(context, attrs);
            AddView(point);
            //textView = new TextView(context)
            //{
            //    LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
            //};
            //textView.Text = "HE HE HE, YUP";
            //AddView(textView);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            MeasureChild(point, widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
            //SetMeasuredDimension(point.MeasuredWidth, point.MeasuredHeight);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var width = r - l;
            var rightBorder = width - PaddingRight;
            point.Layout(PaddingLeft, PaddingTop, PaddingLeft + point.MeasuredWidth, PaddingTop + point.MeasuredHeight);
        }
    }
}
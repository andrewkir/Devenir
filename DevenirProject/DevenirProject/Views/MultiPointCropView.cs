using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;

namespace DevenirProject.Views
{
    public class MultiPointCropView : ViewGroup
    {
        List<Point> points = new List<Point>();

        Paint linePaint = new Paint(PaintFlags.AntiAlias);

        Paint imagePaint = new Paint(PaintFlags.AntiAlias);

        Bitmap image;
        Bitmap srcBitmap;
        Bitmap resultPreviewBitmap;

        float widthDiff = 1;
        float heightDiff = 1;

        public MultiPointCropView(Context context) : base(context)
        {
        }

        public MultiPointCropView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context, attrs);
        }

        public MultiPointCropView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize(context, attrs);
        }

        public MultiPointCropView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize(context, attrs);
        }

        private void Initialize(Context context, IAttributeSet attrs = null)
        {
            Point point = new Point(context, attrs);
            point.pointX = point.radius * 2;
            point.pointY = point.radius * 2;
            AddView(point);
            point.Touch += PointTouchListener;

            Point point2 = new Point(context, attrs);
            point2.pointX = point.radius * 4;
            point2.pointY = point.radius;
            AddView(point2);
            point2.Touch += PointTouchListener;

            points.Add(point);
            points.Add(point2);

            point = new Point(context, attrs);
            point.pointX = point.radius * 4;
            point.pointY = point.radius * 4;
            AddView(point);
            point.Touch += PointTouchListener;

            point2 = new Point(context, attrs);
            point2.pointX = point.radius;
            point2.pointY = point.radius * 4;
            AddView(point2);
            point2.Touch += PointTouchListener;

            points.Add(point);
            points.Add(point2);

            linePaint.Color = point.pointColor;
            linePaint.SetStyle(Paint.Style.Stroke);
            linePaint.StrokeWidth = 5;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            foreach (var point in points)
            {
                MeasureChild(point, widthMeasureSpec, heightMeasureSpec);
            }
            if(image == null) SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
            else
            {
                int needHeight = PaddingTop + points[0].MeasuredHeight + image.Height + PaddingBottom;
                SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), needHeight);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var width = r - l;
            var rightBorder = width - PaddingRight;
            foreach (var point in points)
            {
                point.Layout(PaddingLeft + (int)point.pointX - (int)point.radius, PaddingTop + (int)point.pointY - (int)point.radius, PaddingLeft + (int)point.pointX + point.MeasuredWidth - (int)point.radius, PaddingTop + (int)point.pointY + point.MeasuredHeight - (int)point.radius);
            }
        }

        public void SetDotsDefault()
        {
            if (image != null)
            {
                float radius = points[0].radius;
                points[0].pointX = PaddingLeft + radius;
                points[0].pointY = PaddingTop + radius;

                points[1].pointX = PaddingLeft + image.Width + points[0].radius - 1;
                points[1].pointY = PaddingTop + radius;

                points[2].pointX = PaddingLeft + image.Width + points[2].radius - 1;
                points[2].pointY = image.Height - PaddingBottom + points[2].radius - 1;

                points[3].pointX = PaddingLeft + radius;
                points[3].pointY = image.Height - PaddingBottom + points[3].radius - 1;

                RequestLayout();
            }
        }

        public void SetBitmap(Bitmap bitmap)
        {
            RequestLayout();
            srcBitmap = bitmap;
            image = scaleBitmapAndKeepRation(bitmap, Math.Abs(Height - 2 * (int)points[0].radius - PaddingTop - PaddingBottom), Math.Abs(Width - 2 * (int)points[0].radius - PaddingLeft - PaddingRight));

            widthDiff = (float)bitmap.Width / image.Width;
            heightDiff = (float)bitmap.Height / image.Height;
            resultPreviewBitmap = Bitmap.CreateBitmap(image.Width, image.Height, Bitmap.Config.Argb8888);
            RequestLayout();
        }

        public void PointTouchListener(object s, TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    (s as Point).pointdX = (s as View).GetX() - e.Event.RawX;
                    (s as Point).pointdY = (s as View).GetY() - e.Event.RawY;
                    break;
                case MotionEventActions.Move:
                    float tmp_x = (s as Point).pointX;
                    float tmp_y = (s as Point).pointY;

                    float new_x = e.Event.RawX + (s as Point).pointdX + (s as Point).radius;
                    float new_y = e.Event.RawY + (s as Point).pointdY + (s as Point).radius;

                    if (image != null)
                    {
                        if (new_x < PaddingLeft + image.Width + (s as Point).radius && new_x - (s as Point).radius > PaddingLeft) (s as Point).pointX = new_x;
                        if (new_y < image.Height - PaddingBottom + (s as Point).radius && new_y - (s as Point).radius > PaddingTop) (s as Point).pointY = new_y;

                        if (new_x >= PaddingLeft + image.Width + (s as Point).radius) (s as Point).pointX = PaddingLeft + image.Width + (s as Point).radius - 1;
                        if (new_x - (s as Point).radius <= PaddingLeft) (s as Point).pointX = PaddingLeft + (s as Point).radius + 1;

                        if (new_y >= image.Height - PaddingBottom + (s as Point).radius) (s as Point).pointY = image.Height + (s as Point).radius - 1;
                        if (new_y - (s as Point).radius <= PaddingTop) (s as Point).pointY = PaddingTop + (s as Point).radius + 1;
                    }

                    foreach (var point in points)
                    {
                        if (CheckCollision(s as Point, point))
                        {
                            (s as Point).pointX = tmp_x;
                            (s as Point).pointY = tmp_y;
                            break;
                        }
                    }
                    RequestLayout();
                    break;
                case MotionEventActions.HoverMove:
                    Log.Debug("MOVING", "gotta go hovering");
                    break;
            }
            e.Handled = true;
        }

        protected override void DispatchDraw(Canvas canvas)
        {

            canvas.DrawLine(points[0].pointX, points[0].pointY, points[1].pointX, points[1].pointY, linePaint);
            canvas.DrawLine(points[1].pointX, points[1].pointY, points[2].pointX, points[2].pointY, linePaint);
            canvas.DrawLine(points[2].pointX, points[2].pointY, points[3].pointX, points[3].pointY, linePaint);
            canvas.DrawLine(points[3].pointX, points[3].pointY, points[0].pointX, points[0].pointY, linePaint);

            if (image != null)
            {
                imagePaint.Alpha = 50;
                canvas.DrawBitmap(image, points[0].radius + PaddingLeft, points[0].radius + PaddingTop, imagePaint);
                imagePaint.Reset();
                imagePaint.Flags = PaintFlags.AntiAlias;
            }

            if (image != null)
            {
                resultPreviewBitmap.EraseColor(Color.Transparent);
                Canvas cs = new Canvas(resultPreviewBitmap);
                Path path = new Path();
                path.MoveTo(points[0].pointX - points[0].radius, points[0].pointY - points[0].radius);
                path.LineTo(points[1].pointX - points[0].radius, points[1].pointY - points[0].radius);
                path.LineTo(points[2].pointX - points[0].radius, points[2].pointY - points[0].radius);
                path.LineTo(points[3].pointX - points[0].radius, points[3].pointY - points[0].radius);
                path.LineTo(points[0].pointX - points[0].radius, points[0].pointY - points[0].radius);
                cs.ClipPath(path);
                cs.DrawBitmap(image, 0, 0, imagePaint);
                canvas.DrawBitmap(resultPreviewBitmap, points[0].radius, points[0].radius, imagePaint);
            }

            base.DispatchDraw(canvas);
        }

        public Bitmap CropView()
        {
            if (srcBitmap != null)
            {
                Bitmap croppedImage = Bitmap.CreateBitmap(srcBitmap.Width, srcBitmap.Height, Bitmap.Config.Argb8888);
                Canvas cs = new Canvas(croppedImage);
                Path path = new Path();
                path.MoveTo(widthDiff * (points[0].pointX - points[0].radius), heightDiff * (points[0].pointY - points[0].radius));
                path.LineTo(widthDiff * (points[1].pointX - points[0].radius), heightDiff * (points[1].pointY - points[0].radius));
                path.LineTo(widthDiff * (points[2].pointX - points[0].radius), heightDiff * (points[2].pointY - points[0].radius));
                path.LineTo(widthDiff * (points[3].pointX - points[0].radius), heightDiff * (points[3].pointY - points[0].radius));
                path.LineTo(widthDiff * (points[0].pointX - points[0].radius), heightDiff * (points[0].pointY - points[0].radius));
                cs.ClipPath(path);
                cs.DrawBitmap(srcBitmap, 0, 0, imagePaint);
                RectF pathRect = new RectF();
                path.ComputeBounds(pathRect, true);

                if ((int)pathRect.Width() == 0 || (int)pathRect.Height() == 0)
                {
                    return srcBitmap;
                }
                else return Bitmap.CreateBitmap(croppedImage, (int)pathRect.Left, (int)pathRect.Top, (int)pathRect.Width(), (int)pathRect.Height());
            }
            return null;
        }

        private bool CheckCollision(Point point1, Point point2)
        {
            if (point1 != point2)
            {
                double dist = Math.Sqrt(Math.Pow(point1.pointX - point2.pointX, 2) + Math.Pow(point1.pointY - point2.pointY, 2));
                if (dist - point1.radius - point2.radius <= 0) return true;
                return false;
            }
            return false;
        }
        public static Bitmap scaleBitmapAndKeepRation(Bitmap targetBmp, int reqHeightInPixels, int reqWidthInPixels)
        {
            Matrix matrix = new Matrix();
            matrix.SetRectToRect(new RectF(0, 0, targetBmp.Width, targetBmp.Height), new RectF(0, 0, reqWidthInPixels, reqHeightInPixels), Matrix.ScaleToFit.Center);
            Bitmap scaledBitmap = Bitmap.CreateBitmap(targetBmp, 0, 0, targetBmp.Width, targetBmp.Height, matrix, true);
            return scaledBitmap;
        }
    }
}
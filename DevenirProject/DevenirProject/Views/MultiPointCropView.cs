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
using Java.Interop;

namespace DevenirProject.Views
{
    public class MultiPointCropView : ViewGroup
    {
        List<Point> points = new List<Point>();

        Paint linePaint = new Paint(PaintFlags.AntiAlias);

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

            linePaint.Color = Color.BlueViolet;
            linePaint.SetStyle(Paint.Style.Stroke);
            linePaint.StrokeWidth = 5;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            foreach (var point in points)
            {
                MeasureChild(point, widthMeasureSpec, heightMeasureSpec);
            }
            SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
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

                    if (new_x + (s as Point).radius < Width - PaddingRight && new_x - (s as Point).radius > PaddingLeft) (s as Point).pointX = new_x;
                    if (new_y + (s as Point).radius < Height - PaddingBottom && new_y - (s as Point).radius > PaddingTop) (s as Point).pointY = new_y;

                    if (new_x + (s as Point).radius >= Width - PaddingRight) (s as Point).pointX = Width - PaddingRight - (s as Point).radius - 1;
                    if (new_x - (s as Point).radius <= PaddingLeft) (s as Point).pointX = PaddingLeft + (s as Point).radius + 1;

                    if (new_y + (s as Point).radius >= Height - PaddingBottom) (s as Point).pointY = Height - PaddingBottom - (s as Point).radius - 1;
                    if (new_y - (s as Point).radius <= PaddingTop) (s as Point).pointY = PaddingTop + (s as Point).radius + 1;


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


            base.DispatchDraw(canvas);
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

    }
}
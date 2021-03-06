﻿using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace DevenirProject.Views
{
    public class Point : View
    {
        public int pointRadius;
        private int ringRadius;

        public float pointdX;
        public float pointdY; 
        
        public float pointX;
        public float pointY;


        public float radius;

        Paint circlePaint = new Paint(PaintFlags.AntiAlias);
        RectF circleRect = new RectF();

        Paint ringPaint = new Paint();
        int ringThickness;

        public Color pointColor;


        public Point(Context context) : this(context, null) { }

        public Point(Context context, IAttributeSet attrs) : this(context, attrs, Resource.Attribute.pointViewStyle) { }

        public Point(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            if (attrs != null)
            {
                var array = context.ObtainStyledAttributes(attrs, Resource.Styleable.Point, defStyleAttr, 0);
                pointRadius = array.GetDimensionPixelSize(Resource.Styleable.Point_pointRadius, 30);
                pointColor = array.GetColor(Resource.Styleable.Point_color, Color.Red);
                circlePaint.Color = pointColor;



                ringPaint.Color = array.GetColor(Resource.Styleable.Point_ringColor, Color.Red);
                ringPaint.SetStyle(Paint.Style.Stroke);
                ringRadius = array.GetDimensionPixelSize(Resource.Styleable.Point_ringRadius, 0);
                ringThickness = array.GetDimensionPixelSize(Resource.Styleable.Point_ringThickness, 0);

                ringPaint.StrokeWidth = ringThickness;

                radius = ringRadius + ringThickness;
            }
        }

        public void ChangePointColor(Color color)
        {
            ringPaint.Color = color;
            circlePaint.Color = color;
            Invalidate();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            SetMeasuredDimension(2 * ringRadius + 2 * ringThickness, 2 * ringRadius + 2 * ringThickness);
        }

        protected override void OnDraw(Canvas canvas)
        {
            circleRect.Set(ringRadius - pointRadius, ringRadius - pointRadius, ringRadius - pointRadius + pointRadius * 2 + pointRadius, ringRadius - pointRadius + pointRadius * 2 + pointRadius);

            canvas.DrawCircle(
                ringRadius + ringThickness,
                ringRadius + ringThickness,
                pointRadius,
                circlePaint
                );

            canvas.DrawCircle(ringRadius + ringThickness, ringRadius + ringThickness, ringRadius, ringPaint);
            base.DispatchDraw(canvas);
        }

        
    }
}
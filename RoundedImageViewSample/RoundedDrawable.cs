using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using C = Android.Graphics.Color;

namespace RoundedImageViewSample
{
    internal class RoundedDrawable : Drawable
    {
        private readonly RectF _mBitmapRect = new RectF();
        private readonly BitmapShader _mBitmapShader;
        private readonly RectF _mBounds = new RectF();
        private readonly RectF _mDrawableRect = new RectF();
        private readonly int _mBitmapHeight;
        private readonly Paint _mBitmapPaint;
        private readonly int _mBitmapWidth;
        private readonly Paint _mBorderPaint;
        private readonly RectF _mBorderRect = new RectF();
        private readonly Matrix _mShaderMatrix = new Matrix();
        private ColorStateList _mBorderColor = ColorStateList.ValueOf(Color.Black);
        private float _mBorderWidth;

        private float _mCornerRadius;
        private bool _mOval;
        private ImageView.ScaleType _mScaleType = ImageView.ScaleType.FitCenter;

        public RoundedDrawable(Bitmap bitmap)
        {
            _mBitmapWidth = bitmap.Width;
            _mBitmapHeight = bitmap.Height;
            _mBitmapRect.Set(0, 0, _mBitmapWidth, _mBitmapHeight);

            _mBitmapShader = new BitmapShader(bitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
            _mBitmapShader.SetLocalMatrix(_mShaderMatrix);

            _mBitmapPaint = new Paint();
            _mBitmapPaint.SetStyle(Paint.Style.Fill);
            _mBitmapPaint.AntiAlias = (true);
            _mBitmapPaint.SetShader(_mBitmapShader);

            _mBorderPaint = new Paint();
            _mBorderPaint.SetStyle(Paint.Style.Stroke);
            _mBorderPaint.AntiAlias = (true);
            _mBorderPaint.Color = Color.Black;
            _mBorderPaint.StrokeWidth = (_mBorderWidth);
        }

        public override bool IsStateful
        {
            get { return _mBorderColor.IsStateful; }
        }

        public override int Opacity
        {
            get
            {
                return -3; //PixelFormat.TRANSLUCENT; 
            }
        }

        public override int IntrinsicWidth
        {
            get { return _mBitmapWidth; }
        }

        public override int IntrinsicHeight
        {
            get { return _mBitmapHeight; }
        }

        public static RoundedDrawable FromBitmap(Bitmap bitmap)
        {
            return bitmap != null ? new RoundedDrawable(bitmap) : null;
        }

        public static Drawable FromDrawable(Drawable drawable)
        {
            if (drawable == null) return null;
            if (drawable is RoundedDrawable)
            {
                // just return if it's already a RoundedDrawable
                return drawable;
            }
            var layerDrawable = drawable as LayerDrawable;
            if (layerDrawable != null)
            {
                var ld = layerDrawable;
                int num = ld.NumberOfLayers;

                // loop through layers to and change to RoundedDrawables if possible
                for (int i = 0; i < num; i++)
                {
                    Drawable d = ld.GetDrawable(i);
                    ld.SetDrawableByLayerId(ld.GetId(i), FromDrawable(d));
                }
                return ld;
            }

            // try to get a bitmap from the drawable and
            Bitmap bm = DrawableToBitmap(drawable);
            return bm != null ? new RoundedDrawable(bm) : drawable;
        }

        public static Bitmap DrawableToBitmap(Drawable drawable)
        {
            var bitmapDrawable = drawable as BitmapDrawable;
            if (bitmapDrawable != null)
            {
                return bitmapDrawable.Bitmap;
            }

            Bitmap bitmap;
            int width = Math.Max(drawable.IntrinsicWidth, 1);
            int height = Math.Max(drawable.IntrinsicHeight, 1);
            try
            {
                bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                var canvas = new Canvas(bitmap);
                drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                drawable.Draw(canvas);
            }
            catch (Exception)
            {
                bitmap = null;
            }

            return bitmap;
        }


        protected override bool OnStateChange(int[] state)
        {
            int newColor = _mBorderColor.GetColorForState(state, Color.Black);
            if (_mBorderPaint.Color != newColor)
            {
                _mBorderPaint.Color = (new Color(newColor));
                return true;
            }
            return base.OnStateChange(state);
        }


        private void UpdateShaderMatrix()
        {
            float scale;
            float dx;
            float dy;

            if (_mScaleType.Equals(ImageView.ScaleType.Center))
            {
                _mBorderRect.Set(_mBounds);
                _mBorderRect.Inset((_mBorderWidth) / 2, (_mBorderWidth) / 2);

                _mShaderMatrix.Set(null);
                _mShaderMatrix.SetTranslate((int)((_mBorderRect.Width() - _mBitmapWidth) * 0.5f + 0.5f),
                    (int)((_mBorderRect.Height() - _mBitmapHeight) * 0.5f + 0.5f));
            }
            else if (_mScaleType.Equals(ImageView.ScaleType.CenterCrop))
            {
                _mBorderRect.Set(_mBounds);
                _mBorderRect.Inset((_mBorderWidth) / 2, (_mBorderWidth) / 2);

                _mShaderMatrix.Set(null);

                dx = 0;
                dy = 0;

                if (_mBitmapWidth * _mBorderRect.Height() > _mBorderRect.Width() * _mBitmapHeight)
                {
                    scale = _mBorderRect.Height() / _mBitmapHeight;
                    dx = (_mBorderRect.Width() - _mBitmapWidth * scale) * 0.5f;
                }
                else
                {
                    scale = _mBorderRect.Width() / _mBitmapWidth;
                    dy = (_mBorderRect.Height() - _mBitmapHeight * scale) * 0.5f;
                }

                _mShaderMatrix.SetScale(scale, scale);
                _mShaderMatrix.PostTranslate((int)(dx + 0.5f) + _mBorderWidth,
                    (int)(dy + 0.5f) + _mBorderWidth);
            }
            else if (_mScaleType.Equals(ImageView.ScaleType.CenterInside))
            {
                _mShaderMatrix.Set(null);

                if (_mBitmapWidth <= _mBounds.Width() && _mBitmapHeight <= _mBounds.Height())
                {
                    scale = 1.0f;
                }
                else
                {
                    scale = Math.Min(_mBounds.Width() / _mBitmapWidth,
                        _mBounds.Height() / _mBitmapHeight);
                }

                dx = (int)((_mBounds.Width() - _mBitmapWidth * scale) * 0.5f + 0.5f);
                dy = (int)((_mBounds.Height() - _mBitmapHeight * scale) * 0.5f + 0.5f);

                _mShaderMatrix.SetScale(scale, scale);
                _mShaderMatrix.PostTranslate(dx, dy);

                _mBorderRect.Set(_mBitmapRect);
                _mShaderMatrix.MapRect(_mBorderRect);
                _mBorderRect.Inset((_mBorderWidth) / 2, (_mBorderWidth) / 2);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBorderRect, Matrix.ScaleToFit.Fill);
            }
            else if (_mScaleType.Equals(ImageView.ScaleType.FitCenter))
            {
                _mBorderRect.Set(_mBitmapRect);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBounds, Matrix.ScaleToFit.Center);
                _mShaderMatrix.MapRect(_mBorderRect);
                _mBorderRect.Inset((_mBorderWidth) / 2, (_mBorderWidth) / 2);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBorderRect, Matrix.ScaleToFit.Fill);
            }
            else if (_mScaleType.Equals(ImageView.ScaleType.FitEnd))
            {
                _mBorderRect.Set(_mBitmapRect);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBounds, Matrix.ScaleToFit.End);
                _mShaderMatrix.MapRect(_mBorderRect);
                _mBorderRect.Inset((_mBorderWidth) / 2, (_mBorderWidth) / 2);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBorderRect, Matrix.ScaleToFit.Fill);
            }
            else if (_mScaleType.Equals(ImageView.ScaleType.FitStart))
            {
                _mBorderRect.Set(_mBitmapRect);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBounds, Matrix.ScaleToFit.Start);
                _mShaderMatrix.MapRect(_mBorderRect);
                _mBorderRect.Inset((_mBorderWidth) / 2, (_mBorderWidth) / 2);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBorderRect, Matrix.ScaleToFit.Fill);
            }
            else if (_mScaleType.Equals(ImageView.ScaleType.FitXy))
            {
                _mBorderRect.Set(_mBounds);
                _mBorderRect.Inset((_mBorderWidth) / 2, (_mBorderWidth) / 2);
                _mShaderMatrix.Set(null);
                _mShaderMatrix.SetRectToRect(_mBitmapRect, _mBorderRect, Matrix.ScaleToFit.Fill);
            }

            _mDrawableRect.Set(_mBorderRect);
            _mBitmapShader.SetLocalMatrix(_mShaderMatrix);
        }

        protected override void OnBoundsChange(Rect bounds)
        {
            base.OnBoundsChange(bounds);

            _mBounds.Set(bounds);

            UpdateShaderMatrix();
        }

        public override void Draw(Canvas canvas)
        {
            if (_mOval)
            {
                if (_mBorderWidth > 0)
                {
                    canvas.DrawOval(_mDrawableRect, _mBitmapPaint);
                    canvas.DrawOval(_mBorderRect, _mBorderPaint);
                }
                else
                {
                    canvas.DrawOval(_mDrawableRect, _mBitmapPaint);
                }
            }
            else
            {
                if (_mBorderWidth > 0)
                {
                    canvas.DrawRoundRect(_mDrawableRect, Math.Max(_mCornerRadius, 0),
                        Math.Max(_mCornerRadius, 0), _mBitmapPaint);
                    canvas.DrawRoundRect(_mBorderRect, _mCornerRadius, _mCornerRadius, _mBorderPaint);
                }
                else
                {
                    canvas.DrawRoundRect(_mDrawableRect, _mCornerRadius, _mCornerRadius, _mBitmapPaint);
                }
            }
        }

        public override void SetAlpha(int alpha)
        {
            _mBitmapPaint.Alpha = (alpha);
            InvalidateSelf();
        }


        public override void SetColorFilter(ColorFilter cf)
        {
            _mBitmapPaint.SetColorFilter(cf);
            InvalidateSelf();
        }

        public override void SetDither(bool dither)
        {
            _mBitmapPaint.Dither = (dither);
            InvalidateSelf();
        }

        public override void SetFilterBitmap(bool filter)
        {
            _mBitmapPaint.FilterBitmap = (filter);
            InvalidateSelf();
        }

        public float GetCornerRadius()
        {
            return _mCornerRadius;
        }

        public RoundedDrawable SetCornerRadius(float radius)
        {
            _mCornerRadius = radius;
            return this;
        }

        public float GetBorderWidth()
        {
            return _mBorderWidth;
        }

        public RoundedDrawable SetBorderWidth(float width)
        {
            _mBorderWidth = width;
            _mBorderPaint.StrokeWidth = (_mBorderWidth);
            return this;
        }

        public int GetBorderColor()
        {
            return _mBorderColor.DefaultColor;
        }

        public RoundedDrawable SetBorderColor(int color)
        {
            return SetBorderColor(ColorStateList.ValueOf(new Color(color)));
        }

        public ColorStateList GetBorderColors()
        {
            return _mBorderColor;
        }

        public RoundedDrawable SetBorderColor(ColorStateList colors)
        {
            _mBorderColor = colors ?? ColorStateList.ValueOf(new Color(0));
            _mBorderPaint.Color = new Color(_mBorderColor.GetColorForState(GetState(), Color.Black));
            return this;
        }

        public bool IsOval()
        {
            return _mOval;
        }

        public RoundedDrawable SetOval(bool oval)
        {
            _mOval = oval;
            return this;
        }

        public ImageView.ScaleType GetScaleType()
        {
            return _mScaleType;
        }

        public RoundedDrawable SetScaleType(ImageView.ScaleType scaleType)
        {
            if (scaleType == null)
            {
                scaleType = ImageView.ScaleType.FitCenter;
            }
            if (_mScaleType != scaleType)
            {
                _mScaleType = scaleType;
                UpdateShaderMatrix();
            }
            return this;
        }

        public Bitmap ToBitmap()
        {
            return DrawableToBitmap(this);
        }
    }
}
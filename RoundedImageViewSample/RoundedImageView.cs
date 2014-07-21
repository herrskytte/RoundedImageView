using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Exception = System.Exception;

namespace RoundedImageViewSample
{
    internal class RoundedImageView : ImageView
    {

        public const float DefaultRadius = 0f;
        public const float DefaultBorderWidth = 0f;

        private static readonly ScaleType[] ScaleTypes =
        {
            ScaleType.Matrix,
            ScaleType.FitXy,
            ScaleType.FitStart,
            ScaleType.FitCenter,
            ScaleType.FitEnd,
            ScaleType.Center,
            ScaleType.CenterCrop,
            ScaleType.CenterInside
        };

        private float _cornerRadius = DefaultRadius;
        private float _borderWidth = DefaultBorderWidth;

        private ColorStateList _borderColor =
            ColorStateList.ValueOf(Color.Black);

        private bool _isOval;
        private bool _mutateBackground;

        private int _mResource;
        private Drawable _mDrawable;
        private Drawable _mBackgroundDrawable;

        private ScaleType _mScaleType;

        public RoundedImageView(IntPtr handle, JniHandleOwnership ownerShip)
            : base(handle, ownerShip)
        {
            // nothing to do
            // (need this constructor to avoid exceptions)
        }

        public RoundedImageView(Context context)
            : this(context, null)
        {

        }

        public RoundedImageView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public RoundedImageView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.RoundedImageView, defStyle, 0);

            int index = a.GetInt(Resource.Styleable.RoundedImageView_android_scaleType, -1);
            SetScaleType(index >= 0 ? ScaleTypes[index] : ScaleType.FitCenter);

            _cornerRadius = a.GetDimensionPixelSize(Resource.Styleable.RoundedImageView_corner_radius, -1);
            _borderWidth = a.GetDimensionPixelSize(Resource.Styleable.RoundedImageView_border_width, -1);

            // don't allow negative values for radius and border
            if (_cornerRadius < 0)
            {
                _cornerRadius = DefaultRadius;
            }
            if (_borderWidth < 0)
            {
                _borderWidth = DefaultBorderWidth;
            }

            _borderColor = a.GetColorStateList(Resource.Styleable.RoundedImageView_border_color) ??
                           ColorStateList.ValueOf(Color.Black);

            _mutateBackground = a.GetBoolean(Resource.Styleable.RoundedImageView_mutate_background, false);
            _isOval = a.GetBoolean(Resource.Styleable.RoundedImageView_oval, false);

            UpdateDrawableAttrs();
            UpdateBackgroundDrawableAttrs(true);

            a.Recycle();
        }

        protected override void DrawableStateChanged()
        {
            base.DrawableStateChanged();
            Invalidate();
        }

        /**
   * Return the current scale type in use by this ImageView.
   *
   * @attr ref android.R.styleable#ImageView_scaleType
   * @see android.widget.ImageView.ScaleType
   */

        public override ScaleType GetScaleType()
        {
            return _mScaleType;
        }

        /**
   * Controls how the image should be resized or moved to match the size
   * of this ImageView.
   *
   * @param scaleType The desired scaling mode.
   * @attr ref android.R.styleable#ImageView_scaleType
   */

        public override sealed void SetScaleType(ScaleType scaleType)
        {
            if (_mScaleType == scaleType) return;

            _mScaleType = scaleType;

            if (scaleType.Equals(ScaleType.Center) ||
                scaleType.Equals(ScaleType.CenterCrop) ||
                scaleType.Equals(ScaleType.CenterInside) ||
                scaleType.Equals(ScaleType.FitCenter) ||
                scaleType.Equals(ScaleType.FitStart) ||
                scaleType.Equals(ScaleType.FitEnd) ||
                scaleType.Equals(ScaleType.FitXy))
            {
                base.SetScaleType(ScaleType.FitXy);
            }
            else
            {
                base.SetScaleType(scaleType);
            }
            UpdateDrawableAttrs();
            UpdateBackgroundDrawableAttrs(false);
            Invalidate();
        }


        public override void SetImageDrawable(Drawable drawable)
        {
            _mResource = 0;
            _mDrawable = RoundedDrawable.FromDrawable(drawable);
            UpdateDrawableAttrs();
            base.SetImageDrawable(_mDrawable);
        }


        public override void SetImageBitmap(Bitmap bm)
        {
            _mResource = 0;
            _mDrawable = RoundedDrawable.FromBitmap(bm);
            UpdateDrawableAttrs();
            base.SetImageDrawable(_mDrawable);
        }


        public override void SetImageResource(int resId)
        {
            if (_mResource == resId) return;

            _mResource = resId;
            _mDrawable = ResolveResource();
            UpdateDrawableAttrs();
            base.SetImageDrawable(_mDrawable);
        }

        public override void SetImageURI(global::Android.Net.Uri uri)
        {
            base.SetImageURI(uri);
            SetImageDrawable(Drawable);
        }


        private Drawable ResolveResource()
        {
            Drawable d = null;

            if (_mResource != 0)
            {
                try
                {
                    d = Resources.GetDrawable(_mResource);
                }
                catch (Exception)
                {
                    //Log.w(TAG, "Unable to find resource: " + mResource, e);
                    // Don't try again.
                    _mResource = 0;
                }
            }
            return RoundedDrawable.FromDrawable(d);
        }

        public override Drawable Background
        {
            get { return base.Background; }
            set { SetBackgroundDrawable(value); }
        }


        private void UpdateDrawableAttrs()
        {
            UpdateAttrs(_mDrawable);
        }

        private void UpdateBackgroundDrawableAttrs(bool convert)
        {
            if (_mutateBackground)
            {
                if (convert)
                {
                    _mBackgroundDrawable = RoundedDrawable.FromDrawable(_mBackgroundDrawable);
                }
                UpdateAttrs(_mBackgroundDrawable);
            }
        }

        private void UpdateAttrs(Drawable drawable)
        {
            if (drawable == null)
            {
                return;
            }

            var roundedDrawable = drawable as RoundedDrawable;
            if (roundedDrawable != null)
            {
                roundedDrawable
                    .SetScaleType(_mScaleType)
                    .SetCornerRadius(_cornerRadius)
                    .SetBorderWidth(_borderWidth)
                    .SetBorderColor(_borderColor)
                    .SetOval(_isOval);
            }
            else
            {
                var layerDrawable = drawable as LayerDrawable;
                if (layerDrawable != null)
                {
                    // loop through layers to and set drawable attrs
                    LayerDrawable ld = layerDrawable;
                    for (int i = 0, layers = ld.NumberOfLayers; i < layers; i++)
                    {
                        UpdateAttrs(ld.GetDrawable(i));
                    }
                }
            }
        }

        public override void SetBackgroundDrawable(Drawable background)
        {
            _mBackgroundDrawable = background;
            UpdateBackgroundDrawableAttrs(true);
            base.SetBackgroundDrawable(_mBackgroundDrawable);
        }

        public float GetCornerRadius()
        {
            return _cornerRadius;
        }

        public void SetCornerRadius(int resId)
        {
            SetCornerRadius(Resources.GetDimension(resId));
        }

        public void SetCornerRadius(float radius)
        {
            if (_cornerRadius == radius)
            {
                return;
            }

            _cornerRadius = radius;
            UpdateDrawableAttrs();
            UpdateBackgroundDrawableAttrs(false);
        }

        public float GetBorderWidth()
        {
            return _borderWidth;
        }

        public void SetBorderWidth(int resId)
        {
            SetBorderWidth(Resources.GetDimension(resId));
        }

        public void SetBorderWidth(float width)
        {
            if (_borderWidth == width)
            {
                return;
            }

            _borderWidth = width;
            UpdateDrawableAttrs();
            UpdateBackgroundDrawableAttrs(false);
            Invalidate();
        }

        public int GetBorderColor()
        {
            return _borderColor.DefaultColor;
        }

        public void SetBorderColor(int color)
        {
            SetBorderColor(ColorStateList.ValueOf(new Color(color)));
        }

        public ColorStateList GetBorderColors()
        {
            return _borderColor;
        }

        public void SetBorderColor(ColorStateList colors)
        {
            _borderColor =
                colors ?? ColorStateList.ValueOf(Color.Black);
            UpdateDrawableAttrs();
            UpdateBackgroundDrawableAttrs(false);
            if (_borderWidth > 0)
            {
                Invalidate();
            }
        }

        public bool IsOval()
        {
            return _isOval;
        }

        public void SetOval(bool oval)
        {
            _isOval = oval;
            UpdateDrawableAttrs();
            UpdateBackgroundDrawableAttrs(false);
            Invalidate();
        }

        public bool IsMutateBackground()
        {
            return _mutateBackground;
        }

        public void SetMutateBackground(bool mutate)
        {
            if (_mutateBackground == mutate)
            {
                return;
            }

            _mutateBackground = mutate;
            UpdateBackgroundDrawableAttrs(true);
            Invalidate();
        }
    }
}

using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace RoundedImageViewSample
{
    [Activity(Label = "RoundedImageViewSample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var roundedImage = FindViewById<ImageView>(Resource.Id.sample_picture);

            roundedImage.SetImageResource(Resource.Drawable.dog);
        }
    }
}


using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Views;

namespace Calendar.Droid
{
    [Activity(Label = "Calendar", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
			base.OnCreate(savedInstanceState);

			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

		    // https://stackoverflow.com/a/59377978
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
			{
				// for covering the full screen in android..
				Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);

				// clear FLAG_TRANSLUCENT_STATUS flag:
				Window.ClearFlags(WindowManagerFlags.TranslucentStatus);

				Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
			}

			LoadApplication(new App());
            Window.SetStatusBarColor(Android.Graphics.Color.Argb(255, 32, 32, 32));
            Window.SetNavigationBarColor(Android.Graphics.Color.Argb(255, 32, 32, 32));
        }

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
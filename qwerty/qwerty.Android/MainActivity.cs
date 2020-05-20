using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Toasts;
using Xamarin.Forms;
using Android.Support.V4.App;
using Android.Content;
using Plugin.Permissions;

namespace qwerty.Droid
{
    [Activity(Label = "qwerty", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
          
            LoadApplication(new App());
            Xamarin.FormsMaps.Init(this, savedInstanceState);
            DependencyService.Register<ToastNotification>(); // Register your dependency
           
            ToastNotification.Init(this);
       
        }



    }
}
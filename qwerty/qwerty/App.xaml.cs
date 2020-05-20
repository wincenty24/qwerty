using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using Xamarin.Essentials;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace qwerty
{
    public partial class App : Application 
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
            //MainPage = new NavigationPage(new Page1());
        }

        protected override void OnStart()
        {
          
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            //Preferences.Set("switch_wlacz_caly_program_save", false);
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
          //  MainPage.DisplayAlert("Alert", "resume", "OK");
            // Handle when your app resumes
        }
    }
}

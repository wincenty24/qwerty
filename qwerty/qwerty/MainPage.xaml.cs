using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using System.IO;
using System.Configuration;
using Xamarin.Essentials;
using Plugin.Permissions;
using Xamarin.Forms.Maps;
using System.Globalization;
using System.Threading;
using Plugin.LocalNotifications;
//using Plugin.PushNotification;
using Xamarin.Essentials;
using Plugin.Toasts;
using Android.Media;
using Android;
using Lottie;


namespace qwerty
{
    public partial class MainPage : TabbedPage
    {
        public bool czy_wyslac_powiadomienie_bool = false;
        public int ile_bylo_ostatnio_zapisanych_pojazdow = 0;
        My_Position my_position = new My_Position();
        public IList<Pin> pin_list = new List<Pin>();
        public int lol = 0;
        Pin Pin_Twoja_Pozycja = new Pin
        {
            Label = "TY",
         
            Type = PinType.Place,
            
        };
        public MainPage()
        {
            InitializeComponent();


            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");

            //            Preferences.Set("my_key", true);
            //          Preferences.Set("key", 1234);

            //Mapka.Pins.Add(pin);

        }
        private async void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            bool wlacz_timer = false;
            if(switch_wlacz_caly_program.IsToggled == false)
            {
                Main_Page_Name.BackgroundColor = Color.White;
                switch_wlacz_caly_program.ThumbColor = Color.LightYellow;
                pin_list.Clear();
                Mapka.Pins.Clear();
            }
            if (switch_wlacz_caly_program.IsToggled == true)
            {
                switch_wlacz_caly_program.ThumbColor = Color.YellowGreen;
                if (current == NetworkAccess.Internet)
                {
                    Main_Page_Name.BackgroundColor = Color.MintCream;
                    get_current_location();
                    //Mapka.Pins.Add(pin);
                    wlacz_timer = true;
                    //Timer_funcktion();
                    Device.StartTimer(TimeSpan.FromSeconds(10), Timer_funcktion);

                }
                else
                {
                    DisplayAlert("Alert", "Problem z internetem", "OK");
                    switch_wlacz_caly_program.IsToggled = false;
                }

            }
            else
            {


            }
        }

        private bool Timer_funcktion()
        {

            var current = Connectivity.NetworkAccess;
            string wynik;
            if (switch_wlacz_caly_program.IsToggled == true)
            {
                if (current == NetworkAccess.Internet)
                {
                    Main_Page_Name.BackgroundColor = Color.Azure;
                    // map.Pins.Add(pin);

                    try
                    {
                        get_current_location();
                        var HTTPrequest = (HttpWebRequest)WebRequest.Create("https://uwaga-karetka.firebaseio.com/POJAZDY!/.json");
                        var Response = (HttpWebResponse)HTTPrequest.GetResponse();
                        var StreamReader = new StreamReader(Response.GetResponseStream()).ReadToEnd();
                        if (StreamReader.ToString() != "null")
                        {
                            int p = 0;
                            pin_list.Clear();
                            Mapka.Pins.Clear();
                            wynik = Return_localization(StreamReader);
                            string[] pozycje_string = wynik.Split(' ');
                            double[] pozycje_double = new double[pozycje_string.Length]; //{ 51.210957, 17.343894, 51.188823, 17.109046, 51.109910, 17.054682 };
                            for (int i = 0; i < pozycje_string.Length; i++)
                            {
                                //string qwe = pozycje_string[i];
                                pozycje_double[i] = Convert.ToDouble(pozycje_string[i], CultureInfo.GetCultureInfo("en-GB").NumberFormat);

                                // pozycje_double[i] = double.Parse(pozycje_string[i], CultureInfo.InvariantCulture);
                            }
                           if(ile_bylo_ostatnio_zapisanych_pojazdow != pozycje_string.Length / 2 )
                            {
                                ile_bylo_ostatnio_zapisanych_pojazdow = pozycje_string.Length / 2;
                                if (switch_wibrations.IsToggled==true)
                                {
                                    Vibration.Vibrate(TimeSpan.FromSeconds(1));
                                }
                                if (switch_notifications.IsToggled == true)
                                {
                                    CrossLocalNotifications.Current.Show("UWAGA", $"W twoim otoczeniu jest {ile_bylo_ostatnio_zapisanych_pojazdow} pojazdów", 1);
                                }
                                
                            }
                            for (int i = 0; i < (pozycje_string.Length / 2); i++)
                            {

                                try
                                {
                                    pin_list.Add(new Pin());
                                    pin_list[i].Label = "pojazd";
                                    pin_list[i].Position = new Position(pozycje_double[p], pozycje_double[1 + p]);
                                    double odleglosc = GetDistanceFromLatLonInKm(my_position.Latitude, my_position.Longitude, pozycje_double[p], pozycje_double[1 + p]);
                                    if (odleglosc < 2.0 && switch_show_me_all_ambulance_switch.IsToggled==false)
                                    {
                                        Mapka.Pins.Add(pin_list[i]);
                                    }
                                    else if (switch_show_me_all_ambulance_switch.IsToggled == true)
                                    {
                                        Mapka.Pins.Add(pin_list[i]);
                                    }
                                    p += 2;
                                }
                                catch
                                {
                                   
                                    DisplayAlert("Alert", "Peoblem z wyznaczeniem pozycji elementów", "OK");
                                }




                            }
                            Mapka.Pins.Add(Pin_Twoja_Pozycja);
                        }
                    }
                    catch
                    {
                        DisplayAlert("Alert", "Błąd!", "OK");
                    }


                }
                else
                {
                    Main_Page_Name.BackgroundColor = Color.Red;
                    OnAlertYesNoClicked();
                    //DisplayAlert("Alert", "Problem z internetem", "OK");
                }

            }
            return switch_wlacz_caly_program.IsToggled;
        }

        private async void get_current_location()
        {

            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location != null)
            {
                my_position.Latitude = location.Latitude;
                my_position.Longitude = location.Longitude;
                //Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");

                double i = 0.030000;
                Polygon polygon = new Polygon
                {
                    StrokeWidth = 8,
                    StrokeColor = Color.FromHex("#1BA1E2"),
                    FillColor = Color.FromHex("#881BA1E2"),
                    Geopath =
                        {
                        new Position(location.Latitude - i*0, location.Longitude -i*1),
new Position(location.Latitude - i*0.0174524064372835, location.Longitude -i*0.999847695156391),
new Position(location.Latitude - i*0.034899496702501, location.Longitude -i*0.999390827019096),
new Position(location.Latitude - i*0.0523359562429438, location.Longitude -i*0.998629534754574),
new Position(location.Latitude - i*0.0697564737441253, location.Longitude -i*0.997564050259824),
new Position(location.Latitude - i*0.0871557427476582, location.Longitude -i*0.996194698091746),
new Position(location.Latitude - i*0.104528463267653, location.Longitude -i*0.994521895368273),
new Position(location.Latitude - i*0.121869343405147, location.Longitude -i*0.992546151641322),
new Position(location.Latitude - i*0.139173100960065, location.Longitude -i*0.99026806874157),
new Position(location.Latitude - i*0.156434465040231, location.Longitude -i*0.987688340595138),
new Position(location.Latitude - i*0.17364817766693, location.Longitude -i*0.984807753012208),
new Position(location.Latitude - i*0.190808995376545, location.Longitude -i*0.981627183447664),
new Position(location.Latitude - i*0.207911690817759, location.Longitude -i*0.978147600733806),
new Position(location.Latitude - i*0.224951054343865, location.Longitude -i*0.974370064785235),
new Position(location.Latitude - i*0.241921895599668, location.Longitude -i*0.970295726275996),
new Position(location.Latitude - i*0.258819045102521, location.Longitude -i*0.965925826289068),
new Position(location.Latitude - i*0.275637355816999, location.Longitude -i*0.961261695938319),
new Position(location.Latitude - i*0.292371704722737, location.Longitude -i*0.956304755963035),
new Position(location.Latitude - i*0.309016994374947, location.Longitude -i*0.951056516295154),
new Position(location.Latitude - i*0.325568154457157, location.Longitude -i*0.945518575599317),
new Position(location.Latitude - i*0.342020143325669, location.Longitude -i*0.939692620785908),
new Position(location.Latitude - i*0.3583679495453, location.Longitude -i*0.933580426497202),
new Position(location.Latitude - i*0.374606593415912, location.Longitude -i*0.927183854566787),
new Position(location.Latitude - i*0.390731128489274, location.Longitude -i*0.92050485345244),
new Position(location.Latitude - i*0.4067366430758, location.Longitude -i*0.913545457642601),
new Position(location.Latitude - i*0.422618261740699, location.Longitude -i*0.90630778703665),
new Position(location.Latitude - i*0.438371146789077, location.Longitude -i*0.898794046299167),
new Position(location.Latitude - i*0.453990499739547, location.Longitude -i*0.891006524188368),
new Position(location.Latitude - i*0.469471562785891, location.Longitude -i*0.882947592858927),
new Position(location.Latitude - i*0.484809620246337, location.Longitude -i*0.874619707139396),
new Position(location.Latitude - i*0.5, location.Longitude -i*0.866025403784439),
new Position(location.Latitude - i*0.515038074910054, location.Longitude -i*0.857167300702112),
new Position(location.Latitude - i*0.529919264233205, location.Longitude -i*0.848048096156426),
new Position(location.Latitude - i*0.544639035015027, location.Longitude -i*0.838670567945424),
new Position(location.Latitude - i*0.559192903470747, location.Longitude -i*0.829037572555042),
new Position(location.Latitude - i*0.573576436351046, location.Longitude -i*0.819152044288992),
new Position(location.Latitude - i*0.587785252292473, location.Longitude -i*0.809016994374947),
new Position(location.Latitude - i*0.601815023152048, location.Longitude -i*0.798635510047293),
new Position(location.Latitude - i*0.615661475325658, location.Longitude -i*0.788010753606722),
new Position(location.Latitude - i*0.629320391049837, location.Longitude -i*0.777145961456971),
new Position(location.Latitude - i*0.642787609686539, location.Longitude -i*0.766044443118978),
new Position(location.Latitude - i*0.656059028990507, location.Longitude -i*0.754709580222772),
new Position(location.Latitude - i*0.669130606358858, location.Longitude -i*0.743144825477394),
new Position(location.Latitude - i*0.681998360062498, location.Longitude -i*0.73135370161917),
new Position(location.Latitude - i*0.694658370458997, location.Longitude -i*0.719339800338651),
new Position(location.Latitude - i*0.707106781186547, location.Longitude -i*0.707106781186547),
new Position(location.Latitude - i*0.719339800338651, location.Longitude -i*0.694658370458997),
new Position(location.Latitude - i*0.73135370161917, location.Longitude -i*0.681998360062498),
new Position(location.Latitude - i*0.743144825477394, location.Longitude -i*0.669130606358858),
new Position(location.Latitude - i*0.754709580222772, location.Longitude -i*0.656059028990507),
new Position(location.Latitude - i*0.766044443118978, location.Longitude -i*0.642787609686539),
new Position(location.Latitude - i*0.777145961456971, location.Longitude -i*0.629320391049837),
new Position(location.Latitude - i*0.788010753606722, location.Longitude -i*0.615661475325658),
new Position(location.Latitude - i*0.798635510047293, location.Longitude -i*0.601815023152048),
new Position(location.Latitude - i*0.809016994374947, location.Longitude -i*0.587785252292473),
new Position(location.Latitude - i*0.819152044288992, location.Longitude -i*0.573576436351046),
new Position(location.Latitude - i*0.829037572555042, location.Longitude -i*0.559192903470747),
new Position(location.Latitude - i*0.838670567945424, location.Longitude -i*0.544639035015027),
new Position(location.Latitude - i*0.848048096156426, location.Longitude -i*0.529919264233205),
new Position(location.Latitude - i*0.857167300702112, location.Longitude -i*0.515038074910054),
new Position(location.Latitude - i*0.866025403784439, location.Longitude -i*0.5),
new Position(location.Latitude - i*0.874619707139396, location.Longitude -i*0.484809620246337),
new Position(location.Latitude - i*0.882947592858927, location.Longitude -i*0.469471562785891),
new Position(location.Latitude - i*0.891006524188368, location.Longitude -i*0.453990499739547),
new Position(location.Latitude - i*0.898794046299167, location.Longitude -i*0.438371146789077),
new Position(location.Latitude - i*0.90630778703665, location.Longitude -i*0.422618261740699),
new Position(location.Latitude - i*0.913545457642601, location.Longitude -i*0.4067366430758),
new Position(location.Latitude - i*0.92050485345244, location.Longitude -i*0.390731128489274),
new Position(location.Latitude - i*0.927183854566787, location.Longitude -i*0.374606593415912),
new Position(location.Latitude - i*0.933580426497202, location.Longitude -i*0.3583679495453),
new Position(location.Latitude - i*0.939692620785908, location.Longitude -i*0.342020143325669),
new Position(location.Latitude - i*0.945518575599317, location.Longitude -i*0.325568154457157),
new Position(location.Latitude - i*0.951056516295154, location.Longitude -i*0.309016994374947),
new Position(location.Latitude - i*0.956304755963035, location.Longitude -i*0.292371704722737),
new Position(location.Latitude - i*0.961261695938319, location.Longitude -i*0.275637355816999),
new Position(location.Latitude - i*0.965925826289068, location.Longitude -i*0.258819045102521),
new Position(location.Latitude - i*0.970295726275996, location.Longitude -i*0.241921895599668),
new Position(location.Latitude - i*0.974370064785235, location.Longitude -i*0.224951054343865),
new Position(location.Latitude - i*0.978147600733806, location.Longitude -i*0.207911690817759),
new Position(location.Latitude - i*0.981627183447664, location.Longitude -i*0.190808995376545),
new Position(location.Latitude - i*0.984807753012208, location.Longitude -i*0.17364817766693),
new Position(location.Latitude - i*0.987688340595138, location.Longitude -i*0.156434465040231),
new Position(location.Latitude - i*0.99026806874157, location.Longitude -i*0.139173100960065),
new Position(location.Latitude - i*0.992546151641322, location.Longitude -i*0.121869343405147),
new Position(location.Latitude - i*0.994521895368273, location.Longitude -i*0.104528463267653),
new Position(location.Latitude - i*0.996194698091746, location.Longitude -i*0.0871557427476582),
new Position(location.Latitude - i*0.997564050259824, location.Longitude -i*0.0697564737441253),
new Position(location.Latitude - i*0.998629534754574, location.Longitude -i*0.0523359562429438),
new Position(location.Latitude - i*0.999390827019096, location.Longitude -i*0.034899496702501),
new Position(location.Latitude - i*0.999847695156391, location.Longitude -i*0.0174524064372835),
                        new Position(location.Latitude + i*0, location.Longitude - i*1),
new Position(location.Latitude + i*0.0174524064372835, location.Longitude - i*0.999847695156391),
new Position(location.Latitude + i*0.034899496702501, location.Longitude - i*0.999390827019096),
new Position(location.Latitude + i*0.0523359562429438, location.Longitude - i*0.998629534754574),
new Position(location.Latitude + i*0.0697564737441253, location.Longitude - i*0.997564050259824),
new Position(location.Latitude + i*0.0871557427476582, location.Longitude - i*0.996194698091746),
new Position(location.Latitude + i*0.104528463267653, location.Longitude - i*0.994521895368273),
new Position(location.Latitude + i*0.121869343405147, location.Longitude - i*0.992546151641322),
new Position(location.Latitude + i*0.139173100960065, location.Longitude - i*0.99026806874157),
new Position(location.Latitude + i*0.156434465040231, location.Longitude - i*0.987688340595138),
new Position(location.Latitude + i*0.17364817766693, location.Longitude - i*0.984807753012208),
new Position(location.Latitude + i*0.190808995376545, location.Longitude - i*0.981627183447664),
new Position(location.Latitude + i*0.207911690817759, location.Longitude - i*0.978147600733806),
new Position(location.Latitude + i*0.224951054343865, location.Longitude - i*0.974370064785235),
new Position(location.Latitude + i*0.241921895599668, location.Longitude - i*0.970295726275996),
new Position(location.Latitude + i*0.258819045102521, location.Longitude - i*0.965925826289068),
new Position(location.Latitude + i*0.275637355816999, location.Longitude - i*0.961261695938319),
new Position(location.Latitude + i*0.292371704722737, location.Longitude - i*0.956304755963035),
new Position(location.Latitude + i*0.309016994374947, location.Longitude - i*0.951056516295154),
new Position(location.Latitude + i*0.325568154457157, location.Longitude - i*0.945518575599317),
new Position(location.Latitude + i*0.342020143325669, location.Longitude - i*0.939692620785908),
new Position(location.Latitude + i*0.3583679495453, location.Longitude - i*0.933580426497202),
new Position(location.Latitude + i*0.374606593415912, location.Longitude - i*0.927183854566787),
new Position(location.Latitude + i*0.390731128489274, location.Longitude - i*0.92050485345244),
new Position(location.Latitude + i*0.4067366430758, location.Longitude - i*0.913545457642601),
new Position(location.Latitude + i*0.422618261740699, location.Longitude - i*0.90630778703665),
new Position(location.Latitude + i*0.438371146789077, location.Longitude - i*0.898794046299167),
new Position(location.Latitude + i*0.453990499739547, location.Longitude - i*0.891006524188368),
new Position(location.Latitude + i*0.469471562785891, location.Longitude - i*0.882947592858927),
new Position(location.Latitude + i*0.484809620246337, location.Longitude - i*0.874619707139396),
new Position(location.Latitude + i*0.5, location.Longitude - i*0.866025403784439),
new Position(location.Latitude + i*0.515038074910054, location.Longitude - i*0.857167300702112),
new Position(location.Latitude + i*0.529919264233205, location.Longitude - i*0.848048096156426),
new Position(location.Latitude + i*0.544639035015027, location.Longitude - i*0.838670567945424),
new Position(location.Latitude + i*0.559192903470747, location.Longitude - i*0.829037572555042),
new Position(location.Latitude + i*0.573576436351046, location.Longitude - i*0.819152044288992),
new Position(location.Latitude + i*0.587785252292473, location.Longitude - i*0.809016994374947),
new Position(location.Latitude + i*0.601815023152048, location.Longitude - i*0.798635510047293),
new Position(location.Latitude + i*0.615661475325658, location.Longitude - i*0.788010753606722),
new Position(location.Latitude + i*0.629320391049837, location.Longitude - i*0.777145961456971),
new Position(location.Latitude + i*0.642787609686539, location.Longitude - i*0.766044443118978),
new Position(location.Latitude + i*0.656059028990507, location.Longitude - i*0.754709580222772),
new Position(location.Latitude + i*0.669130606358858, location.Longitude - i*0.743144825477394),
new Position(location.Latitude + i*0.681998360062498, location.Longitude - i*0.73135370161917),
new Position(location.Latitude + i*0.694658370458997, location.Longitude - i*0.719339800338651),
new Position(location.Latitude + i*0.707106781186547, location.Longitude - i*0.707106781186547),
new Position(location.Latitude + i*0.719339800338651, location.Longitude - i*0.694658370458997),
new Position(location.Latitude + i*0.73135370161917, location.Longitude - i*0.681998360062498),
new Position(location.Latitude + i*0.743144825477394, location.Longitude - i*0.669130606358858),
new Position(location.Latitude + i*0.754709580222772, location.Longitude - i*0.656059028990507),
new Position(location.Latitude + i*0.766044443118978, location.Longitude - i*0.642787609686539),
new Position(location.Latitude + i*0.777145961456971, location.Longitude - i*0.629320391049837),
new Position(location.Latitude + i*0.788010753606722, location.Longitude - i*0.615661475325658),
new Position(location.Latitude + i*0.798635510047293, location.Longitude - i*0.601815023152048),
new Position(location.Latitude + i*0.809016994374947, location.Longitude - i*0.587785252292473),
new Position(location.Latitude + i*0.819152044288992, location.Longitude - i*0.573576436351046),
new Position(location.Latitude + i*0.829037572555042, location.Longitude - i*0.559192903470747),
new Position(location.Latitude + i*0.838670567945424, location.Longitude - i*0.544639035015027),
new Position(location.Latitude + i*0.848048096156426, location.Longitude - i*0.529919264233205),
new Position(location.Latitude + i*0.857167300702112, location.Longitude - i*0.515038074910054),
new Position(location.Latitude + i*0.866025403784439, location.Longitude - i*0.5),
new Position(location.Latitude + i*0.874619707139396, location.Longitude - i*0.484809620246337),
new Position(location.Latitude + i*0.882947592858927, location.Longitude - i*0.469471562785891),
new Position(location.Latitude + i*0.891006524188368, location.Longitude - i*0.453990499739547),
new Position(location.Latitude + i*0.898794046299167, location.Longitude - i*0.438371146789077),
new Position(location.Latitude + i*0.90630778703665, location.Longitude - i*0.422618261740699),
new Position(location.Latitude + i*0.913545457642601, location.Longitude - i*0.4067366430758),
new Position(location.Latitude + i*0.92050485345244, location.Longitude - i*0.390731128489274),
new Position(location.Latitude + i*0.927183854566787, location.Longitude - i*0.374606593415912),
new Position(location.Latitude + i*0.933580426497202, location.Longitude - i*0.3583679495453),
new Position(location.Latitude + i*0.939692620785908, location.Longitude - i*0.342020143325669),
new Position(location.Latitude + i*0.945518575599317, location.Longitude - i*0.325568154457157),
new Position(location.Latitude + i*0.951056516295154, location.Longitude - i*0.309016994374947),
new Position(location.Latitude + i*0.956304755963035, location.Longitude - i*0.292371704722737),
new Position(location.Latitude + i*0.961261695938319, location.Longitude - i*0.275637355816999),
new Position(location.Latitude + i*0.965925826289068, location.Longitude - i*0.258819045102521),
new Position(location.Latitude + i*0.970295726275996, location.Longitude - i*0.241921895599668),
new Position(location.Latitude + i*0.974370064785235, location.Longitude - i*0.224951054343865),
new Position(location.Latitude + i*0.978147600733806, location.Longitude - i*0.207911690817759),
new Position(location.Latitude + i*0.981627183447664, location.Longitude - i*0.190808995376545),
new Position(location.Latitude + i*0.984807753012208, location.Longitude - i*0.17364817766693),
new Position(location.Latitude + i*0.987688340595138, location.Longitude - i*0.156434465040231),
new Position(location.Latitude + i*0.99026806874157, location.Longitude - i*0.139173100960065),
new Position(location.Latitude + i*0.992546151641322, location.Longitude - i*0.121869343405147),
new Position(location.Latitude + i*0.994521895368273, location.Longitude - i*0.104528463267653),
new Position(location.Latitude + i*0.996194698091746, location.Longitude - i*0.0871557427476582),
new Position(location.Latitude + i*0.997564050259824, location.Longitude - i*0.0697564737441253),
new Position(location.Latitude + i*0.998629534754574, location.Longitude - i*0.0523359562429438),
new Position(location.Latitude + i*0.999390827019096, location.Longitude - i*0.034899496702501),
new Position(location.Latitude + i*0.999847695156391, location.Longitude - i*0.0174524064372835),
new Position(location.Latitude + i*1, location.Longitude + i*0),
new Position(location.Latitude + i*0.999847695156391, location.Longitude + i*0.0174524064372835),
new Position(location.Latitude + i*0.999390827019096, location.Longitude + i*0.034899496702501),
new Position(location.Latitude + i*0.998629534754574, location.Longitude + i*0.0523359562429438),
new Position(location.Latitude + i*0.997564050259824, location.Longitude + i*0.0697564737441253),
new Position(location.Latitude + i*0.996194698091746, location.Longitude + i*0.0871557427476582),
new Position(location.Latitude + i*0.994521895368273, location.Longitude + i*0.104528463267653),
new Position(location.Latitude + i*0.992546151641322, location.Longitude + i*0.121869343405147),
new Position(location.Latitude + i*0.99026806874157, location.Longitude + i*0.139173100960065),
new Position(location.Latitude + i*0.987688340595138, location.Longitude + i*0.156434465040231),
new Position(location.Latitude + i*0.984807753012208, location.Longitude + i*0.17364817766693),
new Position(location.Latitude + i*0.981627183447664, location.Longitude + i*0.190808995376545),
new Position(location.Latitude + i*0.978147600733806, location.Longitude + i*0.207911690817759),
new Position(location.Latitude + i*0.974370064785235, location.Longitude + i*0.224951054343865),
new Position(location.Latitude + i*0.970295726275996, location.Longitude + i*0.241921895599668),
new Position(location.Latitude + i*0.965925826289068, location.Longitude + i*0.258819045102521),
new Position(location.Latitude + i*0.961261695938319, location.Longitude + i*0.275637355816999),
new Position(location.Latitude + i*0.956304755963035, location.Longitude + i*0.292371704722737),
new Position(location.Latitude + i*0.951056516295154, location.Longitude + i*0.309016994374947),
new Position(location.Latitude + i*0.945518575599317, location.Longitude + i*0.325568154457157),
new Position(location.Latitude + i*0.939692620785908, location.Longitude + i*0.342020143325669),
new Position(location.Latitude + i*0.933580426497202, location.Longitude + i*0.3583679495453),
new Position(location.Latitude + i*0.927183854566787, location.Longitude + i*0.374606593415912),
new Position(location.Latitude + i*0.92050485345244, location.Longitude + i*0.390731128489274),
new Position(location.Latitude + i*0.913545457642601, location.Longitude + i*0.4067366430758),
new Position(location.Latitude + i*0.90630778703665, location.Longitude + i*0.422618261740699),
new Position(location.Latitude + i*0.898794046299167, location.Longitude + i*0.438371146789077),
new Position(location.Latitude + i*0.891006524188368, location.Longitude + i*0.453990499739547),
new Position(location.Latitude + i*0.882947592858927, location.Longitude + i*0.469471562785891),
new Position(location.Latitude + i*0.874619707139396, location.Longitude + i*0.484809620246337),
new Position(location.Latitude + i*0.866025403784439, location.Longitude + i*0.5),
new Position(location.Latitude + i*0.857167300702112, location.Longitude + i*0.515038074910054),
new Position(location.Latitude + i*0.848048096156426, location.Longitude + i*0.529919264233205),
new Position(location.Latitude + i*0.838670567945424, location.Longitude + i*0.544639035015027),
new Position(location.Latitude + i*0.829037572555042, location.Longitude + i*0.559192903470747),
new Position(location.Latitude + i*0.819152044288992, location.Longitude + i*0.573576436351046),
new Position(location.Latitude + i*0.809016994374947, location.Longitude + i*0.587785252292473),
new Position(location.Latitude + i*0.798635510047293, location.Longitude + i*0.601815023152048),
new Position(location.Latitude + i*0.788010753606722, location.Longitude + i*0.615661475325658),
new Position(location.Latitude + i*0.777145961456971, location.Longitude + i*0.629320391049837),
new Position(location.Latitude + i*0.766044443118978, location.Longitude + i*0.642787609686539),
new Position(location.Latitude + i*0.754709580222772, location.Longitude + i*0.656059028990507),
new Position(location.Latitude + i*0.743144825477394, location.Longitude + i*0.669130606358858),
new Position(location.Latitude + i*0.73135370161917, location.Longitude + i*0.681998360062498),
new Position(location.Latitude + i*0.719339800338651, location.Longitude + i*0.694658370458997),
new Position(location.Latitude + i*0.707106781186547, location.Longitude + i*0.707106781186547),
new Position(location.Latitude + i*0.694658370458997, location.Longitude + i*0.719339800338651),
new Position(location.Latitude + i*0.681998360062498, location.Longitude + i*0.73135370161917),
new Position(location.Latitude + i*0.669130606358858, location.Longitude + i*0.743144825477394),
new Position(location.Latitude + i*0.656059028990507, location.Longitude + i*0.754709580222772),
new Position(location.Latitude + i*0.642787609686539, location.Longitude + i*0.766044443118978),
new Position(location.Latitude + i*0.629320391049837, location.Longitude + i*0.777145961456971),
new Position(location.Latitude + i*0.615661475325658, location.Longitude + i*0.788010753606722),
new Position(location.Latitude + i*0.601815023152048, location.Longitude + i*0.798635510047293),
new Position(location.Latitude + i*0.587785252292473, location.Longitude + i*0.809016994374947),
new Position(location.Latitude + i*0.573576436351046, location.Longitude + i*0.819152044288992),
new Position(location.Latitude + i*0.559192903470747, location.Longitude + i*0.829037572555042),
new Position(location.Latitude + i*0.544639035015027, location.Longitude + i*0.838670567945424),
new Position(location.Latitude + i*0.529919264233205, location.Longitude + i*0.848048096156426),
new Position(location.Latitude + i*0.515038074910054, location.Longitude + i*0.857167300702112),
new Position(location.Latitude + i*0.5, location.Longitude + i*0.866025403784439),
new Position(location.Latitude + i*0.484809620246337, location.Longitude + i*0.874619707139396),
new Position(location.Latitude + i*0.469471562785891, location.Longitude + i*0.882947592858927),
new Position(location.Latitude + i*0.453990499739547, location.Longitude + i*0.891006524188368),
new Position(location.Latitude + i*0.438371146789077, location.Longitude + i*0.898794046299167),
new Position(location.Latitude + i*0.422618261740699, location.Longitude + i*0.90630778703665),
new Position(location.Latitude + i*0.4067366430758, location.Longitude + i*0.913545457642601),
new Position(location.Latitude + i*0.390731128489274, location.Longitude + i*0.92050485345244),
new Position(location.Latitude + i*0.374606593415912, location.Longitude + i*0.927183854566787),
new Position(location.Latitude + i*0.3583679495453, location.Longitude + i*0.933580426497202),
new Position(location.Latitude + i*0.342020143325669, location.Longitude + i*0.939692620785908),
new Position(location.Latitude + i*0.325568154457157, location.Longitude + i*0.945518575599317),
new Position(location.Latitude + i*0.309016994374947, location.Longitude + i*0.951056516295154),
new Position(location.Latitude + i*0.292371704722737, location.Longitude + i*0.956304755963035),
new Position(location.Latitude + i*0.275637355816999, location.Longitude + i*0.961261695938319),
new Position(location.Latitude + i*0.258819045102521, location.Longitude + i*0.965925826289068),
new Position(location.Latitude + i*0.241921895599668, location.Longitude + i*0.970295726275996),
new Position(location.Latitude + i*0.224951054343865, location.Longitude + i*0.974370064785235),
new Position(location.Latitude + i*0.207911690817759, location.Longitude + i*0.978147600733806),
new Position(location.Latitude + i*0.190808995376545, location.Longitude + i*0.981627183447664),
new Position(location.Latitude + i*0.17364817766693, location.Longitude + i*0.984807753012208),
new Position(location.Latitude + i*0.156434465040231, location.Longitude + i*0.987688340595138),
new Position(location.Latitude + i*0.139173100960065, location.Longitude + i*0.99026806874157),
new Position(location.Latitude + i*0.121869343405147, location.Longitude + i*0.992546151641322),
new Position(location.Latitude + i*0.104528463267653, location.Longitude + i*0.994521895368273),
new Position(location.Latitude + i*0.0871557427476582, location.Longitude + i*0.996194698091746),
new Position(location.Latitude + i*0.0697564737441253, location.Longitude + i*0.997564050259824),
new Position(location.Latitude + i*0.0523359562429438, location.Longitude + i*0.998629534754574),
new Position(location.Latitude + i*0.034899496702501, location.Longitude + i*0.999390827019096),
new Position(location.Latitude + i*0.0174524064372835, location.Longitude + i*0.999847695156391),

                        }
                };
                Pin_Twoja_Pozycja.Position = new Position(my_position.Latitude, my_position.Longitude);
                Mapka.Pins.Add(Pin_Twoja_Pozycja);
                Mapka.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(location.Latitude, location.Longitude), Distance.FromKilometers(100)).WithZoom(Slider_zoom.Value));

               
                //label_pozycje_pojazdow.Text = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}";
                // Mapka.MapElements.Add(polygon);
            }
        }

        private string Return_localization(string dane)
        {
            string return_polozenia = "";
            string x;
            //  string x= "{pojazd1:{asd:51.210957$17.343894},pojazd2:{asd:51.188823$17.109046},pojazd3:{asd:51.109910$17.054682}}";
            x = dane.Replace("\"", "").Replace("{", "").Replace("}", "").Replace(":", " ").Replace(",", " ").Replace("$", " ");
            //Console.WriteLine(x);
            int p = 0;
            string[] y = x.Split(' ');
            foreach (string r in y)
            {
                //Console.WriteLine(r);
            }
            for (int i = 0; (i < y.Length / 4); i++)
            {
                if (i == 0)
                {
                    return_polozenia = y[p + 2] + " " + y[p + 3];
                }
                else if (i != 0)
                {
                    return_polozenia = return_polozenia + " " + y[p + 2] + " " + y[p + 3];
                }

                p += 4;

            }

            return return_polozenia;
        }
        double GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);  // deg2rad below
            var dLon = Deg2Rad(lon2 - lon1);
            var a = Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
            var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
            var d = R * c; // Distance in km
            return d;
        }
        double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180d);
        }      
        private async void OnAlertYesNoClicked()/////////////////zmień to dałnie
        {
            pin_list.Clear();
            Mapka.Pins.Clear();
        }
        private void Slider_zoom_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Mapka.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(my_position.Latitude, my_position.Longitude), Distance.FromKilometers(100)).WithZoom(Slider_zoom.Value));
            Zoom_label.Text = "Zoom " +Convert.ToString((int)Slider_zoom.Value);
        }
        private void TabbedPage_Appearing(object sender, EventArgs e)
        {
           
            Slider_zoom.Value = (float)Convert.ToDouble(Preferences.Get("zoom_value_save", "30"));
            switch_notifications.IsToggled = (bool)Convert.ToBoolean(Preferences.Get("notifications_setting", true));
            switch_wibrations.IsToggled = (bool)Convert.ToBoolean(Preferences.Get("vibrattions_setting", true));
        }
        private void Switch_show_me_all_ambulance_switch_Toggled(object sender, ToggledEventArgs e)
        {

        }
        private void TabbedPage_Disappearing(object sender, EventArgs e)
        {
            
        }//zamykanie

        private void Switch_wibrations_Toggled(object sender, ToggledEventArgs e)
        {

        }

        private void Switch_notifications_Toggled(object sender, ToggledEventArgs e)
        {

        }

        private async void  Save_Buton_Clicked(object sender, EventArgs e)
        {
            
            Preferences.Set("zoom_value_save", Slider_zoom.Value);
            Preferences.Set("vibrattions_setting", switch_wibrations.IsToggled);
            Preferences.Set("notifications_setting", switch_notifications.IsToggled);
            //Preferences.Set("zoom_value_save", Slider_zoom.Value);

            
            animationView.HeightRequest = 300;
            animationView.WidthRequest = 300;
            animationView.Play();
            
            
        }

        private void Handle_OnFinish(object sender, EventArgs e)
        {
            animationView.HeightRequest = 0;
            animationView.WidthRequest = 0;
        }
    }
    class My_Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
}

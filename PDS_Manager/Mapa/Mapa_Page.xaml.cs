using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMap.NET.WindowsForms;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;
using System.ComponentModel;
using System.Windows.Media;

namespace PDS_Manager.Mapa
{
    /// <summary>
    /// Interaction logic for Mapa_Page.xaml
    /// </summary>
    public partial class Mapa_Page : Page
    {
        bool zavriet;
        GMapControl gMapControl;
        GeoCoderStatusCode statusCode;
        GeocodingProvider geocodingProvider;
        GMapOverlay markers;
        GMapOverlay routes;
        public bool Zavriet { get => zavriet; set => zavriet = value; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public GMapControl GMapControl { get => gMapControl; set => gMapControl = value; }
        public GMapOverlay Routes { get => routes; set => routes = value; }
        public GMapOverlay Markers { get => markers; set => markers = value; }

        public Mapa_Page()
        {
            InitializeComponent();
            DataContext = this;
            Zavriet = false; // pre zavretie okna, ale nie uplne vypnutie

            // Window form vo WPF.
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

            //Gmap zakladne nastavenia
            GMapControl = new GMap.NET.WindowsForms.GMapControl();
            GMapControl.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            geocodingProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            statusCode = GeoCoderStatusCode.Unknow;

            Markers = new GMapOverlay("Markers");
            Routes = new GMapOverlay("Routes");
            GMapControl.Overlays.Add(Markers);
            GMapControl.Overlays.Add(Routes);


            host.Child = GMapControl;
            MainGrid.Children.Add(host);
            GMapControl.MaxZoom = 25;
            GMapControl.Zoom = 5;

            gMapControl.Position = geocodingProvider.GetPoint("Bratislava", out statusCode).Value;
            GMapControl.ShowCenter = false;

        }



        public void pridajMarker(string adresa, GMarkerGoogleType typ)
        {
            GMapMarker marker = new GMarkerGoogle(
            geocodingProvider.GetPoint(adresa, out statusCode).Value,
            typ);
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            marker.ToolTipText = adresa;
            Markers.Markers.Add(marker);
        }

        public void VycistiMapu()
        {
            gMapControl.Overlays.Remove(Markers);
            gMapControl.Overlays.Remove(Routes);

            Markers = new GMapOverlay("Markers");
            Routes = new GMapOverlay("Routes");
            GMapControl.Overlays.Add(Markers);
            GMapControl.Overlays.Add(Routes);
        }
        public void PridajMarker(PointLatLng adresa, String adresaS, GMarkerGoogleType typ)
        {
            GMapMarker marker = new GMarkerGoogle(adresa, typ);
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            marker.ToolTipText = adresaS;
            Markers.Markers.Add(marker);
        }


        public GMapRoute pridajCestu(string startAdresa, string endAdresa, List<string> listAdries, System.Drawing.Color color, int strokeWidth)
        {
            List<PointLatLng> listAdriesLatLng = new List<PointLatLng>();
            foreach (String adresa in listAdries)
            {
                try
                {
                    listAdriesLatLng.Add(geocodingProvider.GetPoint(adresa, out statusCode).Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Nenašla sa adresa " + adresa);
                }
            }

            try
            {
                MapRoute route = GMap.NET.MapProviders.OpenStreetMapProvider.Instance.GetRoute(listAdriesLatLng.First(), listAdriesLatLng.Last(), true, false, 10);
                //.Instance.GetRoute(geocodingProvider.GetPoint(startAdresa, out statusCode).Value,
                //geocodingProvider.GetPoint(endAdresa, out statusCode).Value, listAdriesLatLng, false, false, 15, false);
                GMapRoute r = new GMapRoute(route.Points, "My route");

                var routes = new GMapOverlay("Route");
                Routes.Routes.Add(r);

                foreach (var adresa in listAdries)
                {
                    pridajMarker(adresa, GMarkerGoogleType.blue_dot);
                }

                pridajMarker(startAdresa, GMarkerGoogleType.blue_dot);
                pridajMarker(endAdresa, GMarkerGoogleType.blue_dot);

                r.Stroke.Width = strokeWidth;
                r.Stroke.Color = color;

                return r;

            }
            catch (Exception)
            {
                MessageBox.Show("Nepodarilo sa vygenerovať mapu");
                return null;
            }

        }

        public void PridajMarkre(List<PointLatLng> listAdries)
        {

            foreach (var adresa in listAdries)
            {
                PridajMarker(adresa, $"Zemepisná šŕiaka: {adresa.Lat}\nZemepisná dĺžka{adresa.Lng}", GMarkerGoogleType.blue_dot);
            }


        }

        private void CloseButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;
            window.Close();
        }

        private void MinimizeButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;
            window.WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;
            window.WindowState = System.Windows.WindowState.Maximized;
        }
    }
}

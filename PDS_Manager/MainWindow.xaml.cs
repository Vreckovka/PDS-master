using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IO;
using System.Drawing;

namespace PDS_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OracleConnection orcl;
        private Mapa.Mapa_Page mapa;
        public MainWindow()
        {
            InitializeComponent();

            string oradb = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = obelix.fri.uniza.sk)" +
                   "(PORT = 1521))" +
                   "(CONNECT_DATA = (SERVICE_NAME = orcl.fri.uniza.sk" + "))); " +
                   "User Id = nad; " +
                   "Password = " + "pato1303";
            orcl = new OracleConnection(oradb);
            orcl.Open();
            NacitajVodicov();
            DataContext = this;
            SkryDatagridy();

            mapa = new Mapa.Mapa_Page();
            Frame_mapa.Content = mapa;
        }

        private void NacitajVodicov()
        {
            //using (OracleCommand cmd = new OracleCommand("select * from vodic where rod_cislo <= '0000000100' OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY", orcl))
            //{
            //    using (OracleDataReader reader = cmd.ExecuteReader())
            //    {
            //        DataTable dataTable = new DataTable();
            //        dataTable.Load(reader);


            //        foreach (DataRow item in dataTable.Rows)
            //        {
            //            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //            var ala = item.Field<byte[]>(3);

            //            image.Source = LoadImage(ala);
            //            Stack.Children.Add(image);
            //        }
            //    }
            //}
        }
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
        private void SkryDatagridy()
        {
            DataGrid_mainGridJazda.Visibility = Visibility.Hidden;
            DataGrid_mainGridRezervacia.Visibility = Visibility.Hidden;
            DataGrid_mainGridVodic.Visibility = Visibility.Hidden;
            DataGrid_mainGridVozdila.Visibility = Visibility.Hidden;
        }
        private void VsetkyVozidla_Click(object sender, RoutedEventArgs e)
        {
            using (OracleCommand cmd = new OracleCommand("select spz,znacka,model,rok_vyroby from vozidla", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    SkryDatagridy();
                    DataGrid_mainGridVozdila.Visibility = Visibility.Visible;
                    DataGrid_mainGridVozdila.DataContext = dataTable;
                }
            }
        }

        private void VsetkyJazdy_Click(object sender, RoutedEventArgs e)
        {
            using (OracleCommand cmd = new OracleCommand("select spz,meno,priezvisko,najazdene_km,id_rezervacie,id_jazdy from jazda join rezervacia using(id_rezervacie) " +
                                                         "join vozidla using(spz) join vodic using(rod_cislo)", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    SkryDatagridy();
                    DataGrid_mainGridJazda.Visibility = Visibility.Visible;
                    DataGrid_mainGridJazda.DataContext = dataTable;
                }
            }
        }

        private void DataGrid_mainGridVodic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGrid_mainGridJazda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var jazdaID = ((DataRowView)DataGrid_mainGridJazda.SelectedItem).Row.ItemArray[5];

            using (OracleCommand cmd = new OracleCommand("select * from naklady  join typ_nakladov using(id_typu_nakladov) where id_jazdy = :jazdaID ", orcl))
            {
                cmd.Parameters.Add("jazdaID", OracleDbType.Int32);
                cmd.Parameters[0].Value = Convert.ToInt32(jazdaID);

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    DataGrid_Naklady.Visibility = Visibility.Visible;
                    DataGrid_Naklady.DataContext = dataTable;
                }
            }


            using (OracleCommand cmd = new OracleCommand("select zem_sirka,zem_dlzka from poloha where id_jazdy = :jazdaID ", orcl))
            {
                cmd.Parameters.Add("jazdaID", OracleDbType.Int32);
                cmd.Parameters[0].Value = Convert.ToInt32(jazdaID);

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    mapa.VycistiMapu();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        List<GMap.NET.PointLatLng> pointLatLngs = new List<GMap.NET.PointLatLng>();
                        pointLatLngs.Add(new GMap.NET.PointLatLng(Convert.ToDouble(row.ItemArray[0]), Convert.ToDouble(row.ItemArray[1])));

                        mapa.PridajMarkre(pointLatLngs);
                    }
                }
            }

        }

        private void SkryPravuStranu()
        {
            Grid_VozidlaFotky.Visibility = Visibility.Hidden;
            Grid_Naklady.Visibility = Visibility.Hidden;
        }
        private void DataGrid_mainGridVozdila_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pSpz = ((DataRowView)DataGrid_mainGridVozdila.SelectedItem).Row.ItemArray[0];
            DataTable vozidla = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select value(t) from vozidla p, table (p.fotky) t where spz = :pSpz", orcl))
            {
                cmd.Parameters.Add("pSpz", OracleDbType.Char);
                cmd.Parameters[0].Value = pSpz;

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    vozidla = new DataTable();
                    vozidla.Load(reader);
                }
            }

            //using (OracleCommand cmd = new OracleCommand("select foto from foto_auta where spz = :pSp", orcl))
            //{
            //    cmd.Parameters.Add("pSpz", OracleDbType.Char);
            //    cmd.Parameters[0].Value = pSpz;

            //    using (OracleDataReader reader = cmd.ExecuteReader())
            //    {
            //        vozidla = new DataTable();
            //        vozidla.Load(reader);
            //    }
            //}

            Grid_VozidlaFotky.Children.Clear();
            SkryPravuStranu();
            Grid_VozidlaFotky.Visibility = Visibility.Visible;

            foreach (DataRow dataRow in vozidla.Rows)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                var ala = dataRow.Field<byte[]>(0);

                image.Source = LoadImage(ala);

                Grid_VozidlaFotky.Children.Add(image);
            }
        }
    }
}

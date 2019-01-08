using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
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

namespace PDS_Manager.Pages
{
    /// <summary>
    /// Interaction logic for Jazda_Page.xaml
    /// </summary>
    public partial class Jazda_Page : Page, INotifyPropertyChanged
    {
        private Mapa.Mapa_Page mapa;
        OracleConnection orcl;
        List<KeyValuePair<int, string>> typyNakladov = new List<KeyValuePair<int, string>>();
        public Jazda_Page(OracleConnection oracleConnection)
        {
            InitializeComponent();
            DataContext = this;
            orcl = oracleConnection;
            mapa = new Mapa.Mapa_Page();
            Frame_mapa.Content = mapa;

            using (OracleCommand cmd = new OracleCommand("select * from typ_nakladov", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    foreach (DataRow item in dataTable.Rows)
                    {
                        typyNakladov.Add(new KeyValuePair<int, string>(Convert.ToInt32(item.ItemArray[0]), item.ItemArray[1].ToString()));
                    }

                    TypNakladuCombo.ItemsSource = typyNakladov.Select(x => x.Value);
                }
            }
        }

        private class Jazda
        {
            public Jazda(int idJazdy, int idRezervacie, int najzdeneKm, string spz, string rodCislo, DateTime od, DateTime? @do)
            {
                IdJazdy = idJazdy;
                IdRezervacie = idRezervacie;
                NajzdeneKm = najzdeneKm;
                RodCislo = rodCislo;
                Od = od;
                Do = @do;
                Spz = spz;
            }

            public int IdJazdy { get; set; }
            public int IdRezervacie { get; set; }
            public int NajzdeneKm { get; set; }
            public string Spz { get; set; }

            public string RodCislo { get; set; }
            public DateTime Od { get; set; }
            public DateTime? Do { get; set; }
        }
        ObservableCollection<Jazda> jazdas = new ObservableCollection<Jazda>();
        ObservableCollection<Naklad> naklads = new ObservableCollection<Naklad>();

        private ObservableCollection<Jazda> Jazdas
        {
            get { return jazdas; }
            set
            {
                jazdas = value;
                OnPropertyChanged(nameof(Jazdas));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void DataGrid_mainGridJazda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            naklads.Clear();
            if (DataGrid_mainGridJazda.SelectedItem != null)
            {
                var jazdaID = ((Jazda)DataGrid_mainGridJazda.SelectedItem).IdJazdy;

                using (OracleCommand cmd = new OracleCommand("select id_nakladu, id_jazdy,popis,hodnota,kedy from naklady  join typ_nakladov using(id_typu_nakladov) where id_jazdy = :jazdaID ", orcl))
                {
                    cmd.Parameters.Add("jazdaID", OracleDbType.Int32);
                    cmd.Parameters[0].Value = Convert.ToInt32(jazdaID);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        foreach (DataRow item in dataTable.Rows)
                        {
                            naklads.Add(new Naklad(Convert.ToInt32(item.ItemArray[0]), Convert.ToInt32(item.ItemArray[1]), item.ItemArray[2].ToString(), Convert.ToDouble(item.ItemArray[3]),
                                Convert.ToDateTime(item.ItemArray[4])));
                        }

                        DataGrid_Naklady.ItemsSource = naklads;
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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            Jazdas.Clear();

            Task.Run(() =>
            {
                using (OracleCommand cmd = new OracleCommand("select id_jazdy,id_rezervacie,najazdene_km,spz,rod_cislo, datJazd_od, datJazd_Do from jazda join rezervacia using(id_rezervacie) ", orcl))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        Dispatcher.Invoke(() =>
                        {
                            foreach (DataRow row in dataTable.Rows)
                            if (row.ItemArray[6] != DBNull.Value)
                                Jazdas.Add(new Jazda(Convert.ToInt32(row.ItemArray[0]), Convert.ToInt32(row.ItemArray[1]),
                               Convert.ToInt32(row.ItemArray[2]), row.ItemArray[3].ToString(), row.ItemArray[4].ToString(),
                               Convert.ToDateTime(row.ItemArray[5]), Convert.ToDateTime(row.ItemArray[6])));
                            else
                                Jazdas.Add(new Jazda(Convert.ToInt32(row.ItemArray[0]), Convert.ToInt32(row.ItemArray[1]),
                              Convert.ToInt32(row.ItemArray[2]), row.ItemArray[3].ToString(), row.ItemArray[4].ToString(),
                              Convert.ToDateTime(row.ItemArray[5]), null));
                        });

                    }
                }

                Dispatcher.Invoke(() =>
                            {
                                DataGrid_mainGridJazda.ItemsSource = Jazdas;
                            });
            });
        }

        private void ButtonDeleteJazda_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var spz = ((DataGrid_mainGridJazda as DataGrid).SelectedItem as Jazda);
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Naozaj chceš odstrániť jazda {spz.IdJazdy}?", "Vymazanie jazdy",
                    System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    using (var command = new OracleCommand("DELETE FROM jazda WHERE id_jazdy = :pIdJazdy", orcl))
                    {

                        command.Parameters.Add("pIdJazdy", OracleDbType.Int32);
                        command.Parameters[0].Value = spz.IdJazdy;
                        command.ExecuteNonQuery();
                    }

                    Jazdas.Remove(spz);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie je mozne odstranit jazda, existuju pre nu data");
            }
        }



        private void DataGrid_mainGridJazda_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string t = (e.EditingElement as TextBox).Text;
            var ColumnName = e.Column.Header;
            var rezervacia = ((sender as DataGrid).SelectedItem as Jazda);
            var datum = t.ToString().Split('/');
            try
            {

                DataTable vozidla = new DataTable();
                OracleDataAdapter sqlDa = new OracleDataAdapter();
                sqlDa.SelectCommand = new OracleCommand($"select id_rezervacie,spz,dat_od, dat_do from rezervacia p where rod_cislo = '{rezervacia.IdJazdy}'", orcl);
                OracleCommandBuilder cb = new OracleCommandBuilder(sqlDa);
                sqlDa.Fill(vozidla);

                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "aktualizuj_jazdu";
                objCmd.CommandType = CommandType.StoredProcedure;

                OracleParameter id = new OracleParameter("new_id_jazdy", OracleDbType.Int32);
                OracleParameter spz = new OracleParameter("new_najazdene_km", OracleDbType.Int64);
                OracleParameter @do = new OracleParameter("new_dat_do", OracleDbType.Date);

                objCmd.Parameters.Add(id);
                objCmd.Parameters.Add(spz);
                objCmd.Parameters.Add(@do);

                objCmd.Parameters["new_id_jazdy"].Value = rezervacia.IdJazdy;

                if (ColumnName.Equals("Najazdené km"))
                    objCmd.Parameters["new_najazdene_km"].Value = t;
                else
                    objCmd.Parameters["new_najazdene_km"].Value = rezervacia.NajzdeneKm;

                if (ColumnName.Equals("Dátum do"))
                    objCmd.Parameters["new_dat_do"].Value = new DateTime(Convert.ToInt32(datum[2]), Convert.ToInt32(datum[0]), Convert.ToInt32(datum[1]));
                else
                    objCmd.Parameters["new_dat_do"].Value = rezervacia.Do;


                objCmd.ExecuteNonQuery();

                var ints = sqlDa.Update(vozidla);

            }
            catch (Exception ex)
            {
                if (ColumnName.Equals("Najazdené km"))
                    (e.EditingElement as TextBox).Text = ((sender as DataGrid).SelectedItem as Jazda).NajzdeneKm.ToString();

                if (ColumnName.Equals("Dátum do"))
                    (e.EditingElement as TextBox).Text = Convert.ToDateTime(rezervacia.Do).ToString("MM/dd/yyyy");

                MessageBox.Show(ex.Message);
            }
        }
        private class Naklad
        {
            public Naklad(int idNakladu, int idJazdy, string popis, double hodnota, DateTime kedy)
            {
                IdNakladu = idNakladu;
                IdJazdy = idJazdy;
                Hodnota = hodnota;
                Kedy = kedy;
                PopisNakladu = popis;
            }

            public int IdNakladu { get; set; }
            public int IdJazdy { get; set; }
            public string PopisNakladu { get; set; }
            public double Hodnota { get; set; }
            public DateTime Kedy { get; set; }

        }
        byte[] fotkaByte;
        byte[] fotkaBytePridavaneho;
        private void PridajFotku(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog saveFileDialog = new System.Windows.Forms.OpenFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = "jpg",
                    InitialDirectory = Environment.CurrentDirectory,
                    Filter = "JPG-File | *.jpg|PNG-File | *.png"
                };

                saveFileDialog.ShowDialog();
                fotkaByte = File.ReadAllBytes(saveFileDialog.FileName);
                fotkaBytePridavaneho = fotkaByte;
                FotkaPridavanehoNakladu.Source = LoadImage(fotkaBytePridavaneho);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new System.IO.MemoryStream(imageData))
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

        private void PridajNakladClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sql = "INSERT INTO Naklady (id_nakladu,id_jazdy, id_typu_nakladov, hodnota, kedy, fotografia_blocku) " +
                                    "VALUES (:pid_nakladu, :pid_jazdy, :pid_typu_nakladov, :phodnota, :pkedy,:pfotografia_blocku)";



                int pom = typyNakladov.Where(x => x.Value.Equals(TypNakladuCombo.SelectedItem)).Select(x => x.Key).SingleOrDefault();
                using (OracleCommand command = new OracleCommand(sql, orcl))
                {
                    command.Parameters.Add("pid_nakladu", 1);
                    command.Parameters.Add("pid_jazdy", Convert.ToInt32(IdJazdy.Text));
                    command.Parameters.Add("pid_typu_nakladov", pom);
                    command.Parameters.Add("phodnota", Convert.ToInt32(Hodnota.Text));
                    command.Parameters.Add("pkedy", Kedy.SelectedDate);
                    command.Parameters.Add("pfotografia_blocku", fotkaBytePridavaneho);

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Jazda bola pridana");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void NakladySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTable dataTable;
            if ((Naklad)DataGrid_Naklady.SelectedItem != null)
            {
                var rod_cislo = ((Naklad)DataGrid_Naklady.SelectedItem).IdNakladu;
                using (OracleCommand cmd = new OracleCommand("select fotografia_blocku from naklady where id_nakladu = :pId_nakladu ", orcl))
                {
                    cmd.Parameters.Add("pId_nakladu", OracleDbType.Int32);
                    cmd.Parameters[0].Value = rod_cislo;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dataTable = new DataTable();
                        dataTable.Load(reader);
                    }
                }

                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                var ala = dataTable.Rows[0].Field<byte[]>(0);
                FotkaNakladu.Source = LoadImage(ala);
            }
        }

        private void ButtonDeleteNaklady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var spz = ((DataGrid_Naklady as DataGrid).SelectedItem as Naklad);
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Naozaj chceš odstrániť náklad {spz.IdNakladu}", "Vymazanie nakladu",
                    System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    using (var command = new OracleCommand("DELETE FROM naklady WHERE id_nakladu = :pid_nakladu", orcl))
                    {

                        command.Parameters.Add("id_nakladu", OracleDbType.Int32);
                        command.Parameters[0].Value = spz.IdNakladu;
                        command.ExecuteNonQuery();
                    }

                    naklads.Remove(spz);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}


using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for Vozidla_Page.xaml
    /// </summary>
    public partial class Vozidla_Page : Page, INotifyPropertyChanged
    {
        OracleConnection orcl;
        object[] _vozidlo;
        private Mapa.Mapa_Page mapa;

        public object[] Vozidlo
        {
            get { return _vozidlo; }
            set
            {
                _vozidlo = value;
                OnPropertyChanged(nameof(Vozidlo));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Vozidla_Page(OracleConnection oracleConnection)
        {
            InitializeComponent();
            DataContext = this;
            orcl = oracleConnection;
            Vozidlo = new object[5];
            mapa = new Mapa.Mapa_Page();
            Frame_mapa.Content = mapa;

        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void DataGrid_mainGridVozdila_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            obrazkyByte.Clear();

            if (DataGrid_mainGridVozdila.SelectedItem != null)
            {
                object[] foo = new object[5];

                if (!(DataGrid_mainGridVozdila.SelectedItem is DataRowView))
                    return;
                var pSpz = ((DataRowView)DataGrid_mainGridVozdila.SelectedItem).Row.ItemArray[0];
                DataTable vozidla = new DataTable();

                using (OracleCommand cmd = new OracleCommand("select znacka,model,rok_vyroby from vozidla where spz = :pSpz", orcl))
                {
                    cmd.Parameters.Add("pSpz", OracleDbType.Char);
                    cmd.Parameters[0].Value = pSpz;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        vozidla = new DataTable();
                        vozidla.Load(reader);
                    }

                    if (vozidla.Rows.Count == 0)
                        return;

                    foo[2] = vozidla.Rows[0].ItemArray[0];
                    foo[3] = vozidla.Rows[0].ItemArray[1];
                    foo[4] = vozidla.Rows[0].ItemArray[2];

                }


                using (OracleCommand cmd = new OracleCommand($"select count(*) from rezervacia where DAT_OD < sysdate and DAT_DO > sysdate and SPZ = :pSpz", orcl))
                {
                    cmd.Parameters.Add("pSpz", OracleDbType.Char);
                    cmd.Parameters[0].Value = pSpz;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        vozidla = new DataTable();
                        vozidla.Load(reader);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        foo[0] = vozidla.Rows[0].ItemArray[0];
                    });
                }

                Task.Run(() =>
                {
                    DataTable pom = new DataTable();
                    using (OracleCommand cmd = new OracleCommand($"select round(func_vytazenost_auta('{pSpz}'),8) from dual", orcl))
                    {

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            pom = new DataTable();
                            pom.Load(reader);
                        }

                        Dispatcher.Invoke(() =>
                        {
                            foo[1] = (Convert.ToDecimal(pom.Rows[0].ItemArray[0]) * 100).ToString("N2") + "%";
                        });

                        object[] a = new object[5];
                        a[0] = Vozidlo[0];
                        a[1] = foo[1];
                        a[2] = Vozidlo[2];
                        a[3] = Vozidlo[3];
                        a[4] = Vozidlo[4];

                        Vozidlo = a;
                    }
                });

                Task.Run(() =>
                {
                    using (OracleCommand cmd = new OracleCommand("select poloha.ZEM_SIRKA,poloha.ZEM_DLZKA from jazda join poloha using(id_jazdy) join rezervacia using(id_rezervacie) " +
                    "where DAT_OD < sysdate and DAT_DO > sysdate and SPz = :pSpz order by poloha.CAS_ZAZNAMU FETCH FIRST 1 ROWS ONLY ", orcl))
                    {
                        cmd.Parameters.Add("pSpz", OracleDbType.Char);
                        cmd.Parameters[0].Value = pSpz;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);

                            Dispatcher.Invoke(() =>
                            {

                                mapa.VycistiMapu();

                                if (dataTable.Rows.Count != 0)
                                {
                                    List<GMap.NET.PointLatLng> pointLatLngs = new List<GMap.NET.PointLatLng>();
                                    pointLatLngs.Add(new GMap.NET.PointLatLng(Convert.ToDouble(dataTable.Rows[0].ItemArray[0]), Convert.ToDouble(dataTable.Rows[0].ItemArray[1])));

                                    mapa.PridajMarkre(pointLatLngs);
                                }
                            });
                        }
                    }
                });


                NacitajObrazky(pSpz.ToString());
                Vozidlo = foo;
                fotkyVozidla.Clear();
                FotkyVozidla.Children.Clear();
            }
        }

        public void NacitajObrazky(string pSpz)
        {
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

            Stack_VozidlaFotky.Children.Clear();

            int i = 0;
            foreach (DataRow dataRow in vozidla.Rows)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                var ala = dataRow.Field<byte[]>(0);

                image.Source = LoadImage(ala);
                image.Width = 250;
                image.Height = 250;
                obrazkyByte.Add(ala);

                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                stackPanel.Children.Add(image);

                Button button = new Button();
                button.Tag = i;
                button.Height = 50;
                button.Content = "VYMAZ";
                stackPanel.Children.Add(button);
                button.Click += VymazObrazok;

                Stack_VozidlaFotky.Children.Add(stackPanel);
                i++;
            }

        }
        List<byte[]> obrazkyByte = new List<byte[]>();
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
        DataTable dataTableVozidla;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (OracleCommand cmd = new OracleCommand("select spz,znacka,model,rok_vyroby from vozidla", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    dataTableVozidla = new DataTable();
                    dataTableVozidla.Load(reader);
                    DataGrid_mainGridVozdila.DataContext = dataTableVozidla;
                }
            }
        }

        private void DataGrid_mainGridVozdila_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                string t = (e.EditingElement as TextBox).Text;
                var ColumnName = e.Column.Header;
                var spz = ((sender as DataGrid).SelectedItem as DataRowView);

                DataTable vozidla = new DataTable();
                OracleDataAdapter sqlDa = new OracleDataAdapter();
                sqlDa.SelectCommand = new OracleCommand($"select spz, znacka, model, rok_vyroby from vozidla p where spz = '{spz.Row.ItemArray[0]}'", orcl);
                OracleCommandBuilder cb = new OracleCommandBuilder(sqlDa);
                sqlDa.Fill(vozidla);

                foreach (DataRow dataRow in vozidla.Rows)
                {
                    if (ColumnName.Equals("ŠPZ"))
                    {
                        if (t.Length == 8)
                            dataRow["spz"] = t;
                        else
                            throw new ArgumentException("Nespravna dlzka spz");
                    }

                    else
                        dataRow["spz"] = spz.Row.ItemArray[0];

                    if (ColumnName.Equals("Značka"))
                        dataRow["znacka"] = t;
                    else
                        dataRow["znacka"] = spz.Row.ItemArray[1];

                    if (ColumnName.Equals("Model"))
                        dataRow["model"] = t;
                    else
                        dataRow["model"] = spz.Row.ItemArray[2];

                    if (ColumnName.Equals("Rok výroby"))
                        dataRow["rok_vyroby"] = t;
                    else
                        dataRow["rok_vyroby"] = spz.Row.ItemArray[3];

                    sqlDa.Update(vozidla);
                }

                var ints = sqlDa.Update(vozidla);

            }
            catch (Exception ex)
            {
                (e.EditingElement as TextBox).Text = ((sender as DataGrid).SelectedItem as DataRowView).Row.ItemArray[0].ToString();
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var spz = ((DataGrid_mainGridVozdila as DataGrid).SelectedItem as DataRowView);
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Naozaj chceš odstrániť vozidlo {spz.Row.ItemArray[0]}?", "Vymazanie vozidla",
                    System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    using (var command = new OracleCommand("DELETE FROM Vozidla WHERE spz = :pSpz", orcl))
                    {

                        command.Parameters.Add("pSpz", OracleDbType.Char);
                        command.Parameters[0].Value = spz.Row.ItemArray[0];
                        command.ExecuteNonQuery();
                    }

                    dataTableVozidla.Rows.Remove(spz.Row);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie je mozne odstranit vozidlo, existuju pre neho data");
            }
        }

        private void VymazObrazok(object sender, RoutedEventArgs e)
        {
            if (Stack_VozidlaFotky.Children.Count != 0)
            {

                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "odFoto";
                objCmd.CommandType = CommandType.StoredProcedure;

                OracleParameter id = new OracleParameter("pSpz", OracleDbType.Char);
                OracleParameter spz = new OracleParameter("foto", OracleDbType.Blob);

                objCmd.Parameters.Add(id);
                objCmd.Parameters.Add(spz);

                objCmd.Parameters["pSpz"].Value = ((DataGrid_mainGridVozdila as DataGrid).SelectedItem as DataRowView).Row.ItemArray[0];
                objCmd.Parameters["foto"].Value = obrazkyByte[(int)((Button)sender).Tag];

                objCmd.ExecuteNonQuery();

                NacitajObrazky(((DataGrid_mainGridVozdila as DataGrid).SelectedItem as DataRowView).Row.ItemArray[0].ToString());
            }
        }

        private void PridajVozidloClick(object sender, RoutedEventArgs e)
        {
            foreach (var fotka in fotkyVozidla)
            {
                OracleCommand objCmd1 = new OracleCommand();
                objCmd1.Connection = orcl;
                objCmd1.CommandText = "PridajFotkuAuta";
                objCmd1.CommandType = CommandType.StoredProcedure;

                OracleParameter pSpz = new OracleParameter("pSpz", OracleDbType.Char);
                OracleParameter pFotka = new OracleParameter("pFotka", OracleDbType.Blob);

                objCmd1.Parameters.Add(pSpz);
                objCmd1.Parameters.Add(pFotka);

                objCmd1.Parameters["pSpz"].Value = ((DataGrid_mainGridVozdila as DataGrid).SelectedItem as DataRowView).Row.ItemArray[0].ToString();
                objCmd1.Parameters["pFotka"].Value = fotka;

                objCmd1.ExecuteNonQuery();
            }

            System.Windows.MessageBox.Show("Fotky vozidla boli úspešne pridané");
        }

        List<byte[]> fotkyVozidla = new List<byte[]>();
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
                var picture = File.ReadAllBytes(saveFileDialog.FileName);


                fotkyVozidla.Add(picture);

                FotkyVozidla.Children.Add(new Image()
                {
                    Source = LoadImage(picture),
                    Margin = new Thickness(5)
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}

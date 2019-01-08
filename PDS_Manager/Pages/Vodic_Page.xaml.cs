using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Vodic_Page.xaml
    /// </summary>
    public partial class Vodic_Page : Page
    {
        OracleConnection orcl;
        public Vodic_Page(OracleConnection oracleConnection)
        {
            InitializeComponent();
            DataContext = this;
            orcl = oracleConnection;
        }
        DataTable dataTableVodic;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                using (OracleCommand cmd = new OracleCommand("select rod_cislo,meno, priezvisko, p.adresa.vypis() as adresa from vodic p", orcl))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dataTableVodic = new DataTable();
                        dataTableVodic.Load(reader);

                        Dispatcher.Invoke(() =>
                        {
                            DataGrid_mainGridVodic.DataContext = dataTableVodic;
                        });
                    }
                }
            });

        }

        private void VodicChanged(object sender, SelectionChangedEventArgs e)
        {
            Fotka.Children.Clear();
            DataTable dataTable;
            if ((DataRowView)DataGrid_mainGridVodic.SelectedItem != null)
            {
                var rod_cislo = ((DataRowView)DataGrid_mainGridVodic.SelectedItem).Row.ItemArray[0];
                using (OracleCommand cmd = new OracleCommand("select fotka_vp from vodic where rod_cislo = :rod_cislo ", orcl))
                {
                    cmd.Parameters.Add("rod_cislo", OracleDbType.Char);
                    cmd.Parameters[0].Value = rod_cislo;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dataTable = new DataTable();
                        dataTable.Load(reader);
                    }
                }

                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                var ala = dataTable.Rows[0].Field<byte[]>(0);
                image.Source = LoadImage(ala);
                Fotka.Children.Add(image);
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

        private void DataGrid_mainGridVodic_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                string t = (e.EditingElement as TextBox).Text;
                var pom = e.Column.Header;
                var vlastnik = ((sender as DataGrid).SelectedItem as DataRowView);

                DataTable rezervacie = new DataTable();
                OracleDataAdapter sqlDa = new OracleDataAdapter();
                sqlDa.SelectCommand = new OracleCommand($"select rod_cislo, meno, priezvisko from vodic p where rod_cislo = {vlastnik.Row.ItemArray[0]}", orcl);
                OracleCommandBuilder cb = new OracleCommandBuilder(sqlDa);
                sqlDa.Fill(rezervacie);

                int i = 0;
                string[] adresa = vlastnik.Row.ItemArray[3].ToString().Split(',');
                foreach (DataRow dataRow in rezervacie.Rows)
                {
                    if (pom.Equals("Rodné číslo"))
                    {
                        dataRow["rod_cislo"] = t;
                    }
                    else
                        dataRow["rod_cislo"] = vlastnik.Row.ItemArray[0];

                    if (pom.Equals("Meno"))
                        dataRow["meno"] = t;
                    else
                        dataRow["meno"] = vlastnik.Row.ItemArray[1];

                    if (pom.Equals("Priezvisko"))
                        dataRow["priezvisko"] = t;
                    else
                        dataRow["priezvisko"] = vlastnik.Row.ItemArray[2];

                    sqlDa.Update(rezervacie);


                }

                var ints = sqlDa.Update(rezervacie);

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
                var spz = ((DataGrid_mainGridVodic as DataGrid).SelectedItem as DataRowView);
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Naozaj chceš odstrániť vodica {spz.Row.ItemArray[0]}?", "Vymazanie vodica",
                    System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    using (var command = new OracleCommand("DELETE FROM vodic WHERE rod_cislo = :pRod_cislo", orcl))
                    {

                        command.Parameters.Add("pRod_cislo", OracleDbType.Char);
                        command.Parameters[0].Value = spz.Row.ItemArray[0];
                        command.ExecuteNonQuery();
                    }

                    dataTableVodic.Rows.Remove(spz.Row);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie je mozne odstranit vodica, existuju pre neho data");
            }
        }
    }
}

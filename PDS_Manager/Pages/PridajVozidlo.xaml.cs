using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDS_Manager.Pages
{
    /// <summary>
    /// Interaction logic for PridajVozidlo.xaml
    /// </summary>
    public partial class PridajVozidlo : Page
    {
        OracleConnection orcl;
        public PridajVozidlo(OracleConnection oracleConnection)
        {
            InitializeComponent();
            orcl = oracleConnection;
        }

        List<byte[]> fotkyVozidla = new List<byte[]>();
        private void PridajFotku(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog saveFileDialog = new OpenFileDialog()
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

        private void PridajVozidloClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "insertVozidloBezFotky";
                objCmd.CommandType = CommandType.StoredProcedure;

                OracleParameter spz = new OracleParameter("spz", OracleDbType.Char);
                OracleParameter znacka = new OracleParameter("znacka", OracleDbType.Varchar2);
                OracleParameter model = new OracleParameter("model", OracleDbType.Varchar2);
                OracleParameter rok_vyroby = new OracleParameter("rok_vyroby", OracleDbType.Int32);

                objCmd.Parameters.Add(spz);
                objCmd.Parameters.Add(znacka);
                objCmd.Parameters.Add(model);
                objCmd.Parameters.Add(rok_vyroby);

                objCmd.Parameters["spz"].Value = SPZ.Text;
                objCmd.Parameters["znacka"].Value = Znacka.Text;
                objCmd.Parameters["model"].Value = Model.Text;
                objCmd.Parameters["rok_vyroby"].Value = Convert.ToInt32(RokVyroby.Text);

                objCmd.ExecuteNonQuery();

                //Fotky
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

                    objCmd1.Parameters["pSpz"].Value = SPZ.Text;
                    objCmd1.Parameters["pFotka"].Value = fotka;

                    objCmd1.ExecuteNonQuery();
                }

                System.Windows.MessageBox.Show("Vozidlo bolo úspešne pridané");
            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            fotkyVozidla.Clear();
            FotkyVozidla.Children.Clear();

        }
    }
}

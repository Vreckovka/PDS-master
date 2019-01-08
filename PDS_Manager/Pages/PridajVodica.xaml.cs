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
    /// Interaction logic for PridajVodica.xaml
    /// </summary>
    public partial class PridajVodica : Page
    {
        OracleConnection orcl;
        public PridajVodica(OracleConnection oracleConnection)
        {
            InitializeComponent();
            orcl = oracleConnection;
        }
        byte[] fotkaByte;
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
                fotkaByte = File.ReadAllBytes(saveFileDialog.FileName);
                FotkaVodica.Source = LoadImage(fotkaByte);
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

        private void PridajVodicaClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "insertVodic";
                objCmd.CommandType = CommandType.StoredProcedure;
                OracleParameter id = new OracleParameter("rc", OracleDbType.Char);
                OracleParameter menoP = new OracleParameter("meno", OracleDbType.Varchar2);
                OracleParameter priezvisko = new OracleParameter("priezv", OracleDbType.Varchar2);
                OracleParameter cisloDomu = new OracleParameter("cisloDomu", OracleDbType.Int32);
                OracleParameter ulica = new OracleParameter("ulica", OracleDbType.Varchar2);
                OracleParameter mesto = new OracleParameter("mesto", OracleDbType.Varchar2);
                OracleParameter psc = new OracleParameter("psc", OracleDbType.Char);
                OracleParameter fotka = new OracleParameter("foto", OracleDbType.Blob);

                objCmd.Parameters.Add(id);
                objCmd.Parameters.Add(menoP);
                objCmd.Parameters.Add(priezvisko);
                objCmd.Parameters.Add(cisloDomu);
                objCmd.Parameters.Add(ulica);
                objCmd.Parameters.Add(mesto);
                objCmd.Parameters.Add(psc);

                objCmd.Parameters.Add(fotka);

                objCmd.Parameters["rc"].Value = Rod_Cislo.Text;
                objCmd.Parameters["meno"].Value = Meno.Text;
                objCmd.Parameters["priezv"].Value = Prievisko.Text;
                objCmd.Parameters["cisloDomu"].Value = Convert.ToInt16(CisloDomu.Text);
                objCmd.Parameters["ulica"].Value = Ulica.Text;
                objCmd.Parameters["mesto"].Value = Mesto.Text;
                objCmd.Parameters["psc"].Value = PSC.Text;

                objCmd.Parameters["foto"].Value = fotkaByte;

                objCmd.ExecuteNonQuery();
                System.Windows.MessageBox.Show("Vodic bol úspešne pridaný");
            }
            catch (Exception ex)
            {
               System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}


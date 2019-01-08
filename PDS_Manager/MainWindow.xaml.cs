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
using PDS_Manager.Pages;

namespace PDS_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OracleConnection orcl;

        private Vozidla_Page vozidla_Page;
        private Jazda_Page jazda_Page;
        private PridajVozidlo pridajVozidlo_Page;
        private Vodic_Page Vodic_Page;
        private PridajVodica PridajVodica;
        private Rezervacia_Page Rezervacia_Page;
        private Pridaj_Rezervaciu Pridaj_Rezervaciu;
        private Pridaj_Jazdu Pridaj_Jazdu;
        private Statistiky_Page Statistiky_Page;
        private XML_Report_page xML_Report_Page;
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
            DataContext = this;

            vozidla_Page = new Vozidla_Page(orcl);
            jazda_Page = new Jazda_Page(orcl);
            pridajVozidlo_Page = new PridajVozidlo(orcl);
            Vodic_Page = new Vodic_Page(orcl);
            PridajVodica = new PridajVodica(orcl);
            Rezervacia_Page = new Rezervacia_Page(orcl);
            Pridaj_Rezervaciu = new Pridaj_Rezervaciu(orcl);
            Pridaj_Jazdu = new Pridaj_Jazdu(orcl);
            Statistiky_Page = new Statistiky_Page(orcl);
            xML_Report_Page = new XML_Report_page(orcl);

        }

        private void VsetkyJazdy_Click(object sender, RoutedEventArgs e)
        {
           
                Frame_Main.Content = jazda_Page;
          
           
        }


        private void VsetkyVozidla_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = vozidla_Page;
        }

        private void PridajVozidlo_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = pridajVozidlo_Page;
            Width = 850;
            Height = 400;
        }

        private void VsetciVodici_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = Vodic_Page;
        }

        private void PridajVodica_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = PridajVodica;
        }

        private void VsetkyRezervacie_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = Rezervacia_Page;
        }

        private void PridajRezervaciu_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = Pridaj_Rezervaciu;
        }

        private void PridajJazdu_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = Pridaj_Jazdu;
        }

        private void ZobrazStatistiky_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = Statistiky_Page;
        }

        private void XML_Click(object sender, RoutedEventArgs e)
        {
            Frame_Main.Content = xML_Report_Page;
        }
    }
}

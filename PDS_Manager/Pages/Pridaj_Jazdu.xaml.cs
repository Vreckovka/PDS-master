using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Pridaj_Jazdu.xaml
    /// </summary>
    public partial class Pridaj_Jazdu : Page
    {
        OracleConnection orcl;
        public Pridaj_Jazdu(OracleConnection oracleConnection)
        {
            InitializeComponent();
            orcl = oracleConnection;
        }

        private void PridajJazduClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sql = "INSERT INTO Jazda (id_jazdy,id_rezervacie, najazdene_Km, datJazd_od,datJazd_do) " +
                                    "VALUES (:pid_jazdy, :pid_rezervacie, :pnajazdeneKm, :pdatJazd_od, :pdatJazd_do)";

                using (OracleCommand command = new OracleCommand(sql, orcl))
                {
                    command.Parameters.Add("pid_jazdy", 1);
                    command.Parameters.Add("pid_rezervacie", Convert.ToInt32(Rezervacia.Text));
                    command.Parameters.Add("pnajazdeneKm", Convert.ToInt32(Km.Text));
                    command.Parameters.Add("pdatJazd_od", Od.SelectedDate);
                    command.Parameters.Add("pdatJazd_do", @do.SelectedDate);

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Jazda bola pridana");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

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
    /// Interaction logic for Pridaj_Rezervaciu.xaml
    /// </summary>
    public partial class Pridaj_Rezervaciu : Page
    {
        OracleConnection orcl;
        public Pridaj_Rezervaciu(OracleConnection oracleConnection)
        {
            InitializeComponent();
            orcl = oracleConnection;
        }

        private void PridajRezeravciuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sql = "INSERT INTO Rezervacia (id_rezervacie, spz,rod_cislo,dat_od,dat_do) " +
                                    "VALUES (:id_rezervacie, :spz, :rod_cislo, :dat_od, :dat_do)";

                using (OracleCommand command = new OracleCommand(sql, orcl))
                {
                    command.Parameters.Add("id_rezervacie", 1);
                    command.Parameters.Add("spz", SPZ.Text);
                    command.Parameters.Add("rod_cislo", ROD_CISLO.Text);
                    command.Parameters.Add("dat_od", Od.SelectedDate);
                    command.Parameters.Add("dat_do", @do.SelectedDate);

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Rezervacia bola pridana");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
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
    /// Interaction logic for Rezervacia_Page.xaml
    /// </summary>
    public partial class Rezervacia_Page : Page
    {
        OracleConnection orcl;
        public Rezervacia_Page(OracleConnection oracleConnection)
        {
            InitializeComponent();
            DataContext = this;
            orcl = oracleConnection;
        }
        DataTable dataTableRezervacie;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid_mainGridRezervacia.CommitEdit();
            Task.Run(() =>
            {


                using (OracleCommand cmd = new OracleCommand("select id_rezervacie,rod_cislo, spz, dat_od, dat_do from REZERVACIA", orcl))
                {
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        dataTableRezervacie = new DataTable();
                        dataTableRezervacie.Load(reader);
                        Dispatcher.Invoke(() =>
                        {
                            DataGrid_mainGridRezervacia.DataContext = dataTableRezervacie;
                        });
                    }
                }
            });
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var spz = ((DataGrid_mainGridRezervacia as DataGrid).SelectedItem as DataRowView);
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Naozaj chceš odstrániť rezervaciu {spz.Row.ItemArray[0]}?", "Vymazanie rezervacie",
                    System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    using (var command = new OracleCommand("DELETE FROM rezervacia WHERE id_rezervacie = :pId_rezervacie", orcl))
                    {

                        command.Parameters.Add("pId_rezervacie", OracleDbType.Int32);
                        command.Parameters[0].Value = spz.Row.ItemArray[0];
                        command.ExecuteNonQuery();
                    }

                    dataTableRezervacie.Rows.Remove(spz.Row);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DataGrid_mainGridRezervacia_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string t = (e.EditingElement as TextBox).Text;
            var ColumnName = e.Column.Header;
            var rod_Cislo = ((sender as DataGrid).SelectedItem as DataRowView);
            var datum = t.ToString().Split('/');
            try
            {


                DataTable vozidla = new DataTable();
                OracleDataAdapter sqlDa = new OracleDataAdapter();
                sqlDa.SelectCommand = new OracleCommand($"select id_rezervacie,spz,dat_od, dat_do from rezervacia p where rod_cislo = '{rod_Cislo.Row.ItemArray[0]}'", orcl);
                OracleCommandBuilder cb = new OracleCommandBuilder(sqlDa);
                sqlDa.Fill(vozidla);

                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "aktualizuj_rezervaciu";
                objCmd.CommandType = CommandType.StoredProcedure;

                OracleParameter id = new OracleParameter("new_id_rezervacie", OracleDbType.Int32);
                OracleParameter spz = new OracleParameter("new_spz", OracleDbType.Char);
                OracleParameter od = new OracleParameter("new_dat_od", OracleDbType.Date);
                OracleParameter doDat = new OracleParameter("new_dat_do", OracleDbType.Date);

                objCmd.Parameters.Add(id);
                objCmd.Parameters.Add(spz);
                objCmd.Parameters.Add(od);
                objCmd.Parameters.Add(doDat);

                objCmd.Parameters["new_id_rezervacie"].Value = rod_Cislo.Row.ItemArray[0];

                if (ColumnName.Equals("ŠPZ"))
                    objCmd.Parameters["new_spz"].Value = t;
                else
                    objCmd.Parameters["new_spz"].Value = rod_Cislo.Row.ItemArray[2];

                if (ColumnName.Equals("Dátum od"))
                    objCmd.Parameters["new_dat_od"].Value = new DateTime(Convert.ToInt32(datum[2]), Convert.ToInt32(datum[0]), Convert.ToInt32(datum[1]));
                else
                    objCmd.Parameters["new_dat_od"].Value = rod_Cislo.Row.ItemArray[3];

                if (ColumnName.Equals("Dátum do"))
                    objCmd.Parameters["new_dat_do"].Value = new DateTime(Convert.ToInt32(datum[2]), Convert.ToInt32(datum[0]), Convert.ToInt32(datum[1]));
                else
                    objCmd.Parameters["new_dat_do"].Value = rod_Cislo.Row.ItemArray[4];

                objCmd.ExecuteNonQuery();

                var ints = sqlDa.Update(vozidla);

            }
            catch (Exception ex)
            {
                if (ColumnName.Equals("ŠPZ"))
                    (e.EditingElement as TextBox).Text = ((sender as DataGrid).SelectedItem as DataRowView).Row.ItemArray[1].ToString();

                if (ColumnName.Equals("Dátum od"))
                    (e.EditingElement as TextBox).Text = Convert.ToDateTime(rod_Cislo.Row.ItemArray[3]).ToString("MM/dd/yyyy");

                if (ColumnName.Equals("Dátum do"))
                    (e.EditingElement as TextBox).Text = Convert.ToDateTime(rod_Cislo.Row.ItemArray[4]).ToString("MM/dd/yyyy");

                DataGrid_mainGridRezervacia.UnselectAll();
                MessageBox.Show(ex.Message);
            }
        }
    }
}

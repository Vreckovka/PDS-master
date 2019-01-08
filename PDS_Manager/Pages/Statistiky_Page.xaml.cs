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
    /// Interaction logic for Statistiky_Page.xaml
    /// </summary>
    public partial class Statistiky_Page : Page
    {
        string oradb = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = obelix.fri.uniza.sk)" +
                   "(PORT = 1521))" +
                   "(CONNECT_DATA = (SERVICE_NAME = orcl.fri.uniza.sk" + "))); " +
                   "User Id = nad; " +
                   "Password = " + "pato1303";

        OracleConnection orcl;
        public Statistiky_Page(OracleConnection oracleCommand)
        {
            InitializeComponent();
            orcl = oracleCommand;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //TOP 5 KM
            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select * from " +
                    "(select meno, priezvisko, sum(najazdene_km) as km, row_number() over (order by sum(najazdene_km) desc) rnk " +
                    "from vodic join REZERVACIA using (rod_cislo) join jazda using (Id_rezervacie) GROUP by meno, priezvisko,rod_cislo ) where rnk <= 5", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        Dispatcher.Invoke(() =>
                        {
                            TopKM.DataContext = dataTable; ;
                        });

                    }

                }
            });

            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select * from (select spz, znacka, model, sum(najazdene_km) as km, " +
                " row_number() over (order by sum(najazdene_km) desc) rnk from " +
                " VOZIDLA join REZERVACIA using(spz) join jazda using (Id_rezervacie) " +
                " GROUP by spz,znacka, model ) where rnk <= 5", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        Dispatcher.Invoke(() =>
                        {
                            TopKMVoz.DataContext = dataTable;
                        });
                    }
                }
            });

            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select spz,model,znacka, round(sum(hodnota) / sum(najazdene_km),4) " +
                " as pomer from  VOZIDLA join REZERVACIA using(spz) join jazda using (Id_rezervacie) " +
                "join naklady using(id_jazdy) GROUP by spz,model,znacka order by sum(najazdene_km) desc", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        Dispatcher.Invoke(() =>
                        {
                            PomerEurKM.DataContext = dataTable;
                        });
                    }
                }
            });

            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select meno, " +
                "priezvisko,round(avg(nvl(hodnota,0)),2)||'€' as priemer from  " +
                "vodic join REZERVACIA using(rod_cislo) join jazda using (Id_rezervacie) " +
                " join naklady using(id_jazdy) GROUP by meno, priezvisko,rod_cislo order by round(avg(nvl(hodnota,0)),2) desc", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        Dispatcher.Invoke(() =>
                        {
                            PriemerVodicov.DataContext = dataTable;
                        });
                    }
                }
            });

            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select spz,model, znacka, round(sum(hodnota),2) || '€' as naklady,to_char(kedy,'YYYY.MM') as datum from  VOZIDLA join REZERVACIA using(spz) join jazda using (Id_rezervacie) join naklady using(id_jazdy) GROUP by spz, to_char(kedy,'YYYY.MM'), model,znacka order by to_char(kedy,'YYYY.MM')", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        Dispatcher.Invoke(() =>
                        {
                            MesacneNaklady.DataContext = dataTable;
                        });
                    }
                }
            });

            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select TREAT(obj as t_bestVozidlo).spz as spz,rok ,TREAT(obj as t_bestVozidlo).pocet as pocet from(  select proc_Naj_rezer(to_char(dat_od, 'YYYY')) as obj, to_char(rez.dat_od, 'YYYY') as rok  from rezervacia rez group by to_char(rez.dat_od, 'YYYY')   order by to_char(rez.dat_od, 'YYYY') ) pom", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        Dispatcher.Invoke(() =>
                        {
                            RezervovaneVozidla.DataContext = dataTable;
                        });
                    }
                }
            });

            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select spz, model, znacka ,sum(najazdene_km) as km from vozidla join rezervacia using(spz) join jazda using (id_rezervacie) group by spz, model, znacka having sum(najazdene_km) > ((select sum(najazdene_km) from jazda )/ (select count(*) from vozidla) )", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        Dispatcher.Invoke(() =>
                        {
                            Nadpriemerne.DataContext = dataTable;
                        });
                    }
                }
            });

            Task.Run(() =>
            {

                using (OracleCommand cmd = new OracleCommand("select round(( select sum(najazdene_km) from jazda)  / (select count(*) from vozidla),4) from dual", orcl))
                {

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        Dispatcher.Invoke(() =>
                        {
                            Priemer.Text = "Priemerný počet najazdených km " + dataTable.Rows[0].ItemArray[0].ToString();
                        });
                    }
                }
            });
        }
    }
}

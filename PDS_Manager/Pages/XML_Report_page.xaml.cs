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
    /// Interaction logic for XML_Report_page.xaml
    /// </summary>
    public partial class XML_Report_page : Page
    {
        OracleConnection orcl;
        public XML_Report_page(OracleConnection oracleConnection)
        {
            InitializeComponent();
            orcl = oracleConnection;
        }

        public void VytorXml()
        {
            using (OracleCommand cmd = new OracleCommand("select  xmlroot(xmlElement(Vodici,xmlAgg(vod)),version 1.0).getClobVal()" +
                                                        "from(select XMLELEMENT(Vodic, xmlattributes(ROD_CISLO as ROD_CISLO), XMLELEMENT(Meno, meno), XMLELEMENT(Priezvisko, priezvisko), XMLELEMENT(Rezervacie, xmlAgg(rez))" +
                                                        ") vod" +
                                                         " from(select  rod_cislo, xmlElement(rezervacia, XMLAttributes(ID_REZERVACIE as ID_REZERVACIE)," +
                                                        "xmlagg(XMLELEMENT(Jazda, xmlattributes(Id_jazdy as id_jazdy)," + "xmlForest(NAJAZDENE_KM as NAJAZDENE_KM) " +" ) " +
                                                          ") " +
                                                         "  ) as rez " +
                                                            "from rezervacia left join jazda using (iD_REZERVACIE) group by rod_cislo, id_rezervacie) left join vodic using (rod_cislo) " +
                                                    $"where rod_cislo = '{ROD_CISLO.Text}' " +
                                                        "group by rod_cislo, meno, priezvisko " +
                                                                    ") ", orcl))
            {

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    XML_REPORT.Text = dataTable.Rows[0].ItemArray[0].ToString();
                }
            }
        }

        private void VytvorClick(object sender, RoutedEventArgs e)
        {
            VytorXml();
        }
    }
}

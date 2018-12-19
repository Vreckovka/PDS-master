using GMap.NET;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Drawing;

namespace Generator
{
    class Program
    {
        static GeocodingProvider geocodingProvider;
        static GeoCoderStatusCode statusCode = GeoCoderStatusCode.Unknow;
        static OracleConnection orcl;
        static void Main(string[] args)
        {

            string oradb = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = obelix.fri.uniza.sk)" +
                 "(PORT = 1521))" +
                 "(CONNECT_DATA = (SERVICE_NAME = orcl.fri.uniza.sk" + "))); " +
                 "User Id = nad; " +
                 "Password = " + "pato1303";
            orcl = new OracleConnection(oradb);

            orcl.Open();
            geocodingProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;

            //PridajPolohy();
            //PridajFotkyVozidiel();

            // DataTable jazdy = new DataTable();
            // OracleDataAdapter sqlDa = new OracleDataAdapter();
            // sqlDa.SelectCommand = new OracleCommand("select dat_od,dat_do,id_rezervacie from rezervacia where dat_do is not null", orcl);
            // OracleCommandBuilder cb = new OracleCommandBuilder(sqlDa);
            // sqlDa.Fill(jazdy);

            // foreach (DataRow dataRow in jazdy.Rows)
            // {
            //     DateTime starDate = RandomDate(new DateTime(2000, 1, 1), new DateTime(2018, 12, 31));
            //     dataRow["dat_od"] = starDate;
            //     dataRow["dat_do"] = RandomDate(starDate, new DateTime(2018, 12, 31));
            // }

            //var ints = sqlDa.Update(jazdy);

            PridajFotkyVozidiel();
              
        }
        static List<Mesto> Mestos = new List<Mesto>(); 
        public class Mesto
        {
            public Mesto(string nazov, string pSC)
            {
                Nazov = nazov;
                Ulice = new List<string>();
                PSC = pSC;
                VytvorUlice(random.Next(10, 20));
            }

            public void VytvorUlice(int pocet)
            {
                for (int i = 0; i < pocet; i++)
                {
                    Ulice.Add(ulice[random.Next(0,ulice.Count)]);
                }
            }

            public string Nazov { get; set; }
            public List<string> Ulice { get; set; }
            public string PSC { get; set; }
        }

        public static void VytvorMesta()
        {
            NacitajMesta();
            NacitajUlice();


            for(int i = 0; i < mesta.Count; i++)
            {
                Mestos.Add(new Mesto(mesta[i], random.Next(0, 10000).ToString("00000")));
            }
        }

        public static void PridajAdresy()
        {
            VytvorMesta();
            DataTable vodici = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select rod_cislo from vodic", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    vodici = new DataTable();
                    vodici.Load(reader);
                }
            }

            for (int i = 0; i < vodici.Rows.Count; i++)
            {
                
                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "PridajAdresu";
                objCmd.CommandType = CommandType.StoredProcedure;

                OracleParameter rod_cislo = new OracleParameter("rod_cislo", OracleDbType.Char);
                OracleParameter cisloDomu = new OracleParameter("cisloDomu", OracleDbType.Int32);
                OracleParameter ulica = new OracleParameter("ulica", OracleDbType.Varchar2);
                OracleParameter mesto = new OracleParameter("mesto", OracleDbType.Varchar2);
                OracleParameter psc = new OracleParameter("psc", OracleDbType.Char);

                objCmd.Parameters.Add(rod_cislo);
                objCmd.Parameters.Add(cisloDomu);
                objCmd.Parameters.Add(ulica);
                objCmd.Parameters.Add(mesto);
                objCmd.Parameters.Add(psc);

                Mesto mesto1 = Mestos[random.Next(0, Mestos.Count)];

                objCmd.Parameters["rod_cislo"].Value = vodici.Rows[i].ItemArray[0];
                objCmd.Parameters["cisloDomu"].Value = random.Next(1,1000);
                objCmd.Parameters["ulica"].Value = mesto1.Ulice[random.Next(0, mesto1.Ulice.Count)];
                objCmd.Parameters["mesto"].Value = mesto1.Nazov;
                objCmd.Parameters["psc"].Value = mesto1.PSC;

                objCmd.ExecuteNonQuery();

                Console.WriteLine("Ulica bola pridana " + i);
            }
        }

        private static List<string> mesta = new List<string>();

        public static void PridajPolohy()
        {
            NacitajMesta();
            DataTable jazdy = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select id_jazdy,dat_od, dat_do from jazda join rezervacia using(id_rezervacie)", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    jazdy = new DataTable();
                    jazdy.Load(reader);
                }
            }


            for (int i = 0; i < jazdy.Rows.Count; i++)
            {

                try
                {
                    var sql = "INSERT INTO Poloha (cas_zaznamu, id_jazdy, zem_sirka, zem_dlzka) " +
                              "VALUES (:cas_zaznamu, :id_jazdy, :zem_sirka,:zem_dlzka)";

                    using (OracleCommand command = new OracleCommand(sql, orcl))
                    {
                        if (jazdy.Rows[i].ItemArray[2] != DBNull.Value)
                            command.Parameters.Add("cas_zaznamu",
                               RandomDateWithTime(Convert.ToDateTime(jazdy.Rows[i].ItemArray[1]),
                                   Convert.ToDateTime(jazdy.Rows[i].ItemArray[2])));
                        else
                            command.Parameters.Add("cas_zaznamu",
                                RandomDateWithTime(Convert.ToDateTime(jazdy.Rows[i].ItemArray[1]),
                                    DateTime.Today));

                        command.Parameters.Add("id_jazdy", jazdy.Rows[i].ItemArray[0]);

                        command.Parameters.Add("zem_sirka", random.NextDouble() * (49.8 - 48) + 48);
                        command.Parameters.Add("zem_dlzka", random.NextDouble() * (17 - 15) + 15);

                        command.ExecuteNonQuery();

                    }

                    using (OracleCommand command = new OracleCommand(sql, orcl))
                    {
                        if (jazdy.Rows[i].ItemArray[2] != DBNull.Value)
                            command.Parameters.Add("cas_zaznamu",
                                RandomDateWithTime(Convert.ToDateTime(jazdy.Rows[i].ItemArray[1]),
                                    Convert.ToDateTime(jazdy.Rows[i].ItemArray[2])));
                        else
                            command.Parameters.Add("cas_zaznamu",
                                RandomDateWithTime(Convert.ToDateTime(jazdy.Rows[i].ItemArray[1]),
                                    DateTime.Today));

                        command.Parameters.Add("id_jazdy", jazdy.Rows[i].ItemArray[0]);

                        command.Parameters.Add("zem_sirka", random.NextDouble() * (49.8 - 48) + 48);
                        command.Parameters.Add("zem_dlzka", random.NextDouble() * (17 - 15) + 15);

                        command.ExecuteNonQuery();

                    }

                    Console.WriteLine($"Poloha pridana {i}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }
        public static void NacitajMesta()
        {
            using (StreamReader streamReader = new StreamReader("Mesta.txt"))
            {
                while (!streamReader.EndOfStream)
                {
                    var mesto = streamReader.ReadLine();
                    mesta.Add(mesto);
                }
            }
        }

        public static void PridajJazdy()
        {
            DataTable rezervacie = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select id_rezervacie from Rezervacia", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    rezervacie = new DataTable();
                    rezervacie.Load(reader);
                }
            }

            for (int i = 0; i < rezervacie.Rows.Count; i++)
            {

                var sql = "INSERT INTO Jazda (id_jazdy, id_rezervacie,najazdene_km) " +
                          "VALUES (:id_jazdy, :id_rezervacie, :najazdene_km)";

                using (OracleCommand command = new OracleCommand(sql, orcl))
                {
                    command.Parameters.Add("id_jazdy", i);
                    command.Parameters.Add("id_rezervacie", rezervacie.Rows[i].ItemArray[0]);
                    command.Parameters.Add("najazdene_km", random.Next(50, 1000));

                    command.ExecuteNonQuery();

                }

                Console.WriteLine($"Jazda pridana {i}");
            }

        }
        public static void PridajRezervacie(int pocet)
        {
            DataTable rod_cisla = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select rod_cislo from Vodic", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    rod_cisla = new DataTable();
                    rod_cisla.Load(reader);
                }
            }

            DataTable spz = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select spz from Vozidla", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    spz = new DataTable();
                    spz.Load(reader);
                }
            }

            for (int i = 0; i < pocet; i++)
            {
                string spzRandom = (string)spz.Rows[random.Next(0, spz.Rows.Count)].ItemArray[0];
                string rcRandom = (string)rod_cisla.Rows[random.Next(0, rod_cisla.Rows.Count)].ItemArray[0];

                var sql = "INSERT INTO Rezervacia (id_rezervacie, spz,rod_cislo,dat_od,dat_do) " +
                          "VALUES (:id_rezervacie, :spz, :rod_cislo, :dat_od,:dat_do)";

                using (OracleCommand command = new OracleCommand(sql, orcl))
                {
                    command.Parameters.Add("id_rezervacie", i);
                    command.Parameters.Add("spz", spzRandom);
                    command.Parameters.Add("rod_cislo", rcRandom);
                    command.Parameters.Add("dat_od", RandomDate(new DateTime(2000, 1, 1), new DateTime(2018, 12, 31)));

                    if (random.Next(0, 100) > 10)
                        command.Parameters.Add("dat_do",
                            RandomDate(new DateTime(2000, 1, 1), new DateTime(2018, 12, 31)));
                    else
                        command.Parameters.Add("dat_do", null);

                    command.ExecuteNonQuery();

                }

                Console.WriteLine($"Rezervacia pridana {i}");
            }
        }

        public static DateTime RandomDate(DateTime startDate, DateTime endDate)
        {
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime newDate = startDate + newSpan;
            return newDate.Date;

        }

        public static DateTime RandomDateWithTime(DateTime startDate, DateTime endDate)
        {
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime newDate = startDate + newSpan;
            return newDate;

        }

        public static void PridajFotkyNakladov()
        {
            DataTable jazdy = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select id_jazdy,dat_od,dat_do from jazda join rezervacia using(id_rezervacie)", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    jazdy = new DataTable();
                    jazdy.Load(reader);
                }
            }

            string[] nazvyTypov = new string[] { "Tankovanie", "Pokuta", "Myto", "Poistenie", "Oprava" };

            int idCount = 0;
            for (int i = 0; i < jazdy.Rows.Count; i++)
            {
                int pocetBlockov = random.Next(0, 5);

                for (int j = 0; j < pocetBlockov; j++)
                {
                    OracleCommand objCmd = new OracleCommand();
                    objCmd.Connection = orcl;
                    objCmd.CommandText = "insertFotkaNakladu";
                    objCmd.CommandType = CommandType.StoredProcedure;


                    OracleParameter id_nakladuP = new OracleParameter("id_nakladuP", OracleDbType.Int32);
                    OracleParameter id_jazdyP = new OracleParameter("id_jazdyP", OracleDbType.Int32);
                    OracleParameter id_typu_nakladovP = new OracleParameter("id_typu_nakladovP", OracleDbType.Int32);
                    OracleParameter hodnotaP = new OracleParameter("hodnotaP", OracleDbType.Decimal);
                    OracleParameter kedyP = new OracleParameter("kedyP", OracleDbType.Date);
                    OracleParameter fotka = new OracleParameter("fotka", OracleDbType.Varchar2);

                    objCmd.Parameters.Add(id_nakladuP);
                    objCmd.Parameters.Add(id_jazdyP);
                    objCmd.Parameters.Add(id_typu_nakladovP);
                    objCmd.Parameters.Add(hodnotaP);
                    objCmd.Parameters.Add(kedyP);
                    objCmd.Parameters.Add(fotka);

                    objCmd.Parameters["id_nakladuP"].Value = idCount;
                    idCount++;

                    objCmd.Parameters["id_jazdyP"].Value = jazdy.Rows[i].ItemArray[0];
                    objCmd.Parameters["id_typu_nakladovP"].Value = random.Next(0, 5);
                    objCmd.Parameters["hodnotaP"].Value = random.Next(1, 100) + Math.Round(random.NextDouble(), 2);

                    if (jazdy.Rows[i].ItemArray[2] != DBNull.Value)
                        objCmd.Parameters["kedyP"].Value = RandomDate(Convert.ToDateTime(jazdy.Rows[i].ItemArray[1]),
                            Convert.ToDateTime(jazdy.Rows[i].ItemArray[2]));
                    else
                    {
                        objCmd.Parameters["kedyP"].Value = RandomDate(Convert.ToDateTime(jazdy.Rows[i].ItemArray[1]),
                           new DateTime(2018,12,31));
                    }

                    objCmd.Parameters["fotka"].Value = nazvyTypov[random.Next(0,nazvyTypov.Length)] + ".png";

                    objCmd.ExecuteNonQuery();

                    Console.WriteLine("Blocek bol pridany " + i);
                }
            }
        }

        public static void PridajFotkyVozidiel()
        {
            DataTable vozidla = new DataTable();
            using (OracleCommand cmd = new OracleCommand("select spz,znacka, model from vozidla", orcl))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    vozidla = new DataTable();
                    vozidla.Load(reader);
                }
            }

            for (int i = 0; i < vozidla.Rows.Count; i++)
            {
                byte[] foto;
              

                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "insertFotkaVozidla";
                objCmd.CommandType = CommandType.StoredProcedure;
                OracleParameter id = new OracleParameter("id", OracleDbType.Char);
                OracleParameter spz = new OracleParameter("spz", OracleDbType.Varchar2);
                OracleParameter fotka = new OracleParameter("fotka", OracleDbType.Blob);

                objCmd.Parameters.Add(id);
                objCmd.Parameters.Add(spz);
                objCmd.Parameters.Add(fotka);

                objCmd.Parameters["id"].Value = i;
                objCmd.Parameters["spz"].Value = vozidla.Rows[i].ItemArray[0];
              
                // objCmd.Parameters["fotka"].Value = vozidla.Rows[i].ItemArray[1].ToString().Replace('Š', 'S') + vozidla.Rows[i].ItemArray[2].ToString() + ".png";
                string nazov = vozidla.Rows[i].ItemArray[1].ToString().Replace('Š', 'S') + vozidla.Rows[i].ItemArray[2].ToString() + ".png";

                Bitmap bitmap = new Bitmap(@"C:\Users\pecho\Desktop\Auta\" + nazov);
                ImageConverter converter = new ImageConverter();
                foto = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

                objCmd.Parameters["fotka"].Value = foto;
                objCmd.ExecuteNonQuery();

                Console.WriteLine("Fotka vozidla pridana " + i);
            }
        }

        public static void PridajVozidla(int pocet)
        {

            string[] znacky = new string[] { "Subaru", "Suzuki", "Volvo", "Volkswagen", "Audi", "BMW", "Citroen", "Dacia", "Fiat", "Škoda" };
            for (int i = 0; i < pocet; i++)
            {

                var sql = "INSERT INTO Vozidla (spz, znacka,model,rok_vyroby) " +
                          "VALUES (:spz, :znacka, :model, :rok_vyroby)";

                using (OracleCommand command = new OracleCommand(sql, orcl))
                {
                    command.Parameters.Add("spz", GetRandomSPZ());
                    command.Parameters.Add("znacka", znacky[random.Next(0, znacky.Length)]);
                    command.Parameters.Add("model", random.Next(0, 5));
                    command.Parameters.Add("rok_vyroby", new DateTime(random.Next(2001, 2017), 1, 1));
                    command.ExecuteNonQuery();

                }

                Console.WriteLine("Vozidlo pridane");

            }
        }

        public static string GetRandomSPZ()
        {
            string spz = "";
            spz += RandomString(2);
            spz += "-";
            spz += random.Next(100, 999);
            spz += RandomString(2);

            return spz;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void PridajNaklady()
        {
            string[] naklady = new string[] { "Tankovanie", "Pokuta", "Mýto", "Poistenie", "Oprava" };
            for (int i = 0; i < naklady.Length; i++)
            {
                var sql = "INSERT INTO Typ_nakladov (id_typu_nakladov, popis) " +
                          "VALUES (:id_typu_nakladov, :popis)";

                using (OracleCommand command = new OracleCommand(sql, orcl))
                {
                    command.Parameters.Add("id_typu_nakladov", i);
                    command.Parameters.Add("popis", naklady[i]);
                    command.ExecuteNonQuery();

                }

                Console.WriteLine("Typ_nakladov pridany");
            }
        }
        static List<string[]> mena = new List<string[]>();
        public static void NacitajMena()
        {
            using (StreamReader streamReader = new StreamReader("mena_gen.txt"))
            {
                while (!streamReader.EndOfStream)
                {
                    var meno = streamReader.ReadLine();
                    mena.Add(meno.Split(' '));
                }
            }
        }

        static List<string> ulice = new List<string>();
        public static void NacitajUlice()
        {
            using (StreamReader streamReader = new StreamReader("ulice_gen.txt"))
            {
                while (!streamReader.EndOfStream)
                {
                    var meno = streamReader.ReadLine();
                    ulice.Add(meno);
                }
            }
        }

        static Random random = new Random();

        static int[] fotkyVodicov = new int[] { 1, 2, 3, 4, 5 };
        public static void PridajVodicov()
        {
            for (int i = 0; i < mena.Count; i++)
            {

                string[] meno = mena[i];

                OracleCommand objCmd = new OracleCommand();
                objCmd.Connection = orcl;
                objCmd.CommandText = "insertVodic";
                objCmd.CommandType = CommandType.StoredProcedure;
                OracleParameter id = new OracleParameter("rc", OracleDbType.Char);
                OracleParameter menoP = new OracleParameter("m", OracleDbType.Varchar2);
                OracleParameter priezvisko = new OracleParameter("p", OracleDbType.Varchar2);
                OracleParameter fotka = new OracleParameter("foto", OracleDbType.Varchar2);

                objCmd.Parameters.Add(id);
                objCmd.Parameters.Add(menoP);
                objCmd.Parameters.Add(priezvisko);
                objCmd.Parameters.Add(fotka);

                objCmd.Parameters["rc"].Value = i.ToString("0000000000");
                objCmd.Parameters["m"].Value = meno[0];
                objCmd.Parameters["p"].Value = meno[1];
                objCmd.Parameters["foto"].Value = "vp" + fotkyVodicov[random.Next(0, fotkyVodicov.Length)] + ".png";

                objCmd.ExecuteNonQuery();

                Console.WriteLine("VODIC pridany " + i);
            }
        }
    }
}


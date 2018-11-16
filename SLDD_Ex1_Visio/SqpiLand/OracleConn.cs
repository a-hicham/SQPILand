using Oracle.ManagedDataAccess.Client;
using System;
//using System.Data.SqlClient;
using System.Data.Common;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using SqpiLand.Model;

namespace SqpiLand
{
    class OracleConn : IConnectionObject
    {
        private static readonly string DWTABLEOBJECTS = "DWTABLEOBJECTS";
        private static readonly string DWFIELDS = "DWFIELDS";
        private static readonly string DWRELATIONS = "DWRELATIONS";

        private static DbProviderFactory _dbFactory;
        private static OracleConn instance = null;
        private static OracleConnection conn;
        private static ConnectionStringBuilderOracle connStringBuilder;
        //private static string user = null;
        //private static string pw = null;
        //private static string connString;
        private static DataTable dataTables;
        private static DataTable dataFields;
        private static DataTable dataRelations;

        private OracleConn()
        { }

        public static OracleConn GetInstance(string server, string port, string SID, string username, string password, DbProviderFactory provider)
        {
            if (instance == null)
                instance = new OracleConn();
            _dbFactory = provider;

            try
            {
                connStringBuilder = new ConnectionStringBuilderOracle(server, "", username, password, port, SID, null);
                conn = GetConnection();
            }
            catch (ArgumentException e)
            {
                MessageBox.Show("Connection String Error! (" + e.Message + ")");
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show("Invalid Operation Error! (" + e.Message + ")");
            }

            return instance;
        }

        private static OracleConnection GetConnection()
        {
            var connectionString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=" + connStringBuilder.Server + ")(PORT=" + 
                connStringBuilder.Port + ")) (CONNECT_DATA=(SERVICE_NAME=" + connStringBuilder.Sid + "))); User Id=" + connStringBuilder.Username + 
                ";Password=" + connStringBuilder.Password + ";";
            var connection = new OracleConnection(connectionString);
            return connection;
        }

        public IList<string> GetMetaDatabases()
        {
            IList<string> TablesList = new List<string>();
            try
            {
                conn.Open();
                // DataTable DBs = conn.GetSchema("Databases");
                //OracleConnection dbConn;
                //foreach (DataRow db in DBs.Rows)
                //{
                //    dbConn = new OracleConnection(connString);
                //    dbConn.Open();
                    DataTable Tables = conn.GetSchema("Tables");
                    foreach (DataRow table in Tables.Rows)
                        if (table[1].ToString().ToLower().Equals("massages") && !TablesList.Contains(table[0].ToString()))
                            TablesList.Add(table[0].ToString());
                   // conn.Close();
                   // conn.Dispose();
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (null != conn)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return TablesList;
        }

        public DBModel BuildModel(string dbName, bool withHostory)
        {
            string command;
            OracleCommand sqlCommand;
            OracleDataAdapter sqlAdapter;
            dataTables = new DataTable();
            dataFields = new DataTable();
            dataRelations = new DataTable();
            List<Table> myTables = new List<Table>();
            List<Field> myFields = new List<Field>();
            FieldComparer fieldComparer = new FieldComparer();
            List<Relation> myRelations = new List<Relation>();
            HashSet<Field> relFields = new HashSet<Field>();

            connStringBuilder.Username = dbName;
            connStringBuilder.Password = dbName;
            conn = GetConnection();

            //string connString = "Server=" + serverName + ";Database=" + dbName + ";Trusted_Connection=" + trusted.ToString() + (trusted ? ";" : ";User Id=" + user + ";Password=" + pw + ";");
            //SqlConnection dbConn = new SqlConnection(connString);
            conn.Open();

            command = "SELECT TABLEOBJECTID_SL, TABLEOBJECT_S, LANGT49_S, KINDOFOBJECT_S, DBNAME_S FROM " + DWTABLEOBJECTS + " WHERE DBNAME_S != 'MetaDB'";
            sqlCommand = new OracleCommand(command, conn);
            sqlAdapter = new OracleDataAdapter(sqlCommand);
            sqlAdapter.Fill(dataTables);

            command = "SELECT FIELDID_SL, FIELDNAME_S, LANGF49_S, TABLEOBJECTID_I, ORDERNR_SI FROM " + DWFIELDS;
            sqlCommand = new OracleCommand(command, conn);
            sqlAdapter = new OracleDataAdapter(sqlCommand);
            sqlAdapter.Fill(dataFields);

            command = "SELECT TA_ID_SL, FROMFIELDID_I, TOFIELDID_I, RELATIONTYPE_S FROM " + DWRELATIONS;
            sqlCommand = new OracleCommand(command, conn);
            sqlAdapter = new OracleDataAdapter(sqlCommand);
            sqlAdapter.Fill(dataRelations);

            conn.Close();
            conn.Dispose();

            foreach (DataRow row in dataTables.Rows)
            {
                if (withHostory || !row[2].ToString().StartsWith(@"Änderungshistorie zu:"))
                    myTables.Add(new Table(Convert.ToInt32(row[0]), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString()));
            }

            foreach (DataRow row in dataFields.Rows)
            {
                Table myTable = myTables.Find(table => table.Id == Convert.ToInt32(row[3]));
                if (myTable != null)
                {
                    Field myField = new Field(Convert.ToInt32(row[0]), row[1].ToString(), row[2].ToString(), myTable,
                        Convert.ToInt32(row[4].ToString()));
                    myTable.Fields.Add(myField);
                    myFields.Add(myField);
                }
                myTable?.Fields.Sort(fieldComparer);
            }


            foreach (DataRow row in dataRelations.Rows)
            {
                Field myFieldFrom = myFields.Find(field => field.Id == Convert.ToInt32(row[1]));
                Field myFieldTo = myFields.Find(field => field.Id == Convert.ToInt32(row[2]));
                relFields.Add(myFieldFrom);
                relFields.Add(myFieldTo);
                if (myFieldFrom != null && myFieldTo != null)
                {
                    if (!myRelations.Exists(rel => rel.FromField.Id == myFieldTo.Id && rel.ToField.Id == myFieldFrom.Id))
                    {
                        myRelations.Add(new Relation(Convert.ToInt32(row[0]), myFieldFrom, myFieldTo, row[3].ToString().Trim()));
                    }
                    else
                    {
                        myRelations.Find(rel => rel.FromField.Id == myFieldTo.Id && rel.ToField.Id == myFieldFrom.Id).TypeTo = row[3].ToString().Trim();
                    }
                }
            }

            //foreach(Relation relation in myRelations)
            //{
            //    Relation rel1 = myRelations.Find(rel => rel.ToField.Id == relation.FromField.Id && rel.FromField.Id == relation.ToField.Id);
            //    relation.TypeTo = rel1.TypeFrom;
            //    myRelations.Remove(rel1);
            //}


            return new DBModel(dbName, myTables, myRelations, relFields);
        }
    }
}

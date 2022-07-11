using Oracle.ManagedDataAccess.Client;
using System;
//using System.Data.SqlClient;
using System.Data.Common;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using SqpiLand.Model;
//using Microsoft.Office.Interop.Visio;
using System.Linq;
using Application = System.Windows.Application;

namespace SqpiLand
{
    class OracleConn : AConnectionObject
    {
        private static readonly string DWTABLEOBJECTS = ".DWTABLEOBJECTS";
        private static readonly string DWFIELDS = ".DWFIELDS";
        private static readonly string DWRELATIONS = ".DWRELATIONS";

        //private static DbProviderFactory _dbFactory;
        private static OracleConn instance = null;
        private static DbConnection conn;
        private static ConnectionStringBuilderOracle connStringBuilder;
        private static DataTable MetaDBs;
        private static DataTable dataTables;
        private static DataTable dataFields;
        private static DataTable dataRelations;
        private string command;

        private OracleConn() {}

        public static OracleConn GetInstance(string server, string port, string SID, string username, string password, DbProviderFactory provider)
        {
            if (instance == null)
                instance = new OracleConn();
//            _dbFactory = provider;

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

            return new OracleConnection(connectionString);
        }

        public override IDictionary<string,string> GetMetaDatabases()
        {
            IDictionary<string,string> TablesList = new Dictionary<string,string>();
            try
            {
                conn.Open();

                IEnumerable<DataRow> EntryDBs = conn.GetSchema("Tables").Select().Where(e => e.ItemArray[1].ToString().ToLower() == "liodatabases");
                command = "";

                foreach (DataRow table in EntryDBs)
                {
                    if (command != "")
                        command += " UNION ";
                    command += @"SELECT DBSYN_S, NAME_S, '" + table[0] + "' FROM " + table[0] + "." + table[1] + @" WHERE DBNAME_S = 'MetaDB'";
                }

                MetaDBs = new DataTable();
                OracleCommand sqlCommand = new OracleCommand(command, (OracleConnection)conn);
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(sqlCommand);
                sqlAdapter.Fill(MetaDBs);


                foreach(DataRow row in MetaDBs.Rows)
                {
                    TablesList.Add(row[0].ToString() + " (" + row[2].ToString() + ")", row[1].ToString());
                }
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

        public override DBModel BuildModel(string dbName, bool withHistory)
        {
            DbCommand sqlCommand;
            DbDataAdapter sqlAdapter;
            dataTables = new DataTable();
            dataFields = new DataTable();
            dataRelations = new DataTable();

            connStringBuilder.Username = ((MainWindow)Application.Current.MainWindow).UsernameText.Text;
            connStringBuilder.Password = ((MainWindow)Application.Current.MainWindow).PasswordText.Password;
           
            conn = GetConnection();

            conn.Open();

            command = "SELECT TABLEOBJECTID_SL, TABLEOBJECT_S, LANGT49_S, KINDOFOBJECT_S, DBNAME_S FROM " + dbName + DWTABLEOBJECTS + " WHERE DBNAME_S != 'MetaDB'";
            sqlCommand = new OracleCommand(command, (OracleConnection)conn);
            sqlAdapter = new OracleDataAdapter((OracleCommand)sqlCommand);
            sqlAdapter.Fill(dataTables);

            command = "SELECT FIELDID_SL, FIELDNAME_S, LANGF49_S, TABLEOBJECTID_I, ORDERNR_SI FROM " + dbName + DWFIELDS;
            sqlCommand = new OracleCommand(command, (OracleConnection)conn);
            sqlAdapter = new OracleDataAdapter((OracleCommand)sqlCommand);
            sqlAdapter.Fill(dataFields);

            command = "SELECT TA_ID_SL, FROMFIELDID_I, TOFIELDID_I, RELATIONTYPE_S FROM " + dbName + DWRELATIONS;
            sqlCommand = new OracleCommand(command, (OracleConnection)conn);
            sqlAdapter = new OracleDataAdapter((OracleCommand)sqlCommand);
            sqlAdapter.Fill(dataRelations);

            conn.Close();
            conn.Dispose();

            return CreateModel(dbName, withHistory, dataTables, dataFields, dataRelations);
        }
    }
}

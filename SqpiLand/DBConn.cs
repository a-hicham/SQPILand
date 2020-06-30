using System;
using System.Data.SqlClient;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;
using SqpiLand.Model;
using Oracle.ManagedDataAccess.Client;
using System.Linq;

namespace SqpiLand
{
    class DBConn : AConnectionObject
    {
        private static readonly string DWTABLEOBJECTS = "DWTABLEOBJECTS";
        private static readonly string DWFIELDS = "DWFIELDS";
        private static readonly string DWRELATIONS = "DWRELATIONS";

        private static DBConn instance = null;
        private static DbConnection conn;
        private static DbConnectionStringBuilder dbConnStringBuilder = new DbConnectionStringBuilder();
        private static string serverName;
        private static string initDB;
        private static bool trusted;
        private static string user = null;
        private static string pw = null;
        private static string connString;
        private static DataTable MetaDBs;
        private static DataTable dataTables;
        private static DataTable dataFields;
        private static DataTable dataRelations;
        DbCommand sqlCommand;
        DbDataAdapter sqlAdapter;
        string command;

        private DBConn()
        { }

        public static DBConn GetInstance(string server, string initialDB, bool trustedState, string username, string password, DbProviderFactory provider)
        {
            if (instance == null)
                instance = new DBConn();

            serverName = server;
            if(null != initialDB)
                initDB = initialDB;
            trusted = trustedState;
            user = username;
            pw = password;
            
            try
            {
                //connString = @"Server=" + server + (initDB != null ? @"\" + initDB : "") + ";Trusted_Connection=" + trusted + (trusted ? "" : ";User Id=" + user + ";Password=" + pw) + ";";
                connString = @"Server=" + server + (initDB != null ? @";DataBase=" + initDB : "") + ";Trusted_Connection=" + trusted + (trusted ? "" : ";User Id=" + user + ";Password=" + pw) + ";";
                dbConnStringBuilder.ConnectionString = connString;
                conn = new SqlConnection(connString);
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

        public override IDictionary<string,string> GetMetaDatabases ()
        {
            //*******************************//
            //ToDo: MetaDBs aus EntryDB holen//
            //*******************************//

            IDictionary<string,string> TablesList = new Dictionary<string,string>();
            try
            {
                conn.Open();
                DataTable DBs = conn.GetSchema("Databases");
                SqlConnection dbConn;

                foreach (DataRow db in DBs.Rows)
                {
                    var dbConnString = "Server=" + serverName + ";Database=" + db[0] + ";Trusted_Connection=" + trusted + (trusted ? "" : ";User Id=" + user + ";Password=" + pw) + ";";
                    dbConn = new SqlConnection(dbConnString);
                    dbConn.Open();

                    IEnumerable<DataRow> EntryDbs = dbConn.GetSchema("Tables").Select().Where(e => e[2].ToString().ToLower().Equals("liodatabases"));

                    command = "";

                    foreach (DataRow table in EntryDbs)
                    {
                        if (command != "")
                            command += " UNION ";
                        command += @"SELECT DBSYN_S, NAME_S FROM " + table[0] + "." + table[1] + "." + table[2] + @" WHERE DBNAME_S = 'MetaDB'";
                    }

                    if(command != "")
                    {
                        MetaDBs = new DataTable();

                        sqlCommand = new SqlCommand(command, dbConn);
                        sqlAdapter = new SqlDataAdapter((SqlCommand)sqlCommand);
                        sqlAdapter.Fill(MetaDBs);

                        foreach (DataRow row in MetaDBs.Rows)
                        {
                            TablesList.Add(row[0].ToString(), row[1].ToString());
                        }
                    }


                    /*
                    foreach (DataRow table in Tables.Rows)
                        if (table[2].ToString().Equals("massages") && !TablesList.Contains(table[0].ToString()))
                            TablesList.Add(table[0].ToString());
                    dbConn.Close();
                    dbConn.Dispose();
                    */
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

        public override DBModel BuildModel(string dbName, bool withHostory)
        {
            string command;
            dataTables = new DataTable();
            dataFields = new DataTable();
            dataRelations = new DataTable();

            string connString = "Server=" + serverName + ";Database=" + dbName + ";Trusted_Connection=" + trusted + (trusted ? ";" : ";User Id=" + user + ";Password=" + pw + ";");
            SqlConnection dbConn = new SqlConnection(connString);
            try
            {
                dbConn.Open();
                DataTable tables = dbConn.GetSchema("Tables");
                string schema = ".dbo.";

                foreach(DataRow row in tables.Rows)
                {
                    if (row[2].ToString().ToLower().Equals(DWTABLEOBJECTS.ToLower()))
                    {
                        schema = "." + row[1].ToString() + ".";
                        break;
                    }
                }

                command = "SELECT TABLEOBJECTID_SL, TABLEOBJECT_S, LANGT49_S, KINDOFOBJECT_S, DBNAME_S FROM " + initDB + schema + DWTABLEOBJECTS + " WHERE DBNAME_S != 'MetaDB'";

                sqlCommand = new SqlCommand(command, dbConn);               
                sqlAdapter = new SqlDataAdapter((SqlCommand) sqlCommand);
                sqlAdapter.Fill(dataTables);

                command = "SELECT FIELDID_SL, FIELDNAME_S, LANGF49_S, TABLEOBJECTID_I, ORDERNR_SI FROM " + initDB + schema + DWFIELDS;
                sqlCommand = new SqlCommand(command, dbConn);
                sqlAdapter = new SqlDataAdapter((SqlCommand) sqlCommand);
                sqlAdapter.Fill(dataFields);

                command = "SELECT TA_ID_SL, FROMFIELDID_I, TOFIELDID_I, RELATIONTYPE_S FROM " + initDB + schema + DWRELATIONS;
                sqlCommand = new SqlCommand(command, dbConn);
                sqlAdapter = new SqlDataAdapter((SqlCommand) sqlCommand);
                sqlAdapter.Fill(dataRelations);
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                dbConn.Close();
                dbConn.Dispose();
            }

            return CreateModel(dbName ,withHostory, dataTables, dataFields, dataRelations);
        }
    }
}

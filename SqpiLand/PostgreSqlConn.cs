using System;
//using System.Data.SqlClient;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;
using Npgsql;
using SqpiLand.Model;
//using Oracle.ManagedDataAccess.Client;
using System.Linq;

namespace SqpiLand
{
    class PostgreSqlConn : AConnectionObject
    {
        private static readonly string DWTABLEOBJECTS = ".DWTABLEOBJECTS";
        private static readonly string DWFIELDS = ".DWFIELDS";
        private static readonly string DWRELATIONS = ".DWRELATIONS";

        private static PostgreSqlConn instance = null;
        private static DbConnection conn;
        private static DbConnectionStringBuilder dbConnStringBuilder = new DbConnectionStringBuilder();
        private static string serverName;
        private static string initDB;
        //private static bool trusted;
        private static string user = null;
        private static string pw = null;
        private static string connString;
        private static DataTable MetaDBs;
        DbCommand sqlCommand;
        DbDataAdapter sqlAdapter;
        string command;

        private PostgreSqlConn()
        { }

        public static PostgreSqlConn GetInstance(string server, string port, string initialDB, string username, string password)
        {
            if (instance == null)
                instance = new PostgreSqlConn();

            serverName = server;
            if (null != initialDB)
                initDB = initialDB;
            //trusted = trustedState;
            user = username;
            pw = password;

            try
            {
                //connString = @"Server=" + server + (initDB != null ? @"\" + initDB : "") + ";Trusted_Connection=" + trusted + (trusted ? "" : ";User Id=" + user + ";Password=" + pw) + ";";
                connString = @"Server=" + server + @";DataBase=" + initDB + ";User Id=" + user + ";Password=" + pw + ";Port=" + port + ";";
                dbConnStringBuilder.ConnectionString = connString;
                conn = new NpgsqlConnection(connString);
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

        public override IDictionary<string,string> GetMetaDatabases()
        {
            IDictionary<string,string> TablesList = new Dictionary<string,string>();
            try
            {
                conn.Open();
                DataTable DBs = conn.GetSchema("Databases");
                DbConnection dbConn;
                foreach (DataRow db in DBs.Rows)
                {
                    if (initDB.Equals(db[0]))
                    {
                        var dbConnString = "Server=" + serverName + ";Database=" + db[0] + ";User Id=" + user + ";Password=" + pw + ";";
                        dbConn = new NpgsqlConnection(dbConnString);
                        dbConn.Open();

                        IEnumerable<DataRow> Tables = dbConn.GetSchema("Tables").Select().Where(e => e[2].ToString().ToLower().Equals("liodatabases"));

                        command = "";

                        foreach(DataRow table in Tables)
                        {
                            if (command != "")
                                command += " UNION ";

                            command += @"SELECT DBSYN_S, NAME_S FROM " + table[1] + "." + table[2] + @" WHERE DBNAME_S = 'MetaDB'";
                        }

                        if (command != "")
                        {
                            MetaDBs = new DataTable();

                            sqlCommand = new NpgsqlCommand(command, (NpgsqlConnection)dbConn);
                            sqlAdapter = new NpgsqlDataAdapter((NpgsqlCommand)sqlCommand);
                            sqlAdapter.Fill(MetaDBs);

                            foreach (DataRow row in MetaDBs.Rows)
                            {
                                TablesList.Add(row[0].ToString(), row[1].ToString());
                            }
                        }
                    }
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
            var connString = "Server=" + serverName + ";Database=" + initDB + ";User Id=" + user + ";Password=" + pw + ";";
            using (var dbConn = new NpgsqlConnection(connString))
                try
                {
                    dbConn.Open();

                    var command = "SELECT TABLEOBJECTID_SL, TABLEOBJECT_S, LANGT49_S, KINDOFOBJECT_S, DBNAME_S FROM " + dbName + DWTABLEOBJECTS + " WHERE DBNAME_S != 'MetaDB'";
                    var dataTables = new DataTable();

                    using (var sqlCommand = new NpgsqlCommand(command, dbConn))
                    using (var sqlAdapter = new NpgsqlDataAdapter(sqlCommand))
                        sqlAdapter.Fill(dataTables);


                    command = "SELECT FIELDID_SL, FIELDNAME_S, LANGF49_S, TABLEOBJECTID_I, ORDERNR_SI FROM " + dbName + DWFIELDS;
                    var dataFields = new DataTable();

                    using (var sqlCommand = new NpgsqlCommand(command, dbConn))
                    using (var sqlAdapter = new NpgsqlDataAdapter(sqlCommand))
                        sqlAdapter.Fill(dataFields);


                    command = "SELECT TA_ID_SL, FROMFIELDID_I, TOFIELDID_I, RELATIONTYPE_S FROM " + dbName + DWRELATIONS;
                    var dataRelations = new DataTable();

                    using (var sqlCommand = new NpgsqlCommand(command, dbConn))
                    using (var sqlAdapter = new NpgsqlDataAdapter(sqlCommand))
                        sqlAdapter.Fill(dataRelations);


                    return CreateModel(dbName, withHostory, dataTables, dataFields, dataRelations);
                }

                catch (Exception e)
                { }
                finally
                {
                    dbConn.Close();
                    dbConn.Dispose();
                }

            throw new ApplicationException();
        }

    }
}

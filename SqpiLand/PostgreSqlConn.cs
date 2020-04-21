using System;
using System.Data.SqlClient;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;
using Npgsql;
using SqpiLand.Model;
using Oracle.ManagedDataAccess.Client;

namespace SqpiLand
{
    class PostgreSqlConn : IConnectionObject
    {
        private static readonly string DWTABLEOBJECTS = ".DWTABLEOBJECTS";
        private static readonly string DWFIELDS = ".DWFIELDS";
        private static readonly string DWRELATIONS = ".DWRELATIONS";

        private static PostgreSqlConn instance = null;
        private static DbConnection conn;
        private static DbConnectionStringBuilder dbConnStringBuilder = new DbConnectionStringBuilder();
        private static string serverName;
        private static string initDB;
        private static bool trusted;
        private static string user = null;
        private static string pw = null;
        private static string connString;
        private static DataTable dataTables;
        private static DataTable dataFields;
        private static DataTable dataRelations;

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

        public IList<string> GetMetaDatabases()
        {
            //*******************************//
            //ToDo: MetaDBs aus EntryDB holen//
            //*******************************//

            IList<string> TablesList = new List<string>();
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
                        DataTable Tables = dbConn.GetSchema("Tables");
                        foreach (DataRow table in Tables.Rows)
                            if (table[2].ToString().Equals("massages") && !TablesList.Contains(table[0].ToString()))
                                TablesList.Add(table[1].ToString());
                        dbConn.Close();
                        dbConn.Dispose();
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

        public DBModel BuildModel(string dbName, bool withHostory)
        {
            string command;
            DbCommand sqlCommand;
            DbDataAdapter sqlAdapter;
            dataTables = new DataTable();
            dataFields = new DataTable();
            dataRelations = new DataTable();
            List<Table> myTables = new List<Table>();
            List<Field> myFields = new List<Field>();
            FieldComparer fieldComparer = new FieldComparer();
            List<Relation> myRelations = new List<Relation>();
            HashSet<Field> relFields = new HashSet<Field>();

            int k = 0;
            foreach (Field f in myFields)
            {
                k++;
            }

            string connString = "Server=" + serverName + ";Database=" + initDB + ";User Id=" + user + ";Password=" + pw + ";";
            DbConnection dbConn = new NpgsqlConnection(connString);
            try
            {
                dbConn.Open();

                command = "SELECT TABLEOBJECTID_SL, TABLEOBJECT_S, LANGT49_S, KINDOFOBJECT_S, DBNAME_S FROM " + dbName + DWTABLEOBJECTS + " WHERE DBNAME_S != 'MetaDB'";
                sqlCommand = new NpgsqlCommand(command, (NpgsqlConnection)dbConn);
                sqlAdapter = new NpgsqlDataAdapter((NpgsqlCommand)sqlCommand);

                sqlAdapter.Fill(dataTables);

                command = "SELECT FIELDID_SL, FIELDNAME_S, LANGF49_S, TABLEOBJECTID_I, ORDERNR_SI FROM " + dbName + DWFIELDS;
                sqlCommand = new NpgsqlCommand(command, (NpgsqlConnection)dbConn);
                sqlAdapter = new NpgsqlDataAdapter((NpgsqlCommand)sqlCommand);
                sqlAdapter.Fill(dataFields);

                command = "SELECT TA_ID_SL, FROMFIELDID_I, TOFIELDID_I, RELATIONTYPE_S FROM " + dbName + DWRELATIONS;
                sqlCommand = new NpgsqlCommand(command, (NpgsqlConnection)dbConn);
                sqlAdapter = new NpgsqlDataAdapter((NpgsqlCommand)sqlCommand);
                sqlAdapter.Fill(dataRelations);
            }

            catch (Exception e)
            { }
            finally
            {
                dbConn.Close();
                dbConn.Dispose();
            }

            foreach (DataRow row in dataTables.Rows)
            {
                if (withHostory || !row[2].ToString().StartsWith(@"Änderungshistorie zu:"))
                    myTables.Add(new Table(Convert.ToInt32(row[0]), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString()));
            }

            foreach (DataRow row in dataFields.Rows)
            {
                var myTable = myTables.Find(table => table.Id == Convert.ToInt32(row[3]));
                if (myTable != null)
                {
                    var myField = new Field(Convert.ToInt32(row[0]), row[1].ToString(), row[2].ToString(), myTable, Convert.ToInt32(row[3].ToString()));
                    myTable.Fields.Add(myField);
                    myFields.Add(myField);
                }
                myTable?.Fields.Sort(fieldComparer);
            }

            foreach (DataRow row in dataRelations.Rows)
            {
                var myFieldFrom = myFields.Find(field => field.Id == Convert.ToInt32(row[1]));
                var myFieldTo = myFields.Find(field => field.Id == Convert.ToInt32(row[2]));
                relFields.Add(myFieldFrom);
                relFields.Add(myFieldTo);
                if (myFieldFrom != null && myFieldTo != null)
                {
                    //if(!myRelations.Exists(rel => rel.FromField.Id == myFieldTo.Id && rel.ToField.Id == myFieldFrom.Id))
                    //    myRelations.Add(new Model.Relation(Convert.ToInt32(row[0]), myFieldFrom, myFieldTo, row[3].ToString()));

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

            return new DBModel(dbName, myTables, myRelations, relFields);
        }

    }
}

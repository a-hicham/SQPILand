namespace SqpiLand.Model

{
    class ConnectionStringBuilderOracle : ConnectionStringBuilderBase
    {
        private string port = null;
        private string sid = null;
        private bool? trusted = null;

        public string Port { get => port; set => port = value; }
        public string Sid { get => sid; set => sid = value; }
        public bool? Trusted { get => trusted; set => trusted = value; }

        /// <exception cref="ConnectionStringBuilderException">Parameter passen nicht</exception>
        public ConnectionStringBuilderOracle(string server, string baseDB, string username, string password, string port, string sid, bool? trusted)
            : base(server, baseDB, username, password)
        {
            if (trusted != null)
            {
                base.Art = DBArts.MSSQL;
                Trusted = trusted;
            }
            else
            {
                if (sid != null && port != null)
                {
                    Art = DBArts.ORACLE;
                    Sid = sid;
                    Port = port;
                }
                else
                    throw new ConnectionStringBuilderException();
            }
        }

        /// <exception cref="ConnectionStringBuilderException">Parameter passen nicht</exception>
        public string getConnectionString()
        {
            string connString = null;

            if (Art == DBArts.MSSQL)
            {
                System.Data.Common.DbConnectionStringBuilder dbConnStringBuilder = new System.Data.Common.DbConnectionStringBuilder();
                connString = @"Server=" + Server + ";Initial Catalog=" + BaseDB + ";Trusted_Connection=" + Trusted.ToString() + ((bool)Trusted ? "" : ";User Id=" + Username + ";Password=" + Password) + ";";

                try
                {
                    dbConnStringBuilder.ConnectionString = connString;
                }
                catch (System.ArgumentException e)
                {
                    throw new ConnectionStringBuilderException();
                }
            }
            if (Art == DBArts.ORACLE)
            {
                System.Data.OracleClient.OracleConnectionStringBuilder dbConnStringBuilder = new System.Data.OracleClient.OracleConnectionStringBuilder();
                connString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=" + Server + ")(PORT=" + Port + ")) (CONNECT_DATA=(SERVICE_NAME=" + Sid + "))); User Id=" + Username + ";Password=" + Password + ";";

                try
                {
                    dbConnStringBuilder.ConnectionString = connString;
                }
                catch (System.ArgumentException e)
                {
                    throw new ConnectionStringBuilderException();
                }
            }
            return connString;
        }
    }
}

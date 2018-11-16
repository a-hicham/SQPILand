namespace SqpiLand.Model
{
    internal abstract class ConnectionStringBuilderBase
    {
        public enum DBArts
        {
            MSSQL, ORACLE
        }

        private DBArts art;
        private string server;
        private string username;
        private string password;
        private string baseDB;

        public string Server { get => server; set => server = value; }
        public string BaseDB { get => baseDB; set => baseDB = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public DBArts Art { get => art; set => art = value; }

        protected ConnectionStringBuilderBase(string server, string baseDB, string username, string password)
        {
            Server = server;
            BaseDB = baseDB;
            Username = username;
            Password = password;
        }
    }
}
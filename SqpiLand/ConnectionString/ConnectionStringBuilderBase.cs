namespace SqpiLand.Model
{
    internal abstract class ConnectionStringBuilderBase
    {
        public enum DBArts
        {
            MSSQL, ORACLE, POSTGRES
        }

        public string Server { get; set; }
        public string BaseDB { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DBArts Art { get; set; }

        protected ConnectionStringBuilderBase(string server, string baseDB, string username, string password)
        {
            Server = server;
            BaseDB = baseDB;
            Username = username;
            Password = password;
        }
    }
}
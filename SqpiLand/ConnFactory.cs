using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqpiLand
{
    class ConnFactory
    {
        private readonly static Dictionary<string, string> providers = new Dictionary<string, string>()
            {
                { "MSSQL", "System.Data.SqlClient" },
                { "Oracle", "System.Data.OracleClient" }
            };

        private static System.Data.Common.DbProviderFactory dbFactory;
        //public static IConnectionObject createMSSQLConnection()
        //{
        //    return 
        //}
        internal static IConnectionObject createConnection(string provider, string server, string initDB, bool trusted, string username, string password, string port, string sid)
        {
            dbFactory = System.Data.Common.DbProviderFactories.GetFactory(providers[provider]);
            if(provider.Equals("Oracle"))
            {
                return OracleConn.GetInstance(server, port, sid, username, password, dbFactory);
            }
            if (provider.Equals("MSSQL"))
            {
                return DBConn.GetInstance(server, initDB, trusted, username, password, dbFactory);
            }
            throw new NotImplementedException();
        }
    }
}

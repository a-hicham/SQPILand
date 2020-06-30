namespace SqpiLand
{
    internal interface IConnectionObject
    {
        System.Collections.Generic.IDictionary<string,string> GetMetaDatabases();
        Model.DBModel BuildModel(string dbName, bool withHistory);
    }
}
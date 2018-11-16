namespace SqpiLand
{
    internal interface IConnectionObject
    {
        System.Collections.Generic.IList<string> GetMetaDatabases();
        Model.DBModel BuildModel(string dbName, bool withHistory);
    }
}
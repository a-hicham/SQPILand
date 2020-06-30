using SqpiLand.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqpiLand
{
    abstract class AConnectionObject : IConnectionObject
    {
        public abstract DBModel BuildModel(string dbName, bool withHistory);
        public abstract IDictionary<string, string> GetMetaDatabases();

        protected DBModel CreateModel(string dbName, bool withHistory, DataTable dataTables, DataTable dataFields, DataTable dataRelations)
        {
            List<Table> myTables = new List<Table>();
            List<Field> myFields = new List<Field>();
            FieldComparer fieldComparer = new FieldComparer();
            List<Relation> myRelations = new List<Relation>();
            HashSet<Field> relFields = new HashSet<Field>();

            foreach (DataRow row in dataTables.Rows)
            {
                if (withHistory || !row[2].ToString().StartsWith(@"Änderungshistorie zu:"))
                    myTables.Add(new Table(Convert.ToInt32(row[0]), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString()));
            }

            foreach (DataRow row in dataFields.Rows)
            {
                var myTable = myTables.Find(table => table.Id == Convert.ToInt32(row[3]));
                if (myTable != null)
                {
                    var myField = new Field(Convert.ToInt32(row[0]), row[1].ToString(), row[2].ToString(), myTable,
                        Convert.ToInt32(row[4].ToString()));
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

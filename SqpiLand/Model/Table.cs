using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqpiLand.Model
{
    class Table
    {
        public int Id;
        public string Name;
        public string NameD;
        public string KindOfObject;
        public string DbName;
        public List<Field> Fields = new List<Field>();

        public Table(int id, string name, string nameD, string kindOfObject, string dbName)
        {
            this.Id = id;
            this.Name = name;
            this.NameD = nameD;
            this.KindOfObject = kindOfObject;
            this.DbName = dbName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqpiLand.Model
{
    class Field
    {
        public int Id;
        public string Name;
        public string NameD;
        //public string type;
        public Table Table;
        public int OrderNr;
        public string ShapeID;

        public Field(int id, string name, string nameD, Table table, int orderNr)
        {
            this.Id = id;
            this.Name = name;
            this.NameD = nameD;
            this.Table = table;
            this.OrderNr = orderNr;
        }
    }
}

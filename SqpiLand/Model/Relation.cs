using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqpiLand.Model
{
    class Relation
    {
        public int Id;
        internal Field FromField;
        internal Field ToField;
        public string TypeFrom;
        public string TypeTo;

        public Relation(int id, Field from, Field to, string type)
        {
            this.Id = id;
            this.FromField = from;
            this.ToField = to;
            this.TypeFrom = type;
        }
    }
}

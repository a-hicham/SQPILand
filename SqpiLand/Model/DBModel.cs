using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqpiLand.Model
{
    class DBModel
    {
        public string serverName;
        public List<Table> TablesList;
        public List<Relation> RelationsList;
        public HashSet<Field> FieldList;

        public DBModel(string name,  List<Table> tables, List<Relation> relations, HashSet<Field> fields)
        {
            serverName = name;
            this.TablesList = tables;
            this.RelationsList = relations;
            this.FieldList = fields;
        }
    }
}

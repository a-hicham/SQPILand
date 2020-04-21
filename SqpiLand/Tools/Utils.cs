using System.Linq;
using System.Xml.Linq;
using SqpiLand.Model;

namespace SqpiLand.Tools
{
     class Utils
    {
        public static XElement ModelToXML(DBModel model)
        {
            return new XElement("server", new XAttribute("name", model.serverName),
                new XElement("tables", model.TablesList.Select(x =>
                    new XElement("table", new XAttribute("id", x.Id), new XAttribute("name", x.Name),
                        new XElement("fields", x.Fields.Select(y =>
                            new XElement("field", new XAttribute("id", y.Id), y.Name)))))),
                new XElement("relations",
                    model.RelationsList.Select(r => new XElement("relation", new XAttribute("id", r.Id),
                        new XElement("fromField", new XAttribute("id", r.FromField.Id), new XAttribute("tableID", r.FromField.Table.Id), r.FromField.Name), new XElement("fromType", r.TypeFrom),
                        new XElement("toField", new XAttribute("id", r.ToField.Id), new XAttribute("tableID", r.ToField.Table.Id), r.ToField.Name), new XElement("toType", r.TypeTo)))));
        }
    }
}
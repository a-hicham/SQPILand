using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqpiLand.Model
{
    class FieldComparer : IComparer<Field>
    {
        public int Compare(Field x, Field y)
        {
            return x.OrderNr - y.OrderNr;
        }
    }
}

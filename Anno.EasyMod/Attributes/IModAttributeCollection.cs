using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Attributes
{
    public interface IModAttributeCollection 
        : INotifyCollectionChanged,
        IList<IModAttribute>
    {
        IEnumerable<IModAttribute> OfAttributeType(string attributeType);
        bool HasAttributeOfType(string attributeType);
        void RemoveByType(string attributeType);
    }
}

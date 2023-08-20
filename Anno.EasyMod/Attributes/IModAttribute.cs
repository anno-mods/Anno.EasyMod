using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Attributes
{
    public interface IModAttribute
    {
        string AttributeType { get; }
        bool MultipleAllowed { get; }
    }
}

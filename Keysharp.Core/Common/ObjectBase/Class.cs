using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Core.Common.ObjectBase
{
    public class Class : KeysharpObject
    {
        public Class(params object[] args) : base(args) { }
    }

    public class Prototype : KeysharpObject
    {
        public Prototype(params object[] args) : base(args) { }
    }
}

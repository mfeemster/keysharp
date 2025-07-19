using System.ComponentModel.Design.Serialization;

namespace Keysharp.Core.Common.ObjectBase
{
    public class Class : KeysharpObject
    {
        public Class(params object[] args) : base(args) { }

		public virtual object staticCall(params object[] args) => Activator.CreateInstance(this.GetType(), args);
	}

    public class Prototype : KeysharpObject
    {
        public Prototype(params object[] args) : base(args) { }
    }
}

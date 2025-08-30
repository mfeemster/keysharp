#if WINDOWS
[assembly: ComVisible(true)]
#endif

namespace Keysharp.Core.Common.ObjectBase
{
#if WINDOWS
	[Guid("98D592E1-0CE8-4892-82C5-F219B040A390")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ProgId("Keysharp.Script")]
	public partial class Any : IReflect
#else
	public class Any
#endif
	{
		protected internal Dictionary<string, OwnPropsDesc> op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

		internal Any _base;
		[PublicForTestOnly]
		public virtual Any Base
		{
			get => _base;
			set => Errors.ErrorOccurred($"The base can't be changed for the type {GetType()}");
		}

		// In some cases we wish to skip the automatic calls to __Init and __New (eg when creating OwnProps),
		// so in those cases we can initialize with `skipLogic: true`
		protected bool SkipConstructorLogic { get; }

		public Any(params object[] args)
		{
			// Skip Map and OwnPropsMap because SetPropertyValue will cause recursive stack overflow
			// (if the property doesn't exist then a new Map is created which calls this function again)
			if (Script.TheScript.Vars.Prototypes == null || SkipConstructorLogic
				// Hack way to check that Prototypes/Statics are initialized
				|| Script.TheScript.Vars.Statics.Count < 10)
			{
				__New(args);
				return;
			}

			var t = GetType();
			Script.TheScript.Vars.Statics.TryGetValue(t, out Any value);
			if (value == null)
			{
				__New(args);
				return;
			}

			_base = (Any)value.op["prototype"].Value;
			GC.SuppressFinalize(this); // Otherwise if the constructor throws then the destructor is called
			Script.InvokeMeta(this, "__Init");
			Script.InvokeMeta(this, "__New", args);
			GC.ReRegisterForFinalize(this);
		}

		public Any(bool skipLogic)
		{
			SkipConstructorLogic = skipLogic;
		}

		~Any()
		{
			Script.InvokeMeta(this, "__Delete");
		}

		public virtual object __New(params object[] args) => "";
		public virtual object static__New(params object[] args) => "";
		public virtual object __Init() => "";
		public virtual object static__Init() => "";
		public virtual object __Delete() => "";
		public virtual object static__Delete() => "";

		private static Type GetCallingType()
        {
            var frame = new System.Diagnostics.StackTrace().GetFrame(2); // Get the caller two levels up
            return frame?.GetMethod()?.DeclaringType;
        }

		public virtual object GetMethod(object obj0 = null, object obj1 = null) => Functions.GetMethod(this, obj0, obj1);

		public long HasBase(object obj) => Types.HasBase(this, obj);

		public long HasMethod(object obj0 = null, object obj1 = null) => Functions.HasMethod(this, obj0, obj1);

		public long HasProp(object obj) => Functions.HasProp(this, obj);

		//public virtual string tostring() => ToString();

		internal virtual object Clone()
		{
			return MemberwiseClone();
		}
	}
}
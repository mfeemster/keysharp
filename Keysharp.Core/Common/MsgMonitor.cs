using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Core.Common
{
	internal class MsgMonitorStruct
	{
		//union//Unsure if union will be needed here.
		//{
		internal IFuncObj func;
		internal string methodName; // Used only by GUI.
		//LPVOID union_value; // Internal use.
		//};
		internal uint msg;
		// Keep any members smaller than 4 bytes adjacent to save memory:
		internal static int MAX_INSTANCES = Keysharp.Scripting.Script.MAX_THREADS_LIMIT; // For maintainability.  Causes a compiler warning if MAX_THREADS_LIMIT > MAX_UCHAR.
		internal int instanceCount; // Distinct from func.mInstances because the script might have called the function explicitly.
		internal int maxInstances; // v1.0.47: Support more than one thread.
		internal int msgType; // Used only by GUI, so may be ignored by some methods.
		internal bool isMethod; // Used only by GUI.
	}

	internal class MsgMonitorList
	{
		internal MsgMonitorInstance top;
		internal List<MsgMonitorStruct> monitor = new List<MsgMonitorStruct>(MsgMonitorStruct.MAX_INSTANCES);
		internal int count => monitor.Count;

		internal MsgMonitorStruct this[int index] => monitor[index];

		internal MsgMonitorStruct Find(uint msg, IFuncObj callback, int msgType)
		{
			foreach (var mon in monitor)
				if (mon.msg == msg
						&& mon.func == callback // No need to check is_method, since it's impossible for an object and string to exist at the same address.
						&& mon.msgType == msgType) // Checked last because it's nearly always true.
					return mon;

			return null;
		}

		internal MsgMonitorStruct Find(uint msg, string methodName, int msgType)
		{
			foreach (var mon in monitor)
				if (mon.msg == msg
						&& mon.isMethod && string.Compare(methodName, mon.methodName, true) == 0
						&& mon.msgType == msgType) // Checked last because it's nearly always true.
					return mon;

			return null;
		}

		internal MsgMonitorStruct AddInternal(uint msg, bool append)
		{
			MsgMonitorStruct newMon;

			if (!append)
			{
				for (var inst = top; inst != null; inst = inst.previous)
				{
					inst.index++; // Correct the index of each running monitor.
					inst.count++; // Iterate the same set of items which existed before.
					// By contrast, count isn't adjusted when adding at the end because we do not
					// want new items to be called by messages received before they were registered.
				}

				monitor.Insert(0, newMon = new MsgMonitorStruct());
			}
			else
				monitor.Add(newMon = new MsgMonitorStruct());

			newMon.msg = msg;
			newMon.msgType = 0; // Must be initialised to 0 for all callers except GUI.
			// These are initialised by OnMessage, since OnExit and OnClipboardChange don't use them:
			//new_mon.instance_count = 0;
			//new_mon.max_instances = 1;
			return newMon;
		}

		internal MsgMonitorStruct Add(uint msg, IFuncObj callback, bool append)
		{
			var new_mon = AddInternal(msg, append);

			if (new_mon != null)
			{
				new_mon.func = callback;
				new_mon.isMethod = false;
			}

			return new_mon;
		}

		internal MsgMonitorStruct Add(uint msg, string aMethodName, bool append)
		{
			var new_mon = AddInternal(msg, append);

			if (new_mon != null)
			{
				new_mon.methodName = aMethodName;
				new_mon.isMethod = true;
			}

			return new_mon;
		}

		internal void Delete(MsgMonitorStruct mms)
		{
			var monIndex = monitor.IndexOf(mms);//  int(monitor - mMonitor);

			if (monIndex != -1)
			{
				// Adjust the index of any active message monitors affected by this deletion.  This allows a
				// message monitor to delete older message monitors while still allowing any remaining monitors
				// of that message to be called (when there are multiple).
				for (var inst = top; inst != null; inst = inst.previous)
					inst.Delete(monIndex);

				monitor.RemoveAt(monIndex);// Remove the item from the array.
			}
		}

		internal bool IsMonitoring(uint msg, int msgType)
		{
			for (var i = 0; i < count; ++i)
				if (monitor[i].msg == msg && monitor[i].msgType == msgType)
					return true;

			return false;
		}

		/// <summary>
		/// Returns true if there are any monitors for a message currently executing.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="msgType"></param>
		/// <returns></returns>
		internal bool IsRunning(uint msg, int msgType)
		{
			for (var inst = top; inst != null; inst = inst.previous)
				if (!inst.deleted && monitor[inst.index].msg == msg && monitor[inst.index].msgType == msgType)
					return true;

			return false;
		}

		internal void Dispose() => monitor.Clear();
	}


	internal class MsgMonitorInstance
	{
		internal MsgMonitorList list;
		internal MsgMonitorInstance previous;
		internal int index;
		internal int count;
		internal bool deleted;

		internal MsgMonitorInstance(MsgMonitorList l)
		{
			list = l;
			previous = l.top;
			count = l.count;
			l.top = this;
		}

		~MsgMonitorInstance()
		{
			list.top = previous;
		}

		internal void Delete(int monIndex)
		{
			if (index >= monIndex && index >= 0)
			{
				if (index == monIndex)
					deleted = true; // Callers who care about this will reset it after each iteration.

				index--; // So index+1 is the next item.
			}

			count--;
		}

		internal void Done()
		{
			count = 0; // Prevent further iteration.
			deleted = true; // Mark the current item as deleted, so it won't be accessed.
		}
	};
}

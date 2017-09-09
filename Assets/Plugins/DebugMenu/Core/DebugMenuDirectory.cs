#if DEBUG
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DebugMenuSystem
{
	public class DebugMenuDirectory : DebugMenuItem
	{
		readonly List<DebugMenuItem> m_itemList = new List<DebugMenuItem>();

		//------------------------------------------------------
		// lifetime
		//------------------------------------------------------

		public DebugMenuDirectory(string name, DebugMenuDirectory directory)
			: base(name, directory)
		{ }


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		internal DebugMenuItem GetItem(Stack<string> stack)
		{
			if (stack.Count == 0)
			{
				return this;
			}

			var itemName = stack.Pop();

			var item = m_itemList.Find(i => i.name == itemName);
			if (item == null)
			{
				return null;
			}

			if (stack.Count == 0)
			{
				return item;
			}

			var dir = item as DebugMenuDirectory;
			if (dir == null)
			{
				return null;
			}

			return dir.GetItem(stack);
		}

		internal DebugMenuDirectory GetDirectory(Stack<string> stack)
		{
			Assert.IsTrue(stack.Count > 0);

			var itemName = stack.Pop();

			var item = m_itemList.Find(i => i.name == itemName);
			if (item == null)
			{
				item = new DebugMenuDirectory(itemName, this);
				m_itemList.Add(item);
				m_itemList.Sort(CompareItem);
			}

			var dir = item as DebugMenuDirectory;
			if (dir == null)
			{
				return null;
			}

			return stack.Count == 0 ? dir : dir.GetDirectory(stack);
		}

		internal void AddItem(DebugMenuItem item)
		{
			m_itemList.Add(item);
			m_itemList.Sort(CompareItem);
		}

		internal void RemoveItem(DebugMenuItem item)
		{
			m_itemList.Remove(item);
			if (m_itemList.Count == 0)
			{
				Remove();
			}
		}

		static int CompareItem(DebugMenuItem x, DebugMenuItem y)
		{
			var nX = x is DebugMenuDirectory ? 1 : 0;
			var nY = y is DebugMenuDirectory ? 1 : 0;
			var r = nX - nY;
			return r != 0 ? r : x.name.CompareTo(y.name);
		}


		//------------------------------------------------------
		// GUI
		//------------------------------------------------------

		public override void OnGUI()
		{
			foreach (var item in m_itemList)
			{
				var dir = item as DebugMenuDirectory;
				if (dir != null)
				{
					if (GUILayout.Button(item.name))
					{
						DebugMenuManager.SetCurrent(dir);
					}
					continue;
				}

				item.OnGUI();
			}
		}
	}
}
#endif
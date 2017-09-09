#if DEBUG
using System.IO;

namespace DebugMenuSystem
{
	public abstract class DebugMenuItem
	{
		readonly public string name;
		readonly DebugMenuDirectory m_directory;

		//------------------------------------------------------
		// lifetime
		//------------------------------------------------------

		public DebugMenuItem(string path)
		{
			name = Path.GetFileName(path);
			m_directory = DebugMenuManager.GetDirectory(Path.GetDirectoryName(path));
			m_directory.AddItem(this);
		}

		public DebugMenuItem(string name, DebugMenuDirectory dir)
		{
			this.name = name;
			m_directory = dir;
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public void Remove()
		{
			if (m_directory != null)
			{
				m_directory.RemoveItem(this);
			}
		}

		public DebugMenuDirectory directory
		{
			get { return m_directory; }
		}

		public abstract void OnGUI();

	}
}
#endif
using System.Text.RegularExpressions;

namespace AssetPost
{
	class AssetPostman
	{
		AssetPostAddress m_address;
		readonly Regex m_fileNameRegex;

		//------------------------------------------------------
		// lifetime
		//------------------------------------------------------

		public AssetPostman(AssetPostAddress address)
		{
			m_address = address;
			m_fileNameRegex = new Regex(address.fileNamePattern);
		}

		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public string Delivery(string fileName)
		{
			return m_fileNameRegex.IsMatch(fileName) ?
				m_address.GetAssetPath(fileName) : null;
		}
	}
}
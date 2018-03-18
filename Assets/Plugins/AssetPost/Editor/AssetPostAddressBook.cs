using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AssetPost
{
	[Serializable]
	class AssetPostAddressBook
	{
		const string kPath = "ProjectSettings/AssetPostAddressbook.txt";

		public List<AssetPostAddress> addressList = new List<AssetPostAddress>();


		//------------------------------------------------------
		// lifetime
		//------------------------------------------------------

		private AssetPostAddressBook()
		{}


		//------------------------------------------------------
		// save/load
		//------------------------------------------------------

		public static AssetPostAddressBook Load()
		{
			try
			{
				return File.Exists(kPath) ?
					JsonUtility.FromJson<AssetPostAddressBook>(File.ReadAllText(kPath)) :
					new AssetPostAddressBook();
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
				return new AssetPostAddressBook();
			}
		}

		public void Save()
		{
			try
			{
				File.WriteAllText(kPath, EditorJsonUtility.ToJson(this));
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
	}

	[Serializable]
	class AssetPostAddress
	{
		[Serializable]
		public class ArgumentInfo
		{
			public int elementIndex;
			public int startIndex;
			public int endIndex;
		}

		public AssetPostAddress()
		{
			name = string.Empty;
			fileNamePattern = string.Empty;
			separators = new char[0];
			assetPathFormat = string.Empty;
			argumentList = new List<ArgumentInfo>();
		}

		public string name;
		public string fileNamePattern;

		public char[] separators;
		public string assetPathFormat;
		public List<ArgumentInfo> argumentList;


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public string GetAssetPath(string fileName)
		{
			try
			{
				var elements = fileName.Split(separators);

				var args = new object[argumentList.Count];
				for (int i = 0; i < argumentList.Count; ++i)
				{
					args[i] = GetArgument(argumentList[i], elements);
				}

				var dir = string.Format(assetPathFormat, args);
				return string.IsNullOrEmpty(dir) ? string.Empty : string.Format("Assets/{0}/{1}", dir, fileName);
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				return string.Empty;
			}
		}

		static string GetArgument(ArgumentInfo info, string[] elements)
		{
			var index = info.elementIndex >= 0 ?
				info.elementIndex :
				elements.Length + info.elementIndex;
			if (index < 0 || index >= elements.Length)
			{
				return string.Empty;
			}

			var str = elements[index];
			if (info.startIndex != 0 || info.endIndex != 0)
			{
				var startIndex = info.startIndex >= 0 ? info.startIndex : str.Length + info.startIndex;
				var endIndex = info.endIndex > 0 ? info.endIndex : str.Length + info.endIndex;
				str = str.Substring(startIndex, endIndex - startIndex);
			}
			return str;
		}
	}
}
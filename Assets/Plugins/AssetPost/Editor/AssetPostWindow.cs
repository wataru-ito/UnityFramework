using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace AssetPost
{
	/// <summary>
	/// このウィンドウにアセットをドロップすると、プロジェクトの適切な所に配置される。
	/// どこに配置されるかは AssetPostman が決める。
	/// </summary>
	public class AssetPostWindow : EditorWindow
	{
		enum Mode
		{
			Delivery,
			Addressbook,
			RegisterAddress,
		}

		AssetPostAddressBook m_addressbook;
		AssetPostman[] m_postmans;
		List<string> m_assetPathList = new List<string>();
		List<string> m_unknownFileList = new List<string>();

		Mode m_mode;

		AssetPostAddress m_edittingAddress;
		bool m_patternEnabled;
		int m_needArgmentCount;
		string m_sampleString = string.Empty;

		GUIStyle m_messageStyle;
		GUIStyle m_labelStyle;
		GUIStyle m_deleteBtnStyle;
		GUIStyle m_plusStyle;
		GUIStyle m_registerStyle;


		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		[MenuItem("Tools/AssetPost")]
		public static void Open()
		{
			GetWindow<AssetPostWindow>();
		}


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		private void OnEnable()
		{
			titleContent = new GUIContent("Asset Post");
			minSize = new Vector2(330f, 100f);

			m_addressbook = AssetPostAddressBook.Load();
			UpdatePostman();

			InitGUI();
		}

		private void OnGUI()
		{
			switch (m_mode)
			{
				case Mode.Delivery:
					DeliveryMode();
					break;
				case Mode.Addressbook:
					DrawAddressbook();
					break;

				case Mode.RegisterAddress:
					RegisterAddresseeMode();
					break;		
			}
		}


		//------------------------------------------------------
		// gui
		//------------------------------------------------------

		void InitGUI()
		{
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			m_messageStyle = skin.FindStyle("TL Selection H1");
			m_labelStyle = skin.FindStyle("PreToolbar");

			m_deleteBtnStyle = skin.FindStyle("OL Minus");
			m_plusStyle = skin.FindStyle("OL Plus");
			m_registerStyle = skin.FindStyle("AC Button");
		}

		void DrawToolbar(string label, string button, Mode mode)
		{			
			var r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbar, GUILayout.ExpandWidth((true)));
			EditorGUI.LabelField(r, label, EditorStyles.toolbar);

			r.x = r.xMax - 64 - 8;
			r.width = 64;
			if (GUI.Button(r, button, EditorStyles.toolbarButton))
			{
				m_mode = mode;
			}
		}


		//------------------------------------------------------
		// delivery
		//------------------------------------------------------

		void UpdatePostman()
		{
			m_postmans = m_addressbook.addressList.Select(i => new AssetPostman(i)).ToArray();
		}

		void DeliveryMode()
		{
			DrawToolbar("Asset Post", "配達先一覧", Mode.Addressbook);
			GUILayout.FlexibleSpace();

			EditorGUILayout.LabelField("ドロップされたアセットを\n適切な場所へ配置します。", m_messageStyle, GUILayout.Height(m_messageStyle.fontSize * 3f));

			GUILayout.FlexibleSpace();

			DrawDeliveryInfo();

			DeliveryDropFiles();
		}

		void DeliveryDropFiles()
		{
			var controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
			var e = Event.current;
			switch (e.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					DragAndDrop.AcceptDrag();
					DragAndDrop.activeControlID = controlID;
					DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
					e.Use();
					break;

				case EventType.DragExited:
					DeliveryFiles(DragAndDrop.paths);
					e.Use();
					break;
			}
		}

		void DeliveryFiles(string[] filePath)
		{
			m_assetPathList.Clear();
			m_unknownFileList.Clear();
			AssetDatabase.StartAssetEditing();
			Array.ForEach(filePath, DeliveryFile);
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
		}

		void DeliveryFile(string filePath)
		{
			var fileName = Path.GetFileName(filePath);
			foreach (var postman in m_postmans)
			{
				var assetPath = postman.Delivery(fileName);
				if (!string.IsNullOrEmpty(assetPath))
				{
					try
					{
						var dir = Path.GetDirectoryName(assetPath);
						if (!Directory.Exists(dir))
							Directory.CreateDirectory(dir);
						
						File.Copy(filePath, assetPath, true);

						m_assetPathList.Add(assetPath);
					}
					catch (Exception e)
					{
						Debug.LogError(e.Message);
					}
					return;
				}
			}

			m_unknownFileList.Add(fileName);
		}

		void DrawDeliveryInfo()
		{
			if (m_assetPathList.Count > 0)
			{
				EditorGUILayout.LabelField("配達完了", m_labelStyle);
				m_assetPathList.ForEach(DrawDeliveredAssetPath);
			}

			if (m_unknownFileList.Count > 0)
			{
				EditorGUILayout.HelpBox("配達できなかったアセットがあります", MessageType.Warning);
				GUI.color = Color.yellow;
				m_unknownFileList.ForEach(DrawUnknownFileName);
				GUI.color = Color.white;
			}
		}

		void DrawDeliveredAssetPath(string assetPath)
		{
			EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), typeof(UnityEngine.Object), false);
		}

		void DrawUnknownFileName(string fileName)
		{
			EditorGUILayout.LabelField(fileName);
		}


		//------------------------------------------------------
		// addresseebook
		//------------------------------------------------------

		void DrawAddressbook()
		{
			DrawToolbar("配達先一覧", "戻る", Mode.Delivery);

			for (int i = 0; i < m_addressbook.addressList.Count; ++i)
			{
				if (AddressField(m_addressbook.addressList[i]))
				{
					m_addressbook.addressList.RemoveAt(i--);
				}
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("新規登録", m_registerStyle))
			{
				SetEditAddress(null);
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		bool AddressField(AssetPostAddress address)
		{
			bool deleteFlag = false;
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("編集", GUILayout.Width(32)))
			{
				SetEditAddress(address);
			}

			EditorGUILayout.LabelField(address.name);

			if (GUILayout.Button(GUIContent.none, m_deleteBtnStyle, GUILayout.Width(16)))
			{
				if (EditorUtility.DisplayDialog("配達先削除", string.Format("配達先[{0}]を本当に削除しますか？", address.name), "削除"))
				{
					deleteFlag = true;
				}
			}

			EditorGUILayout.EndHorizontal();
			return deleteFlag;
		}

	
		//------------------------------------------------------
		// 配達先登録
		//------------------------------------------------------

		readonly GUIContent kPatternContent = new GUIContent("ファイル命名規約", "正規表現");
		readonly GUIContent kAssetPathContent = new GUIContent("Assets/", "string.Format()形式で指定");
		readonly GUIContent kIndexContent = new GUIContent("Index", "マイナスを指定すると最後から");

		void SetEditAddress(AssetPostAddress address)
		{
			m_edittingAddress = address ?? new AssetPostAddress();
			m_patternEnabled = false;
			m_needArgmentCount = GetFormatArgumentCount(m_edittingAddress.assetPathFormat);
			m_mode = Mode.RegisterAddress;
		}

		bool CanRegisterAddress()
		{
			return m_edittingAddress != null && 
				!string.IsNullOrEmpty(m_edittingAddress.name) &&
				!string.IsNullOrEmpty(m_edittingAddress.fileNamePattern) &&
				m_patternEnabled &&
				!string.IsNullOrEmpty(m_edittingAddress.assetPathFormat) &&
				m_edittingAddress.argumentList.Count >= m_needArgmentCount;
		}

		void RegisterAddress()
		{
			if (!m_addressbook.addressList.Contains(m_edittingAddress))
			{
				m_addressbook.addressList.Add(m_edittingAddress);
				m_addressbook.addressList.Sort((x, y) => x.name.CompareTo(y.name));
			}
			m_addressbook.Save();
			UpdatePostman();

			m_edittingAddress = null;
			m_mode = Mode.Addressbook;
		}

		void RegisterAddresseeMode()
		{
			DrawToolbar("配達先登録", "戻る", Mode.Addressbook);

			if (m_edittingAddress == null)
				return;

			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 100;

			EditAddress(m_edittingAddress);

			EditorGUILayout.Space();

			DrawPatternCheck();

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = CanRegisterAddress();
				if (GUILayout.Button("登録", m_registerStyle))
				{
					RegisterAddress();
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUIUtility.labelWidth = labelWidth;
		}

		void EditAddress(AssetPostAddress address)
		{
			address.name = EditorGUILayout.TextField("登録名", address.name ?? string.Empty);
			address.fileNamePattern = EditorGUILayout.TextField(kPatternContent, address.fileNamePattern);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("お届け先", m_labelStyle);
			EditorGUI.BeginChangeCheck();
			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 45;
			address.assetPathFormat = EditorGUILayout.TextField(kAssetPathContent, address.assetPathFormat);
			EditorGUIUtility.labelWidth = labelWidth;
			if (EditorGUI.EndChangeCheck())
			{
				m_needArgmentCount = GetFormatArgumentCount(address.assetPathFormat);
			}
						
			++EditorGUI.indentLevel;
			EditorGUILayout.LabelField("format引数設定 - ファイル名をSplitして使う");

			address.separators = SeparatorsField(address.separators, address.fileNamePattern);

			for (int i = 0; i < address.argumentList.Count; ++i)
			{
				if (ArgumentInfoField(i, address.argumentList[i]))
				{
					address.argumentList.RemoveAt(i--);
				}
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("引数追加", m_plusStyle))
			{
				var info = new AssetPostAddress.ArgumentInfo();
				address.argumentList.Add(info);
			}
			EditorGUILayout.EndHorizontal();

			if (address.argumentList.Count < m_needArgmentCount)
			{
				EditorGUILayout.HelpBox("引数の数がたりません", MessageType.Warning);
			}

			--EditorGUI.indentLevel;
		}

		static char[] SeparatorsField(char[] separators, string sample)
		{
			var str = EditorGUILayout.TextField("separator", new string(separators));
			separators = new char[str.Length];
			for (int i = 0; i < str.Length; ++i)
			{
				separators[i] = str[i];
			}

			var elements = sample.Split(separators);
			var sb = new System.Text.StringBuilder();
			for (int i = 0; i < elements.Length; ++i)
			{
				if (sb.Length > 0)
					sb.Append(" | ");
				sb.Append(elements[i]);
			}
			sb.Insert(0, "Split = ");

			EditorGUILayout.HelpBox(sb.ToString(), MessageType.None);

			return separators;
		}

		bool ArgumentInfoField(int index, AssetPostAddress.ArgumentInfo info)
		{
			bool deleteFlag = false;

			var numW = GUILayout.Width(32);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField("{"+index+"}", GUILayout.Width(24));

			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 40;
			info.elementIndex = EditorGUILayout.IntField(kIndexContent, info.elementIndex, GUILayout.Width(65));
			EditorGUIUtility.labelWidth = labelWidth;

			GUILayout.Space(8);
			info.startIndex = EditorGUILayout.IntField(info.startIndex, numW);
			EditorGUILayout.LabelField("文字目 -", GUILayout.Width(45));
			info.endIndex = EditorGUILayout.IntField(info.endIndex, numW);
			EditorGUILayout.LabelField("文字目", GUILayout.Width(45));

			if (GUILayout.Button(GUIContent.none, m_deleteBtnStyle, GUILayout.Width(16)))
			{
				deleteFlag = true;
			}

			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel = indent;

			return deleteFlag;
		}

		static int GetFormatArgumentCount(string format)
		{
			int count = 0;
			var regex = new Regex(@"{[0-9]+}");
			var m = regex.Match(format);
			while (m.Success)
			{
				var str = m.Value;
				int n;
				if (int.TryParse(str.Substring(1, str.Length - 2), out n))
				{
					count = Math.Max(n + 1, count);	
				}
				m = m.NextMatch();
			}
			return count;
		}

		void DrawPatternCheck()
		{
			m_patternEnabled = false;

			EditorGUILayout.LabelField("テスト", m_labelStyle);
			m_sampleString = EditorGUILayout.TextField("ファイル名", m_sampleString);
			EditorGUILayout.LabelField("お届け先");
			if (m_edittingAddress.argumentList.Count >= m_needArgmentCount)
			{
				try
				{
					if (Regex.IsMatch(m_sampleString, m_edittingAddress.fileNamePattern))
					{
						EditorGUILayout.LabelField(m_edittingAddress.GetAssetPath(m_sampleString));
					}
					else
					{
						EditorGUILayout.HelpBox("ファイル名が規約に合っていません", MessageType.Info);
					}

					m_patternEnabled = true;
				}
				catch (Exception)
				{
					EditorGUILayout.HelpBox("命名規約の正規表現が異常", MessageType.Warning);		
				}
			}
		}
	}
}
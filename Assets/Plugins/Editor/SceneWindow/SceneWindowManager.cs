using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SceneWindow
{
	[InitializeOnLoad]
	public static class SceneWindowManager
	{
		class WindowKeeper
		{
			readonly Type m_type;
			EditorWindow m_window;

			public WindowKeeper(Type type)
			{
				m_type = type;
			}

			public void Close()
			{
				if (m_window)
				{
					m_window.Close();
					m_window = null;
				}
			}

			public void Check()
			{
				if (!m_window)
				{
					m_window = EditorWindow.GetWindow(m_type);
				}
			}
		}


		static Dictionary<string, List<Type>> s_sceneClassMap;
		static WindowKeeper[] s_keepers;


		//----------------------------------------------
		// lifetime
		//----------------------------------------------

		static SceneWindowManager()
		{
			s_sceneClassMap = CreateMap();
			EditorSceneManager.sceneOpened += OnSceneOpend;
		}

		static Dictionary<string, List<Type>> CreateMap()
		{
			var map = new Dictionary<string, List<Type>>();

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			foreach (var type in assembly.GetTypes().Where(i => i.IsSubclassOf(typeof(EditorWindow))))
			foreach (OpenAttribute attr in type.GetCustomAttributes(typeof(OpenAttribute), false))
			foreach (var sceneName in attr.sceneNames)
			{
				List<Type> typeList;
				if (!map.TryGetValue(sceneName, out typeList))
				{
					typeList = new List<Type>();
					map.Add(sceneName, typeList);
				}
				typeList.Add(type);
			}

			return map;
		}


		//----------------------------------------------
		// events
		//----------------------------------------------

		static void OnSceneOpend(Scene scene, OpenSceneMode mode)
		{
			if (mode != OpenSceneMode.Single)
				return;

			ClearWindow();

			List<Type> typeList;
			if (s_sceneClassMap.TryGetValue(scene.name, out typeList))
			{
				SetWindow(typeList);
			}
		}		


		//----------------------------------------------
		// window keeper
		//----------------------------------------------

		static void ClearWindow()
		{
			if (s_keepers == null)
				return;

			EditorApplication.update -= KeepWindow;

			Array.ForEach(s_keepers, i => i.Close());
			s_keepers = null;
		}

		static void SetWindow(List<Type> typeList)
		{
			s_keepers = typeList.ConvertAll(i => new WindowKeeper(i)).ToArray();
			KeepWindow();
			EditorApplication.update += KeepWindow;
		}

		static void KeepWindow()
		{
			Array.ForEach(s_keepers, i => i.Check());
		}
	}
}
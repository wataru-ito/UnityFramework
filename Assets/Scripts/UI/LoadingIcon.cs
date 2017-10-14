using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI
{
	public class LoadingIcon : MonoBehaviour
	{
		[SerializeField] int m_remainingFrame;
		int m_frameCount;

		static HashSet<Object> s_requests = new HashSet<Object>();
		static LoadingIcon s_instance;


		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		/// <summary>
		/// 自殺するオブジェクトなら投げっぱなしでもOK
		/// </summary>
		public static void Add(Object request)
		{
			s_requests.Add(request);
			if (!s_instance)
			{
				s_instance = Create();
			}
		}

		public static void Remove(Object request)
		{
			s_requests.Remove(request);
			if (s_requests.Count == 0 && s_instance)
			{
				Destroy(s_instance.gameObject);
				s_instance = null;
			}
		}

		static LoadingIcon Create()
		{
			var prefab = Resources.Load<GameObject>("UI/LoadingIcon");
			var go = Instantiate<GameObject>(prefab);
			DontDestroyOnLoad(go);
			return go.GetComponent<LoadingIcon>();
		}


		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void LateUpdate()
		{
			s_requests.RemoveWhere(i => i == null);
			if (s_requests.Count == 0)
			{
				// チラチラ対策
				if (--m_frameCount <= 0)
				{
					Destroy(gameObject);
				}
			}
			else
			{
				m_frameCount = m_remainingFrame;
			}
		}
	}
}
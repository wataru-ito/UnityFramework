using UnityEngine;
using System;

namespace Framework
{
	public class DestroyNotice : MonoBehaviour
	{
		Action m_callback;

		//------------------------------------------------------
		// static functions
		//------------------------------------------------------

		public static DestroyNotice Create(GameObject go, Action callback)
		{
			if (callback == null) return null;
			var notice = go.AddComponent<DestroyNotice>();
			notice.m_callback = callback;
			return notice;
		}

		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void OnDestroy()
		{
			if (m_callback != null)
			{
				m_callback();
				m_callback = null;
			}
		}
	}
}
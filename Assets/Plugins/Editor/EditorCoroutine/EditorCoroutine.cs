using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace Framework
{
	public class EditorCoroutine : CustomYieldInstruction
	{
		IEnumerator m_enumerator;
		CustomYieldInstruction m_current;


		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		/// <summary>
		/// わざわざファクトリにする必要はないんだけど、インスタンス生成して起動するのも気持ち悪いので。
		/// </summary>
		public static EditorCoroutine Run(IEnumerator enumerator)
		{
			return new EditorCoroutine(enumerator);
		}
		

		//------------------------------------------------------
		// lifetime
		//------------------------------------------------------

		private EditorCoroutine(IEnumerator enumerator)
		{
			m_enumerator = enumerator;
			EditorApplication.update += Update;
		}


		//------------------------------------------------------
		// update
		//------------------------------------------------------

		void Update()
		{
			if (m_current != null && m_current.keepWaiting)
				return;

			if (m_enumerator.MoveNext())
			{
				m_current = m_enumerator.Current as CustomYieldInstruction;
				return;
			}
				
			m_enumerator = null;
			m_current = null;
			EditorApplication.update -= Update;
		}

		public override bool keepWaiting
		{
			get { return m_enumerator != null; }
		}
	}
}
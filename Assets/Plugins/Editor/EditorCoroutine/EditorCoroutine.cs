using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace Framework
{
	public class EditorCoroutine
	{
		private class Instance : CustomYieldInstruction
		{
			IEnumerator m_enumerator;
			CustomYieldInstruction m_current;

			public Instance(IEnumerator enumerator)
			{
				m_enumerator = enumerator;
				EditorApplication.update += Update;
			}

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

		//------------------------------------------------------
		// static function
		//------------------------------------------------------

		public static CustomYieldInstruction Run(IEnumerator enumerator)
		{
			return new Instance(enumerator);
		}
	}
}
using System.Collections;
using UnityEngine;

namespace Framework.SceneManagement
{
	public sealed class SceneTransitionFade : SceneTransition
	{
		[SerializeField] Color m_color;
		[SerializeField] float m_duration;
		[SerializeField] VFX.Fader m_fader;

		//------------------------------------------------------
		// ISceneTransition
		//------------------------------------------------------

		public override IEnumerator Enter()
		{
			m_fader.color = m_color;
			m_fader.SetThickness(1f, m_duration);
			return new WaitWhile(() => m_fader.IsInterpolating);
		}

		public override IEnumerator Exit()
		{
			m_fader.color = m_color;
			m_fader.SetThickness(1f, 0f, m_duration);
			return new WaitWhile(() => m_fader.IsInterpolating);
		}
	}
}
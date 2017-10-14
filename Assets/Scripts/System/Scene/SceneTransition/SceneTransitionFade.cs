using UnityEngine;

namespace Framework.SceneManagement
{
	public class SceneTransitionFade : SceneTransition
	{
		[SerializeField] VFX.Fader.ColorType m_color;
		[SerializeField] float m_duration;
		[SerializeField] VFX.Fader m_fader;

		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		void Awake()
		{
			if (!m_fader)
				m_fader = VFX.Fader.instance;
		}

		//------------------------------------------------------
		// ISceneTransition
		//------------------------------------------------------

		public override string transitionName
		{
			get { return "Fade"; }
		}

		public override void Enter()
		{
			m_fader.colorType = m_color;
			m_fader.SetThickness(1f, m_duration);
		}

		public override void Exit()
		{
			m_fader.colorType = m_color;
			m_fader.SetThickness(1f, 0f, m_duration);
		}

		public override bool IsTransiting
		{
			get { return m_fader.IsInterpolating; }
		}
	}
}
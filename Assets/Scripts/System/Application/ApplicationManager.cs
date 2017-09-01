using System.Collections;
using UnityEngine;
using Framework.UI;


namespace Framework
{
	public class ApplicationManager : SingletonBehaviour<ApplicationManager>
	{
		[SerializeField] int m_targetFrameRate;
		bool m_setup;

		//------------------------------------------------------
		// unity system function
		//------------------------------------------------------

		protected override void Awake()
		{
			base.Awake();
			DontDestroyOnLoad(gameObject);
			Application.targetFrameRate = m_targetFrameRate;
		}

		IEnumerator Start()
		{
			m_setup = true;
			yield break;
		}


		//------------------------------------------------------
		// accessor
		//------------------------------------------------------

		public bool IsSetup()
		{
			return m_setup;
		}


		//------------------------------------------------------
		// events
		//------------------------------------------------------

		public void OnBackButtonDown()
		{
			if (MessageBox.Exists()) 
				return;

			MessageBox.YesNo("Game", "ゲームを終了する", yes:Application.Quit);
		}
	}
}
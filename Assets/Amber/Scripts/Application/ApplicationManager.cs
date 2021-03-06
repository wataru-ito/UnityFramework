﻿using System.Collections;
using UnityEngine;
using Amber.UI;
using Amber.DebugSystem;

namespace Amber
{
	public class ApplicationManager : SingletonBehaviour<ApplicationManager>
	{
		[SerializeField] int m_targetFrameRate;
		[SerializeField] VFX.Fader m_fader;
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
			#if DEBUG
			DebugManager.Create(transform);
			#endif

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


		//------------------------------------------------------
		// entities
		//------------------------------------------------------

		public VFX.Fader fader
		{
			get { return m_fader; }
		}
	}
}
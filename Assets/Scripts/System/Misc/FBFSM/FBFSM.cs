using System;
using UnityEngine;
using StateFunc = System.Action<Framework.FSM.FBFSM.Step>;

/// <summary>
/// 関数ベースの状態マシン
/// (Function Based Finite State Machine)
/// </summary>
namespace Framework.FSM
{ 
	public class FBFSM
	{
		public enum Step
		{
			OnEnter,
			OnExit,
			OnUpdate,
		}

		StateFunc m_state;
		public float time;
		public event Action onChanged;


		//--------------------------------------------------
		// lifetime
		//--------------------------------------------------

		public FBFSM()
		{
		}

		public FBFSM(StateFunc state)
		{
			Set(state);
		}

		public override string ToString()
		{
			return string.Format("{0} : {1:F2}", m_state == null ? "---" : m_state.Method.Name, time);
		}


		//--------------------------------------------------
		// event
		//--------------------------------------------------

		public void Update()
		{
			time += Time.deltaTime;

			if (m_state != null)
				m_state(Step.OnUpdate);
		}


		//--------------------------------------------------
		// accessor
		//--------------------------------------------------

		public void Set(StateFunc next, bool force = false)
		{
			if (!force && m_state == next)
			{
				return;
			}

			// コールバックの中でSetState()呼ばれるのを考慮
			var prev = m_state;
			m_state = next;
			time = 0f;

			Debug.LogFormat("State[<color=blue>{0}</color>] >>> [<color=blue>{1}</color>]",
				prev == null ? "" : prev.Method.Name,
				next == null ? "" : next.Method.Name);

			if (prev != null) prev(Step.OnExit);
			if (next != null && m_state == next) next(Step.OnEnter);
			if (onChanged != null) onChanged();
		}

		public bool Is(StateFunc state)
		{
			return m_state == state;
		}

		public string name
		{
			get { return m_state != null ? m_state.Method.Name : string.Empty; }
		}
	}
}

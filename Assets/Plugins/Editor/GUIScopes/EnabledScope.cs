using UnityEngine;

public class EnabledScope : GUI.Scope
{
	bool m_prev;

	public EnabledScope(bool enabled)
	{
		m_prev = GUI.enabled;
		GUI.enabled = enabled;
	}

	protected override void CloseScope()
	{
		GUI.enabled = m_prev;
	}
}

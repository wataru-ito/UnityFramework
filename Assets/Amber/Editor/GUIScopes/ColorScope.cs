using UnityEngine;

public class ColorScope : GUI.Scope
{
	readonly Color m_prev;

	public ColorScope(Color color)
	{
		m_prev = GUI.color;
		GUI.color = color;
	}

	protected override void CloseScope()
	{
		GUI.color = m_prev;
	}
}

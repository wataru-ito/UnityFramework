using UnityEngine;

// <summary>
// Power of two
// </summary>
public class POTAttribute : PropertyAttribute 
{
	public int min;
	public int max;

	public POTAttribute(int min = 1, int max = 4096)
	{
		this.min = min;
		this.max = max;
	}
}

using UnityEngine;

namespace Amber
{
	// <summary>
	// Power of two
	// </summary>
	public class POTAttribute : PropertyAttribute
	{
		public int min;
		public int max;

		public POTAttribute(int min = 1, int max = 4068)
		{
			this.min = min;
			this.max = max;
		}
	}
}
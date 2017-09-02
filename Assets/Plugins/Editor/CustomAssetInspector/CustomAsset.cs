// UNIBOOK 総集編 アセット編 (https://unity-bu.booth.pm/items/392553) より
using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CustomAssetAttribute : Attribute
{
	public string[] extensions;

	public CustomAssetAttribute(params string[] extensions)
	{
		this.extensions = extensions;
	}
}
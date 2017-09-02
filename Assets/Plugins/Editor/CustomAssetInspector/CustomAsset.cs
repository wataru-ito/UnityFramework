// GitHub : https://github.com/anchan828/custom-asset
// MIT License
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
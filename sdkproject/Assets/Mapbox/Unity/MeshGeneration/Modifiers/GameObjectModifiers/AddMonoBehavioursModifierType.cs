namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using System;
	using UnityEngine;

	[Serializable]
	public class AddMonoBehavioursModifierType
	{
		[SerializeField]
		string _typeString;

		Type _type;

		[SerializeField]
		MonoBehaviour _script;

		public Type Type
		{
			get
			{
				if (_type == null)
				{
					_type = Type.GetType(_typeString);
				}
				return _type;
			}
		}
	}
}

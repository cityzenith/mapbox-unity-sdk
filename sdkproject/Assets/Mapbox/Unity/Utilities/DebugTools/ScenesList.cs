namespace Mapbox.Unity.Utilities.DebugTools
{
	//using System;
	using UnityEngine;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	public class ScenesList : ScriptableObject
	{
		public SceneData[] SceneList;

		private System.Type AssetDBType
		{
			get
			{
				if (null == assetDBType)
					assetDBType = MapboxProperties.EditorAssembly.GetType("UnityEditor.AssetDatabase");

				return assetDBType;
			}
		}
		private System.Type assetDBType;

		private System.Type EditorUtilityType
		{
			get
			{
				if (null == editorUtilityType)
					editorUtilityType = MapboxProperties.EditorAssembly.GetType("UnityEditor.EditorUtility");

				return editorUtilityType;
			}
		}
		private System.Type editorUtilityType;

		//ensure that linked scenes are stored in this object

		public void LinkScenes()
		{
			if (MapboxProperties.IsEditor)
			{
				for (int i = 0; i < SceneList.Length; i++)
				{
					if (!ThisAssetContainsScene(SceneList[i]))
					{
						//duplicate the asset
						var path = CallAssetDbFunct<string>("GetAssetPath", this);
						var newScene = ScriptableObject.CreateInstance<SceneData>();
						newScene.name = SceneList[i].name;
						newScene.ScenePath = SceneList[i].ScenePath;
						newScene.Text = SceneList[i].Text;
						newScene.Image = SceneList[i].Image;

						//assign it to the current scene list
						CallAssetDbAction("AddObjectToAsset", newScene, path);
						SceneList[i] = newScene;

						//save the scenelist
						EditorUtilityType.GetMethod("SetDirty").Invoke(null, new object[] { this });
						CallAssetDbAction("SaveAssets");

						//TODO: clean up unreferenced sub-assets with Destroy
					}
				}
			}
		}

		private bool ThisAssetContainsScene(SceneData scene)
		{
			if (MapboxProperties.IsEditor)
			{
				var path = CallAssetDbFunct<string>("GetAssetPath", this);
				Object[] assets = CallAssetDbFunct<Object[]>("LoadAllAssetsAtPath", path);
				foreach (var asset in assets)
				{
					if (asset == scene)
					{
						//Debug.Log("Asset " + scene + " is contained in " + path);
						return true;
					}
				}
			}

			return false;

		}

		private T CallAssetDbFunct<T>(string methodName, params object[] args)
		{
			return (T)AssetDBType.GetMethod(methodName).Invoke(null, args);
		}

		private void CallAssetDbAction(string methodName, params object[] args)
		{
			AssetDBType.GetMethod(methodName).Invoke(null, args);
		}
	}
}

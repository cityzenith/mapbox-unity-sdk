namespace Mapbox.Unity.Utilities.DebugTools
{
	using UnityEngine;
	public class ScenesList : ScriptableObject
	{
		public SceneData[] SceneList;

		//ensure that linked scenes are stored in this object

		public void LinkScenes()
		{
			for (int i = 0; i < SceneList.Length; i++)
			{
				if (!ThisAssetContainsScene(SceneList[i]))
				{
					//duplicate the asset
					var path = EditorHelper.GetAssetPath(this);
					var newScene = ScriptableObject.CreateInstance<SceneData>();
					newScene.name = SceneList[i].name;
					newScene.ScenePath = SceneList[i].ScenePath;
					newScene.Text = SceneList[i].Text;
					newScene.Image = SceneList[i].Image;

					//assign it to the current scene list
					EditorHelper.AddObjectToAsset(newScene, path);
					SceneList[i] = newScene;

					//save the scenelist
					EditorHelper.EditorUtilitySetDirty(this);
					EditorHelper.SaveAssets();

					//TODO: clean up unreferenced sub-assets with Destroy
				}
			}
		}

		private bool ThisAssetContainsScene(SceneData scene)
		{
			var path = EditorHelper.GetAssetPath(this);
			Object[] assets = EditorHelper.LoadAllAssetsAtPath(path);
			foreach (var asset in assets)
			{
				if (asset == scene)
				{
					return true;
				}
			}

			return false;

		}
	}
}

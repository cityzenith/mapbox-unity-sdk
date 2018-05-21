namespace Mapbox.Unity.Utilities.DebugTools
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(ScenesList))]
	public class ScenesListEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			ScenesList e = target as ScenesList;

			if (GUILayout.Button("Link Listed Scenes"))
			{
				LinkScenes(e.SceneList);
			}
		}

		private void LinkScenes(SceneData[] scenesList)
		{
			for (int i = 0; i < scenesList.Length; i++)
			{
				if (!ThisAssetContainsScene(scenesList[i]))
				{
					//duplicate the asset
					var path = AssetDatabase.GetAssetPath(this);
					var newScene = ScriptableObject.CreateInstance<SceneData>();
					newScene.name = scenesList[i].name;
					newScene.ScenePath = scenesList[i].ScenePath;
					newScene.Text = scenesList[i].Text;
					newScene.Image = scenesList[i].Image;

					//assign it to the current scene list
					AssetDatabase.AddObjectToAsset(newScene, path);
					scenesList[i] = newScene;

					//save the scenelist
					EditorUtility.SetDirty(this);
					AssetDatabase.SaveAssets();

					//TODO: clean up unreferenced sub-assets with Destroy
				}
			}
		}

		private bool ThisAssetContainsScene(SceneData scene)
		{
			var path = AssetDatabase.GetAssetPath(this);
			Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (var asset in assets)
			{
				if (asset == scene)
				{
					//Debug.Log("Asset " + scene + " is contained in " + path);
					return true;
				}
			}

			return false;

		}
	}
}

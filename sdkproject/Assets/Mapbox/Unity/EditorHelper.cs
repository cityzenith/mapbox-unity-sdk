using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

public static class EditorHelper
{
	public static event Action Update;

	static EditorHelper()
	{
		if (MapboxHelper.IsEditor)
			HookToEditorUpdate();
	}

	/// <summary>
	/// Gets the editor assembly at runtime
	/// </summary>
	public static Assembly EditorAssembly
	{
		get
		{
			if (null == editorAssembly)
			{
				string assemblyPath = Assembly.GetAssembly(typeof(Vector3)).Location; // We get the location of the UnityEngine Assembly

				if (assemblyPath.EndsWith("UnityEngine.dll") && File.Exists(assemblyPath.Replace("UnityEngine", "UnityEditor")))
				{
					assemblyPath = assemblyPath.Replace("UnityEngine", "UnityEditor"); //UnityEditor is in the same path
				}
				else
				{
					FileInfo fileInfo = new FileInfo(assemblyPath);
					assemblyPath = Path.Combine(fileInfo.Directory.Parent.FullName, "UnityEditor.dll"); //editor is in parent
				}

				//We load the editor assembly and get the method we need 
				editorAssembly = Assembly.LoadFrom(assemblyPath);
			}

			return editorAssembly;
		}
	}
	private static Assembly editorAssembly;

	#region UnityEditor wrapper

	public static bool EditorIsPlaying
	{
		get { return (bool)GetPropertyFieldValue("UnityEditor.EditorApplication", "isPlaying"); }
	}

	public static bool IsRemoteConnected
	{
		get { return (bool)GetPropertyFieldValue("UnityEditor.EditorApplication", "isRemoteConnected"); }
	}

	public static string ApplicationIdentifier
	{
		get { return (string)GetPropertyFieldValue("UnityEditor.PlayerSettings", "applicationIdentifier"); }
	}

	public static string BundleVersion
	{
		get { return (string)GetPropertyFieldValue("UnityEditor.PlayerSettings", "bundleVersion"); }
	}

	public static string IosBuildNumber
	{
		get { return (string)GetPropertyFieldValue("UnityEditor.PlayerSettings.iOS", "buildNumber"); }
	}

	public static int AndroidBundleVersionCode
	{
		get { return (int)GetPropertyFieldValue("UnityEditor.PlayerSettings.Android", "bundleVersionCode"); }
	}

	public static string GetAssetPath(UnityObject unityObject)
	{
		return (string)ExecuteEditorFucntion("UnityEditor.AssetDatabase", "GetAssetPath", new[] { typeof(UnityObject) }, unityObject);
	}

	public static UnityObject[] LoadAllAssetsAtPath(string assetPath)
	{
		return (UnityObject[])ExecuteEditorFucntion("UnityEditor.AssetDatabase", "LoadAllAssetsAtPath", null, assetPath);
	}

	public static void AddObjectToAsset(UnityObject newScene, string path)
	{
		ExecuteEditorMethod("UnityEditor.AssetDatabase", "AddObjectToAsset", new[] { typeof(UnityObject), typeof(string) }, newScene, path);
	}

	public static void SaveAssets()
	{
		ExecuteEditorMethod("UnityEditor.AssetDatabase", "SaveAssets", null, null);
	}

	public static void EditorUtilitySetDirty(UnityObject target)
	{
		ExecuteEditorMethod("UnityEditor.EditorUtility", "SaveAssets", null, target);
	}

	#endregion

	#region private methods

	private static object ExecuteEditorFucntion(string typeName, string methodName, Type[] types, params object[] param)
	{
		MethodInfo methodInfo = GetMethodInfo(typeName, methodName, types);

		return methodInfo.Invoke(null, param);
	}

	private static void ExecuteEditorMethod(string typeName, string methodName, Type[] types, params object[] param)
	{
		MethodInfo methodInfo = GetMethodInfo(typeName, methodName, types);

		methodInfo.Invoke(null, param);
	}

	private static object GetPropertyFieldValue(string typeName, string propertyValue)
	{
		Type type = GetEditorType(typeName);
		PropertyInfo propertyInfo = type.GetProperty(propertyValue);
		return propertyInfo.GetValue(null, null);
	}

	private static MethodInfo GetMethodInfo(string typeName, string methodName, Type[] types)
	{
		Type type = GetEditorType(typeName);
		MethodInfo methodInfo;

		if (null == types)
			methodInfo = type.GetMethod(methodName);
		else
			methodInfo = type.GetMethod(methodName, types);

		return methodInfo;
	}

	private static Type GetEditorType(string type)
	{
		return EditorAssembly.GetType(type);
	}

	private static void OnEditorUpdate()
	{
		if (null != Update)
			Update.Invoke();
	}

	private static void HookToEditorUpdate()
	{
		Type type = GetEditorType("UnityEditor.EditorApplication");
		FieldInfo fieldInfo = type.GetField("update");
		MethodInfo miHandler = typeof(EditorHelper).GetMethod("OnEditorUpdate", BindingFlags.NonPublic | BindingFlags.Static);
		Delegate d = Delegate.CreateDelegate(fieldInfo.FieldType, null, miHandler);
		object fieldValue = fieldInfo.GetValue(null);
		Delegate comb = Delegate.Combine(fieldValue as Delegate, d);
		fieldInfo.SetValue(null, comb);
	}

	#endregion
}

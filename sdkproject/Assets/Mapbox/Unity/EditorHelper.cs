using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class EditorHelper
{
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

	#region private methods

	private static object ExecuteEditorFucntion(string typeName, string methodName, Type[] types, params object[] param)
	{
		MethodInfo methodInfo = GetMethodInfo(typeName, methodName, types);

		return methodInfo.Invoke(null, param);
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

	#endregion
}

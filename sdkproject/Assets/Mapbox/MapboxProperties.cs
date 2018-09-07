using System.IO;
using System.Reflection;
using UnityEngine;

namespace Mapbox.Unity
{
	public static class MapboxProperties
	{
		/// <summary>
		/// Indicates if the Application is on Android Only
		/// </summary>
		public static bool IsAndroid
		{
			get { return Application.platform == RuntimePlatform.Android && !Application.isEditor; }
		}

		/// <summary>
		/// Indicates if the Application is on IOS Only
		/// </summary>
		public static bool IsIOS
		{
			get { return Application.platform == RuntimePlatform.IPhonePlayer && !Application.isEditor; }
		}

		/// <summary>
		/// Indicates if the Application is on Windows Only
		/// </summary>
		public static bool IsWindows
		{
			get
			{
				return Application.platform == RuntimePlatform.WindowsEditor ||
					   Application.platform == RuntimePlatform.WindowsPlayer ||
					   Application.platform == RuntimePlatform.WSAPlayerARM ||
					   Application.platform == RuntimePlatform.WSAPlayerX64 ||
					   Application.platform == RuntimePlatform.WSAPlayerX86;
			}
		}

		/// <summary>
		/// Indicates if the Application is on Editor Only
		/// </summary>
		public static bool IsEditor
		{
			get { return Application.isEditor; }
		}

		public static bool IsWebGl
		{
			get { return Application.platform == RuntimePlatform.WebGLPlayer; }
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

					if (assemblyPath.EndsWith("UnityEngine.dll") && System.IO.File.Exists(assemblyPath.Replace("UnityEngine", "UnityEditor")))
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
	}
}

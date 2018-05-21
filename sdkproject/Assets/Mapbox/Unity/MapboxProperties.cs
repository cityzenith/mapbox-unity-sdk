using UnityEngine;

public static class MapboxProperties
{
	public static bool IsUnityEditor
	{
		get { return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor; }
	}

	public static bool IsWindowsPlatform
	{
		get { return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WSAPlayerARM ||
				Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.WSAPlayerX86; }
	}

	public static bool IsWebGL
	{
		get { return Application.platform == RuntimePlatform.WebGLPlayer; }
	}

	public static bool HasEvents
	{
		get { return IsUnityEditor || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android; }
	}
}

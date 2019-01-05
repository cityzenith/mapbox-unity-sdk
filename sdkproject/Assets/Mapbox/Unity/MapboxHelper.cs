using UnityEngine;

public static class MapboxHelper
{
	static MapboxHelper()
	{
		isWebGl = Application.platform == RuntimePlatform.WebGLPlayer;
		isEditor = Application.isEditor;
		isIOS = Application.platform == RuntimePlatform.IPhonePlayer;
		isWinRT = Application.platform == RuntimePlatform.WSAPlayerX64 ||
			Application.platform == RuntimePlatform.WSAPlayerX86 ||
			Application.platform == RuntimePlatform.WSAPlayerARM;
		isAndroid = Application.platform == RuntimePlatform.Android;
		isWindows = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
		isMac = Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor;
		isLinux = Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor;
	}

	public static bool IsWebGL { get { return isWebGl; } }
	private static readonly bool isWebGl;

	public static bool IsEditor { get { return isEditor; } }
	private static readonly bool isEditor;

	public static bool IsIOS { get { return isIOS; } }
	private static readonly bool isIOS;

	public static bool IsWinRT { get { return isWinRT; } }
	private static readonly bool isWinRT;

	public static bool IsAndroid { get { return isAndroid; } }
	private static readonly bool isAndroid;

	public static bool IsWindows { get { return isWindows; } }
	private static readonly bool isWindows;

	public static bool IsMac { get { return isMac; } }
	private static readonly bool isMac;

	public static bool IsLinux { get { return isLinux; } }
	private static readonly bool isLinux;
}

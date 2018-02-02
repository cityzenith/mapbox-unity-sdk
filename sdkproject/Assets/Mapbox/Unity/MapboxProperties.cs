using UnityEngine;

public static class MapboxProperties
{
    public static bool IsUnityEditor
    {
        get { return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor; }
    }

    public static bool HasEvents
    {
        get { return IsUnityEditor || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android; }
    }
}

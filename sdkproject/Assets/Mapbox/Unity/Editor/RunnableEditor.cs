using UnityEngine;
using UnityEditor;
using Mapbox.Unity.Utilities;

public class RunnableEditor
{
    private static bool sm_EditorRunnable = false;

    /// <summary>
    /// This function enables the Runnable in edit mode.
    /// </summary>
    public static void EnableRunnableInEditor()
    {
        if (!sm_EditorRunnable)
        {
            sm_EditorRunnable = true;
            EditorApplication.update += UpdateRunnable;
        }
    }

    static void UpdateRunnable()
    {
        if (!Application.isPlaying)
        {
            Runnable.Instance.UpdateRoutines();
        }
    }
}

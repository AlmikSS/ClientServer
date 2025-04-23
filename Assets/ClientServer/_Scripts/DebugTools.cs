using UnityEngine;

public class DebugTools
{
    public static void ShowDebug(string text, bool enableDebugging, DebugType debugType = DebugType.Log)
    {
        if (!enableDebugging)
            return;

        switch (debugType)
        {
            case DebugType.Log:
                Debug.Log(text);
                break;
            case DebugType.Warning:
                Debug.LogWarning(text);
                break;
            case DebugType.Error:
                Debug.LogError(text);
                break;
            default:
                Debug.Log(text);
                break;
        }
    }
}

public enum DebugType
{
    Log,
    Warning,
    Error,
}
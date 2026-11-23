using UnityEditor;

// Editor-only shortcuts for testing progression.
public static class DebugProgress
{
    // Last index, not Length: unlocks everything without triggering the "all levels beaten" FormSG popup.
    [MenuItem("Tools/Unlock All Levels")]
    static void UnlockAll() => GameSettings.LevelUnlocked = GameLevels.All.Length - 1;

    [MenuItem("Tools/Reset Progress")]
    static void Reset() => GameSettings.ResetAll();
}

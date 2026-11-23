// One place for the level list: titles, level-select descriptions and scene names.
// Index 0 is the tutorial; index N is "LevelN" (matches GameManager.currentLevel).
public static class GameLevels
{
    public struct Level
    {
        public string Scene, Title, Description;
        public Level(string scene, string title, string description) { Scene = scene; Title = title; Description = description; }
    }

    public const string MainMenu = "MainMenu";
    public const string LevelEnd = "Level End";
    public const string Tutorial = "Tutorial";

    public static readonly Level[] All =
    {
        new Level(Tutorial, "Tutorial",
            "Learn the basics of interacting with customers and making decisions."),
        new Level("Level1", "Nervous Behaviour",
            " - Look out for customers looking around frantically and excessively \n- Some may appear to be sweaty due to anxiety or stress \n- These customers may be overly alert of their surroundings"),
        new Level("Level2", "My Own Shopping Bag",
            "- Look out for customers using their own shopping bags"),
        new Level("Level3", "Looking Cool",
            " - Look out for customers wearing shades or overly covered clothing \n- These customers might try avoiding areas that are well surveilled by CCTVs"),
        new Level("Level4", "No Loitering",
            "- Look out for customers standing around in the same area for too long and aren't shopping \n- These customers may walk occasionally to change positions"),
    };

    public static string ScenePath(string scene) => "Assets/Scenes/" + scene + ".unity";
    public static string ScenePath(int level) => ScenePath(All[level].Scene);

    public static bool HasLevel(int level) => level >= 0 && level < All.Length;

    // A scored level (not the tutorial).
    public static bool IsLevel(string scene)
    {
        for (int i = 1; i < All.Length; i++) if (All[i].Scene == scene) return true;
        return false;
    }
}

using System;

public class Levels
{
    public int CurrentLevel;
    public int RequiredXpThisLevel;
    public int MissingXpThisLevel;
    public int XpThisLevel;
    public float XpBarPos;

    int LevelFromXp(int xp)
    {
        // Start with 100. Increase 25 per level.
        int requirementThisLevel = 100;
        int sum = 0;
        int result = 1;
        while (true)
        {
            int requiredForNextlevel = sum + requirementThisLevel;
            int missingXpThisLevel = requiredForNextlevel - xp;
            if (missingXpThisLevel > 0)
            {
                RequiredXpThisLevel = requirementThisLevel;
                MissingXpThisLevel = missingXpThisLevel;
                XpThisLevel = RequiredXpThisLevel - MissingXpThisLevel;
                XpBarPos = (float)XpThisLevel / RequiredXpThisLevel;
                return result;
            }

            result++;
            if (result > 100)
                throw new Exception("Level loop overflow");

            sum += requirementThisLevel;
            requirementThisLevel += 25;
        }
    }

    public void UpdateXp(int xp)
    {
        CurrentLevel = LevelFromXp(xp);
    }
}

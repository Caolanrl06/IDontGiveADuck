using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelId;
    public string levelName;
    public int goodDucks;
    public int decoyDucks;
    public float timeLimit;
    public float spawnRate;
    public float duckLifetime;
    public int decoyPenalty;
    
    // New fields for continuous spawning logic
    public int maxTotalSpawns = 10;  // Maximum total good ducks that can spawn
    public bool continueSpawning = true;  // Whether to continue spawning after initial good ducks
    
    [System.Serializable]
    public class SizeDistribution
    {
        public float large;
        public float medium;
        public float small;
    }
    
    public SizeDistribution sizeDistribution;
    public string[] specialMechanics;
    public string backgroundMusic;
    public string backgroundImage;
    public string difficulty;
    public string designNotes;
    public float targetSuccessRate;
    public string learningObjective;
    public bool powerUpsAvailable;
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// LevelLoader - Handles loading level data from JSON files
/// 
/// This class demonstrates several important programming concepts:
/// - Singleton Pattern: Ensures only one instance exists
/// - Caching: Improves performance by storing loaded data
/// - Error Handling: Graceful fallback when files are missing
/// - Resource Loading: Unity's Resources system for file access
/// </summary>
public class LevelLoader : MonoBehaviour
{
    public event Action<LevelData> OnLevelLoaded;
    // Singleton pattern - static instance accessible from anywhere
    public static LevelLoader Instance { get; private set; }
    
    // Cache to store loaded levels and avoid reloading from disk
    private Dictionary<int, LevelData> levelCache = new Dictionary<int, LevelData>();
    
    void Awake()
    {
        
        // Singleton pattern implementation
        // If no instance exists, make this the singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep alive when scenes change
        }
        else
        {
            // If another instance exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads a level by its ID number
    /// 
    /// Process:
    /// 1. Check if level is already cached (performance optimisation)
    /// 2. If not cached, load from JSON file in Resources folder
    /// 3. Parse JSON into LevelData object
    /// 4. Cache the result for future use
    /// 5. Return default level if loading fails
    /// </summary>
    public LevelData LoadLevel(int levelId)
    {
        // Performance optimisation: Check cache first
        if (levelCache.ContainsKey(levelId))
        {
            return levelCache[levelId];
        }
        
        // Create filename with zero-padding (e.g., "level_001", "level_012")
        // The :D3 format specifier ensures the level ID is always 3 digits with leading zeros
        // This creates consistent filenames that sort correctly in file systems
        string fileName = $"level_{levelId:D3}";
        
        // Load JSON file from Unity's Resources system
        // Path: Assets/Resources/Data/Levels/level_001.json
        TextAsset levelFile = Resources.Load<TextAsset>($"Data/Levels/{fileName}");
        
        if (levelFile != null)
        {
            try
            {
                // Convert JSON text to LevelData object
                LevelData levelData = JsonUtility.FromJson<LevelData>(levelFile.text);
                
                // Store in cache for future use
                levelCache[levelId] = levelData;
                return levelData;
            }
            catch (System.Exception e)
            {
                // Error handling: If JSON parsing fails, create default level
                Debug.LogError($"Failed to parse level {levelId}: {e.Message}");
                return CreateDefaultLevel(levelId);
            }
        }
        else
        {
            // Error handling: If file doesn't exist, create default level
            Debug.LogWarning($"Level file not found: {fileName}");
            return CreateDefaultLevel(levelId);
        }
    }
    
    /// <summary>
    /// Checks if the next level exists and returns its ID
    /// 
    /// This method enables level progression by checking if level files exist
    /// Returns -1 if no more levels are available (end of game)
    /// </summary>
    public int GetNextLevelId(int currentLevelId)
    {
        int nextLevelId = currentLevelId + 1;
        string fileName = $"level_{nextLevelId:D3}";
        
        // Try to load the next level file
        TextAsset levelFile = Resources.Load<TextAsset>($"Data/Levels/{fileName}");
        
        // Return next level ID if file exists, otherwise -1 (no more levels)
        return levelFile != null ? nextLevelId : -1;
    }
    
    /// <summary>
    /// Creates a default level when the actual level file is missing or corrupted
    /// 
    /// This is an example of defensive programming - always provide a fallback
    /// so the game doesn't crash when data is missing
    /// </summary>
    private LevelData CreateDefaultLevel(int levelId)
    {
        LevelData defaultLevel = new LevelData
        {
            levelId = levelId,
            levelName = $"Default Level {levelId}",
            goodDucks = 3,           // Number of good ducks to click
            decoyDucks = 1,          // Number of decoy ducks to avoid
            timeLimit = 30f,         // Time limit in seconds
            spawnRate = 3.0f,        // How often ducks spawn
            duckLifetime = 5.0f,     // How long ducks stay on screen
            decoyPenalty = 3,        // Time penalty for clicking decoys
            sizeDistribution = new LevelData.SizeDistribution
            {
                large = 0.6f,        // 60% chance of large ducks
                medium = 0.3f,       // 30% chance of medium ducks
                small = 0.1f         // 10% chance of small ducks
            },
            specialMechanics = new string[0],  // No special mechanics
            backgroundMusic = "tutorial_theme",
            backgroundImage = "background1",
            difficulty = "normal",
            designNotes = "Default level created due to missing level file",
            targetSuccessRate = 0.8f,          // 80% success rate target
            learningObjective = "Complete the level",
            powerUpsAvailable = false
        };
        
        return defaultLevel;
    }
    
    /// <summary>
    /// Clears the level cache to free up memory
    /// 
    /// Useful when switching between different level sets
    /// or when you want to force reloading of level data
    /// </summary>
    public void ClearCache()
    {
        levelCache.Clear();
    }
}

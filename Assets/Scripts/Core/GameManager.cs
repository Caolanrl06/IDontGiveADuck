using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - The central nervous system of the game
/// 
/// This class demonstrates several important game development concepts:
/// - Singleton Pattern: Ensures only one game manager exists
/// - State Machine: Manages different game states (Menu, Playing, Paused, etc.)
/// - Event-Driven Architecture: Notifies other systems of game changes
/// - Game Loop: Controls the main game flow and progression
/// - Score System: Tracks player progress and achievements
/// - Level Management: Handles level loading and progression
/// - Input Handling: Processes player interactions with game objects
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton pattern - accessible from anywhere in the game
    public static GameManager Instance { get; private set; }
    
    [Header("Game Configuration")]
    [SerializeField] private int startingLives = 1;    // Number of lives player starts with
    static public int currentLevelId = 1;   // Current level being played
    
    [Header("Testing Tools")]
    [SerializeField] private int testLevelId = 12;     // Level to jump to for testing
    [SerializeField] private bool enableTestTools = true; // Enable/disable test tools in inspector
    
    [Header("Current Game State")]
    [SerializeField] private int score = 0;             // Player's current score
    [SerializeField] private int lives = 1;             // Remaining lives
    [SerializeField] private float timeLeft = 30f;      // Time remaining in current level
    [SerializeField] private int goodDucksClicked = 0;  // Number of good ducks clicked
    [SerializeField] private int goodDucksMissed = 0;   // Number of good ducks missed
    [SerializeField] private int totalGoodDucksSpawned = 0; // Total good ducks spawned this level
    
    // Private state variables
    private LevelData currentLevel;                     // Data for the current level
    private GameState currentState = GameState.Menu;    // Current game state
    private float levelStartTime;                       // When the level started
    private int totalDucksSpawned = 0;                  // Total ducks spawned this level
    
    // Events that other systems can subscribe to
    // This creates loose coupling between systems
    public System.Action<int> OnScoreChanged;           // Fired when score changes
    public System.Action<int> OnLivesChanged;           // Fired when lives change
    public System.Action<float> OnTimeChanged;          // Fired when time changes
    public System.Action<GameState> OnGameStateChanged; // Fired when game state changes
    public System.Action<LevelData> OnLevelLoaded;      // Fired when a new level is loaded
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Called when the GameObject is created
    /// Sets up the singleton pattern and initialises the game
    /// </summary>
    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep alive when scenes change
            InitializeGame();
        }
        else
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Called after Awake, when the GameObject becomes active
    /// Loads the first level to start the game
    /// </summary>
    void Start()
    {
        LoadCurrentLevel();
    }
    
    /// <summary>
    /// Called every frame
    /// Updates the game timer when actively playing
    /// </summary>
    void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameTimer();
        }
    }
    
    #endregion
    
    #region Initialisation
    
    /// <summary>
    /// Sets up the initial game state
    /// Called once when the game starts
    /// </summary>
    private void InitializeGame()
    {
        lives = startingLives;
        score = 0;
        currentState = GameState.Menu;
    }
    
    #endregion
    
    #region Level Management
    
    /// <summary>
    /// Loads the current level data and resets level-specific variables
    /// 
    /// This method:
    /// 1. Loads level data from JSON files
    /// 2. Resets level-specific counters
    /// 3. Notifies other systems about the new level
    /// </summary>
    private void LoadCurrentLevel()
    {
        if (LevelLoader.Instance == null)
        {
            Debug.LogError("LevelLoader not found! Make sure LevelLoader is in the scene.");
            return;
        }
        
        // Load level data from JSON file
        currentLevel = LevelLoader.Instance.LoadLevel(currentLevelId);
        
        if (currentLevel == null)
        {
            Debug.LogError($"Failed to load level {currentLevelId}");
            return;
        }
        
        // Reset level-specific variables
        timeLeft = currentLevel.timeLimit;
        goodDucksClicked = 0;
        goodDucksMissed = 0;
        totalDucksSpawned = 0;
        totalGoodDucksSpawned = 0;
        
        // Notify other systems (UI, Audio) about the new level
        OnLevelLoaded?.Invoke(currentLevel);
    }
    
    /// <summary>
    /// Advances to the next level in sequence
    /// 
    /// Checks if there are more levels available
    /// If no more levels, completes the game
    /// </summary>
    public void AdvanceToNextLevel()
    {
        int nextLevelId = LevelLoader.Instance.GetNextLevelId(currentLevelId);
        
        if (nextLevelId > 0)
        {
            currentLevelId = nextLevelId;
            LoadCurrentLevel();
            StartGame(false);
        }
        else
        {
            // No more levels - game complete!
            CompleteGame();
        }
    }
    
    /// <summary>
    /// Jumps directly to a specific level (for testing)
    /// 
    /// Stops current spawning, loads the target level, and starts the game
    /// </summary>
    public void JumpToLevel(int levelId)
    {
        // Stop current duck spawning
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        currentLevelId = levelId;
        LoadCurrentLevel();
        StartGame(false);
    }
    
    /// <summary>
    /// Jumps to the test level specified in the inspector
    /// 
    /// This method can be called from the inspector for easy testing
    /// </summary>
    [ContextMenu("Jump To Test Level")]
    public void JumpToTestLevel()
    {
        if (enableTestTools)
        {
            JumpToLevel(testLevelId);
        }
        else
        {
            Debug.LogWarning("Test tools are disabled. Enable 'enableTestTools' to use this feature.");
        }
    }
    
    /// <summary>
    /// Jumps to a specific level (for inspector use)
    /// 
    /// This method can be called from the inspector with a specific level number
    /// </summary>
    [ContextMenu("Jump To Level 1")]
    public void JumpToLevel1() => JumpToLevel(1);
    
    [ContextMenu("Jump To Level 5")]
    public void JumpToLevel5() => JumpToLevel(5);
    
    [ContextMenu("Jump To Level 10")]
    public void JumpToLevel10() => JumpToLevel(10);
    
    [ContextMenu("Jump To Level 12")]
    public void JumpToLevel12() => JumpToLevel(12);
    
    /// <summary>
    /// Restarts the entire game from level 1
    /// 
    /// This is a complete reset - all progress is lost
    /// Returns the player to the main menu
    /// </summary>
    public void RestartLevel()
    {
        // Stop current duck spawning
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        // Complete reset to level 1
        currentLevelId = 1;
        score = 0;
        lives = 1;
        
        // Reset all game state
        timeLeft = 30f;
        goodDucksClicked = 0;
        goodDucksMissed = 0;
        totalDucksSpawned = 0;
        totalGoodDucksSpawned = 0;
        levelStartTime = 0f;
        
        // Update UI with reset values
        OnLivesChanged?.Invoke(lives);
        OnScoreChanged?.Invoke(score);
        OnTimeChanged?.Invoke(timeLeft);
        
        // Load level 1 and return to menu
        LoadCurrentLevel();
        currentState = GameState.Menu;
        OnGameStateChanged?.Invoke(currentState);
    }
    
    #endregion
    
    #region Game Flow Control
    
    /// <summary>
    /// Starts the current level
    /// 
    /// This method:
    /// 1. Validates that a level is loaded
    /// 2. Changes game state to Playing
    /// 3. Starts duck spawning
    /// 4. Notifies other systems of the state change
    /// </summary>
    public void StartGame(bool fromMenu = false)
    {
        if (currentLevel == null)
        {
            Debug.LogError("Cannot start game - no level loaded!");
            return;
        }
        
        currentState = GameState.Playing;
        
        // Only trigger level load event if coming from menu
        // (prevents duplicate audio/music changes when advancing levels)
        if (fromMenu)
        {
            OnLevelLoaded?.Invoke(currentLevel);
        }
        
        levelStartTime = Time.time;
        
        // Start spawning ducks
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            spawner.StartSpawning(currentLevel);
        }
        else
        {
            Debug.LogError("DuckSpawner not found! Make sure DuckSpawner is in the scene.");
        }
        
        // Notify other systems of state change
        OnGameStateChanged?.Invoke(currentState);
    }
    
    /// <summary>
    /// Ends the current level (win or lose)
    /// 
    /// This method:
    /// 1. Sets the appropriate game state
    /// 2. Stops duck spawning
    /// 3. Handles level completion or game over
    /// </summary>
    public void EndGame(bool won)
    {
        // Handle the result first (including time bonus calculation)
        if (won)
        {
            HandleLevelComplete();
        }
        else
        {
            HandleGameOver();
        }
        
        currentState = won ? GameState.LevelComplete : GameState.GameOver;
        
        // Stop spawning ducks
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        // Notify other systems of state change (with final score already calculated)
        OnGameStateChanged?.Invoke(currentState);
    }
    
    /// <summary>
    /// Pauses or unpauses the game
    /// 
    /// Uses Unity's Time.timeScale to pause the entire game
    /// This affects all time-based systems (animations, physics, etc.)
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f; // Pause the entire game
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f; // Resume normal speed
        }
        
        OnGameStateChanged?.Invoke(currentState);
    }
    
    #endregion
    
    #region Duck Event Handlers
    
    /// <summary>
    /// Called when player clicks a good duck
    /// 
    /// This method:
    /// 1. Adds points to the score
    /// 2. Increments the good duck counter
    /// 3. Updates the UI
    /// 4. Checks if the level is complete
    /// </summary>
    public void OnGoodDuckClicked(GoodDuck duck)
    {
        if (currentState != GameState.Playing) return;
        
        score += duck.PointValue;
        goodDucksClicked++;
        
        OnScoreChanged?.Invoke(score);
        
        // Check win condition - player got required good ducks
        if (goodDucksClicked >= currentLevel.goodDucks)
        {
            EndGame(true);
        }
    }
    
    /// <summary>
    /// Called when a good duck expires (player missed it)
    /// 
    /// Currently just tracks the statistic
    /// Could be extended to add penalties or other mechanics
    /// </summary>
    public void OnGoodDuckMissed(GoodDuck duck)
    {
        if (currentState != GameState.Playing) return;
        
        goodDucksMissed++;
    }
    
    /// <summary>
    /// Called when player clicks a decoy duck
    /// 
    /// This method:
    /// 1. Applies a time penalty
    /// 2. Updates the UI
    /// 3. Checks if the penalty caused game over
    /// </summary>
    public void OnDecoyDuckClicked(DecoyDuck duck)
    {
        if (currentState != GameState.Playing) return;
        
        // Apply time penalty from level configuration
        timeLeft -= currentLevel.decoyPenalty;
        OnTimeChanged?.Invoke(timeLeft);
        
        // Check if penalty caused game over
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            EndGame(false);
        }
    }
    
    /// <summary>
    /// Called when a decoy duck expires naturally
    /// 
    /// No penalty for decoys that expire naturally
    /// Could be extended for additional mechanics
    /// </summary>
    public void OnDecoyDuckExpired(DecoyDuck duck)
    {
        if (currentState != GameState.Playing) return;
    }
    
    /// <summary>
    /// Called when a new duck is spawned
    /// 
    /// Tracks total ducks spawned for statistics
    /// Could be used for difficulty scaling or achievements
    /// </summary>
    public void OnDuckSpawned()
    {
        totalDucksSpawned++;
    }
    
    /// <summary>
    /// Called when a good duck is spawned
    /// 
    /// Tracks total good ducks spawned
    /// </summary>
    public void OnGoodDuckSpawned()
    {
        totalGoodDucksSpawned++;
    }
    
    #endregion
    
    #region Game Timer
    
    /// <summary>
    /// Updates the game timer every frame
    /// 
    /// This method:
    /// 1. Decreases time remaining
    /// 2. Updates the UI
    /// 3. Checks if time ran out
    /// </summary>
    private void UpdateGameTimer()
    {
        timeLeft -= Time.deltaTime;
        OnTimeChanged?.Invoke(timeLeft);
        
        // Check if time ran out
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            EndGame(false);
        }
    }
    
    #endregion
    
    #region Game Completion Handlers
    
    /// <summary>
    /// Handles level completion
    /// 
    /// Awards bonus points based on remaining time
    /// 10 points per second remaining
    /// </summary>
    private void HandleLevelComplete()
    {
        int timeBonus = Mathf.RoundToInt(timeLeft * 10);
        score += timeBonus;
        
        OnScoreChanged?.Invoke(score);
    }
    
    /// <summary>
    /// Handles game over
    /// 
    /// Currently empty - could be extended for:
    /// - Saving high scores
    /// - Showing game over screen
    /// - Playing game over sounds
    /// </summary>
    private void HandleGameOver()
    {
        // Could add game over logic here
    }
    
    /// <summary>
    /// Handles game completion (all levels finished)
    /// 
    /// Sets the game state to GameComplete
    /// Notifies other systems of the completion
    /// </summary>
    private void CompleteGame()
    {
        currentState = GameState.GameComplete;
        OnGameStateChanged?.Invoke(currentState);
    }
    
    #endregion
    
    #region Public Getters
    
    // Properties that other systems can access to get game state
    // These provide read-only access to private variables
    public int Score => score;
    public int Lives => lives;
    public float TimeLeft => timeLeft;
    public GameState CurrentState => currentState;
    public LevelData CurrentLevel => currentLevel;
    public int CurrentLevelId => currentLevelId;
    public int GoodDucksClicked => goodDucksClicked;
    public int GoodDucksRequired => currentLevel?.goodDucks ?? 0;
    public int TotalGoodDucksSpawned => totalGoodDucksSpawned;
    public int MaxTotalSpawns => currentLevel?.maxTotalSpawns ?? 10;
    public float LevelProgress => currentLevel != null ? (float)goodDucksClicked / currentLevel.goodDucks : 0f;
    
    #endregion
    
    #region Scene Management
    
    /// <summary>
    /// Restarts the entire game by reloading the scene
    /// 
    /// This is a complete restart - all game state is reset
    /// Useful for returning to a clean state
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure game is not paused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Quits the game
    /// 
    /// Works in both build and editor
    /// In editor, stops play mode
    /// In build, closes the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    #endregion
}

/// <summary>
/// GameState enumeration - defines all possible states of the game
/// 
/// This creates a state machine that controls game flow:
/// - Menu: Main menu/instructions screen
/// - Playing: Active gameplay
/// - Paused: Game is paused
/// - LevelComplete: Level finished successfully
/// - GameOver: Level failed
/// - GameComplete: All levels completed
/// </summary>
public enum GameState
{
    Menu,           // Main menu/instructions
    Playing,        // Active gameplay
    Paused,         // Game paused
    LevelComplete,  // Level finished successfully
    GameOver,       // Level failed
    GameComplete    // All levels completed
}
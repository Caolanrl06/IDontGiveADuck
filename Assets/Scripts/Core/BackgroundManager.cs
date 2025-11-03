using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Level-Specific Backgrounds")]
    [SerializeField] private Sprite backGround1;
    [SerializeField] private Sprite backGround2;
    [SerializeField] private Sprite backGround3;
    [SerializeField] private Sprite backGround4;
    [SerializeField] private Sprite normalBackground;

    [SerializeField] private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Subscribe your method to the GameManager's event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelLoaded += HandleLevelLoaded;
        }
    }

    // This method will run automatically whenever GameManager triggers OnLevelLoaded
    private void HandleLevelLoaded(LevelData levelData)
    {
        ChangeBackground(levelData);
    }

    private void ChangeBackground(LevelData levelData)
    {
        if (levelData.levelId <= 3)
        {
            spriteRenderer.sprite = backGround1;
        }
        else if (levelData.levelId >= 4 && levelData.levelId <= 6)
        {
            spriteRenderer.sprite = backGround2;
        }
        else if (levelData.levelId >= 7 && levelData.levelId <= 9)
        {
            spriteRenderer.sprite = backGround3;
        }
        else if (levelData.levelId >= 10 && levelData.levelId <= 12)
        {
            spriteRenderer.sprite = backGround4;
        }
        else
        {
            spriteRenderer.sprite = normalBackground;
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks or null references
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelLoaded -= HandleLevelLoaded;
        }
    }
}
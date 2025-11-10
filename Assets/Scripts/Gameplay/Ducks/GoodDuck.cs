using UnityEngine;

/// <summary>
/// Good duck that players should click for points
/// Save this as: Assets/Scripts/Gameplay/Ducks/GoodDuck.cs/// </summary>
public class GoodDuck : BaseDuck
{
    [Header("Good Duck Settings")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private GameObject successTextPrefab; // Optional floating text
    [SerializeField] private GameObject scoreDisplay;
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
   
    
    protected override void Start()
    {
        base.Start();
        
    }
    
    #region Abstract Implementation
    
    protected override void OnClicked()
    {
        Debug.Log($"Good duck clicked! Awarded {pointValue} points");
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckClicked(this);
        }
        
        // Play success feedback
        PlaySuccessEffects();
        DisplayModifyier();
        
        // Destroy duck
        DestroyDuck();
    }
    
    protected override void OnLifetimeExpired()
    {
        Debug.Log("Good duck expired - player missed it!");
        
        // Notify game manager about missed duck
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckMissed(this);
        }
        
        // No special effects for missed ducks
    }
    
    #endregion
    
    #region Virtual Overrides
    
    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        
        // Good duck specific spawn behaviour
        // Could add spawn animation, sound, etc.
        
        // Ensure proper tag for identification
        gameObject.tag = "GoodDuck";
    }
    
    protected override void OnLifetimeLow()
    {
        base.OnLifetimeLow();
        // Could add sprite swap or animation here if needed
    }
    
    #endregion
    
    #region Good Duck Specific Methods
    
    /// <summary>
    /// Play success effects when clicked
    /// </summary>
    private void PlaySuccessEffects()
    {
        // Particle effect
        if (successParticles != null)
        {
            ParticleSystem effect = Instantiate(successParticles, transform.position, transform.rotation);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        // Sound effect - use AudioManager for consistency
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckClickGood(transform.position);
        }
        
        // Floating score text (optional)
        if (successTextPrefab != null)
        {
            GameObject scoreText = Instantiate(successTextPrefab, transform.position, Quaternion.identity);
            // Assume the prefab has a script to handle floating animation
        }
    }

    private void DisplayModifyier()
    {
        Instantiate(scoreDisplay);
    }
    
    #endregion

}
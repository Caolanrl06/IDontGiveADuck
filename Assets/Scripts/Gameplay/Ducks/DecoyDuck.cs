using UnityEngine;

/// <summary>
/// Decoy duck that penalises players when clicked
/// </summary>
public class DecoyDuck : BaseDuck
{
    [Header("Decoy Duck Settings")]
    [SerializeField] private ParticleSystem penaltyParticles;
    [SerializeField] private AudioClip penaltySound;
    [SerializeField] public int timePenalty = 3; // seconds to subtract
    [SerializeField] private GameObject penaltyTextPrefab; // Optional floating text
    
    [Header("Visual Distinction")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool subtleVisualDifference = true; // Make it harder to distinguish
    
    #region Initialization Override
    
    /// <summary>
    /// Initialise decoy duck with custom properties
    /// </summary>
    public override void Initialize(float customLifetime = -1, int customPointValue = -1)
    {
        base.Initialize(customLifetime, customPointValue);
    
    }
    
    #endregion
    
    #region Abstract Implementation
    
    protected override void OnClicked()
    {
        
        // Notify game manager about penalty
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDecoyDuckClicked(this);
        }
        
        // Play penalty feedback
        PlayPenaltyEffects();
        
        // Destroy duck
        DestroyDuck();
    }
    
    protected override void OnLifetimeExpired()
    {
        Debug.Log("Decoy duck expired naturally - no penalty");
        
        // Notify game manager (no penalty for natural expiration)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDecoyDuckExpired(this);
        }
        
        // No penalty effects for natural expiration
    }
    
    #endregion
    
    #region Virtual Overrides
    
    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        
        // Decoy duck specific spawn behaviour
        // Could add subtle spawn differences
        
        // Ensure proper tag for identification
        gameObject.tag = "DecoyDuck";
        
        // Optional: Add subtle behavioural differences
        if (subtleVisualDifference)
        {
            AddSubtleBehavioralDifferences();
        }
    }
    
    protected override void HandleMovement()
    {
        base.HandleMovement();
        
        // NOTE: This movement code is currently not active!
        // The base BaseDuck class has moveSpeed = 0f by default
        // To see this movement in action, you would need to:
        // 1. Set moveSpeed > 0 in the Inspector, OR
        // 2. Override the moveSpeed property in this DecoyDuck class
        
        // Decoy ducks could have slightly different movement patterns
        // This could help players learn to distinguish them from good ducks
        if (moveSpeed > 0)
        {
            // Example: Decoy ducks move in a slightly different pattern (subtle wiggle)
            // This creates a small horizontal oscillation that observant players might notice
            float wiggle = Mathf.Sin(Time.time * 2f) * 0.1f;
            transform.position += Vector3.right * wiggle * Time.deltaTime;
        }
        // If moveSpeed is 0 (default), this duck won't move at all
    }
    
    #endregion
    
    #region Decoy Duck Specific Methods
    
    /// <summary>
    /// Play penalty effects when clicked
    /// </summary>
    private void PlayPenaltyEffects()
    {
        // Use AudioManager for sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckClickDecoy(transform.position);
        }
        
        // Particle effect (different from good duck)
        if (penaltyParticles != null)
        {
            ParticleSystem effect = Instantiate(penaltyParticles, transform.position, transform.rotation);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        // Floating penalty text (optional)
        if (penaltyTextPrefab != null)
        {
            GameObject penaltyText = Instantiate(penaltyTextPrefab, transform.position, Quaternion.identity);
            // Assume the prefab has a script to handle floating animation
        }
        
        
    }
    
    /// <summary>
    /// Add subtle differences to make decoys learnable but not obvious
    /// </summary>
    private void AddSubtleBehavioralDifferences()
    {
        // Example: Decoy ducks spawn slightly closer to edges
        // Or have slightly different timing patterns
        // This gives observant players a chance to learn the differences
        
        // Slight scale difference (barely noticeable)
        float scaleVariation = Random.Range(0.95f, 1.05f);
        transform.localScale *= scaleVariation;
        
        // Slight rotation variation
        transform.rotation *= Quaternion.Euler(0, 0, Random.Range(-2f, 2f));
    }
    
    #endregion
}
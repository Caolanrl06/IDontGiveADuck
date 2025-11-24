using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Good duck that players should click for points.
/// Save this as: Assets/Scripts/Gameplay/Ducks/GoodDuck.cs
/// </summary>
public class GoodDuck : BaseDuck
{
    [Header("Good Duck Settings")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private GameObject successTextPrefab; // Optional floating text prefab
    [SerializeField] private TextMeshPro scoreDisplayPrefab; 

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    #region Unity Methods

    protected override void Start()
    {
        base.Start();
    }

    #endregion

    #region Abstract Implementation

    protected override void OnClicked()
    {
        Debug.Log($"Good duck clicked! Awarded {pointValue} points");

        // Notify game manager
        GameManager.Instance?.OnGoodDuckClicked(this);

        // Play particle/sound/score text effects
        PlaySuccessEffects();
        DisplayModifier();

        // Destroy duck
        DestroyDuck();
    }

    protected override void OnLifetimeExpired()
    {
        Debug.Log("Good duck expired - player missed it!");

        GameManager.Instance?.OnGoodDuckMissed(this);
    }

    #endregion

    #region Virtual Overrides

    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        gameObject.tag = "GoodDuck";
    }

    protected override void OnLifetimeLow()
    {
        base.OnLifetimeLow();
        // Optional: add sprite swap or animation here
    }

    #endregion

    #region Good Duck Specific Methods

    /// <summary>
    /// Plays particle, sound, and floating text effects when clicked
    /// </summary>
    private void PlaySuccessEffects()
    {
        // Particle effect
        if (successParticles != null)
        {
            ParticleSystem effect = Instantiate(successParticles, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Sound effect
        AudioManager.Instance?.PlayDuckClickGood(transform.position);

        // Floating score prefab (optional)
        if (successTextPrefab != null)
        {
            Instantiate(successTextPrefab, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Displays the floating +points text at the mouse click position on the canvas
    /// </summary>
    private void DisplayModifier()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.z = -5.5f;
        scoreDisplayPrefab.text = $"+ {pointValue}";
        Instantiate(scoreDisplayPrefab, spawnPosition, transform.rotation);
    }

    #endregion
}
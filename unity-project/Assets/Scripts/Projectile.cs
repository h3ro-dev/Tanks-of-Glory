using UnityEngine;

/// <summary>
/// Handles projectile behavior, collisions, and explosions
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 100f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool explodeOnImpact = true;
    
    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private ParticleSystem trailEffect;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private AudioSource audioSource;
    
    // Internal variables
    private GameObject owner;
    private bool hasExploded = false;
    private float aliveTime = 0f;
    
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && impactSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    private void Start()
    {
        // Set up trail effect if available
        if (trailEffect != null)
        {
            trailEffect.Play();
        }
    }
    
    private void Update()
    {
        // Handle lifetime
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
        {
            if (explodeOnImpact)
            {
                Explode();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Don't collide with owner
        if (owner != null && collision.gameObject == owner)
        {
            return;
        }
        
        // Handle impact
        if (explodeOnImpact)
        {
            Explode();
        }
        else
        {
            // Non-exploding projectile (like a bullet)
            HandleDirectHit(collision);
        }
    }
    
    private void HandleDirectHit(Collision collision)
    {
        // Apply damage to target if it has health
        Health targetHealth = collision.gameObject.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }
        
        // Apply impact force
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForceAtPosition(transform.forward * damage, collision.contacts[0].point, ForceMode.Impulse);
        }
        
        // Play impact effects
        PlayImpactEffects(collision.contacts[0].point, collision.contacts[0].normal);
        
        // Destroy projectile
        Destroy(gameObject);
    }
    
    private void Explode()
    {
        if (hasExploded)
            return;
            
        hasExploded = true;
        
        // Find all objects within explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider nearbyObject in colliders)
        {
            // Skip the owner
            if (owner != null && nearbyObject.gameObject == owner)
                continue;
                
            // Apply damage to objects with health
            Health targetHealth = nearbyObject.GetComponent<Health>();
            if (targetHealth != null)
            {
                // Calculate damage based on distance
                float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
                float damagePercent = 1f - Mathf.Clamp01(distance / explosionRadius);
                float damageAmount = damage * damagePercent;
                
                targetHealth.TakeDamage(damageAmount);
            }
            
            // Apply explosive force to rigidbodies
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(damage * 10f, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
        }
        
        // Spawn explosion effect
        SpawnExplosionEffect();
        
        // Destroy the projectile
        Destroy(gameObject);
    }
    
    private void SpawnExplosionEffect()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // If the explosion has an audio source, play the impact sound
            AudioSource explosionAudio = explosion.GetComponent<AudioSource>();
            if (explosionAudio != null && impactSound != null)
            {
                explosionAudio.PlayOneShot(impactSound);
            }
            else if (audioSource != null && impactSound != null)
            {
                // Play on this object's audio source if explosion doesn't have one
                audioSource.PlayOneShot(impactSound);
            }

            // Clean up explosion after 3 seconds
            Destroy(explosion, 3f);
        }
        else
        {
            // Use procedural explosion effect when no prefab is provided
            TankCommander.ExplosionEffect.CreateExplosion(transform.position, explosionRadius);

            // Play sound if available
            if (audioSource != null && impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }
        }
    }
    
    private void PlayImpactEffects(Vector3 position, Vector3 normal)
    {
        // Play impact sound
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, position);
        }
        
        // Handle other effects (particles, decals, etc.)
        // This would depend on the specific game design
    }
    
    /// <summary>
    /// Sets the projectile's properties
    /// </summary>
    public void Initialize(float newDamage, float newExplosionRadius, GameObject projectileOwner)
    {
        damage = newDamage;
        explosionRadius = newExplosionRadius;
        owner = projectileOwner;
    }
    
    /// <summary>
    /// Sets the lifetime of the projectile
    /// </summary>
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }
    
    /// <summary>
    /// Draw explosion radius in editor for debugging
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
} 
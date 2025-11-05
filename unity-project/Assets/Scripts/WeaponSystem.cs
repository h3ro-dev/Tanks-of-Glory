using System.Collections;
using UnityEngine;

/// <summary>
/// Handles firing of weapons and manages ammo/cooldowns
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public class WeaponStats
    {
        public string weaponName = "Main Cannon";
        public float damage = 100f;
        public float fireRate = 1f; // Shots per second
        public float reloadTime = 2f;
        public int maxAmmo = 10;
        public int currentAmmo = 10;
        public bool infiniteAmmo = false;
        public float projectileSpeed = 50f;
        public float projectileLifetime = 5f;
        public float explosionRadius = 2f;
        public GameObject projectilePrefab;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;
        public ParticleSystem muzzleFlash;
        public Transform firePoint;
    }

    [Header("Primary Weapon")]
    [SerializeField] private WeaponStats primaryWeapon = new WeaponStats();
    
    [Header("Secondary Weapon")]
    [SerializeField] private WeaponStats secondaryWeapon = new WeaponStats();
    
    [Header("Audio")]
    [SerializeField] private AudioSource weaponAudioSource;

    // Internal state
    private float primaryLastFireTime;
    private float secondaryLastFireTime;
    private bool isReloadingPrimary;
    private bool isReloadingSecondary;
    
    // Events
    public delegate void OnAmmoChangedDelegate(int currentAmmo, int maxAmmo, bool isPrimary);
    public event OnAmmoChangedDelegate OnAmmoChanged;
    
    public delegate void OnWeaponFiredDelegate(bool isPrimary);
    public event OnWeaponFiredDelegate OnWeaponFired;
    
    public delegate void OnReloadingDelegate(bool isReloading, float reloadTime, bool isPrimary);
    public event OnReloadingDelegate OnReloading;

    private void Start()
    {
        // Find weapon audio source if not set
        if (weaponAudioSource == null)
        {
            weaponAudioSource = GetComponent<AudioSource>();
            if (weaponAudioSource == null)
            {
                weaponAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Ensure we have fire points
        EnsureFirePoints();
        
        // Initialize ammo UI
        if (OnAmmoChanged != null)
        {
            OnAmmoChanged.Invoke(primaryWeapon.currentAmmo, primaryWeapon.maxAmmo, true);
            OnAmmoChanged.Invoke(secondaryWeapon.currentAmmo, secondaryWeapon.maxAmmo, false);
        }
    }

    private void EnsureFirePoints()
    {
        // Try to find primary fire point if not set
        if (primaryWeapon.firePoint == null)
        {
            Transform turret = transform.Find("Turret");
            if (turret != null)
            {
                primaryWeapon.firePoint = turret.Find("FirePoint");
                if (primaryWeapon.firePoint == null)
                {
                    Debug.LogWarning("Primary weapon fire point not found. Creating one at default position.");
                    GameObject firePoint = new GameObject("FirePoint");
                    firePoint.transform.parent = turret;
                    firePoint.transform.localPosition = new Vector3(0, 0, 2f); // Default position at barrel end
                    primaryWeapon.firePoint = firePoint.transform;
                }
            }
        }
        
        // Try to find secondary fire point if not set
        if (secondaryWeapon.firePoint == null)
        {
            Transform turret = transform.Find("Turret");
            if (turret != null)
            {
                secondaryWeapon.firePoint = turret.Find("SecondaryFirePoint");
                if (secondaryWeapon.firePoint == null && primaryWeapon.firePoint != null)
                {
                    // Use primary fire point as fallback
                    secondaryWeapon.firePoint = primaryWeapon.firePoint;
                }
            }
        }
    }

    /// <summary>
    /// Fires the primary weapon if possible
    /// </summary>
    /// <returns>True if weapon was fired, false otherwise</returns>
    public bool FirePrimaryWeapon()
    {
        return FireWeapon(primaryWeapon, ref primaryLastFireTime, ref isReloadingPrimary, true);
    }

    /// <summary>
    /// Fires the secondary weapon if possible
    /// </summary>
    /// <returns>True if weapon was fired, false otherwise</returns>
    public bool FireSecondaryWeapon()
    {
        return FireWeapon(secondaryWeapon, ref secondaryLastFireTime, ref isReloadingSecondary, false);
    }

    /// <summary>
    /// Generic method to fire a weapon based on its stats
    /// </summary>
    private bool FireWeapon(WeaponStats weapon, ref float lastFireTime, ref bool isReloading, bool isPrimary)
    {
        // Check if we're reloading
        if (isReloading)
        {
            PlaySound(weapon.emptySound);
            return false;
        }
        
        // Check fire rate cooldown
        if (Time.time - lastFireTime < 1f / weapon.fireRate)
        {
            return false;
        }
        
        // Check if we have ammo
        if (weapon.currentAmmo <= 0 && !weapon.infiniteAmmo)
        {
            PlaySound(weapon.emptySound);
            StartCoroutine(ReloadWeapon(weapon, ref isReloading, isPrimary));
            return false;
        }
        
        // Update ammo
        if (!weapon.infiniteAmmo)
        {
            weapon.currentAmmo--;
            if (OnAmmoChanged != null)
            {
                OnAmmoChanged.Invoke(weapon.currentAmmo, weapon.maxAmmo, isPrimary);
            }
        }
        
        // Update fire time
        lastFireTime = Time.time;
        
        // Fire projectile
        FireProjectile(weapon);
        
        // Play effects
        PlayFireEffects(weapon);
        
        // Trigger event
        if (OnWeaponFired != null)
        {
            OnWeaponFired.Invoke(isPrimary);
        }
        
        // Auto-reload if empty
        if (weapon.currentAmmo <= 0 && !weapon.infiniteAmmo)
        {
            StartCoroutine(ReloadWeapon(weapon, ref isReloading, isPrimary));
        }
        
        return true;
    }

    /// <summary>
    /// Fires a projectile from the weapon
    /// </summary>
    private void FireProjectile(WeaponStats weapon)
    {
        if (weapon.projectilePrefab != null && weapon.firePoint != null)
        {
            // Instantiate projectile
            GameObject projectile = Instantiate(weapon.projectilePrefab, 
                                               weapon.firePoint.position, 
                                               weapon.firePoint.rotation);
            
            // Set up projectile
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = weapon.firePoint.forward * weapon.projectileSpeed;
            }
            
            // Set up projectile properties
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(weapon.damage, weapon.explosionRadius, gameObject);
                projectileScript.SetLifetime(weapon.projectileLifetime);
            }
            else
            {
                // If no projectile script, destroy after lifetime
                Destroy(projectile, weapon.projectileLifetime);
            }
        }
    }

    /// <summary>
    /// Plays firing effects (sound, muzzle flash, etc.)
    /// </summary>
    private void PlayFireEffects(WeaponStats weapon)
    {
        // Play sound
        PlaySound(weapon.fireSound);

        // Play muzzle flash particle system
        if (weapon.muzzleFlash != null)
        {
            weapon.muzzleFlash.Play();
        }

        // Play procedural muzzle flash effect
        if (weapon.firePoint != null)
        {
            TankCommander.MuzzleFlash flash = weapon.firePoint.GetComponent<TankCommander.MuzzleFlash>();
            if (flash != null)
            {
                flash.Flash();
            }
        }

        // Add camera shake or other effects here
    }

    /// <summary>
    /// Helper to play sounds on the weapon audio source
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && weaponAudioSource != null)
        {
            weaponAudioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Reloads the weapon after a delay
    /// </summary>
    private IEnumerator ReloadWeapon(WeaponStats weapon, ref bool isReloading, bool isPrimary)
    {
        if (isReloading || weapon.currentAmmo == weapon.maxAmmo)
            yield break;
        
        isReloading = true;
        
        // Notify listeners that we're reloading
        if (OnReloading != null)
        {
            OnReloading.Invoke(true, weapon.reloadTime, isPrimary);
        }
        
        // Play reload sound
        PlaySound(weapon.reloadSound);
        
        // Wait for reload time
        yield return new WaitForSeconds(weapon.reloadTime);
        
        // Reload complete
        weapon.currentAmmo = weapon.maxAmmo;
        isReloading = false;
        
        // Notify listeners that reload is complete
        if (OnReloading != null)
        {
            OnReloading.Invoke(false, 0f, isPrimary);
        }
        
        // Update ammo UI
        if (OnAmmoChanged != null)
        {
            OnAmmoChanged.Invoke(weapon.currentAmmo, weapon.maxAmmo, isPrimary);
        }
    }

    /// <summary>
    /// Manually reload the primary weapon
    /// </summary>
    public void ReloadPrimaryWeapon()
    {
        if (!isReloadingPrimary && primaryWeapon.currentAmmo < primaryWeapon.maxAmmo)
        {
            StartCoroutine(ReloadWeapon(primaryWeapon, ref isReloadingPrimary, true));
        }
    }

    /// <summary>
    /// Manually reload the secondary weapon
    /// </summary>
    public void ReloadSecondaryWeapon()
    {
        if (!isReloadingSecondary && secondaryWeapon.currentAmmo < secondaryWeapon.maxAmmo)
        {
            StartCoroutine(ReloadWeapon(secondaryWeapon, ref isReloadingSecondary, false));
        }
    }
} 
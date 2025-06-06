using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab; // Assign bullet prefab in inspector
    public Transform firePoint; // The position where the bullet spawns
    public float shootCooldown = 0.2f; // Time between shots
    private float lastShootTime = 0f;

    private ShootingManager shootingManager;
    private UDPReceiver udpReceiver; // Reference to UDPReceiver script

    void Start()
    {
        shootingManager = FindObjectOfType<ShootingManager>();
        udpReceiver = FindObjectOfType<UDPReceiver>();

        if (shootingManager == null)
            Debug.LogError("❌ PlayerShoot: No ShootingManager found in scene!");
        if (udpReceiver == null)
            Debug.LogError("❌ PlayerShoot: No UDPReceiver found in scene!");
    }

    void Update()
    {
        //if (udpReceiver != null && udpReceiver.triggerPressed)
        //{
            
            
               // Shoot();
                //lastShootTime = Time.time; // Reset cooldown timer
            
      //  }
    }

    public void Shoot()
    {
        var obj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        obj.transform.forward = firePoint.forward;

        // Increase shot count even if it doesn't hit
        if (shootingManager != null)
            shootingManager.IncreaseShotCount();
    }
}

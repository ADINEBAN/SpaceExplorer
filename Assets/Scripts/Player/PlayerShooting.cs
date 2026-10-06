using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Fire Rate")]
    [SerializeField] private float shootCooldown = 0.15f;

    private float shootTimer;

    private void Update()
    {
        shootTimer -= Time.deltaTime;

        if (Keyboard.current == null)
            return;

        // GIỮ SPACE = BẮN LIÊN TỤC
        if (Keyboard.current.spaceKey.isPressed &&
            shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }
    }

    private void Shoot()
    {
        if (laserPrefab == null || firePoint == null)
            return;

        Instantiate(
            laserPrefab,
            firePoint.position,
            Quaternion.identity
        );
    }
}
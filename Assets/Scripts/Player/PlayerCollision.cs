using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private bool gameOver;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameOver)
            return;

        if (!other.CompareTag("Asteroid"))
            return;

        gameOver = true;

        // Trừ 25 điểm khi Player va chạm asteroid
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(-25);
        }

        // Tạo hiệu ứng nổ
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                explosionEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            ParticleSystem[] particles =
                effect.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particle in particles)
            {
                particle.Play();
            }

            Destroy(effect, 2f);
        }

        Debug.Log("GAME OVER - Player hit by asteroid!");

        // Ẩn Player
        gameObject.SetActive(false);
    }
}
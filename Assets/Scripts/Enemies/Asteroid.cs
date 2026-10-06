using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int health = 1;
    [SerializeField] private int scoreValue = 15;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Movement")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float verticalDrift = 1.5f;
    [SerializeField] private float rotationSpeed = 120f;

    private float speed;
    private float drift;
    private float rotationDirection;

    private void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);

        drift = Random.Range(
            -verticalDrift,
            verticalDrift
        );

        rotationDirection =
            Random.value < 0.5f ? -1f : 1f;
    }

    private void Update()
    {
        transform.position += new Vector3(
            -speed,
            drift,
            0f
        ) * Time.deltaTime;

        transform.Rotate(
            Vector3.forward,
            rotationDirection *
            rotationSpeed *
            Time.deltaTime
        );

        Camera cam = Camera.main;

        if (cam != null)
        {
            float leftEdge =
                cam.ViewportToWorldPoint(
                    new Vector3(0f, 0.5f, 0f)
                ).x;

            if (transform.position.x < leftEdge - 5f)
            {
                Destroy(gameObject);
            }
        }
    }

  public void TakeDamage(int damage)
{
    health -= damage;

    SpawnEffect(hitEffectPrefab);

    Debug.Log($"{gameObject.name} hit! Remaining HP = {health}");

    if (health > 0)
        return;

    // Cộng điểm khi phá hủy asteroid
    if (ScoreManager.Instance != null)
    {
        ScoreManager.Instance.AddScore(scoreValue);
    }

    SpawnEffect(explosionEffectPrefab);

    Debug.Log($"{gameObject.name} destroyed! +{scoreValue} score");

    Destroy(gameObject);
}

private void SpawnEffect(GameObject effectPrefab)
{
    if (effectPrefab == null)
    {
        Debug.LogWarning(
            $"{gameObject.name}: Effect Prefab chưa được gán!"
        );
        return;
    }

    GameObject effect = Instantiate(
        effectPrefab,
        transform.position,
        Quaternion.identity
    );

    ParticleSystem[] particles =
        effect.GetComponentsInChildren<ParticleSystem>(true);

    foreach (ParticleSystem particle in particles)
    {
        particle.Play();
    }
}
}
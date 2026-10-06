using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(
            Vector3.right * speed * Time.deltaTime,
            Space.Self
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Asteroid"))
            return;

        Asteroid asteroid =
            other.GetComponent<Asteroid>();

        if (asteroid != null)
        {
            asteroid.TakeDamage(1);
        }

        Destroy(gameObject);
    }
}
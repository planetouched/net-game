using UnityEngine;

public class RailGunScript : MonoBehaviour
{
    ParticleSystem[] _particleSystems;

    [SerializeField] float speed = 2000f;

    [SerializeField] float timeAlive = 1f;

    private void Awake()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        Invoke(nameof(SelfDestruct), timeAlive);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void SelfDestruct()
    {
        DetatchParticles();
        Destroy(gameObject);
    }

    private void DetatchParticles()
    {
        foreach (ParticleSystem ps in _particleSystems)
        {
            ps.transform.parent = null;
            ps.Stop();
        }
    }
}
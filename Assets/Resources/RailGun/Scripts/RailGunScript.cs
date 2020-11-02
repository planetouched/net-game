using UnityEngine;

public class RailGunScript : MonoBehaviour
{
    ParticleSystem[] _particleSystems;

    private void Awake()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        Invoke(nameof(SelfDestruct), 1);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * 2000 * Time.deltaTime);
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
using UnityEngine;

public class RailGunScript : MonoBehaviour
{
	ParticleSystem[] particleSystems;

	[SerializeField]
	float speed = 1000f;

	[SerializeField]
	float timeAlive = 1f;

	private void Awake() {
		particleSystems = GetComponentsInChildren<ParticleSystem>();
		Invoke(nameof(SelfDestruct), timeAlive);
	}

	private void Update() {
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

	private void SelfDestruct() {
		DetatchParticles();
		Destroy(gameObject);
	}

	void DetatchParticles() {
		foreach(ParticleSystem ps in particleSystems) {
			ps.transform.parent = null;
			ps.Stop();
		}
	}
}

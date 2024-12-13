using UnityEngine;

public class PooledParticle : MonoBehaviour
{
    private ParticleSystem particleSystem;

    public void Initialize()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        particleSystem.Play();
        
        // 파티클 재생이 끝나면 풀로 반환
        float duration = particleSystem.main.duration;
        Invoke("ReturnToPool", duration);
    }

    private void ReturnToPool()
    {
        particleSystem.Stop();
        ObjectManager.Instance.Despawn(this);
    }

    private void OnDisable()
    {
        CancelInvoke();
        if(particleSystem != null)
            particleSystem.Stop();
    }
} 
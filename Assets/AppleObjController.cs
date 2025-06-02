using UnityEngine;
using Mirror;
using System.Collections;
public class AppleObjController : NetworkBehaviour
{
    public bool collectable = false;

    public SkinnedMeshRenderer appleSkin;
    public float growDuration = 0.6f;

    private bool started = false;

    void Update()
    {
        if (!started && isServer) // sadece server başlatır, client'lara RPC ile aktarırız
        {
            started = true;
            StartCoroutine(GrowEffectCoroutine());
            Invoke(nameof(LateActive), growDuration);
        }
    }

    void LateActive()
    {
        collectable = true;
        this.gameObject.layer = 0;
    }

    [ClientRpc]
    void RpcPlayGrowEffect(float totalDuration)
    {
        StartCoroutine(GrowEffectCoroutine(totalDuration));
    }

    IEnumerator GrowEffectCoroutine() => GrowEffectCoroutine(growDuration);

    IEnumerator GrowEffectCoroutine(float totalDuration)
    {
        float elapsed = 0f;

        float firstPhase = totalDuration * 0.6f; // 60% of time to go from 100 to 0
        float secondPhase = totalDuration * 0.4f; // 40% to go from 0 to 10

        // Phase 1: 100 → 0
        while (elapsed < firstPhase)
        {
            float t = elapsed / firstPhase;
            float weight = Mathf.Lerp(100f, 0f, t);
            appleSkin.SetBlendShapeWeight(0, weight);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to exact 0
        appleSkin.SetBlendShapeWeight(0, 0f);

        elapsed = 0f;

        // Phase 2: 0 → 10
        while (elapsed < secondPhase)
        {
            float t = elapsed / secondPhase;
            float weight = Mathf.Lerp(0f, 10f, t);
            appleSkin.SetBlendShapeWeight(0, weight);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to exact 10
        appleSkin.SetBlendShapeWeight(0, 10f);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Start animation on all clients
        RpcPlayGrowEffect(growDuration);
    }
}

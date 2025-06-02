using UnityEngine;
using Mirror;
using System.Collections;

public class AppleObjController : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCollectableChanged))]
    public bool collectable = false;

    public SkinnedMeshRenderer appleSkin;
    public float growDuration = 0.6f;

    private bool animationStarted = false;

    void Start()
    {
        if (isServer)
        {
            StartCoroutine(AppleLifecycle());
        }
    }

    [Server]
    IEnumerator AppleLifecycle()
    {
        // Start grow animation on all clients
        RpcPlayGrowEffect(growDuration);

        // Wait for grow animation to complete
        yield return new WaitForSeconds(growDuration);

        // Make apple collectable
        collectable = true;
        gameObject.layer = 0; // Default layer
    }

    [ClientRpc]
    void RpcPlayGrowEffect(float duration)
    {
        if (!animationStarted)
        {
            animationStarted = true;
            StartCoroutine(GrowEffectCoroutine(duration));
        }
    }

    IEnumerator GrowEffectCoroutine(float totalDuration)
    {
        float elapsed = 0f;
        float firstPhase = totalDuration * 0.6f;
        float secondPhase = totalDuration * 0.4f;

        // Phase 1: 100 → 0
        while (elapsed < firstPhase)
        {
            float t = elapsed / firstPhase;
            float weight = Mathf.Lerp(100f, 0f, t);
            appleSkin.SetBlendShapeWeight(0, weight);
            elapsed += Time.deltaTime;
            yield return null;
        }

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

        appleSkin.SetBlendShapeWeight(0, 10f);
    }

    void OnCollectableChanged(bool oldValue, bool newValue)
    {
        // This will be called on all clients when collectable changes
        collectable = newValue;
        gameObject.layer = newValue ? 0 : LayerMask.NameToLayer("Ignore Raycast");
    }

    /* Optional: If you need to handle collision/trigger differently
    public void OnTriggerEnter(Collider other)
    {
        if (!collectable) return;
        
        // Handle collection logic here
    }
    */
}
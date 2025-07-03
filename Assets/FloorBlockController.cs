using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class FloorBlockController : NetworkBehaviour
{
    GameObject brokenroot;
    GameObject arenablock;

    void Start()
    {
        brokenroot = transform.GetChild(0).gameObject;
        arenablock = transform.GetChild(1).gameObject;
    }

    [Command(requiresAuthority = false)]
    public void CmdBreakBlock()
    {
        //RpcBreakBlock();
    }

    [ClientRpc]
    void RpcBreakBlock()
    {
        arenablock.gameObject.SetActive(false);
        brokenroot.transform.parent.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(breakBlocks());
    }



    [Command(requiresAuthority = false)]
    public void CmdArrowBreakBlock()
    {
      // RpcArrowBreakBlock();
    }

    [ClientRpc]
    void RpcArrowBreakBlock()
    {
        arenablock.gameObject.SetActive(false);
        brokenroot.transform.parent.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(ArrowbreakBlocks());
    }




    IEnumerator ArrowbreakBlocks()
    {

        int n = brokenroot.transform.childCount;

        for (int i = 0; i < n; i++)
        {
            Transform block = brokenroot.transform.GetChild(i);
            Rigidbody rb = block.GetComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 randomDir = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-1.2f, -0.8f),
                Random.Range(-0.3f, 0.3f)
            ).normalized;

            rb.AddForce(randomDir * 5, ForceMode.Impulse);
        }

        // === Scale to zero over 0.5 seconds ===
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3[] originalScales = new Vector3[n];

        for (int i = 0; i < n; i++)
        {
            originalScales[i] = brokenroot.transform.GetChild(i).localScale;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < n; i++)
            {
                Transform block = brokenroot.transform.GetChild(i);
                block.localScale = Vector3.Lerp(originalScales[i], Vector3.zero, t);
            }

            yield return null;
        }

        for (int i = 0; i < n; i++)
        {
            brokenroot.transform.GetChild(i).localScale = Vector3.zero;
        }
    }

    IEnumerator breakBlocks()
    {
        yield return new WaitForSeconds(1);
        int n = brokenroot.transform.childCount;
        for (int i = 0; i < n; i++)
        {
            brokenroot.transform.GetChild(i).transform.localScale -= brokenroot.transform.GetChild(i).transform.localScale / 15;
        }

        yield return new WaitForSeconds(1);
        for (int i = 0; i < n; i++)
        {
            brokenroot.transform.GetChild(i).transform.localScale -= brokenroot.transform.GetChild(i).transform.localScale / 15;
        }

        yield return new WaitForSeconds(1);

        for (int i = 0; i < n; i++)
        {
            Transform block = brokenroot.transform.GetChild(i);
            Rigidbody rb = block.GetComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 randomDir = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-1.2f, -0.8f),
                Random.Range(-0.3f, 0.3f)
            ).normalized;

            rb.AddForce(randomDir * 5, ForceMode.Impulse);
        }

        // === Scale to zero over 0.5 seconds ===
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3[] originalScales = new Vector3[n];

        for (int i = 0; i < n; i++)
        {
            originalScales[i] = brokenroot.transform.GetChild(i).localScale;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < n; i++)
            {
                Transform block = brokenroot.transform.GetChild(i);
                block.localScale = Vector3.Lerp(originalScales[i], Vector3.zero, t);
            }

            yield return null;
        }

        for (int i = 0; i < n; i++)
        {
            brokenroot.transform.GetChild(i).localScale = Vector3.zero;
        }
    }
}

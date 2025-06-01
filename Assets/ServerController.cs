using Mirror;
using UnityEngine;

public class ServerController : NetworkBehaviour
{
    public static ServerController Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (isClient)
        {
            Debug.Log("client aktif");
        }
        if (isServer)
        {
            Debug.Log("server aktif");
        }
    }
    // parenting .. 
    private void OnEnable()
    {
        if (isClient)
        {
            Debug.Log("client aktif");
        }
        if (isServer)
        {
            Debug.Log("server aktif");
        }
    }
    public void MirrorSetParent(GameObject child, Transform exactParent)
    {
        if (child == null || exactParent == null)
        {
            Debug.LogError("Child veya parent null olamaz!");
            return;
        }

        NetworkIdentity childIdentity = child.GetComponent<NetworkIdentity>();
        NetworkIdentity parentIdentity = exactParent.GetComponentInParent<NetworkIdentity>();

        if (!isServer)
        {
            // Client ise servera tam parent yolunu gönder
            CmdSetParentWithPath(
                childIdentity,
                parentIdentity,
                GetRelativePath(parentIdentity.transform, exactParent)
            );
            return;
        }

        // Server tarafýnda direkt parentla
        SetParentInternal(child.transform, exactParent);

        // Clientlara tam konum bilgisini gönder
        if (childIdentity != null && parentIdentity != null)
        {
            RpcSyncParentExact(
                childIdentity,
                parentIdentity,
                GetRelativePath(parentIdentity.transform, exactParent)
            );
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSetParentWithPath(
        NetworkIdentity child,
        NetworkIdentity rootParent,
        string relativePath)
    {
        Transform exactParent = FindExactParent(rootParent.transform, relativePath);
        if (exactParent != null)
        {
            SetParentInternal(child.transform, exactParent);
            RpcSyncParentExact(child, rootParent, relativePath);
        }
    }

    [ClientRpc]
    private void RpcSyncParentExact(
        NetworkIdentity child,
        NetworkIdentity rootParent,
        string relativePath)
    {
        if (isServer || child == null || rootParent == null) return;

        Transform exactParent = FindExactParent(rootParent.transform, relativePath);
        if (exactParent != null)
        {
            SetParentInternal(child.transform, exactParent);
        }
    }

    // Yardýmcý Metot: Transform hiyerarþisinde tam yolu bulur
    private string GetRelativePath(Transform root, Transform target)
    {
        if (root == target) return "";

        System.Text.StringBuilder path = new System.Text.StringBuilder();
        Transform current = target;

        while (current != null && current != root)
        {
            if (path.Length > 0) path.Insert(0, "/");
            path.Insert(0, current.name);
            current = current.parent;
        }

        return path.ToString();
    }

    // Yardýmcý Metot: Yolu kullanarak tam parentý bulur
    private Transform FindExactParent(Transform root, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return root;

        Transform current = root;
        string[] pathParts = relativePath.Split('/');

        foreach (string part in pathParts)
        {
            current = current?.Find(part);
            if (current == null) break;
        }

        return current;
    }

    private void SetParentInternal(Transform child, Transform parent)
    {
        child.SetParent(parent);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
    }


    // spawning
  /*  public void RequestSpawnAndParent(string prefabName, NetworkIdentity parentPlayer)
    {
        if (!isServer)
        {
            CmdRequestSpawn(prefabName, parentPlayer);
            return;
        }

        SpawnAndParentObject(prefabName, parentPlayer);
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestSpawn(string prefabName, NetworkIdentity parentPlayer)
    {
        SpawnAndParentObject(prefabName, parentPlayer);
    }
  */
    /*[Server]
    private void SpawnAndParentObject(string prefabName, NetworkIdentity parentPlayer)
    {
        if (!isServer) return;

        GameObject prefab = NetworkManager.singleton.spawnPrefabs.Find(x => x.name == prefabName);

        if (prefab == null)
        {
            Debug.LogError($"Prefab {prefabName} not found in NetworkManager spawnable prefabs!");
            return;
        }

        GameObject spawnedObj = Instantiate(prefab);
        NetworkServer.Spawn(spawnedObj);

        NetworkIdentity spawnedIdentity = spawnedObj.GetComponent<NetworkIdentity>();
        Transform parentTransform = parentPlayer.GetComponent<N_ChefController>().ChefHandPoint.transform;

        spawnedObj.transform.SetParent(parentTransform);
        spawnedObj.transform.localPosition = Vector3.zero;
        spawnedObj.transform.localRotation = Quaternion.identity;

        RpcSetParent(spawnedIdentity, parentPlayer);
    }

    [ClientRpc]
    private void RpcSetParent(NetworkIdentity spawnedObject, NetworkIdentity parentPlayer)
    {
        if (spawnedObject == null || parentPlayer == null || isServer) return;

        Transform parentTransform = parentPlayer.GetComponent<N_ChefController>().ChefHandPoint.transform;

        spawnedObject.transform.SetParent(parentTransform);
        spawnedObject.transform.localPosition = Vector3.zero;
        spawnedObject.transform.localRotation = Quaternion.identity;
    }

    */
    // setactive

    public void ElementActive(GameObject targetObject, bool isActive)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is null!");
            return;
        }

        NetworkIdentity identity = targetObject.GetComponent<NetworkIdentity>();

        if (identity != null)
        {
            // Eðer obje bir NetworkIdentity'ye sahipse, að üzerinden güncelle
            if (isServer)
            {
                RpcSetActive(targetObject, isActive);
            }
            else
            {
                CmdSetActive(targetObject, isActive);
            }
        }
        else
        {
            // Eðer obje bir NetworkIdentity'ye sahip deðilse, sadece yerel olarak güncelle
            targetObject.SetActive(isActive);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSetActive(GameObject targetObject, bool isActive)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is null in CmdSetActive!");
            return;
        }

        targetObject.SetActive(isActive);
        RpcSetActive(targetObject, isActive);
    }

    [ClientRpc]
    private void RpcSetActive(GameObject targetObject, bool isActive)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is null in RpcSetActive!");
            return;
        }

        targetObject.SetActive(isActive);
    }


}

using UnityEngine;
using Mirror;
public class OnlinePrefabController : NetworkBehaviour
{
    public bool _server;
    public bool _local;

    public GameObject CameraHolder;
    public GameObject TPSCamera;

    void Start()
    {
        if (isLocalPlayer)
            _local = true;
        else
            _local = false;

        if (isServer)
            _server = true;
        else
            _server = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

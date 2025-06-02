using UnityEngine;
using Mirror;

public class AppleGameSceneSpawner : NetworkBehaviour
{
    public GameObject appleobj; // Network prefab (NetworkIdentity olmalý)
    public GameObject referanceplatforn; // Platform (MeshRenderer'ý olmalý)
    public Terrain referanceTerrain; // Terrain objesi

    public float spawnInterval = 2.7f;
    private float timer;

    void Update()
    {
       

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnApple();
        }
    }

    [Server]
    void SpawnApple()
    {
        if (appleobj == null || referanceplatforn == null || referanceTerrain == null)
            return;

        // Platform boyutlarýný al
        MeshRenderer rend = referanceplatforn.GetComponent<MeshRenderer>();
        if (rend == null) return;

        Vector3 platformSize = rend.bounds.size;
        Vector3 platformCenter = rend.bounds.center;

        // Platform alaný içinde rastgele X-Z pozisyonu oluþtur
        float x = Random.Range(platformCenter.x - platformSize.x / 2f, platformCenter.x + platformSize.x / 2f);
        float z = Random.Range(platformCenter.z - platformSize.z / 2f, platformCenter.z + platformSize.z / 2f);

        // Terrain yüksekliðini al
        float y = referanceTerrain.SampleHeight(new Vector3(x, 0, z)) + referanceTerrain.GetPosition().y;

        Vector3 spawnPosition = new Vector3(x, y + 0.5f, z); // Yüksekliði biraz yükseltiyoruz (0.5f)

        GameObject spawnedApple = Instantiate(appleobj, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(spawnedApple); // Network üzerinden spawnla
    }
}

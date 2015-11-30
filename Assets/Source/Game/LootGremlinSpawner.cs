using UnityEngine;
using System.Collections;

public class LootGremlinSpawner : MonoBehaviour {

    /** X = minimum time b/w spawns, Y = max time */
    [SerializeField]
    public Vector2 spawnRate;

    /** How much is the spawn time biased towards the end of the spawn period */
    [SerializeField]
    private float spawnRateBias;

    [SerializeField]
    public Vector2 spawnRange;

    [SerializeField]
    public GameObject gremlinPrototype;


    // Should gremlins be spawned right now
    private bool bDoSpawns;

    // Where the bag is located so that the gremlins will go towards it
    private Vector3 bagLocation;

    // the actuall dropped bag object reference: can be null
    private DroppedLootBag lootBag;

    private float lastSpawnTime;

    public bool IsDoingSpawns() { return bDoSpawns; }

	private void Start() {
	
	}


	private void Update() {
        bDoSpawns = ShouldDoSpawns();

        if(bDoSpawns && lootBag != null) {
            bagLocation = lootBag.transform.position;

            float sinceLastSpawn = Time.time - lastSpawnTime;
            if(sinceLastSpawn > spawnRate.y) {
                SpawnGremlin();
            }
            else if(sinceLastSpawn > spawnRate.x) {
                // Minimum time has passed, decide if it should do a spawn. Likelyhood increases over the valid spawn period
                float spawnScore = ((sinceLastSpawn - spawnRate.x) / (spawnRate.y - spawnRate.x)) * spawnRateBias;
                float randomScore = Random.Range(0.0f, 1.0f);

                if(randomScore < spawnScore) {
                    SpawnGremlin();
                }
            }
        }
        else {
            lastSpawnTime = Time.time;
        }
	}


    // Is the bag dropped right now, so that gremlins should be spawned
    private bool ShouldDoSpawns() {
        Player player = WorldManager.instance.GetActivePlayer();
        lootBag = player.GetDroppedLootBag();
        return !player.IsCarryingLootBag();
    }


    public void SpawnGremlin() {
        if(gremlinPrototype == null) {
            Debug.LogWarning("LootGremmlinSpawner::SpawnGremlin --- Gremlin Prototype is not set! Cannot do spawns!");
            return;
        }

        // Find a location within the range of the loot bag
        float distance = Random.Range(spawnRange.x, spawnRange.y);
        Vector2 direction = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        Vector3 spawnLocation = bagLocation + (Vector3)direction * distance;

        float left = 0.0f, right = 0.0f, top = 0.0f, bottom = 0.0f;
        WorldManager.instance.GetWorldBounds(ref left, ref right, ref top, ref bottom);

        if(spawnLocation.x < left) {
            spawnLocation.x = left + 1.0f;
        }

        if(spawnLocation.y < bottom) {
            spawnLocation.y = bottom + 1.0f;
        }

        if(spawnLocation.y > top) {
            spawnLocation.y = top - 1.0f;
        }

        LootGremlin gremlin = (LootGremlin)WorldManager.instance.SpawnBot(gremlinPrototype, spawnLocation);
        lastSpawnTime = Time.time;

        gremlin.DoSpawnEvent();
    }
}
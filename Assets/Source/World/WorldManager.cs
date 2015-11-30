using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour {

    /** Singleton reference to the main world manage: there will only ever be one */
    public static WorldManager instance;

	private static string saveLocation = "save";

	[SerializeField]
	private Player playerPrototype;

	[SerializeField]
	private Light sunPrototype;

	[SerializeField] // in seconds
	private float dayLength = 20.0f;

	[SerializeField]
	private Color daylightColor;

	[SerializeField]
	private Color sunsetColor;

    [SerializeField] /* Maanges all the commonly used assorted textures that cannot be assigned to an object in editor: like items an gui textures */
    public TextureLoader textureManager;

	[SerializeField] /** List of objects to that can be spawned by abilities: added to a big lookup dictionary */
	private List<GameObject> spawnableObjects;

    [SerializeField] /** Background layers to spawn at game start */
    private List<ParallaxBackground> backGroundSet;

    [SerializeField]
    public bool bFirstChunkSpawnsEnemies = false;

	private Dictionary<string, GameObject> abilityObjectMap;

	private float timeOfDay;
	private float maxSunIntensity;
	private float minSunIntensity = 0.0f;

	private GameStateUtility gameState;

	/** Main player character that the world is focused around */
	private Player playerCharacter;
    private Vector3 playerStartLocation;
    private TerrainChunk relevantChunk;

	/** Master directional light that controlls overall light level */
	private Light sunLight;

	/** Master world generator that creates and populates chunks as the palyer movs around */
	private WorldGenerator worldGenerator;

    /** manages the spawning of loot gremlins when the loot bag is dropped */
    private LootGremlinSpawner lootGremlinSpawnManager;

    private UI_Manager uiManager;

    private Diety[] dieties;
    private const int NUM_DIETIES = 3;

	/** Is the game currently in combat mode, pausing all auto updates */
	private bool bCombatPause;

    /** List that pawns insert themselves into when they are visible in the main camera */
    private List<Pawn> pawnsOnScreen;

    private ObjectPool objectPool;

    /** A global list of all crafting items loaded in from an XML file */
    private ItemUtility craftingItemState;
    private string craftingItemFile = "CraftingItems";

    private ItemUtility wearableItemState;
    private string wearableItemFile = "WearableItems";

    /** Class for generating procedural names and other types of text */
    private ProceduralTextUtility textUtility;
    private string textSourceFile = "ProceduralText";

	public void CombatPause() { bCombatPause = true; }
	public bool GetIsCombatPaused() { return bCombatPause; }
    public List<Pawn> GetVisiblePawns() { return pawnsOnScreen; }
    public Player GetActivePlayer() { return playerCharacter; }
    public float GetTimeOfDay() { return timeOfDay; }
    public Diety[] GetDieties() { return dieties; }
    public LootGremlinSpawner GetGremlinManager() { return lootGremlinSpawnManager; }
    public UI_Manager GetUIManager() { return uiManager; }

	void OnEnable () {
        // First thing is to se the singleton reference, many things will use this, and this object spawns pretty much everything
        instance = this;

        if(textureManager == null) {
            Debug.LogError("WorldManager Does not have a texture manager assigned! Please add one as a child and assign it to the world manager in the inspector!");
        }

        // Start by loading all data 
        craftingItemState = ItemUtility.LoadCraftingItems(craftingItemFile);
        if(craftingItemState == null) {
            Debug.Log("ERROR! No crafting items found at " + craftingItemFile);
        }

        wearableItemState = ItemUtility.LoadWearableItems(wearableItemFile);
        if(wearableItemState == null) {
            Debug.Log("ERROR! No wearable items found at " + wearableItemFile);
        }

        textUtility = ProceduralTextUtility.LoadTextTypes(textSourceFile);
        if(textUtility == null) {
            Debug.Log("ERROR! No procedural text source file found at " + textSourceFile);
        }

        dieties = new Diety[NUM_DIETIES];
        for(int i = 0; i < NUM_DIETIES; i++) {
            string prefix = "";
            string title = "";
            string suffix = "";

            textUtility.GenerateCompleteName(ref prefix, ref title, ref suffix);

            Diety d = new Diety(prefix, title, suffix);
            dieties[i] = d;
        }

        // Next grabs the world generator, which should be attached to the world manager object
		worldGenerator = GetComponent<WorldGenerator>();
		if(worldGenerator == null) {
			Debug.LogWarning("WARNING! No world generator found on "+name+". No terrain will be created!");
		}

        // Grab the gramlin manager: this isnt suuuuper critical, but the game really doesnt work without it
        lootGremlinSpawnManager = GetComponent<LootGremlinSpawner>();
        if(worldGenerator == null) {
            Debug.LogWarning("WARNING! No LootGremlinSpawner found on " + name + ". No Loot gremlins will be spawned!");
        }

        uiManager = new UI_Manager();

        objectPool = new ObjectPool();

		sunLight = (Light)Instantiate(sunPrototype, new Vector3(0.0f, 0.0f, -10.0f), Quaternion.identity);
		maxSunIntensity = sunLight.intensity;

		// attempt to load game state, if it exists, otherwise create a new one
		gameState = GameStateUtility.LoadGameState(saveLocation);
		if(gameState == null) {
			gameState = new GameStateUtility();
			GameStateUtility.SaveGameState(saveLocation, gameState);
		}

		if(playerPrototype == null) {
			Debug.LogError("Player prototype is not set, cannot initialize game!");
			return;
		}
        playerStartLocation = worldGenerator.GeneratorOrigin();
		playerCharacter = (Player)Instantiate(playerPrototype, playerStartLocation, Quaternion.identity);

		// Assign the state that was loaded
		if(gameState.playerState != null) {
			playerCharacter.SetState(gameState.playerState);
		}
		else {
			Debug.Log("ERROR! Player state was not loaded / created in the game state!");
		}

		if(worldGenerator != null) {
			worldGenerator.StartGeneratingWorld(playerCharacter);
		}

        // Instaniate all of the background layers from prototypes: they are self managing
        foreach(ParallaxBackground bg in backGroundSet) {
            Instantiate(bg.gameObject);
        }

		// Fill out the ability object map with the editor assigned objects, mapped to names
		abilityObjectMap = new Dictionary<string, GameObject>();
		foreach(GameObject obj in spawnableObjects) {
			if(obj != null) {
				abilityObjectMap.Add(obj.name, obj);
			}
		}

        pawnsOnScreen = new List<Pawn>();
	}


	void Update () {
		timeOfDay += Time.deltaTime;
		float dayProgress = timeOfDay / dayLength;
		float dayPeriod = (Mathf.PI * 2.0f) * dayProgress;
		float intensityPct = (Mathf.Cos(dayPeriod) + 1.0f) / 2.0f;

		sunLight.intensity = (intensityPct * maxSunIntensity) + minSunIntensity;

		Vector3 dayColor = new Vector3(daylightColor.r, daylightColor.g, daylightColor.b);
		Vector3 setColor = new Vector3(sunsetColor.r, sunsetColor.g, sunsetColor.b);

		Vector3 interpColor = Vector3.Lerp(dayColor, setColor, 1.0f - intensityPct);

		Color currentColor = new Color(interpColor.x, interpColor.y, interpColor.z);
		sunLight.color = currentColor;

		if(timeOfDay > dayLength) {
			StartOfNewDay();
		}

        // Which chunk is the player currently in
        foreach(TerrainChunk chunk in worldGenerator.ChunkList()) {
            if(chunk.ContainsPoint(playerCharacter.transform.position)) {
                if(chunk != relevantChunk) {
                    chunk.BecomeRelevant();
                    if(chunk.GetRightChunk() != null) {
                        chunk.GetRightChunk().BecomeRelevant();
                    }
                }

                relevantChunk = chunk;
                break;
            }
        }
	}

	private void StartOfNewDay() {
		timeOfDay = 0.0f;
	}

	// Cache all important game state and save it to XML
	public void SaveGame() {
		playerCharacter.CachePlayerState();
		gameState.timeOfDay = timeOfDay;

		GameStateUtility.SaveGameState(saveLocation, gameState);
	}


	// Gets a prototype object from the avilable list, and gets a new instance from the object pool. Returns null if it cant find a prototype
	public GameObject SpawnObject(string objectName, Vector3 location) {
        GameObject objPrototype;
        if(abilityObjectMap.TryGetValue(objectName, out objPrototype)) {
            GameObject obj = objectPool.GetInactiveGameObjectInstance(objPrototype);
            if(obj != null) {
                obj.transform.position = location;
                obj.SetActive(true);

                return obj;
            }
        }

        return null;
	}


    public BotBase SpawnBot(GameObject botPrototype, Vector3 location) {
        if(botPrototype.GetComponent<BotBase>() == null) {
            Debug.Log("WARNING! Tried to spawn bot object with no bot component: "+botPrototype.name);
            return null;
        }

        GameObject obj = objectPool.GetInactiveGameObjectInstance(botPrototype);
        if(obj != null) {
            obj.transform.position = location;
            obj.SetActive(true);

            BotBase bot = obj.GetComponent<BotBase>();
            return bot;
        }
        
        return null;
    }


    public void PawnBecameVisible(Pawn newPawn) {
        if(!pawnsOnScreen.Contains(newPawn)) {
            pawnsOnScreen.Add(newPawn);
            
            BotBase bot = newPawn.GetComponent<BotBase>();
            if(bot != null) {
                bot.Wakeup();
            }
        }
    }

    public void PawnBecameInvisible(Pawn oldPawn) {
        if (pawnsOnScreen.Contains(oldPawn)) {
            pawnsOnScreen.Remove(oldPawn);

            BotBase bot = oldPawn.GetComponent<BotBase>();
            if(bot != null) {
                bot.SetAsleep();
            }
        }
    }

    // Find the next visible pawn after the input one
    public Pawn GetVisiblePawnAfter(Pawn currentPawn) {
        if(pawnsOnScreen.Count == 0) {
            return null;
        }

        int curIdx = pawnsOnScreen.IndexOf(currentPawn);
        if(curIdx >= 0) {
            // Found the input pawn, try to return the next one
            if(curIdx + 1 >= pawnsOnScreen.Count) {
                curIdx = 0;
            }

            return (pawnsOnScreen.Count > 1) ? pawnsOnScreen[curIdx + 1] : pawnsOnScreen[0];
        }
        else {
            return pawnsOnScreen[0];
        }
    }


    public IEnumerable<Pawn> VisiblePawns() {
        for (int i = 0; i < pawnsOnScreen.Count; i++) {
            yield return pawnsOnScreen[i];
        }
    }


    // Called by pawns when they take damage
    public void DamageEvent(float dmgAmount, Pawn victim, Pawn instigator) {
        playerCharacter.NotifiedDamageEvent(dmgAmount, victim, instigator);
    }


    // Called by pawns when they die
    public void DeathEvent(Pawn killed) {
        if(killed == playerCharacter) {
            StartCoroutine("RespawnPlayer");
        }
    }


    private IEnumerator RespawnPlayer() {
        const float respawnTime = 1.0f;
        float timeSinceStart = 0.0f;

        while(timeSinceStart < respawnTime) {
            timeSinceStart += Time.deltaTime;
            yield return null;
        }

        playerCharacter.transform.position = playerStartLocation;
        playerCharacter.Revive();
    }



    public CraftingItem[] GetAllCraftingItems() {
        return (craftingItemState != null) ? craftingItemState.craftingItems : null;
    }

    // Try to find a crafting item with the input name: can return null
    public CraftingItem GetCraftingItemByName(string desiredName) {
        if(craftingItemState == null) {
            return null;
        }

        CraftingItem[] allItems = craftingItemState.craftingItems;
        for(int i = 0; i < allItems.Length; i++ ) {
            if(allItems[i].craftingItemName == desiredName) {
                return allItems[i];
            }
        }

        return null;
    }


    public WearableItem[] GetAllWearableItems() {
        return (wearableItemState != null) ? wearableItemState.wearableItems : null;
    }

    // Try to find a wearable item with the input name: can return null
    public WearableItem GetWearableItemByName(string desiredName) {
        if(wearableItemState == null) {
            return null;
        }

        WearableItem[] allItems = wearableItemState.wearableItems;
        for(int i = 0; i < allItems.Length; i++) {
            if(allItems[i].wearableItemName == desiredName) {
                return allItems[i];
            }
        }

        return null;
    }



    // Should the input enemy spawn in a swarm
    public static bool EnemyDoesSwarmSpawn(BotBase bot) {
        if(bot == null) {
            return false;
        }

        System.Type botType = bot.GetType();

        // Add any other evaluations here
        if(botType == typeof(Spider)) {
            return true;
        }

        return false;
    }


    public void GetWorldBounds(ref float left, ref float right, ref float top, ref float bottom) {
        left = 0.0f;
        right = 1000000.0f; // TODO: determine this by the number of chunks
        top = WorldGenerator.CHUNK_HALF_HEIGHT * 2.0f;
        bottom = 0.0f;
    }

}

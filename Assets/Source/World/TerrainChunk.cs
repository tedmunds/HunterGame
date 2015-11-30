using UnityEngine;
using System.Collections.Generic;

public class TerrainChunk : MonoBehaviour {

    public const int NUM_BIOME_TYPES = 3;
	public enum Biome {
		Grassland, 
		Snow,
        Corrupted,
		//Ice,
		//Tundra
	}

	/** NOTE: These lits must be in the same order as the biomes!!!!! */
	[SerializeField]
	private List<Material> terrainMaterials;

	[SerializeField]
	private List<GameObject> treePrototypes;

	[SerializeField] // TODO: make a biome diversity object that can be configured in editor that holds multiple creature types
	private List<Pawn> creatureTypes;

	private Vector2[] polygon;
	private int[] triIndices;
	private Vector2 centroid;
	private int numTris;

	private Biome chunkBiome;

    /** The poi (if any) that is on this chunk: likely to be null in many cases */
    private PointOfInterest myPoi;

    /** Index is in chunk generation sequence */
    private int index;

    /** If these chunks are linear, then they will also create a linked list of chunks from left to right */
    private TerrainChunk previousChunk;
    private TerrainChunk nextChunk;

    /** How difficult or advanced is this chunk. 0 to 100 range */
    private int difficultyRating;

    /** Flag set when this chunk populates enemies, if it is off and it becomes relevant it will populate */
    private bool bHasPopulatedEnemies;

    /** List of all trees spawned in this biome */
    private List<GameObject> treesList;

    public void SetLeftChunk(TerrainChunk prev) { previousChunk = prev; }
    public void SetRightChunk(TerrainChunk next) { nextChunk = next; }

    public TerrainChunk GetLeftChunk() { return previousChunk; }
    public TerrainChunk GetRightChunk() { return nextChunk; }
    public int GetIndex() { return index; }

	public void Initialize(int chunkIdx, Vector2[] createdPoly, int[] indices) {
        treesList = new List<GameObject>();
        
        index = chunkIdx;
        polygon = createdPoly;
		triIndices = indices;
		centroid = createdPoly[0];
		numTris = triIndices.Length / 3;

		// Decide on biome
        if(index == 0 || index == 1) {
            // The first and second chunks are always grasslands
            chunkBiome = Biome.Grassland;
        }
        else {
            // TODO: weight the biome selection to the index (lower index = lower biome type? then order biomes correspondingly)
            chunkBiome = (Biome)Random.Range(0, NUM_BIOME_TYPES);
        }
        

		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
		if(renderer != null) {
			renderer.material = terrainMaterials[(int)chunkBiome];
		}
		else {
			Debug.Log("ERROR! Chunk could not find mesh renderer!");
		}

		PopulateTrees();
		//PopulateCreatures();
	}

	public Vector2[] GetPolygon() { return polygon; }
	public int[] GetTriIndices() { return triIndices; }
	public int NumTriangles() { return numTris; }
	public Vector2 GetCenter() { return centroid; }

    /**
     * Called when this chunk becomes active 
     */ 
    public void BecomeRelevant() {
        // First chunk probably doesnt want any creatures, but well allow for the possibility
        if(!bHasPopulatedEnemies && (index != 0 || WorldManager.instance.bFirstChunkSpawnsEnemies)) {
            PopulateCreatures();
        }
        
        bHasPopulatedEnemies = true;
    }


	// Adds trees for the biome type
	private void PopulateTrees() {
		int maxTress = 30;
		int treesToAdd = Random.Range(25, maxTress);

		for(int i = 0; i < treesToAdd; i++) {
            Vector2 treePos = RandomPointInChunkRect();
			
			// Add the tree
			GameObject treeObj = (GameObject)Instantiate(treePrototypes[(int)chunkBiome], treePos, Quaternion.identity);
            treesList.Add(treeObj);
		}
	}

	private void PopulateCreatures() {
		int maxCreatures = 10;
        const int minCreatures = 5;
        int creaturesToAdd = Random.Range(minCreatures, maxCreatures);

		for(int i = 0; i < creaturesToAdd; i++) {
            Vector2 spawnLoc = RandomPointInChunkRect();
			SpawnCreature(spawnLoc);
		}
	}


	private void SpawnCreature(Vector3 location) {
		// TODO: randomly decide what creture to spawn from the diversity object for this biome
		//Pawn spawnedCreature = (Pawn)Instantiate(creatureTypes[(int)chunkBiome], location, Quaternion.identity);
        Pawn spawnedCreature = WorldManager.instance.SpawnBot(creatureTypes[(int)chunkBiome].gameObject, location);
		
        // Check for a swarm spawn, in which case it will spawn several more enemies around it
        if(WorldManager.EnemyDoesSwarmSpawn((BotBase)spawnedCreature)) {
            int swarmSize = Random.Range(BotBase.MIN_SWARM_SIZE, BotBase.MAX_SWARM_SIZE);
            for(int i = 0; i < swarmSize; i++) {
                float offset = Random.Range(0.5f, 2.0f);
                Vector3 swarmLoc = location + (Vector3)Random.insideUnitCircle * offset;

                //Instantiate(creatureTypes[(int)chunkBiome], swarmLoc, Quaternion.identity);
                WorldManager.instance.SpawnBot(creatureTypes[(int)chunkBiome].gameObject, swarmLoc);
            }
        }
	}


    // Spawn a given type of poi in this chunk: suppliued by the world manager since poi are more relevant to it 
    public void AddPointOfInterest(PointOfInterest poiType) {
        float spawnRange = 0.5f;
        float posWidthRatio = Random.Range(-spawnRange, spawnRange);
        float xPos = WorldGenerator.CHUNK_HALF_WIDTH * posWidthRatio + centroid.x;

        Vector3 spawnLoc = new Vector3(xPos, centroid.y + WorldGenerator.CHUNK_HALF_HEIGHT, 0.0f);
        myPoi = (PointOfInterest)Instantiate(poiType, spawnLoc, Quaternion.identity);

        // remove any trees that are too near th poi and may block it visually
        if(treesList.Count > 0) {
            float minObscureDistance = 1.5f;

            for(int i = treesList.Count - 1; i >= 0; i--) {
                if((treesList[i].transform.position - myPoi.transform.position).magnitude < minObscureDistance) {
                    GameObject tempObj = treesList[i];
                    treesList.RemoveAt(i);

                    Destroy(tempObj);
                }
            }

        }
    }


    public TerrainChunk GetRightmostChunk() {
        if(nextChunk == null) {
            return this;
        }
        else {
            return nextChunk.GetRightmostChunk();
        }
    }


	/******************************************* Polygon Utilities ***********************************************/

	/** Gets a totally random point that is almost garunteed to be in the chunk */
	public Vector2 RandomPointInChunk(TerrainChunk chunk) {
		int segmentIdx = Random.Range(0, chunk.NumTriangles());
		Vector2[] tri = chunk.GetTriangle(segmentIdx);
		if(tri == null) {
			return chunk.GetCenter();
		}
		
		// this 0 - 0.5 range actually seems to give the most even distribution, with some nice natural looking clumping
		float posA = Random.Range(0.0f, 0.5f);
		float posB = Random.Range(0.0f, 0.5f);
		Vector2 mirroredPoint = tri[0] + posA * (tri[1] - tri[0]) + posB * (tri[2] - tri[0]);
		
		Vector2 invertedV0 = tri[0] + (tri[1] - tri[0]) + (tri[2] - tri[0]);
		Vector2 pointInTri = tri[0] - (mirroredPoint - invertedV0);
		
		bool bInTriAlready = PointInTriangle(tri, mirroredPoint);
		Vector2 finalPos =  bInTriAlready? mirroredPoint : pointInTri;
		
		return finalPos;
	}

    public Vector2 RandomPointInChunkRect() {
        float xOffset = Random.Range(-WorldGenerator.CHUNK_HALF_WIDTH, WorldGenerator.CHUNK_HALF_WIDTH);
        float yOffset = Random.Range(-WorldGenerator.CHUNK_HALF_HEIGHT, WorldGenerator.CHUNK_HALF_HEIGHT);

        return centroid + new Vector2(xOffset, yOffset);
    }


    public bool ContainsPoint(Vector3 pos) {
        if((pos.x >= centroid.x - WorldGenerator.CHUNK_HALF_WIDTH && pos.x <= centroid.x + WorldGenerator.CHUNK_HALF_WIDTH) &&
           (pos.y >= centroid.y - WorldGenerator.CHUNK_HALF_HEIGHT && pos.y <= centroid.y + WorldGenerator.CHUNK_HALF_HEIGHT)) {
               return true;
        }

        return false;
    }

	
	private bool PointInTriangle(Vector2[] tri2D, Vector2 point) {
		if(SameSide(point, tri2D[0], tri2D[1], tri2D[2]) && 
		   SameSide(point, tri2D[1], tri2D[0], tri2D[2]) && 
		   SameSide(point, tri2D[2], tri2D[0], tri2D[1])){
			return true;
		}
		return false;
	}
	
	private bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b) {
		Vector3 cp1 = Vector3.Cross(b - a, p1 - a);
		Vector3 cp2 = Vector3.Cross(b - a, p2 - a);
		if(Vector3.Dot(cp1, cp2) >= 0.0f) {
			return true;
		}
		return false;
	}

	/** Itereates through all of the triangles that make up this terrain chunks polygon */
	public System.Collections.Generic.IEnumerable<Vector2[]> Triangles() {
		Vector2[] triangle = new Vector2[3];
		int triIndex = 0;

		for(int i = 0; i < triIndices.Length; i++) {
			triangle[triIndex] = polygon[triIndices[i]];
			triIndex++;
			if(triIndex == 3) {
				yield return triangle;
				triIndex = 0;
			}
		}
	}


	public Vector2[] GetTriangle(int idx) {
		if(idx > numTris) {
			return null;
		}

		Vector2[] tri = new Vector2[3];
		int indicesIdx = idx * 3;

		tri[0] = polygon[triIndices[indicesIdx]];
		tri[1] = polygon[triIndices[indicesIdx+1]];
		tri[2] = polygon[triIndices[indicesIdx+2]];

		return tri;
	}


	public Vector2[] GetEdgesFacingPoint(Vector2 point) {
		List<Vector2> visiblePoints = new List<Vector2>();

		float seperation = (point - centroid).magnitude;
        if(seperation > 70.0f) {
            return visiblePoints.ToArray();
        }


		for(int i = 1; i < polygon.Length; i += 1) {
			// Next vertex index
			int j = i+1;
			if(j >= polygon.Length) {
				j = 1;
			}

			Vector2 edgeMid = polygon[i] + (polygon[j] - polygon[i])/2.0f;
			Vector2 testDirection = (edgeMid - point);

			// check if this line crosses an odd number of other edges
			int numEdgesCrossed = 0;

			for(int testI = 0; testI < polygon.Length; testI += 1) {
				// Next vertex index
				int testJ = testI+1;
				if(testJ >= polygon.Length) {
					testJ = 0;
				}

				if(LineIntersection(point, point + testDirection, polygon[testI], polygon[testJ]))  {
					numEdgesCrossed++;
				}
			}

			if(numEdgesCrossed <= 1) {
				if(!visiblePoints.Contains(polygon[i]) && (point - polygon[i]).magnitude < seperation) {
					visiblePoints.Add(polygon[i]);
				}
				if(!visiblePoints.Contains(polygon[j]) && (point - polygon[j]).magnitude < seperation) {
					visiblePoints.Add(polygon[j]);
				}
			}
		}

		return visiblePoints.ToArray();
	}


	public static bool LineIntersection( Vector2 p1,Vector2 p2, Vector2 p3, Vector2 p4) {
		float Ax, Bx, Cx, Ay, By, Cy, d, e, f /*,offset*/;
		float x1lo, x1hi, y1lo, y1hi;

		Ax = p2.x - p1.x;
		Bx = p3.x - p4.x;

		// X bound box test
		if(Ax < 0) {
			x1lo = p2.x;
			x1hi = p1.x;
		} 
		else {
			x1hi = p2.x; 
			x1lo = p1.x;
		}

		if(Bx > 0) {
			if(x1hi < p4.x || p3.x < x1lo) return false;
		}
		else {
			if(x1hi < p3.x || p4.x < x1lo) return false;
		}

		Ay = p2.y - p1.y;
		By = p3.y - p4.y;

		// Y bound box test//
		if(Ay < 0) {                  
			y1lo = p2.y; 
			y1hi = p1.y;
		} 
		else {
			y1hi = p2.y; 
			y1lo = p1.y;
		}

		if(By > 0) {
			if(y1hi < p4.y || p3.y < y1lo) return false;
		} 
		else {
			if(y1hi < p3.y || p4.y < y1lo) return false;
		}

		Cx = p1.x - p3.x;
		Cy = p1.y - p3.y;
		d = By * Cx - Bx * Cy;  // alpha numerator
		f = Ay * Bx - Ax * By;  // both denominator

		// alpha tests
		if(f > 0) {
			if(d < 0 || d > f) return false;
		} 
		else {
			
			if(d > 0 || d < f) return false;
		}

		e = Ax * Cy - Ay * Cx;  // beta numerator

		// beta tests
		if(f > 0) {   
			if(e < 0 || e > f) return false;
		}
		else {
			if(e > 0 || e < f) return false;
		}
		// check if they are parallel
		if(f == 0) return false;

		return true;
	}



}

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(WorldManager))]
public class WorldGenerator : MonoBehaviour {

    // deprecated
    private const int NUM_CHUNKS = 12;
    private const float MIN_CHUNK_SPACING = 25.0f;
    private const float BASE_CHUNK_SPACING = 30.0f;
    private const float MAX_CHUNK_SPACING = 50.0f;
    // --------------

    // chunk constants
    public const float CHUNK_HALF_WIDTH = 35.0f;
    public const float CHUNK_HALF_HEIGHT = 8.0f;
    public const bool DO_RANDOM_CHUNK_EDGES = true;
    public const int NUM_EDGE_SEGMENTS = 8;
    public const float CHUNK_EDGE_MAG = 1.0f;

	[SerializeField]
	private TerrainChunk chunkPrototype;
	
	[SerializeField]
	private float textureScale = 16.0f;

    [SerializeField]
    private List<PointOfInterest> pointsOfInterest;

	// Turns out this is mostly managed in the vertex generators delta rotation per vert
	private int minVertsPerChunk = 100;
	private int maxVertsPerChunk = 100;

	private Player player;

	/** Flags whther or not the generator is currently producing chunks */
	private bool bDoGeneration = false;

    /** Cache the left edge for the next chunk */
    private Vector2[] chunkLeftEdge;

    /** Highest chunk index generated so far */
    private int maxChunkIndex;

    private Vector2 generationOrigin;

    private TerrainChunk startingChunk;

    public Vector3 GeneratorOrigin() { return new Vector3(5.0f, CHUNK_HALF_HEIGHT, 0.0f); }


	public void StartGeneratingWorld(Player focusPlayer) {
		if(focusPlayer == null) {
			Debug.Log("ERROR!       Cannot generate world when input focus playter is null!");
			return;
		}

		player = focusPlayer;

        generationOrigin = new Vector2(CHUNK_HALF_WIDTH, CHUNK_HALF_HEIGHT);

        // Initialise some starting terrain chunks
        for(int i = 0; i < 5; i++ ) {
            BuildNextChunk();
        }

		// Flag as started generating
		bDoGeneration = true;
	}


    // Iterator for the linked list of terrain chunks
    public IEnumerable<TerrainChunk> ChunkList() {
        TerrainChunk nextChunk = startingChunk;

        while(nextChunk != null) {
            yield return nextChunk;
            nextChunk = nextChunk.GetRightChunk();
        }
    }


    private TerrainChunk BuildNextChunk() {
        if(chunkLeftEdge == null) {
            chunkLeftEdge = new Vector2[0];
        }

        Vector2[] verts = GetNextLinearChunkVertices(ref chunkLeftEdge, GetNextLinearCenter(generationOrigin, maxChunkIndex), DO_RANDOM_CHUNK_EDGES);
        TerrainChunk tempChunk = CreateChunk(verts, maxChunkIndex);

        // If this is the very first chunk
        if(maxChunkIndex == 0) {
            startingChunk = tempChunk;

            if(pointsOfInterest.Count > 0) {
                // TODO: slest which poi to spawn (not 0, thats the tutorial priest)
                tempChunk.AddPointOfInterest(pointsOfInterest[0]);
            }
        }
        else {
            TerrainChunk previous = startingChunk.GetRightmostChunk();

            tempChunk.SetLeftChunk(previous);
            previous.SetRightChunk(tempChunk);

            // Handle spawning of points of interest
            if(pointsOfInterest.Count > 0) {
                // TODO: select which poi to spawn (not 0, thats the tutorial priest)
                tempChunk.AddPointOfInterest(pointsOfInterest[1]);
            }
            
        }

        maxChunkIndex += 1;

        return tempChunk;
    }



    // Origin = first chunk centroid, chunkIdx = which chunk is being generated 
    private Vector2 GetNextLinearCenter(Vector2 origin, int chunkIdx) {
        return origin + new Vector2(CHUNK_HALF_WIDTH * 2.0f, 0.0f) * chunkIdx;
    }


    // Linear chunks progress to the right and are rectalinear in shape, with the right edge beign randomly staggerd
    private Vector2[] GetNextLinearChunkVertices(ref Vector2[] leftEdge, Vector2 centroid, bool bRandomRightEdge = true) {
        int newVerts = Random.Range(minVertsPerChunk, maxVertsPerChunk);
        List<Vector2> verts = new List<Vector2>();

        verts.Add(centroid);

        // Find the corners, winding cw
        Vector2 p0 = centroid + new Vector2(CHUNK_HALF_WIDTH, CHUNK_HALF_HEIGHT);   // top right
        Vector2 p1 = centroid + new Vector2(CHUNK_HALF_WIDTH, -CHUNK_HALF_HEIGHT);  // bottom right
        Vector2 p2 = centroid + new Vector2(-CHUNK_HALF_WIDTH, -CHUNK_HALF_HEIGHT); // bottom left
        Vector2 p3 = centroid + new Vector2(-CHUNK_HALF_WIDTH, CHUNK_HALF_HEIGHT);  // top left

        // Generate the right edge
        Vector2[] rightEdge = new Vector2[NUM_EDGE_SEGMENTS + 1];
        rightEdge[0] = p0;
        rightEdge[NUM_EDGE_SEGMENTS] = p1;

        // Generate right edge
        if(bRandomRightEdge) {
            float horizontalOffset = (p0.y - p1.y) / NUM_EDGE_SEGMENTS;

            for(int i = 0; i < NUM_EDGE_SEGMENTS - 1; i++) {
                Vector2 edgeVert = new Vector2(p0.x, p0.y - horizontalOffset - horizontalOffset*i);
                edgeVert.x += Random.Range(-CHUNK_EDGE_MAG, CHUNK_EDGE_MAG);
                rightEdge[i + 1] = edgeVert;
            }
        }

        // Add right edge to the main list
        verts.AddRange(rightEdge);

        // And the add the left edge: if it is matching up to an existing edge, do that, otherwise just use straight edge
        if(leftEdge.Length > 0) {
            verts.AddRange(leftEdge);
        }
        else {
            verts.Add(p2);
            verts.Add(p3);
        }

        // The next left edge is this ones right edge, but flipped to maintain cw winding
        List<Vector2> rightEdgeFlipped = new List<Vector2>(rightEdge);
        rightEdgeFlipped.Reverse(0, rightEdgeFlipped.Count);
        leftEdge = rightEdgeFlipped.ToArray();
        
        return verts.ToArray();
    }



    private TerrainChunk CreateChunk(Vector2[] vertices2D, int idx) {
        int vertexIdx = 1;
        int[] indices = new int[vertices2D.Length * 3];

        for(int i = 0; i < indices.Length; i += 3) {
            indices[i] = 0; // we know the first vertex is the centroid
            indices[i + 1] = vertexIdx;

            // wrap around
            int nextVertex = (vertexIdx + 1 < vertices2D.Length) ? vertexIdx + 1 : 1;

            indices[i + 2] = nextVertex;

            vertexIdx = nextVertex;
        }

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for(int i = 0; i < vertices.Length; i++) {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0.0f);
        }

        // create the uvs
        Vector2[] uvs = new Vector2[vertices.Length];
        for(int i = 0; i < uvs.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x / textureScale, vertices[i].y / textureScale);
        }

        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();
        msh.uv = uvs;

        // Set up game object with mesh;
        TerrainChunk chunkObj = (TerrainChunk)Instantiate(chunkPrototype);

        chunkObj.gameObject.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = chunkObj.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        chunkObj.Initialize(idx, vertices2D, indices);

        return chunkObj;
    }


}

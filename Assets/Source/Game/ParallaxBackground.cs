using UnityEngine;
using System.Collections;

public class ParallaxBackground : MonoBehaviour {

    private static Vector3 VIEWPORT_RIGHT_EDGE = new Vector3(1.0f, 0.0f, 0.0f);
    private static Vector3 VIEWPORT_LEFT_EDGE = new Vector3(0.0f, 0.0f, 0.0f);

    public enum ParallaxLayer {
        Front, Mid, Back
    }

    [SerializeField]
    public ParallaxLayer parallaxLayer;

    [SerializeField]
    public float parallaxFactor = 1.0f;

    [SerializeField]
    public SpriteRenderer[] backgroundSegments;

    private PlayerCamera masterCamera;

    private float yPosition;
    private float segmentWidth;

    // Index into segments that track which segment is leftmost or rightmost
    private int leftEdgeIdx;
    private int rightEdgeIdx;
	
	void Start () {
        if(backgroundSegments.Length <= 0) {
            Debug.Log("ERROR! ParalaxBackground: " + name + "requires at least one background segment!");
            return;
        }

        // Backgrounds are positioned just above the top of the ground polygons
        yPosition = WorldGenerator.CHUNK_HALF_HEIGHT * 2.0f + backgroundSegments[0].bounds.size.y / 2.0f;
        segmentWidth = backgroundSegments[0].bounds.size.x;

        // Set ordering based on desired background layer
        switch(parallaxLayer) {
            case ParallaxLayer.Front:
                for(int i = 0; i < backgroundSegments.Length; i++) {
                    backgroundSegments[i].sortingOrder = 2;
                }
                break;
            case ParallaxLayer.Mid:
                for(int i = 0; i < backgroundSegments.Length; i++) {
                    backgroundSegments[i].sortingOrder = 1;
                }
                break;
            case ParallaxLayer.Back:
                for(int i = 0; i < backgroundSegments.Length; i++) {
                    backgroundSegments[i].sortingOrder = 0;
                }
                break;
        }

        leftEdgeIdx = 0;
        rightEdgeIdx = backgroundSegments.Length - 1;
	}
	
	
	void Update () {
        // Find the players camera: need to do this in update because the camera may not be spawned for a frame or two
        if(masterCamera == null) {
            masterCamera = FindObjectOfType<PlayerCamera>();
        }
        if(masterCamera == null) {
            return;
        }

        /**
         * Update all background segments: move them to some fraction of the camera position for parallax.
         * When one segment reaches the end of the camera frustum, cycle it around for infini scrolling
         */
        float xOrigin = masterCamera.transform.position.x / parallaxFactor + leftEdgeIdx * segmentWidth;

        backgroundSegments[leftEdgeIdx].transform.position = new Vector3(xOrigin, yPosition, 0.0f);

        int idx = leftEdgeIdx + 1;
        for(int counter = 0; counter < backgroundSegments.Length; counter++) {
            if(idx >= backgroundSegments.Length) {
                idx = 0;
            }

            if(idx == leftEdgeIdx) {
                continue;
            }

            float xOffset = (counter + 1) * segmentWidth;

            backgroundSegments[idx].transform.position = new Vector3(xOrigin + xOffset, yPosition, 0.0f);
            
            idx += 1;
        }

        // Calculate if the right edge is showing
        Vector3 rightViewEdge = masterCamera.GetComponent<Camera>().ViewportToWorldPoint(VIEWPORT_RIGHT_EDGE);
        if(backgroundSegments[rightEdgeIdx].transform.position.x + segmentWidth / 2.0f <= rightViewEdge.x) {
            // Need to flip left edge to become rightmost, and move the left edge up one, wrapping back to 0 since that one could have been flipped around
            rightEdgeIdx = leftEdgeIdx;
            leftEdgeIdx = leftEdgeIdx + 1;
            if(leftEdgeIdx >= backgroundSegments.Length) {
                leftEdgeIdx = 0;
            }
        }

        // and the left edge
        Vector3 leftViewEdge = masterCamera.GetComponent<Camera>().ViewportToWorldPoint(VIEWPORT_LEFT_EDGE);
        if(backgroundSegments[leftEdgeIdx].transform.position.x - segmentWidth / 2.0f >= leftViewEdge.x) {
            leftEdgeIdx = rightEdgeIdx;
            rightEdgeIdx = rightEdgeIdx - 1;
            if(rightEdgeIdx < 0) {
                rightEdgeIdx = backgroundSegments.Length - 1;
            }
        }

	}


}

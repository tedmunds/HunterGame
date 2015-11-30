using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class FlickerLight : MonoBehaviour {

	[SerializeField]
	private float noisetraversalRate = 0.05f;
	
	[SerializeField]
	private float maxDistanceOffset = 2.0f;
	
	[SerializeField]
	private float maxSpeed = 0.5f;

	private Light lightSource;
	private Vector3 baseLocation;

	// location along x and y axis of some perlin noise
	private Vector2 xyNoisePos;

	// Use this for initialization
	void Start () {
		lightSource = GetComponent<Light>();
	}
	
    void OnEnable() {
        baseLocation = transform.position;
    }

	// Update is called once per frame
	void Update () {
		// first move the sample through the noise: we are sampling once along each axis
		xyNoisePos.x += noisetraversalRate;
		xyNoisePos.y += noisetraversalRate;

		// normalize the scalar values to be -1.0 to 10
		float xAmp = Mathf.PerlinNoise(xyNoisePos.x, 0.0f) * 2.0f - 1.0f;
		float yAmp = Mathf.PerlinNoise(0.0f, xyNoisePos.y) * 2.0f - 1.0f;

		// and lerp to the new offset at the max speed
		Vector3 offset = new Vector3(xAmp * maxDistanceOffset, yAmp * maxDistanceOffset, 0.0f);
		transform.position = Vector3.Lerp(transform.position , 
		                                  baseLocation + offset,
		                                  maxSpeed * Time.deltaTime);
	}
}

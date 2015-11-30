using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour {

    public const float CAMERA_SHAKE_AMPLITUDE = 0.3f;

	[SerializeField]
	private float speed;

	[SerializeField]
	private float acceleration;

    /* Should the y coord be clamped at the bottom of the level */
    private bool bClampLevelBottom = true;
    private float bottomLevelOffset = 2.0f;

	private Player target;

    private Camera myCamera;

	public void InitailizeCamera(Player owner) {
		target = owner;
	}

	// Use this for initialization
	void Start () {
        myCamera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		if(target == null) {
			return;
		}	

		Vector3 moveTo = target.transform.position;
		Vector2 offset = target.GetLookOffset();
		moveTo.z = transform.position.z;

		moveTo.x += offset.x;
		moveTo.y += offset.y;

		transform.position = Vector3.Lerp(transform.position, moveTo, Time.deltaTime * speed);

        if(bClampLevelBottom) {
            float bottomBound = GetBottomBounds();
            float frustumSize = (bottomBound - transform.position.y);

            if(bottomBound < -bottomLevelOffset) {
                Vector3 clampedPos = transform.position;
                clampedPos.y = -(frustumSize + bottomLevelOffset);
                transform.position = clampedPos;
            }
        }
	}


    public void DoCameraShake(Vector3 dir, float amplitude) {
        transform.position += dir.normalized * amplitude;
    }


    // Find the y coord of the bottom bound of the camera frustum
    public float GetBottomBounds() {
        Vector3 bottomLeft = myCamera.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f));
        return bottomLeft.y;
    }
}

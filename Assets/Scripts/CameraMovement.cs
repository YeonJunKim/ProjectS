using UnityEngine;
using System.Collections;


public class CameraMovement : MonoBehaviour {
	
	public float dragSpeed;
	public float scrollSpeed;
	public int minX;
	public int maxX;
	public int minZ;
	public int maxZ;

	float originY;	// for reseting camera Y pos
	float screenWidth;
	float screenHeight;

	CsMouseCursor csMouseCursor;

	void Start ()
	{
		originY = transform.position.y;
		screenWidth = Screen.width;
		screenHeight = Screen.height;

		csMouseCursor = GameObject.Find ("Player1").GetComponent<CsMouseCursor> ();
	}

	// Update is called once per frame
	void Update () {
		SetScrollSpeed ();
		CheckCameraZoom ();
	}

	void OnGUI()
	{
		AutoCameraMove ();
	}

	void AutoCameraMove()
	{
		Vector3 move = new Vector3 (0, 0, 0);

		if(Event.current.mousePosition.x < 3)
		{
			move.x = -1;
			csMouseCursor.CameraScrolling();
		}
		if(Event.current.mousePosition.x > screenWidth - 3)
		{
			move.x = 1;
			csMouseCursor.CameraScrolling();
		}
		if(Event.current.mousePosition.y < 3)
		{
			move.z = 1;
			csMouseCursor.CameraScrolling();
		}
		if(Event.current.mousePosition.y > screenHeight - 3)
		{
			move.z = -1;
			csMouseCursor.CameraScrolling();
		}

		transform.Translate(move * Time.smoothDeltaTime * scrollSpeed, Space.World);
	}

	void SetScrollSpeed()
	{
		if(Input.GetKeyDown(KeyCode.LeftBracket))
	   	{
			scrollSpeed -= 1;
			if(scrollSpeed < 0)
				scrollSpeed = 0;
		}
		else if(Input.GetKeyDown(KeyCode.RightBracket))
		{
			scrollSpeed += 1;
		}
	}

	void CheckCameraZoom()
	{	
		Vector3 zoom = new Vector3 (0, 0, 0);

		if(Input.GetKeyDown(KeyCode.Home))
		{
			transform.position = new Vector3(transform.position.x, originY, transform.position.z);
		}

		if(Input.mouseScrollDelta.y > 0)
		{
			zoom.y = 1;
		}
		else if(Input.mouseScrollDelta.y < 0)
		{
			zoom.y = -1;
		}

		transform.Translate(zoom, Space.World);
	}


}






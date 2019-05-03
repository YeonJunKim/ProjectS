using UnityEngine;
using System.Collections;

public enum ObjectColor
{
	RED,
	BLUE,
}


public class CsEffectManager : MonoBehaviour {

	public ObjectColor myColor;
	public string objectName;
	public EffectManager effectManager;
	Vector3 originPos;

	// Use this for initialization
	void Start () {
		originPos.Set (0, 0, 0);
		ReturnObject ();
    }
    
	// Update is called once per frame
	void Update () {
	
	}
	
	public void ActivateObject(Vector3 position, Quaternion rotation)
	{
		transform.position = position;
		transform.rotation = rotation;
		gameObject.SetActive (true);
	}

	void WaitForEvaporation()
	{
		Invoke ("ReturnObject", 4);
	}

	public void ReturnObject()
	{
		gameObject.SetActive (false);
		transform.position = originPos;
		effectManager.ReturnGameObject (gameObject);
	}
	
}

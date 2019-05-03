using UnityEngine;
using System.Collections;

public class CsChildObject : MonoBehaviour {


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public Vector3 GetColliderSize()
	{
		return collider.bounds.size;
	}
}

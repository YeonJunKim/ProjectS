using UnityEngine;
using System.Collections;


public class CsObjectManager : MonoBehaviour {

	Vector3 originPos;
	ObjectManager playerObjectManager;
	CsProperties csProperties;
	CsCommandProcessor csCommandProcessor;
	CsFieldOfVision csFieldOfVision;
	CsAttackRange csAttackRange;

	// Use this for initialization
	void Start () {
		originPos = new Vector3 (0, 0, 0);

		csProperties = transform.GetComponent<CsProperties> ();
		csCommandProcessor = transform.GetComponent<CsCommandProcessor> ();
		csFieldOfVision = GetComponentInChildren<CsFieldOfVision> ();
		csAttackRange = GetComponentInChildren<CsAttackRange> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ActivateObject(Vector3 position, Quaternion rotation)
	{
		transform.position = position;
		transform.rotation = rotation;
		csProperties = transform.GetComponent<CsProperties> ();
		csCommandProcessor = transform.GetComponent<CsCommandProcessor> ();
		csProperties.Initialize ();
		csCommandProcessor.Initialize ();
		csFieldOfVision.Initialize ();
		csAttackRange.Initialize ();
	}

	public void ReturnObject()
	{
		gameObject.SetActive (false);
		transform.position = originPos;
		playerObjectManager.ReturnGameObject (gameObject);
	}

	public void SetPlayerObjectManager(ObjectManager objectManager)
	{
		playerObjectManager = objectManager;
	}
}

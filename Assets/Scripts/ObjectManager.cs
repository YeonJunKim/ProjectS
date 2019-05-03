using UnityEngine;
using System.Collections;

public class ObjectManager : MonoBehaviour {

	ArrayList MarineList = new ArrayList();
	ArrayList ScvList = new ArrayList();
	ArrayList TankList = new ArrayList ();
	ArrayList CommandCenterList = new ArrayList();


	ArrayList ActiveCommandList = new ArrayList();	// for Scvs, finding CommandCenters
	ArrayList ActiveMineralList = new ArrayList();	// for Scvs, finding Minerals

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
    }

	public void ReturnGameObject(GameObject target)
	{
		string objectName = target.GetComponent<CsProperties> ().objectName;

		switch(objectName)
		{
		case "Marine":
			MarineList.Add (target);
			break;
		case "Scv":
			ScvList.Add (target);
			break;
		case "CommandCenter":
			CommandCenterList.Add (target);
			ActiveCommandList.Remove(target);
			break;
		case "Tank":
			TankList.Add (target);
			break;
		}
	}

	public bool GetGameObject(string objectName, Vector3 position, Quaternion rotation)
	{
		GameObject targetObject;

		switch(objectName)
		{
		case "Marine":
			if(MarineList.Count <= 0)
			{
				Debug.LogWarning("Cannot Get Object From List: " + objectName);
				return false;
			}
			targetObject = MarineList[0] as GameObject;
			targetObject.SetActive(true);
			targetObject.GetComponent<CsObjectManager>().ActivateObject(position, rotation);
			MarineList.RemoveAt(0);
			break;
		case "Scv":
			if(ScvList.Count <= 0)
			{
				Debug.LogWarning("Cannot Get Object From List: " + objectName);
				return false;
			}
			targetObject = ScvList[0] as GameObject;
			targetObject.SetActive(true);
			targetObject.GetComponent<CsObjectManager>().ActivateObject(position, rotation);
			ScvList.RemoveAt(0);
			break;
		case "Tank":
			if(TankList.Count <= 0)
			{
				Debug.LogWarning("Cannot Get Object From List: " + objectName);
				return false;
			}
			targetObject = TankList[0] as GameObject;
			targetObject.SetActive(true);
			targetObject.GetComponent<CsObjectManager>().ActivateObject(position, rotation);
			TankList.RemoveAt(0);
			break;
		case "CommandCenter":
			if(CommandCenterList.Count <= 0)
			{
				Debug.LogWarning("Cannot Get Object From List: " + objectName);
				return false;
			}
			targetObject = CommandCenterList[0] as GameObject;
			targetObject.SetActive(true);
			targetObject.GetComponent<CsObjectManager>().ActivateObject(position, rotation);
			ActiveCommandList.Add(targetObject);
			CommandCenterList.RemoveAt(0);
			break;
		}
		return true;
	}

	// this will be used for Scvs, finding Minerals and CommandCenters
	public ArrayList GetActiveGameObjects(string objectName)
	{
		ArrayList targetObjects = new ArrayList();
		
		switch(objectName)
		{
		case "CommandCenter":	// we are only going to get CommandCenter for now
			if(ActiveCommandList.Count <= 0)
			{
				Debug.LogWarning("Cannot Get Object From List: " + objectName);
				break;
			}
			foreach(GameObject target in ActiveCommandList)
			{
				targetObjects.Add(target);
			}
			break;
		case "Mineral":	// we are only going to get CommandCenter for now
			if(ActiveMineralList.Count <= 0)
			{
				break;
			}
			foreach(GameObject target in ActiveMineralList)
			{
				targetObjects.Add(target);
			}
			break;
		}

		return targetObjects;
	}

	public void AddToActiveList(GameObject target)
	{
		string objectName = target.GetComponent<CsProperties> ().objectName;
		
		switch(objectName)
		{
		case "CommandCenter":
			ActiveCommandList.Add (target);
			break;
		case "Mineral":
			ActiveMineralList.Add (target);
			break;
		}
	}
}







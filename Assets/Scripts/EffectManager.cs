using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {

	ArrayList MarineDeathList_Red = new ArrayList();
	ArrayList MarineDeathList_Blue = new ArrayList();
	ArrayList ScvDeathList_Red = new ArrayList();
	ArrayList ScvDeathList_Blue = new ArrayList();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void ReturnGameObject(GameObject target)
	{
		string objectName = target.GetComponent<CsEffectManager> ().objectName;
		ObjectColor color = target.GetComponent<CsEffectManager> ().myColor;
		
		switch(objectName)
		{
		case "MarineDeath":
			if(color == ObjectColor.BLUE)
				MarineDeathList_Blue.Add (target);
			else if(color == ObjectColor.RED)
				MarineDeathList_Red.Add (target);
			break;
		case "ScvDeath":
			if(color == ObjectColor.BLUE)
				ScvDeathList_Blue.Add (target);
			else if(color == ObjectColor.RED)
				ScvDeathList_Red.Add (target);
			break;
		}
	}
	
	public void GetGameObject(string objectName, ObjectColor color, Vector3 position, Quaternion rotation)
	{
		switch(objectName)
		{
		case "MarineDeath":
			if(color == ObjectColor.BLUE)
			{
				if(MarineDeathList_Blue.Count <= 0)
				{
					Debug.LogWarning("Cannot Get Effect From List: " + objectName);
					break;
				}
				GameObject targetObject = MarineDeathList_Blue[0] as GameObject;
				targetObject.GetComponent<CsEffectManager>().ActivateObject(position, rotation);
				MarineDeathList_Blue.RemoveAt(0);
			}
			else if(color == ObjectColor.RED)
			{
				if(MarineDeathList_Red.Count <= 0)
				{
					Debug.LogWarning("Cannot Get Effect From List: " + objectName);
					break;
				}
				GameObject targetObject = MarineDeathList_Red[0] as GameObject;
				targetObject.GetComponent<CsEffectManager>().ActivateObject(position, rotation);
				MarineDeathList_Red.RemoveAt(0);
			}
			break;
		case "ScvDeath":
			if(color == ObjectColor.BLUE)
			{
				if(ScvDeathList_Blue.Count <= 0)
				{
					Debug.LogWarning("Cannot Get Effect From List: " + objectName);
					break;
				}
				GameObject targetObject = ScvDeathList_Blue[0] as GameObject;
				targetObject.GetComponent<CsEffectManager>().ActivateObject(position, rotation);
				ScvDeathList_Blue.RemoveAt(0);
			}
			else if(color == ObjectColor.RED)
			{
				if(ScvDeathList_Red.Count <= 0)
				{
					Debug.LogWarning("Cannot Get Effect From List: " + objectName);
					break;
				}
				GameObject targetObject = ScvDeathList_Red[0] as GameObject;
				targetObject.GetComponent<CsEffectManager>().ActivateObject(position, rotation);
				ScvDeathList_Red.RemoveAt(0);
			}
			break;
		}
	}
}

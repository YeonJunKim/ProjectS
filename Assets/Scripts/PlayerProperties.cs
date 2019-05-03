using UnityEngine;
using System.Collections;

public class PlayerProperties : MonoBehaviour {

	public int startMin;
	public int startGas;

	[HideInInspector] public int currentMin;
	[HideInInspector] public int currentGas;
	[HideInInspector] public int currentPopulation;


	// Use this for initialization
	void Start () {
		Initialize ();
	}

	void Initialize()
	{
		currentMin = startMin;
		currentGas = startGas;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void IncreasePopulation(int amount)
	{
		currentPopulation += amount;
	}

	public void DecreasePopulation(int amount)
	{
		currentPopulation -= amount;
	}

	public void IncreaseMin(int amount)
	{
		currentMin += amount;
	}
	
	public void DecreaseMin(int amount)
	{
		currentMin -= amount;
	}

	public void IncreaseGas(int amount)
	{
		currentGas += amount;
	}
	
	public void DecreaseGas(int amount)
	{
		currentGas -= amount;
	}
}

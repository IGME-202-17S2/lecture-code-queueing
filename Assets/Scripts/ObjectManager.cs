using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ObjectManager 
 * This component tracks collections of PhysicsObjects
 * It also orchestrates the forces and applies them.
 */
public class ObjectManager : MonoBehaviour {

	public int population = 30;

	// for PhysicsObjects generated at runtime
	List<PhysicsObject> movers = new List<PhysicsObject>();

	// a reference to the prefab used for PhysicsObjects
	// (also populated in the inspector)
	public GameObject moverPrefab;

	void Start () {
		GenerateMovers ();
	}

	void GenerateMovers() {
		for (int i = 0; i < population; i++) {
			Vector3 pos = Camera.main.ScreenToWorldPoint (
				new Vector3 (Random.Range (0, Screen.width), Random.Range (0, Screen.height), 0));
			pos.z = 0;
			GameObject go = Instantiate (moverPrefab, pos, Quaternion.identity);
			PhysicsObject po = go.GetComponent<PhysicsObject> ();
			movers.Add (po);
			po.neighbors = movers;
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This initializes all level variables
public class Floor_Control : MonoBehaviour {

	private Vector3 velocity;
	
	
	public int towerLevel;
	public int globalTurn = 0; //Cycle through turns, 0 - 1 is outside of combat free movement, 2+ is based on Player_Unit or Enemy_Unit initiative int i.e. if Player[1].GetComponent<Player_Unit>().initiative == 1, then their turn would take place on globalTurn 2
	public int globalStep = 0; //this will check that the current step is the end of the action queue for the player, which will advance the global turn if in combat or reset it if outside of combat
	public int finalStep;
	public bool inCombat = false;
	public GameObject[] Player = new GameObject[4];
	public GameObject selectedPlayer;
	public GameObject Floor; //Floor block used for room generation
	public int roomHeight;
	public int roomWidth;
	
	public int[] highlightedFloor = new int[2]; //Floor block highlighted by the mouse, saved in Coord array (Z,X)
	public bool routeHighlight = true;
	public GameObject p1Marker;
	
	public int[] p1Coord = new int[2]; //Player 1 coordinates 
	public int[] p2Coord = new int[2]; //Player 2 coordinates 
	public int[] p3Coord = new int[2]; //Player 3 coordinates 
	public int[] p4Coord = new int[2]; //Player 4 coordinates 
	
	public int[] p1DestCoord = new int[2]; //Player 1 destination coordinates 
	public int[] p2DestCoord = new int[2]; //Player 2 destination coordinates 
	public int[] p3DestCoord = new int[2]; //Player 3 destination coordinates 
	public int[] p4DestCoord = new int[2]; //Player 4 destination coordinates 
	
	public int[] p1SavedCoord = new int[2]; //Player 1 saved coordinates 
	public int[] p2SavedCoord = new int[2]; //Player 2 saved coordinates 
	public int[] p3SavedCoord = new int[2]; //Player 3 saved coordinates 
	public int[] p4SavedCoord = new int[2]; //Player 4 saved coordinates 
	
	public Dictionary<int, int[]> p1MoveQueueSpace = new Dictionary<int, int[]>(); //Player 1 Saved Route coord
	public Dictionary<int, int[]> p2MoveQueueSpace = new Dictionary<int, int[]>(); //Player 2 Saved Route coord
	public Dictionary<int, int[]> p3MoveQueueSpace = new Dictionary<int, int[]>(); //Player 3 Saved Route coord
	public Dictionary<int, int[]> p4MoveQueueSpace = new Dictionary<int, int[]>(); //Player 4 Saved Route coord

	// Use this for initialization
	void Start () 
	{
		//Room floor generation
		for (int wi = 1; wi < roomHeight; wi++)
		{
			for (int hi = 1; hi < roomWidth; hi++)
			{
				GameObject Floor_Block;
				Floor_Block = Instantiate(Floor, new Vector3(transform.position.x + hi, transform.position.y-0.48f, transform.position.z + wi), Quaternion.identity) as GameObject;
				Floor_Block.GetComponent<Floor_Unit>().floorController = this.gameObject;
				Floor_Block.GetComponent<Floor_Unit>().floorCoord[0] = wi;
				Floor_Block.GetComponent<Floor_Unit>().floorCoord[1] = hi;
				Floor_Block.transform.parent = transform;
			}		
		}
		
		//Initializes the player's default selection to their starting location
		p1SavedCoord = p1Coord;
		p2SavedCoord = p2Coord;
		p3SavedCoord = p3Coord;
		p4SavedCoord = p4Coord;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(globalTurn == 0)//Everything before 1 is selection phase
		{
			//Begin turn
			
		}

	}
}

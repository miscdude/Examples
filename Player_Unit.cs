using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player_Unit : MonoBehaviour {
	
	private Vector3 velocity;
	
	public int[] myCoord = new int[2]; //Floor coordinates of this player
	public bool resetSelection = true; //Clicking will save a new coordinate
	public int globalTurn;
	public int globalStep;
	public bool inCombat;
	public float moveSpeed = 1.0f;
	
	//public bool canMove = true;
	//public bool canAttack = true;
	public bool generate = true; //Can generate rangemarkers
	public bool selected; //if this is the selected player
	
	public GameObject floorController; //Floor controller object
	public GameObject RangeMarker; //Range markers to be generated
	public GameObject myGhost; //Cursor attached to player
	public GameObject ghost; //Cursor object to generate
	public GameObject camera; //camera focused on player
	
	public Dictionary<int, int[]> moveQueueSpace = new Dictionary<int, int[]>();
	public Dictionary<int, Transform> moveQueueTransform = new Dictionary<int, Transform>();
	public Dictionary<int, string> moveQueueType = new Dictionary<int, string>();
	
	public Material moveRangeIndicator; //Material for move range marker
	public Material attackRangeIndicator; //Material for attack range marker
	public Material interactRangeIndicator; //Material for interact range marker
	public Material moveQueueIndicator; //Material for move range marker
	public Material attackQueueIndicator; //Material for attack range marker
	public Material interactQueueIndicator; //Material for interact range marker
	
	public Material selectedMat; //Material for cursor
	public Material deselectedMat; //Deselcted material for cursor
	
	public int playerNumber; //Player int for management purposes
	public int moveRange; //Player's move range
	public int attackRange; //Player's attack range
	
	// Use this for initialization
	void Start () 
	{
		Invoke("GetTile", 0); //Call the tile-check function at start to initialize location
		
		 //Set playerNumber based on Floor Controller's designation
		if(floorController.GetComponent<Floor_Control>().Player[0] == this.gameObject)
			playerNumber = 1;
		if(floorController.GetComponent<Floor_Control>().Player[1] == this.gameObject)
			playerNumber = 2;
		if(floorController.GetComponent<Floor_Control>().Player[2] == this.gameObject)
			playerNumber = 3;
		if(floorController.GetComponent<Floor_Control>().Player[3] == this.gameObject)
			playerNumber = 4;
	
		//Generate attached cursor
		if(floorController.GetComponent<Floor_Control>().selectedPlayer == this.gameObject)
		{
			Invoke("GenerateGhost", 0.1f);
		}
	}
	
	// Fixed update is called once per second
	void FixedUpdate()
	{
		if(floorController.GetComponent<Floor_Control>().selectedPlayer == this.gameObject && myGhost != null)
		{
			//Player will look at destination
			float cursorDist = Vector3.Distance(myGhost.transform.position, transform.position);
			if (cursorDist > 0f)
			{
				Vector3 targetDir = myGhost.transform.position - transform.position;
				float step = 0.1f * Time.deltaTime;
				Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
				Debug.DrawRay(transform.position, newDir, Color.red);
				Vector3 finalDir = newDir;
				finalDir.y = transform.position.y;
				transform.rotation = Quaternion.LookRotation(finalDir);
			}
			
		}
		else
		{
			moveQueueSpace.Clear();
			moveQueueType.Clear();
		}
		
		globalTurn = floorController.GetComponent<Floor_Control>().globalTurn; //returns globalTurn from floor control master
		inCombat = floorController.GetComponent<Floor_Control>().inCombat; //returns globalTurn from floor control master
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Space) && globalStep == 0 && inCombat == false && floorController.GetComponent<Floor_Control>().selectedPlayer == this.gameObject)
		{
			Invoke("MoveTo", 0.1f);
		}
	}
	
	void MoveTo ()
	{
		int finalStep = moveQueueTransform.Count;
		floorController.GetComponent<Floor_Control>().globalTurn = 1;
		floorController.GetComponent<Floor_Control>().globalStep = 1;
		Vector3 targetTransform = new Vector3(moveQueueTransform[globalStep+1].position.x, 2, moveQueueTransform[globalStep+1].position.z);
		transform.position = Vector3.SmoothDamp(transform.position, targetTransform, ref velocity, moveSpeed * Time.deltaTime);
		if(transform.position == moveQueueTransform[globalStep+1].position)
		{
			floorController.GetComponent<Floor_Control>().globalStep += 1;
			CancelInvoke("MoveTo");
		}
	}
	
	// Line cast pathing
	void HighlightPath ()
	{
		RaycastHit[] floors;
		Vector3 floorPos = transform.position;
		floorPos.y = 1; //1 is the height level of the floor, 2 is the height level of the player
		floorPos.z = transform.position.z + 0.1f; //slight offset to avoid the ray hitting two blocks per one space on diagonal paths
		int floorMask = 1 << 8; //only check floor layer
		Vector3 forward = transform.TransformDirection(Vector3.down); //forward is accurate to the in-game representation, but the actual vector rotation is down. I may have to fix orientation later.
		floors = Physics.RaycastAll(floorPos, forward, Vector3.Distance(transform.position, myGhost.transform.position), floorMask); //Raytraced collisions, RaycastAll returns all hits instead of the first hit
		//Debug.DrawRay(transform.position, forward, Color.red);
		for (int i = 0; i < floors.Length; i++)
		{
			RaycastHit hit = floors[i];
			hit.collider.gameObject.GetComponent<Floor_Unit>().tileDistance = i; //Distance to player
			int[] coord = hit.collider.gameObject.GetComponent<Floor_Unit>().floorCoord;
			moveQueueSpace[i+1] = coord;
			moveQueueTransform[i+1] = hit.collider.gameObject.transform;
			moveQueueType[i+1] = "Move";
			//floorController.GetComponent<Floor_Control>().p1MoveQueueSpace[i+1] = moveQueueSpace[i+1];
			//Debug.Log(coord[0] + ", " + coord[1]);
			if(hit.collider.gameObject.GetComponent<Floor_Unit>().occupant !=null)
			{
				Debug.Log("Space " + coord[0] + ", " + coord[1] + " is occupied by " + hit.collider.gameObject.GetComponent<Floor_Unit>().occupant.name);
				i = floors.Length;
			}
			GameObject myMarker = Instantiate(RangeMarker, new Vector3(moveQueueSpace[i+1][1], transform.position.y - 0.35f , moveQueueSpace[i+1][0]), Quaternion.identity) as GameObject;
			myMarker.GetComponent<Range_Marker>().originPlayer = this.gameObject;
			myMarker.GetComponent<Range_Marker>().type = "MoveQueue";
			myMarker.GetComponent<Range_Marker>().rangeIndicator = moveQueueIndicator;
			myMarker.GetComponent<Range_Marker>().number = i;
 		}
		CancelInvoke("HighlightPath");
	}
	
	//Check for the tile the player is occupying and display range
	void GetTile()
	{
		generate = true; //Will create range markers
		RaycastHit currentTile;
		if (Physics.Raycast(transform.position, Vector3.down, out currentTile)  && resetSelection == true) //Assigns current block to selection if passes reset check
		{
			currentTile.collider.GetComponent<Floor_Unit>().occupant = this.gameObject;
			if(floorController.GetComponent<Floor_Control>().Player[0] == this.gameObject)
			{
				floorController.GetComponent<Floor_Control>().p1Coord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
				floorController.GetComponent<Floor_Control>().p1SavedCoord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
			}
			if(floorController.GetComponent<Floor_Control>().Player[1] == this.gameObject)
			{
				floorController.GetComponent<Floor_Control>().p2Coord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
				floorController.GetComponent<Floor_Control>().p2SavedCoord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
			}
			if(floorController.GetComponent<Floor_Control>().Player[2] == this.gameObject)
			{
				floorController.GetComponent<Floor_Control>().p3Coord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
				floorController.GetComponent<Floor_Control>().p3SavedCoord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
			}
			if(floorController.GetComponent<Floor_Control>().Player[3] == this.gameObject)
			{
				floorController.GetComponent<Floor_Control>().p4Coord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
				floorController.GetComponent<Floor_Control>().p4SavedCoord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
			}
			myCoord = currentTile.collider.GetComponent<Floor_Unit>().floorCoord;
		}
		
		//Range marker generation, creates one quadrant per for command
		if (floorController.GetComponent<Floor_Control>().globalTurn == 0 && selected == true && generate == true)
		{
			
			for (int nRange = 0; nRange - moveRange < moveRange; nRange++)//SECornerFill
			{
				for (int startRange = 0; startRange + nRange < moveRange; startRange++)//South-East
				{
					GameObject RangeMarkSE;
					RangeMarkSE = Instantiate(RangeMarker, new Vector3(transform.position.x + nRange + 1, transform.position.y-0.4f, transform.position.z + startRange), Quaternion.identity) as GameObject;
					RangeMarkSE.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkSE.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkSE.GetComponent<Range_Marker>().rangeIndicator = moveRangeIndicator;
					RangeMarkSE.GetComponent<Range_Marker>().type = "Move";
					generate = false;
				}
				for (int startRange = 0; startRange + nRange < moveRange; startRange++)//South-West
				{
					GameObject RangeMarkSW;
					RangeMarkSW = Instantiate(RangeMarker, new Vector3(transform.position.x + nRange, transform.position.y-0.4f, transform.position.z - startRange - 1), Quaternion.identity) as GameObject;
					RangeMarkSW.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkSW.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkSW.GetComponent<Range_Marker>().rangeIndicator = moveRangeIndicator;
					RangeMarkSW.GetComponent<Range_Marker>().type = "Move";
					generate = false;
				}
				for (int startRange = 0; startRange + nRange < moveRange; startRange++)//North-West
				{
					GameObject RangeMarkNW;
					RangeMarkNW = Instantiate(RangeMarker, new Vector3(transform.position.x - startRange - 1, transform.position.y-0.4f, transform.position.z - nRange), Quaternion.identity) as GameObject;
					RangeMarkNW.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkNW.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkNW.GetComponent<Range_Marker>().rangeIndicator = moveRangeIndicator;
					RangeMarkNW.GetComponent<Range_Marker>().type = "Move";
					generate = false;
				}
				for (int startRange = 0; startRange + nRange < moveRange; startRange++)//North-East
				{
					GameObject RangeMarkNE;
					RangeMarkNE = Instantiate(RangeMarker, new Vector3(transform.position.x - startRange, transform.position.y-0.4f, transform.position.z + nRange + 1), Quaternion.identity) as GameObject;
					RangeMarkNE.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkNE.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkNE.GetComponent<Range_Marker>().rangeIndicator = moveRangeIndicator;
					RangeMarkNE.GetComponent<Range_Marker>().type = "Move";
					generate = false;
				}
			}
		}
		
		//Range marker generation for attack markers
		if (floorController.GetComponent<Floor_Control>().globalTurn == 0 && selected == true)
		{
			for (int nRange = 0; nRange - attackRange < attackRange; nRange++)//SECornerFill
			{
				for (int startRange = 0; startRange + nRange < attackRange; startRange++)//South-East
				{
					GameObject RangeMarkSE;
					RangeMarkSE = Instantiate(RangeMarker, new Vector3(transform.position.x + nRange + 1, transform.position.y-0.39f, transform.position.z + startRange), Quaternion.identity) as GameObject;
					RangeMarkSE.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkSE.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkSE.GetComponent<Range_Marker>().rangeIndicator = attackRangeIndicator;
					RangeMarkSE.GetComponent<Range_Marker>().type = "Attack";
					generate = false;
				}
				for (int startRange = 0; startRange + nRange < attackRange; startRange++)//South-West
				{
					GameObject RangeMarkSW;
					RangeMarkSW = Instantiate(RangeMarker, new Vector3(transform.position.x + nRange, transform.position.y-0.39f, transform.position.z - startRange - 1), Quaternion.identity) as GameObject;
					RangeMarkSW.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkSW.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkSW.GetComponent<Range_Marker>().rangeIndicator = attackRangeIndicator;
					RangeMarkSW.GetComponent<Range_Marker>().type = "Attack";
					generate = false;
				}
				for (int startRange = 0; startRange + nRange < attackRange; startRange++)//North-West
				{
					GameObject RangeMarkNW;
					RangeMarkNW = Instantiate(RangeMarker, new Vector3(transform.position.x - startRange - 1, transform.position.y-0.39f, transform.position.z - nRange), Quaternion.identity) as GameObject;
					RangeMarkNW.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkNW.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkNW.GetComponent<Range_Marker>().rangeIndicator = attackRangeIndicator;
					RangeMarkNW.GetComponent<Range_Marker>().type = "Attack";
					generate = false;
				}
				for (int startRange = 0; startRange + nRange < attackRange; startRange++)//North-East
				{
					GameObject RangeMarkNE;
					RangeMarkNE = Instantiate(RangeMarker, new Vector3(transform.position.x - startRange, transform.position.y-0.39f, transform.position.z + nRange + 1), Quaternion.identity) as GameObject;
					RangeMarkNE.GetComponent<Range_Marker>().originPlayer = this.gameObject;
					RangeMarkNE.GetComponent<Range_Marker>().playerNumber = playerNumber;
					RangeMarkNE.GetComponent<Range_Marker>().rangeIndicator = attackRangeIndicator;
					RangeMarkNE.GetComponent<Range_Marker>().type = "Attack";
					generate = false;
				}
			}
		}
		CancelInvoke("GetTile");
	}
	
	void GenerateGhost()
	{
			//Generate attached cursor
			if(floorController.GetComponent<Floor_Control>().selectedPlayer == this.gameObject)
			{
				myGhost = Instantiate(ghost, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity) as GameObject;
				myGhost.GetComponent<Cursor_Control>().floorController = floorController;
				myGhost.GetComponent<Cursor_Control>().myPlayer = this.gameObject;
				myGhost.GetComponent<Cursor_Control>().selectedMat = selectedMat;
				myGhost.GetComponent<Cursor_Control>().deselectedMat = deselectedMat;
				myGhost.GetComponent<Cursor_Control>().playerNumber = playerNumber;
			}
		CancelInvoke("GetTile");
	}
	//Click the player you want to control
	void OnMouseDown()
	{
		Debug.Log("Player " + playerNumber + " on space " + myCoord[0] + ", " + myCoord[1]);
		if(playerNumber == 1 && globalTurn == 0)
		{
			floorController.GetComponent<Floor_Control>().Player[0].GetComponent<Player_Unit>().selected = true;
			floorController.GetComponent<Floor_Control>().Player[1].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[2].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[3].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().selectedPlayer = this.gameObject;
			Invoke("GetTile", 0.1f);
			Invoke("GenerateGhost", 0.1f);
			floorController.GetComponent<Floor_Control>().p1SavedCoord = myCoord;
			resetSelection = false;
		}
		if(playerNumber == 2 && globalTurn == 0)
		{
			floorController.GetComponent<Floor_Control>().Player[0].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[1].GetComponent<Player_Unit>().selected = true;
			floorController.GetComponent<Floor_Control>().Player[2].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[3].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().selectedPlayer = this.gameObject;
			Invoke("GetTile", 0.1f);
			Invoke("GenerateGhost", 0.1f);
			floorController.GetComponent<Floor_Control>().p2SavedCoord = myCoord;
			resetSelection = false;
		}
		if(playerNumber == 3 && globalTurn == 0)
		{
			floorController.GetComponent<Floor_Control>().Player[0].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[1].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[2].GetComponent<Player_Unit>().selected = true;
			floorController.GetComponent<Floor_Control>().Player[3].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().selectedPlayer = this.gameObject;
			Invoke("GetTile", 0.1f);
			Invoke("GenerateGhost", 0.1f);
			floorController.GetComponent<Floor_Control>().p3SavedCoord = myCoord;
			resetSelection = false;
			
		}
		if(playerNumber == 4 && globalTurn == 0)
		{
			floorController.GetComponent<Floor_Control>().Player[0].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[1].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[2].GetComponent<Player_Unit>().selected = false;
			floorController.GetComponent<Floor_Control>().Player[3].GetComponent<Player_Unit>().selected = true;
			floorController.GetComponent<Floor_Control>().selectedPlayer = this.gameObject;
			Invoke("GetTile", 0.1f);
			Invoke("GenerateGhost", 0.1f);
			floorController.GetComponent<Floor_Control>().p4SavedCoord = myCoord;
			resetSelection = false;
		}
	}
}

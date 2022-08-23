using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager Class of the State Machine for Characters
/// Attached to a Character Game Object with the CharacterGameData Component
/// </summary>
public class CharacterStateManager : MonoBehaviour
{
    [HideInInspector]
    public CharacterGameData CharacterData;

    CharacterBaseState CharacterState;
    public CharacterIdleState Idle = new CharacterIdleState();
    public CharacterWalkState Walk = new CharacterWalkState();
    public CharacterJumpUpState JumpUp = new CharacterJumpUpState();
    public CharacterLeapState Leap = new CharacterLeapState();

    //Used for movement reference in states
    [HideInInspector]
    public GameTileTracker GameTileTracker;
    [HideInInspector]
    public Vector2Int MoveOrigin;
    [HideInInspector]
    public Vector2Int MoveDestination;
    [HideInInspector]
    public Vector3 MoveDestinationPosition;
    public LinkedList<GameObject> RemainingDestinationWaypoints = new LinkedList<GameObject>();

    void Start()
    {
        GameTileTracker = GameObject.FindGameObjectWithTag(Constants.TilemapTag).GetComponent<GameTileTracker>();
        if (GameTileTracker == null)
            Debug.LogError("No GameTileTracker component attached to the Tilemap!");

        CharacterData = this.gameObject.GetComponent<CharacterGameData>();
        if (CharacterData == null)
            Debug.LogError("No paired CharacterGameData attached to Character!");

        CharacterState = Idle;
        CharacterData.AnimatorState = Constants.Idle;
        CharacterState.Start(this);
    }

    void Update()
    {
        CharacterState.Update(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="NewState"></param>
    public void ChangeState(CharacterBaseState NewState)
    {
        CharacterState = NewState;
        CharacterState.Start(this); //Call Start before next update
    }

    /// <summary>
    /// First call to state manager to make a unit move to a destination from where it is currently
    /// </summary>
    /// <param name="DestinationGameTileObject"></param>
    public void StartMoveSequence(GameObject DestinationGameTileObject)
    {
        //Prepare Travel Waypoints
        GameObject TileToEnqueue = DestinationGameTileObject;
        for (int i = 0; i < CharacterData.Movement + 1; i++) //Maximum travel distance possible
        {
            if (GameTileTracker.DestinationPathfindingMap[TileToEnqueue] == null)
                break;
            RemainingDestinationWaypoints.AddFirst(TileToEnqueue);
            TileToEnqueue = GameTileTracker.DestinationPathfindingMap[TileToEnqueue];
            //if (i == CharacterData.Movement)
            //    Debug.LogError("Character's movement exceeded when preparing travel waypoints!");
        }

        GameTile FirstOrigin = TileToEnqueue.GetComponent<GameTile>();
        //GameTile FirstDestination = RemainingDestinationWaypoints.First.Value.GetComponent<GameTile>();

        //Recouple character from origin to destination game tile
        FirstOrigin.OccupyingCharacter = null;
        DestinationGameTileObject.GetComponent<GameTile>().OccupyingCharacter = gameObject;

        //This will be moved into MoveOrigin when MoveToNextWaypoint runs
        MoveDestination = new Vector2Int(FirstOrigin.CellPositionX, FirstOrigin.CellPositionY);
        MoveToNextWaypoint();

        //MoveOrigin = new Vector2Int(FirstOrigin.CellPositionX, FirstOrigin.CellPositionY);
        //MoveDestination = new Vector2Int(FirstDestination.CellPositionX, FirstDestination.CellPositionY);
        //MoveDestinationPosition = RemainingDestinationWaypoints.First.Value.transform.position;


        //Remove first destination once set
        //RemainingDestinationWaypoints.RemoveFirst();

        //TODO: Determine move sequence between Walk/JumpUp/Leap
        //ChangeState(Walk);
    }

    /// <summary>
    /// 
    /// </summary>
    public void MoveToNextWaypoint()
    {
        if (RemainingDestinationWaypoints.Count == 0)
        {
            //On Move finish, return to Idle
            ChangeState(Idle);
            return;
        }
        
        GameTile NextDestination = RemainingDestinationWaypoints.First.Value.GetComponent<GameTile>();

        MoveOrigin = MoveDestination;
        MoveDestination = new Vector2Int(NextDestination.CellPositionX, NextDestination.CellPositionY);
        MoveDestinationPosition = CharacterFunctions.GetCharacterPositionOnGameTile(RemainingDestinationWaypoints.First.Value);

        //Remove first destination once set
        RemainingDestinationWaypoints.RemoveFirst();

        //TODO: Determine move sequence between Walk/JumpUp/Leap
        ChangeState(Walk);
    }
}

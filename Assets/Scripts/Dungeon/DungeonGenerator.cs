using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DG.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject _roomPrefab;
        [SerializeField] private ushort _desiredNumberOfRooms;
        [SerializeField] private Transform _dungeonParent;

        private Transform _transform;

        private const float XSteps = 18f;
        private const float YSteps = 9.25f;

        private Room _lastRoom;
        private Stack<Room> _dungeonRooms;

        private bool _canSpawn = true;

        public string DesiredNumberOfRooms {
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                var parsedValue = ushort.Parse(value);
                _desiredNumberOfRooms = (ushort) Mathf.Clamp(parsedValue, 0, 65535);
            }
        }

        private void Awake()
        {
            _transform = transform;
            _dungeonRooms = new Stack<Room>();
        }

        private void Start()
        {
            GenerateDungeon();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                GenerateDungeon();
        }

        public void GenerateDungeon()
        {
            ClearDungeon();

            // Iterate till the desired number of rooms is meet
            while (_dungeonRooms.Count < _desiredNumberOfRooms)
            {
                if (_dungeonRooms.Count == 0)
                    FirstRoom();

                // Calculate the next door we'll walk in
                var roomDoors = _lastRoom.doors;
                var visitedRoomDoors = _lastRoom.visitedDoorsToNeighborRooms.Keys.ToArray();
                
                var unvisitedRoomDoors = roomDoors.Except(visitedRoomDoors).ToArray();
                var anyUnvisitedRoomDoorsLeft = unvisitedRoomDoors.Length != 0;
                
                // Prioritize unvisited, if there's no unvisited pick a random from the available ones
                var selectedDoor = GetRandomRoomDoor(anyUnvisitedRoomDoorsLeft ? unvisitedRoomDoors : roomDoors);
                var neededDoor = GetDoorNeeded(selectedDoor);

                // Walk to the selected exit
                var foundRoom = WalkAndCheckSpawnPossibility(selectedDoor);

                if (!_canSpawn)
                {
                    FillLastAndNextRoomsData(foundRoom, selectedDoor, neededDoor);
                }
                else
                {
                    // Instantiate new room
                    var instantiatedRoom = Instantiate(_roomPrefab, _transform.position, Quaternion.identity,
                        _dungeonParent);
                    var instantiatedRoomComponent = instantiatedRoom.GetComponent<Room>();

                    FillLastAndNextRoomsData(instantiatedRoomComponent, selectedDoor, neededDoor);

                    _dungeonRooms.Push(_lastRoom);
                }
            }
        }

        private void ClearDungeon()
        {
            _transform.position = Vector3.zero;
            _dungeonRooms.Clear();
            for (var index = 0; index < _dungeonParent.childCount; index++)
                Destroy(_dungeonParent.GetChild(index).gameObject);
        }

        private void FirstRoom()
        {
            var instantiatedRoom = Instantiate(_roomPrefab, _transform.position, Quaternion.identity,
                _dungeonParent);
            _lastRoom = instantiatedRoom.GetComponent<Room>();

            _dungeonRooms.Push(_lastRoom);
        }

        private void FillLastAndNextRoomsData(Room nextRoom, OpenDoor selectedDoor, OpenDoor neededDoor)
        {
            // Fill the visitedDoorsToNeighborRooms data of the last and new/found rooms
            _lastRoom.visitedDoorsToNeighborRooms[selectedDoor] = nextRoom;
            nextRoom.visitedDoorsToNeighborRooms[neededDoor] = _lastRoom;

            // Open the door we came from
            nextRoom.OpenSelectedDoor(neededDoor);

            _lastRoom = nextRoom;
        }

        private OpenDoor GetDoorNeeded(OpenDoor selectedDoor)
        {
            var result = selectedDoor switch
            {
                OpenDoor.Top => OpenDoor.Down,
                OpenDoor.Left => OpenDoor.Right,
                OpenDoor.Right => OpenDoor.Left,
                OpenDoor.Down => OpenDoor.Top
            };

            return result;
        }

        private OpenDoor GetRandomRoomDoor(OpenDoor[] roomDoors)
        {
            return roomDoors[Random.Range(0, roomDoors.Length)];
        }

        private Room WalkAndCheckSpawnPossibility(OpenDoor selectedDoor)
        {
            var step = selectedDoor switch
            {
                OpenDoor.Top => new Vector3(0, YSteps, 0),
                OpenDoor.Left => new Vector3(-XSteps, 0, 0),
                OpenDoor.Right => new Vector3(XSteps, 0, 0),
                OpenDoor.Down => new Vector3(0, -YSteps, 0),
                _ => Vector3.zero
            };

            _lastRoom.OpenSelectedDoor(selectedDoor);
            
            _transform.position += step;

            // Check if the new position had a room to spawn a new one or not
            var foundRoom = _dungeonRooms.FirstOrDefault(room => room.position == _transform.position);
            _canSpawn = !foundRoom;

            return foundRoom;
        }
    }
}
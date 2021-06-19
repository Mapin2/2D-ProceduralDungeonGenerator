using System.Collections.Generic;
using UnityEngine;

namespace DG.Dungeon
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private GameObject _topDoor;
        [SerializeField] private GameObject _leftDoor;
        [SerializeField] private GameObject _rightDoor;
        [SerializeField] private GameObject _downDoor;
        public OpenDoor[] doors;
        
        private Dictionary<OpenDoor, GameObject> _openDoorToDoors;
        
        public Dictionary<OpenDoor, Room> visitedDoorsToNeighborRooms;
       
        public bool IsEndRoom => visitedDoorsToNeighborRooms.Keys.Count == 1;

        public Vector3 position; 

        private void Awake()
        {
            visitedDoorsToNeighborRooms = new Dictionary<OpenDoor, Room>();
            
            _openDoorToDoors = new Dictionary<OpenDoor, GameObject>
            {
                [OpenDoor.Top] = _topDoor,
                [OpenDoor.Left] = _leftDoor,
                [OpenDoor.Right] = _rightDoor,
                [OpenDoor.Down] = _downDoor
            };

            position = transform.position;
        }

        public void OpenSelectedDoor(OpenDoor selectedDoor)
        {
            _openDoorToDoors[selectedDoor].SetActive(false);
        }
    }
}

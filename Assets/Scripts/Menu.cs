using System;
using UnityEngine;

namespace DG
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenu;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !_mainMenu.activeSelf)
                _mainMenu.SetActive(true);
        }
    }
}

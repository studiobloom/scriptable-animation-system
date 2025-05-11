using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coherence.Toolkit;
using Coherence.Connection;

public class PlayerSpawnHandler : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CoherenceSync playerPrefabSync;  // Reference to the CoherenceSync component on the prefab

    private CoherenceBridge _coherenceBridge;
    private GameObject _playerReference;

    // Start is called before the first frame update
    private void OnEnable()
    {
        _coherenceBridge = FindFirstObjectByType<CoherenceBridge>();
        _coherenceBridge.onConnected.AddListener(OnConnected);
        _coherenceBridge.onDisconnected.AddListener(OnDisconnected);    
    }

    private void Start()
    {
        // Ensure the playerPrefabSync is assigned if not set in inspector
        if (playerPrefabSync == null && playerPrefab != null)
        {
            playerPrefabSync = playerPrefab.GetComponent<CoherenceSync>();
        }
    }

    private void OnConnected(CoherenceBridge bridge)
    {
        // Only spawn if we're the local client
        if (bridge.Client.IsConnected())
        {
            Vector3 spawnPosition = new Vector3(-6.53312731f, 0.5f, -0.0750164986f);
            
            _playerReference = Instantiate(playerPrefab, new Vector3(-6.53312731f, 0.5f, -0.0750164986f), transform.rotation);
            Debug.Log("Player instantiated: " + _playerReference.name);
            
            // Get the CoherenceSync component of the spawned instance
            CoherenceSync playerSync = _playerReference.GetComponent<CoherenceSync>();

            // Try multiple methods to find and enable the camera
            Camera playerCamera = _playerReference.GetComponentInChildren<Camera>(true); // true to include inactive objects
            if (playerCamera != null)
            {
                Debug.Log("Found camera: " + playerCamera.gameObject.name);
                playerCamera.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("No camera found in GetComponentInChildren");
                // Try finding camera in all children manually
                foreach (Transform child in _playerReference.GetComponentsInChildren<Transform>(true))
                {
                    Camera cam = child.GetComponent<Camera>();
                    if (cam != null)
                    {
                        Debug.Log("Found camera in manual search: " + child.name);
                        child.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnDisconnected(CoherenceBridge bridge, ConnectionCloseReason reason)
    {
        if (_playerReference != null)
        {
            Destroy(_playerReference);
            _playerReference = null;
        }
        Cursor.lockState = CursorLockMode.None;
    }
}

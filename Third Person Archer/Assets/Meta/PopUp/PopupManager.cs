using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    private class PopupRequest
    {
        public PopupType Type;
        public string Message;

        public PopupRequest(PopupType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
    
    [System.Serializable]
    public class PopupEntry
    {
        public PopupType Type;
        public GameObject Prefab;
    }

    [SerializeField] private List<PopupEntry> _popupEntries;
    [SerializeField] private Transform _popupContainer;
    [SerializeField] private Image _backgroundBlocker;
    private Queue<PopupRequest> _queue = new();
    private Dictionary<PopupType, GameObject> _popupPrefabs = new();
    private bool _isShowing = false;

    public static PopupManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePopupDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPopup(PopupType type, GameObject prefab)
    {
        if (!_popupPrefabs.ContainsKey(type))
        {
            _popupPrefabs[type] = prefab;
        }
    }

    public void EnqueuePopup(PopupType type, string message)
    {
        _queue.Enqueue(new PopupRequest(type, message));

        if (!_isShowing)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private void InitializePopupDictionary()
    {
        foreach (var entry in _popupEntries)
        {
            if (!_popupPrefabs.ContainsKey(entry.Type))
            {
                _popupPrefabs[entry.Type] = entry.Prefab;
            }
        }
    }

    private IEnumerator ProcessQueue()
    {
        _isShowing = true;
        _backgroundBlocker.gameObject.SetActive(true); 

        while (_queue.Count > 0)
        {
            var request = _queue.Dequeue();

            if (!_popupPrefabs.TryGetValue(request.Type, out var prefab))
            {
                Debug.LogError($"No popup prefab registered for type: {request.Type}");
                continue;
            }

            GameObject popupGO = Instantiate(prefab, _popupContainer);
            PopupView popup = popupGO.GetComponent<PopupView>();

            bool closed = false;
            popup.OnCloseRequested += () => closed = true;

            popup.Initialize(request.Message);

            while (!closed)
            {
                yield return null;
            }

            Destroy(popupGO);
        }

        _backgroundBlocker.gameObject.SetActive(false); 
        _isShowing = false;
    }
}


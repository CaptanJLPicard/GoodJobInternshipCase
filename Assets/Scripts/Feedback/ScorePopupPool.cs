// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - To begin, begin
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace GoodJobInternshipCase.Feedback
{
    /// <summary>
    /// Object pool for ScorePopup instances.
    /// Pre-allocates popups to avoid runtime instantiation.
    /// </summary>
    public class ScorePopupPool : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private ScorePopup _popupPrefab;
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private Transform _popupContainer;

        // Pool
        private List<ScorePopup> _pool;
        private int _nextAvailableIndex;

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the pool
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
                return;

            _pool = new List<ScorePopup>(_initialPoolSize);

            // Create container if not assigned
            if (_popupContainer == null)
            {
                GameObject container = new GameObject("ScorePopupContainer");
                container.transform.SetParent(transform);
                _popupContainer = container.transform;
            }

            // Pre-allocate popups
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreatePopup();
            }

            _nextAvailableIndex = 0;
            IsInitialized = true;
        }

        /// <summary>
        /// Get a popup from the pool
        /// </summary>
        public ScorePopup Get()
        {
            if (!IsInitialized)
                Initialize();

            // Find available popup
            int startIndex = _nextAvailableIndex;
            int poolSize = _pool.Count;

            for (int i = 0; i < poolSize; i++)
            {
                int index = (startIndex + i) % poolSize;
                ScorePopup popup = _pool[index];

                if (!popup.IsAnimating && !popup.gameObject.activeSelf)
                {
                    _nextAvailableIndex = (index + 1) % poolSize;
                    return popup;
                }
            }

            // No available popup, create new one
            ScorePopup newPopup = CreatePopup();
            return newPopup;
        }

        /// <summary>
        /// Return popup to pool (called automatically on animation complete)
        /// </summary>
        public void Return(ScorePopup popup)
        {
            popup.ResetPopup();
        }

        /// <summary>
        /// Return all popups to pool
        /// </summary>
        public void ReturnAll()
        {
            if (_pool == null)
                return;

            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null)
                {
                    _pool[i].ResetPopup();
                }
            }

            _nextAvailableIndex = 0;
        }

        private ScorePopup CreatePopup()
        {
            if (_popupPrefab == null)
            {
                Debug.LogError("ScorePopup prefab is not assigned!");
                return null;
            }

            ScorePopup popup = Instantiate(_popupPrefab, _popupContainer);
            popup.PoolIndex = _pool.Count;
            popup.gameObject.SetActive(false);
            popup.OnAnimationComplete += OnPopupComplete;

            _pool.Add(popup);
            return popup;
        }

        private void OnPopupComplete(ScorePopup popup)
        {
            Return(popup);
        }

        /// <summary>
        /// Set prefab at runtime
        /// </summary>
        public void SetPrefab(ScorePopup prefab)
        {
            _popupPrefab = prefab;
        }

        /// <summary>
        /// Set container at runtime
        /// </summary>
        public void SetContainer(Transform container)
        {
            _popupContainer = container;
        }
    }
}

// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - Begin to begin
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace GoodJobInternshipCase.Rendering
{
    /// <summary>
    /// Object pool for BlockVisual instances.
    /// Pre-allocates blocks to avoid runtime instantiation and GC pressure.
    /// </summary>
    public class BlockPool : MonoBehaviour
    {
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private Transform _poolContainer;

        private BlockVisual[] _pool;
        private Stack<int> _availableIndices;
        private int _poolSize;
        private bool _isInitialized;

        /// <summary>
        /// Initialize pool with specified capacity
        /// </summary>
        public void Initialize(int maxBlocks)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("BlockPool already initialized!");
                return;
            }

            _poolSize = maxBlocks;
            _pool = new BlockVisual[maxBlocks];
            _availableIndices = new Stack<int>(maxBlocks);

            // Create pool container if not assigned
            if (_poolContainer == null)
            {
                GameObject container = new GameObject("PoolContainer");
                container.transform.SetParent(transform);
                _poolContainer = container.transform;
            }

            // Pre-instantiate all blocks
            for (int i = maxBlocks - 1; i >= 0; i--)
            {
                GameObject go = Instantiate(_blockPrefab, _poolContainer);
                go.name = $"Block_{i}";
                go.SetActive(false);

                BlockVisual block = go.GetComponent<BlockVisual>();
                if (block == null)
                {
                    block = go.AddComponent<BlockVisual>();
                }

                block.PoolIndex = i;
                _pool[i] = block;
                _availableIndices.Push(i);
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Get a block from the pool
        /// </summary>
        public BlockVisual Get()
        {
            if (!_isInitialized)
            {
                Debug.LogError("BlockPool not initialized!");
                return null;
            }

            if (_availableIndices.Count == 0)
            {
                Debug.LogError("BlockPool exhausted! Consider increasing pool size.");
                return null;
            }

            int index = _availableIndices.Pop();
            BlockVisual block = _pool[index];
            block.gameObject.SetActive(true);
            block.ResetVisual();

            return block;
        }

        /// <summary>
        /// Return a block to the pool
        /// </summary>
        public void Return(BlockVisual block)
        {
            if (block == null)
                return;

            block.gameObject.SetActive(false);
            block.transform.SetParent(_poolContainer);
            block.ResetVisual();

            _availableIndices.Push(block.PoolIndex);
        }

        /// <summary>
        /// Return all active blocks to the pool
        /// </summary>
        public void ReturnAll()
        {
            _availableIndices.Clear();

            for (int i = _poolSize - 1; i >= 0; i--)
            {
                _pool[i].gameObject.SetActive(false);
                _pool[i].transform.SetParent(_poolContainer);
                _pool[i].ResetVisual();
                _availableIndices.Push(i);
            }
        }

        /// <summary>
        /// Get number of available blocks in pool
        /// </summary>
        public int AvailableCount => _availableIndices?.Count ?? 0;

        /// <summary>
        /// Get total pool size
        /// </summary>
        public int TotalSize => _poolSize;

        /// <summary>
        /// Check if pool is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Set block prefab (for runtime configuration)
        /// </summary>
        public void SetPrefab(GameObject prefab)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("Cannot change prefab after initialization!");
                return;
            }
            _blockPrefab = prefab;
        }
    }
}

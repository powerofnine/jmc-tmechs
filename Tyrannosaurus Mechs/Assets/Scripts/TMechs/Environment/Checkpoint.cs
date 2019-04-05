using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMechs.Data;
using TMechs.InspectorAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TMechs.Environment
{
    public class Checkpoint : MonoBehaviour
    {
        [Id("Unique Checkpoint ID")]
        public string id;
        public Vector3 anchorOffset;

        [Header("Visual")]
        public Color unsetColor = Color.red;
        public Color setColor = Color.green;

        public float transitionDuration = .5F;

        public Renderer statusRenderer;
        private static readonly int emissionColor = Shader.PropertyToID("_EmissiveColor");

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarningFormat("Checkpoint {0} does not have an ID, the checkpoint will not be remembered", name);
                id = Guid.NewGuid().ToString();
            }

            if (CheckpointRegistry.Instance.IsRegistered(id))
                throw new CheckpointIdNotUniqueException("Checkpoint ID of " + this + " not unique");

            CheckpointRegistry.Instance.Register(this);
        }

        private void Start() => UpdateState();

        private void OnTriggerEnter(Collider other)
        {
            CheckpointRegistry.Instance.Set(id);
        }

        private void UpdateState()
        {
            StopAllCoroutines();
            
            if (statusRenderer)
                StartCoroutine(Transition(CheckpointRegistry.Instance.IsActive(id) ? setColor : unsetColor));
        }

        private void MovePlayer()
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            
            if(!go)
            {
                Debug.LogError("Could not spawn player as the player could not be found");
                return;
            }

            go.transform.position = transform.position + anchorOffset;
        }
        
        private IEnumerator Transition(Color c)
        {
            Color old = statusRenderer.material.GetColor(emissionColor);

            float time = 0F;

            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                statusRenderer.material.SetColor(emissionColor, Color.Lerp(old, c, time / transitionDuration));
                yield return null;
            }
        }

        public class CheckpointIdNotUniqueException : Exception
        {
            public CheckpointIdNotUniqueException(string message) : base(message)
            {
            }
        }

        public class CheckpointRegistry : MonoBehaviour
        {
            public static CheckpointRegistry Instance
            {
                get
                {
                    if (!instance)
                    {
                        instance = new GameObject("Checkpoint Registry").AddComponent<CheckpointRegistry>();
                        instance.gameObject.transform.SetAsFirstSibling();
                    }

                    return instance;
                }
            }

            private static CheckpointRegistry instance;
            private readonly Dictionary<string, Checkpoint> checkpoints = new Dictionary<string, Checkpoint>();

            private string activeCheckpoint;

            public void Register(Checkpoint cp)
            {
                if (IsRegistered(cp.id))
                    return;
                checkpoints.Add(cp.id, cp);
            }

            [Pure]
            public bool IsRegistered(string id) => checkpoints.ContainsKey(id);

            public void Set(string id)
            {
                if (IsActive(id))
                    return;

                if (!IsRegistered(id))
                    return;

                SetUnsaved(id);

                SaveSystem.SaveData data = new SaveSystem.SaveData()
                {
                        sceneId = SceneManager.GetActiveScene().path,
                        checkpointId = id
                };
                
                // TODO: more descriptive text for meta
                SaveSystem.CreateSave(data, data.sceneId);
            }

            public void SetUnsaved(string id)
            {
                activeCheckpoint = id;
                foreach (Checkpoint cp in checkpoints.Values)
                    cp.UpdateState();
            }

            public void MovePlayerTo(string id)
            {
                if (!IsRegistered(id))
                    return;
                
                SetUnsaved(id);

                checkpoints[id].MovePlayer();
            }

            [Pure]
            public bool IsActive(string id) => IsRegistered(id) && id.Equals(activeCheckpoint);
        }
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;
using Zenject;

namespace MistRidge
{
    public class DeathManager : IInitializable, ITickable
    {
        private readonly Settings settings;
        private readonly Camera camera;
        private readonly InputManager inputManager;
        private readonly PlayerManager playerManager;
        private readonly DisplayManager displayManager;
        private readonly CheckpointActionSignal.Trigger checkpointActionTrigger;

        private bool isActive;
        private Dictionary<PlayerFacade, float> deathTimers;
        private Dictionary<PlayerFacade, bool> playerDeaths;

        public DeathManager(
                Settings settings,
                Camera camera,
                InputManager inputManager,
                PlayerManager playerManager,
                DisplayManager displayManager,
                CheckpointActionSignal.Trigger checkpointActionTrigger)
        {
            this.settings = settings;
            this.camera = camera;
            this.inputManager = inputManager;
            this.playerManager = playerManager;
            this.displayManager = displayManager;
            this.checkpointActionTrigger = checkpointActionTrigger;
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
            }
        }

        public List<PlayerFacade> AlivePlayerFacades
        {
            get
            {
                List<PlayerFacade> alivePlayerFacades = new List<PlayerFacade>();

                foreach (KeyValuePair<PlayerFacade, bool> entry in playerDeaths)
                {
                    if (!entry.Value)
                    {
                        alivePlayerFacades.Add(entry.Key);
                    }
                }

                return alivePlayerFacades;
            }
        }

        public List<PlayerFacade> DeadPlayerFacades
        {
            get
            {
                List<PlayerFacade> deadPlayerFacades = new List<PlayerFacade>();

                foreach (KeyValuePair<PlayerFacade, bool> entry in playerDeaths)
                {
                    if (entry.Value)
                    {
                        deadPlayerFacades.Add(entry.Key);
                    }
                }

                return deadPlayerFacades;
            }
        }

        public int AlivePlayerCount
        {
            get
            {
                return AlivePlayerFacades.Count;
            }
        }

        public List<Vector3> AlivePlayerGroundingPositions
        {
            get
            {
                List<Vector3> playerPositions = new List<Vector3>();
                foreach (PlayerFacade playerFacade in AlivePlayerFacades)
                {
                    playerPositions.Add(playerFacade.GroundingPosition);
                }

                return playerPositions;
            }
        }

        public List<Vector3> AliveRelevantPlayerGroundingPositions
        {
            get
            {
                return RelevantPositions(AlivePlayerGroundingPositions);
            }
        }

        public void Initialize()
        {
            isActive = false;
            deathTimers = new Dictionary<PlayerFacade, float>();
            playerDeaths = new Dictionary<PlayerFacade, bool>();
        }

        public void Tick()
        {
            if (!isActive)
            {
                return;
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

            foreach (Input input in inputManager.Inputs)
            {
                if (!playerManager.HasPlayerFacade(input))
                {
                    continue;
                }

                PlayerFacade playerFacade = playerManager.PlayerFacade(input);

                if (playerFacade == null || !playerDeaths.ContainsKey(playerFacade) || playerDeaths[playerFacade])
                {
                    continue;
                }

                if (GeometryUtility.TestPlanesAABB(planes, playerFacade.Bounds))
                {
                    DeathTimerReset(input, playerFacade);
                }
                else
                {
                    DeathTimerTick(input, playerFacade);
                }
            }
        }

        public void PopulatePlayerDeaths()
        {
            foreach (Input input in inputManager.Inputs)
            {
                AddPlayer(input);
            }
        }

        public void AddPlayer(Input input)
        {
            if (!playerManager.HasPlayerFacade(input))
            {
                return;
            }

            PlayerFacade playerFacade = playerManager.PlayerFacade(input);

            if (playerFacade == null)
            {
                return;
            }

            if (!deathTimers.ContainsKey(playerFacade))
            {
                deathTimers.Add(playerFacade, 0f);
            }

            if (!playerDeaths.ContainsKey(playerFacade))
            {
                playerDeaths.Add(playerFacade, false);
            }
        }

        public void DeathTimerTick(Input input, PlayerFacade playerFacade)
        {
            Vector3 position = camera.WorldToViewportPoint(playerFacade.Position);
            displayManager.UpdatePointer(input, position);

            if (deathTimers[playerFacade] == 0)
            {
                deathTimers[playerFacade] = Time.time;
            }

            if (Time.time - deathTimers[playerFacade] > settings.deathTimeLimit)
            {
                Kill(input, playerFacade);
                DeathTimerReset(input, playerFacade);
            }
        }

        public void DeathTimerReset(Input input, PlayerFacade playerFacade)
        {
            deathTimers[playerFacade] = 0;
            displayManager.UpdatePointer(input, Vector2.zero);
        }

        public void Kill(Input input, PlayerFacade playerFacade)
        {
            playerDeaths[playerFacade] = true;
            playerFacade.Die();
            displayManager.UpdatePointer(input, Vector2.zero);

            if (AlivePlayerCount == 0)
            {
                checkpointActionTrigger.Fire(CheckpointAction.Respawn);
            }
            else
            {
                checkpointActionTrigger.Fire(CheckpointAction.Finish);
            }
        }

        public void Respawn(PlayerFacade playerFacade)
        {
            playerDeaths[playerFacade] = false;
            playerFacade.Respawn();
        }

        private List<Vector3> RelevantPositions(List<Vector3> positions)
        {
            if (positions.Count == 0)
            {
                return positions;
            }

            positions.Sort((a, b) => a.y.CompareTo(b.y));

            Vector3 firstPlayerPosition = positions[positions.Count - 1];
            Vector3 lastPlayerPosition = positions[0];

            float playerDistance = (firstPlayerPosition - lastPlayerPosition).magnitude;
            float relevantDistance = Mathf.Max(settings.minRelevantDistance, settings.relevance * playerDistance);

            List<Vector3> relevantPositions = new List<Vector3>();
            foreach (Vector3 position in positions)
            {
                Vector3 firstPlayerDirection = firstPlayerPosition - position;
                float magnitude = firstPlayerDirection.magnitude + (firstPlayerDirection.y * settings.altitudeWeight);

                if (magnitude <= relevantDistance)
                {
                    relevantPositions.Add(position);
                }
                else
                {
                    float displacement = magnitude - relevantDistance;
                    relevantPositions.Add(Vector3.MoveTowards(position, firstPlayerPosition, settings.displacementWeight * displacement));
                }
            }

            return relevantPositions;
        }

        [Serializable]
        public class Settings
        {
            public float relevance;
            public float minRelevantDistance;
            public float displacementWeight;
            public float altitudeWeight;
            public float deathTimeLimit;
        }
    }
}

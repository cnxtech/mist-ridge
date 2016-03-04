using UnityEngine;
using System;
using System.Collections.Generic;
using Zenject;

namespace MistRidge
{
    public class AetherManager : IInitializable
    {
        private Dictionary<PlayerView, int> playerAethers;

        public void Initialize()
        {
            playerAethers = new Dictionary<PlayerView, int>();
        }

        public PlayerView LeadPlayerView
        {
            get
            {
                PlayerView leadPlayerView = null;
                int maxAethers = 0;

                foreach (KeyValuePair<PlayerView, int> entry in playerAethers)
                {
                    if (entry.Value > maxAethers)
                    {
                        leadPlayerView = entry.Key;
                        maxAethers = entry.Value;
                    }
                }

                return leadPlayerView;
            }
        }

        public int Aethers(PlayerView playerView)
        {
            return playerAethers[playerView];
        }

        public void AddAether(PlayerView playerView, int aetherCount)
        {
            if (!playerAethers.ContainsKey(playerView))
            {
                playerAethers.Add(playerView, 0);
            }

            playerAethers[playerView] += aetherCount;

            Debug.Log("player has " + playerAethers[playerView] + " aethers now");
        }
    }
}

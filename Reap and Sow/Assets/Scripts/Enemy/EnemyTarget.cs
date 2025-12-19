using UnityEngine;

namespace TargetUtils
{
    public static class EnemyTarget
    {
        private const string DECOY_TAG = "DecoyTag";
        private const string PLAYER_TAG = "PlayerTag";

        /// <summary>
        /// Find the nearest target, prioritizing decoys within range
        ///     if no decoys are in range, but player is then target player
        ///     else target decoy if they exist
        ///     else target player (or return null)
        /// </summary>
        /// <param name="position">position we are finding targets relative to</param>
        /// <param name="tauntRange"> preferred targeting distance (Prioritizes enemies within tauntrange)</param>
        /// <returns></returns>
        public static Transform GetTarget(Vector2 position, float tauntRange = float.MaxValue)
        {
            //determine the nearest decoy
            Transform nearestDecoy = null;
            float nearestDecoyDist = float.MaxValue;


            // Find the nearest decoy if one exists
            GameObject[] decoys = GameObject.FindGameObjectsWithTag(DECOY_TAG);
            foreach (GameObject decoy in decoys)
            {
                // Distance between decoy and current position
                float dist = Vector2.Distance(position, decoy.transform.position);
                // Log only if decoy is in range and is closer than all other decoys
                if (dist < nearestDecoyDist)
                {
                    nearestDecoy = decoy.transform;
                    nearestDecoyDist = dist;
                }
            }

            //if there is a decoy within range, target it
            if (nearestDecoyDist <= tauntRange)
            {
                return nearestDecoy;
            }

            //if that did not return, then we need more info about player
            Transform player = GameObject.FindGameObjectWithTag(PLAYER_TAG)?.transform;
            float playerDist = player != null ? Vector2.Distance(position, player.position) : float.MaxValue;

            //if player is in taunt range, target then return player
            if (playerDist <= tauntRange)
            {
                return player;
            }
            //else if ANY decoy exists target them
            else if (nearestDecoy != null)
            {
                return nearestDecoy;
            }
            //else target player
            else
            {
                return player;
            }
        }
    }
}
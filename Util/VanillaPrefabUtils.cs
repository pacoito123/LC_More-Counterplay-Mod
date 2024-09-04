using UnityEngine;

namespace MoreCounterplay.Util
{
	/// <summary>
	/// 	Helper methods for obtaining prefabs from vanilla objects.
	/// </summary>
	internal class VanillaPrefabUtils
	{
		/// <summary>
		/// 	Obtain the prefab of an enemy that spawns inside.
		/// </summary>
		/// <param name="enemyName">The name of the enemy to find.</param>
		/// <param name="enemyPrefab">The prefab of the specified enemy as an out parameter.</param>
		/// <returns>Whether the enemy prefab was found or not.</returns>/
		internal static bool GetInsideEnemyPrefab(string enemyName, out GameObject? enemyPrefab)
		{
			enemyPrefab = null;

			// Iterate through every moon.
			for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
			{
				// Find specified enemy type in the current moon's list of spawnable inside enemies.
				EnemyType? enemyType = StartOfRound.Instance.levels[i]?.Enemies?.Find(enemy =>
					string.CompareOrdinal(enemy.enemyType.enemyName, enemyName) == 0)?.enemyType;

				// Check if specified enemy was found.
				if (enemyType != null)
				{
					// Set out parameter to the found enemy prefab and exit method.
					enemyPrefab = enemyType.enemyPrefab;
					return true;
				}
			}

			MoreCounterplay.LogWarning($"Could not find enemy '{enemyName}' in any moon's list of spawns.");
			return false;
		}

		/// <summary>
		/// 	Obtain the prefab of an enemy that spawns outside.
		/// </summary>
		/// <param name="enemyName">The name of the enemy to find.</param>
		/// <param name="enemyPrefab">The prefab of the specified enemy as an out parameter.</param>
		/// <returns>Whether the enemy prefab was found or not.</returns>/
		internal static bool GetOutsideEnemyPrefab(string enemyName, out GameObject? enemyPrefab)
		{
			enemyPrefab = null;

			// Iterate through every moon.
			for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
			{
				// Find specified enemy type in the current moon's list of spawnable outside enemies.
				EnemyType? enemyType = StartOfRound.Instance.levels[i]?.OutsideEnemies?.Find(enemy =>
					string.CompareOrdinal(enemy.enemyType.enemyName, enemyName) == 0)?.enemyType;

				// Check if specified enemy was found.
				if (enemyType != null)
				{
					// Set out parameter to the found enemy prefab and exit method.
					enemyPrefab = enemyType.enemyPrefab;
					return true;
				}
			}

			MoreCounterplay.LogWarning($"Could not find enemy '{enemyName}' in any moon's list of spawns.");
			return false;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {

	[Header("Simulation")]
	public LayerMask groundLayerMask;
	public LayerMask placementCheckLayerMask;

	[Header("Simulation | Unit")]
	[M8.TagSelector]
	public string unitAllyTag;
	[M8.TagSelector]
	public string unitEnemyTag;

	public LayerMask unitLayerMask;

	[M8.SortingLayer]
	public string unitSortLayer;
	[M8.SortingLayer]
	public string unitGhostSortLayer;

	public Color unitGhostColor;

	public float unitPaletteSpawnDelay = 2f;

	public float unitFallSpeed = 10f;
	public float unitUpdateAIDelay = 0.3f;
	public float unitHurtDelay = 0.5f; //how long to stay in hurt state
	public float unitDyingDelay = 5f; //how long to stay in dying state
	public float unitIdleWanderDelay = 2f; //how long to stay in idle before moving to a new spot
	public float unitGatherDelay = 0.5f; //how long to 'act' before getting resource

	public M8.RangeFloat unitRetreatDistanceRange;
}

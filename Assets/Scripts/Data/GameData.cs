using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {

	[Header("Simulation")]
	public LayerMask groundLayerMask;
	public LayerMask placementCheckLayerMask;
}

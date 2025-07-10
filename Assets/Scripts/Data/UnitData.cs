using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "unit", menuName = "Game/Data/Unit")]
public class UnitData : GenericData {
	[Header("Population Info")]
	public PopulationData popData;
	public int popCount;

	[Header("Card Info")]
	public int cardDeployCost;
	public int cardUnitCount;

	[Header("Spawn Info")]
	public Unit spawnTemplate;

	[Header("Stats")]
	public int hitpoints;

	[SerializeField]
	float _moveSpeed = 1f;
	[SerializeField]
	float _moveSpeedDeviation = 0f;

	public float moveSpeed {
		get {
			if(_moveSpeedDeviation != 0f)
				return _moveSpeed + Random.Range(-_moveSpeedDeviation, _moveSpeedDeviation);
			else
				return _moveSpeed;
		}
	}
}

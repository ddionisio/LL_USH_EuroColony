using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "generic", menuName = "Game/Data/Generic")]
public class GenericData : ScriptableObject {
	[Header("Info")]
	[M8.Localize]
	public string nameRef;
	[M8.Localize]
	public string descRef;

	public Sprite icon;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalBattle : M8.ModalController, M8.IModalPush, M8.IModalPop {
	public enum Mode {
		None,
		Deploy,
		Play
	}

	[Header("Card")]
	public UnitCardWidget cardTemplate;
	public int cardCapacity = 8;
	public RectTransform cardContainerRoot;

	[Header("Population")]
	public PopulationWidget playerPopDisplay;
	public PopulationWidget enemyPopDisplay;

	[Header("POI")]
	public POIOffscreenWidget playerPOIWidget;
	public POIOffscreenWidget enemyPOIWidget;

	[Header("Deploy Counter")]
	public Image deployCountFillImage;
	public Image deployCostFillImage; //put this behind count fill

	[Header("Drag")]
	public GameObject dragRootGO;
	public RectTransform dragRemoveRoot; //area where we remove/recall or cancel unit placement

	void M8.IModalPop.Pop() {
		
	}

	void M8.IModalPush.Push(M8.GenericParams parms) {
		
	}
}

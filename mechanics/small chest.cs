datablock TSShapeConstructor(SmallChestDTS) {
	baseShape = "./shapes/small chest.dts";
};

datablock PlayerData(SmallChestArmor : PlayerStandardArmor)  {
	shapeFile = SmallChestDTS.baseShape;
	uiName = "";

	canJet = false;
	jumpForce = 0;
	boundingBox = "5 5 2";

	isInteractable = true;
	minImpactSpeed = 0;
	isChest = true;
};

function SmallChestArmor::onInteract(%this, %obj, %interactee) {
	Armor::onChestInteract(%this, %obj, %interactee);
}
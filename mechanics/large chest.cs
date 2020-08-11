datablock TSShapeConstructor(LargeChestDTS) {
	baseShape = "./shapes/large chest.dts";
};

datablock PlayerData(LargeChestArmor : PlayerStandardArmor)  {
	shapeFile = LargeChestDTS.baseShape;
	uiName = "";

	canJet = false;
	jumpForce = 0;
	boundingBox = "5 5 4";

	isInteractable = true;
	minImpactSpeed = 0;
	isChest = true;
};

function LargeChestArmor::onInteract(%this, %obj, %interactee) {
	Armor::onChestInteract(%this, %obj, %interactee);
}
datablock TSShapeConstructor(MouseDTS) {
	baseShape = "./shapes/mouse.dts";
};

datablock PlayerData(MouseArmor : PlayerStandardArmor)  {
	shapeFile = MouseDTS.baseShape;
	emap = "1";
	boundingBox = "4 4 4";
	crouchBoundingBox = "4 4 4";
	maxDamage = 25;
	canRide = false;
	
	uiName = "Mouse ";
};
datablock ItemData(HealthFlaskItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/flask.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Health Flask";
	iconName = "./";
	doColorShift = false;

	image = HealthFlaskImage;
	canDrop = true;
};

datablock ShapeBaseImageData(HealthFlaskImage) {
	shapeFile = "./shapes/flask.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";
	correctMuzzleVector = true;

	className = "WeaponImage";

	item = HealthFlaskItem;
	ammo = " ";

	melee = false;
	armReady = true;

	doColorShift = false;
	colorShiftColor = HealthFlaskItem.colorShiftColor;

	isPotion = true;
	maxFluid = 150;
	sipAmount = 15;
	gulpAmount = 50;
};

function HealthFlaskImage::drinkPotion(%this, %obj, %amount) {
	%obj.addHealthFromPotion(%amount);
	%obj.potionFluid[%this, %obj.currTool] -= %amount;

	%obj.playThread(0, "shiftAway");
}
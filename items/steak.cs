datablock ItemData(SteakItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/steak.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Steak";
	iconName = "./";
	doColorShift = false;

	isPickup = true;
	pickupHealth = 25;
	pickupSound = EatFoodSound;
};

addPickupItem(SteakItem);
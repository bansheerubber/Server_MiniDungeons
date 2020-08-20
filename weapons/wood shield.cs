datablock ItemData(WoodShieldItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/wood shield2.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Schild";
	iconName = "./";
	doColorShift = false;

	image = WoodShieldImage;
	canDrop = true;
};

datablock ShapeBaseImageData(WoodShieldImage) {
	shapeFile = "./shapes/wood shield2.dts";
	emap = true;
	mountPoint = 1;
	offset = "0 0 0";
	eyeOffset = "0 0 0";
	rotation = eulerToMatrix("0 0 0");
	correctMuzzleVector = true;
	className = "WeaponImage";
	item = WoodShieldItem;

	melee = false;
	armReady = false;

	doColorShift = false;

	parryCooldown						= 500;
	parryThread 						= "shield1Block";
	parryIsShield						= true;

	parryShieldHealth = 200;
	parryShieldIncrease = 1; // how much health we regenerate per shield tick (100 ms)
	parryShieldDecrease = 1; // how much health we lose per shield tick (100 ms)

	parryStunDurationSuccess			= 0; // how long targets are stunned for
	parryDamageSuccess					= 0;
	parrySelfImpactImpulseSuccess 		= 0;
	parrySelfVerticalImpulseSuccess 	= 0;
	parryProgressiveKnockbackSuccess 	= 0 SPC 0;

	parryStunDuration					= 0;
	parryDamage							= 0;
	parrySelfImpactImpulse				= 0;
	parrySelfVerticalImpulse 			= 0;
	parryProgressiveKnockback			= 0 SPC 0;

	

	stateName[0]					= "Activate";
	stateSound[0]					= WeaponSwitchSound;
	stateTimeoutValue[0]			= 0.01;
	stateTransitionOnTimeout[0]		= "IdleAnimation";
	stateSequence[0]				= "root";
	
	stateName[1]					= "Idle";
	stateTransitionOnLoaded[1]		= "GuardAnimation";

	stateName[2]					= "GuardAnimation";
	stateSequence[2]				= "hold";
	stateTimeoutValue[2]			= 0.1;
	stateTransitionOnTimeout[2]		= "Guard";

	stateName[3]					= "Guard";
	stateScript[3]					= "onShieldGuard";
	stateTransitionOnNotLoaded[3]	= "IdleAnimation";

	stateName[4]					= "IdleAnimation";
	stateSequence[4]				= "root";
	stateTimeoutValue[4]			= 0.1;
	stateTransitionOnTimeout[4]		= "Idle";
};
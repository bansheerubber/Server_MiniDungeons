datablock ItemData(DoubleHandedItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/double handed sword.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Korbschwert";
	iconName = "./";
	doColorShift = false;

	sword = DoubleHandedSwordArmor;
	canDrop = true;
};

datablock TSShapeConstructor(DoubleHandedSwordDTS) {
	baseShape = "./shapes/double handed sword.dts";
};

datablock PlayerData(DoubleHandedSwordArmor : PlayerStandardArmor)  {
	shapeFile = DoubleHandedSwordDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
	item = DoubleHandedItem;


	swordStartMount[0] 					= 0; // start point for raycast/interpolation
	swordEndMount[0] 					= 1; // end point for raycast/interpolation
	swordStepInterpolationCount 		= 3; // how many linear interpolations we do between steps, based on distance
	swordStepTick						= 33; // how fast we do sword stepping

	swordInterpolationDistance[0, 0]	= 1;
	swordInterpolationRadius[0, 0]		= 0.75;

	swordStopSwingOnBrickHit			= false; // whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)


	// GUARD CYCLE: M -> L -> M
	swordMaxCycles = 3;
	swordDoubleHanded = true;
	
	// middle attack 1, just a standard attack
	swordCycle[0] 					= "middle left";
	swordCycleThread[0] 			= "mid3";
	swordCycleThreadSlot[0]			= 1;
	swordCycleDamage[0]				= 60;
	swordCycleImpactImpulse[0]		= 300;
	swordCycleVerticalImpulse[0]	= 500;
	swordCycleHitExplosion[0]		= MidHitProjectile;
	swordCycleHitSound[0]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[0]			= 0.7;
	swordCycleGuardSound[0]			= CycleMidReadySound;
	swordCycleSwingSound[0]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[0]			= 4;

	// low attack, sends the opponent flying
	swordCycle[1] 					= "low right";
	swordCycleThread[1] 			= "low1";
	swordCycleThreadSlot[1]			= 1;
	swordCycleDamage[1]				= 35;
	swordCycleImpactImpulse[1]		= 1500;
	swordCycleVerticalImpulse[1]	= 800;
	swordCycleHitExplosion[1]		= LowHitProjectile;
	swordCycleHitSound[1]			= LowHit1Sound;
	swordCyclePrepTime[1]			= 0.3;
	swordCycleGuardSound[1]			= CycleLowReadySound;
	swordCycleSwingSound[1]			= LowSwingSound;
	swordCycleSwingEmitter[1]		= HighFlareImage;
	swordCycleSwingEmitterTime[1]	= 120;
	swordCycleSwingEmitterSlot[1]	= 1;
	swordCycleSwingSteps[1]			= 5;

	swordCycle[2] 					= "middle right";
	swordCycleThread[2] 			= "mid4";
	swordCycleThreadSlot[2]			= 1;
	swordCycleDamage[2]				= 60;
	swordCycleImpactImpulse[2]		= 300;
	swordCycleVerticalImpulse[2]	= 500;
	swordCycleHitExplosion[2]		= MidHitProjectile;
	swordCycleHitSound[2]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[2]			= 0.7;
	swordCycleGuardSound[2]			= CycleMidReadySound;
	swordCycleSwingSound[2]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[2]			= 4;

	// parry information
	parryCooldown						= 1500;
	parryDuration						= 900;
	parryThread 						= "parry1";

	parryStunDurationSuccess			= 500; // how long targets are stunned for
	parryDamageSuccess					= 10;
	parrySelfImpactImpulseSuccess 		= 350;
	parrySelfVerticalImpulseSuccess 	= 350;
	parryProgressiveKnockbackSuccess 	= 15 SPC 15;

	parryStunDuration					= 0;
	parryDamage							= 0;
	parrySelfImpactImpulse				= 800;
	parrySelfVerticalImpulse 			= 800;
	parryProgressiveKnockback			= 8 SPC 8;
};
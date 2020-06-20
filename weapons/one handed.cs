datablock ItemData(OneHandedItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/one handed.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Ritterschwert";
	iconName = "./";
	doColorShift = false;

	sword = OneHandedSwordArmor;
	canDrop = true;
};

datablock TSShapeConstructor(OneHandedSwordDTS) {
	baseShape = "./shapes/one handed.dts";
};

datablock PlayerData(OneHandedSwordArmor : PlayerStandardArmor)  {
	shapeFile = OneHandedSwordDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
	item = OneHandedItem;

	swordStartMount[0] 					= 0; // start point for raycast/interpolation
	swordEndMount[0] 					= 1; // end point for raycast/interpolation
	swordStepInterpolationCount 		= 3; // how many linear interpolations we do between steps, based on distance
	swordStepTick						= 33; // how fast we do sword stepping

	swordInterpolationDistance[0, 0]	= 1;
	swordInterpolationRadius[0, 0]		= 0.9;

	swordStopSwingOnBrickHit			= false; // whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)


	// GUARD CYCLE: M -> H -> M
	swordMaxCycles = 3;
	
	// middle attack 1, just a standard attack
	swordCycle[0] 					= "middle right";
	swordCycleThread[0] 			= "mid2";
	swordCycleThreadSlot[0]			= 1;
	swordCycleDamage[0]				= 35;
	swordCycleImpactImpulse[0]		= 500;
	swordCycleVerticalImpulse[0]	= 500;
	swordCycleHitExplosion[0]		= MidHitProjectile;
	swordCycleHitSound[0]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[0]			= 0.2;
	swordCycleGuardSound[0]			= CycleMidReadySound;
	swordCycleSwingSound[0]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[0]			= 5;

	// high attack, does big damage
	swordCycle[1] 					= "high left";
	swordCycleThread[1] 			= "high1";
	swordCycleThreadSlot[1]			= 1;
	swordCycleDamage[1]				= 60;
	swordCycleImpactImpulse[1]		= 600;
	swordCycleVerticalImpulse[1]	= 800;
	swordCycleHitExplosion[1]		= HighHitProjectile;
	swordCycleHitSound[1]			= HighHit1Sound;
	swordCyclePrepTime[1]			= 0.8;
	swordCycleGuardSound[1]			= CycleHighReadySound;
	swordCycleSwingSound[1]			= HighSwing1Sound TAB HighSwing2Sound TAB HighSwing3Sound;
	swordCycleSwingEmitter[1]		= HighFlareImage;
	swordCycleSwingEmitterTime[1]	= 66;
	swordCycleSwingEmitterSlot[1]	= 1;
	swordCycleSwingSteps[1]			= 5;

	// middle attack 2, just a standard attack
	swordCycle[2] 					= "middle left";
	swordCycleThread[2] 			= "mid1";
	swordCycleThreadSlot[2]			= 1;
	swordCycleDamage[2]				= 35;
	swordCycleImpactImpulse[2]		= 500;
	swordCycleVerticalImpulse[2]	= 500;
	swordCycleHitExplosion[2]		= MidHitProjectile;
	swordCycleHitSound[2]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[2]			= 0.2;
	swordCycleGuardSound[2]			= CycleMidReadySound;
	swordCycleSwingSound[2]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[2]			= 5;

	// parry information
	parryCooldown						= 1500;
	parryDuration						= 700;
	parryThread 						= "parry1";

	parryStunDurationSuccess			= 700; // how long targets are stunned for
	parryDamageSuccess					= 25;
	parrySelfImpactImpulseSuccess 		= 450;
	parrySelfVerticalImpulseSuccess 	= 450;
	parryProgressiveKnockbackSuccess 	= 12 SPC 12;

	parryStunDuration					= 300;
	parryDamage							= 0;
	parrySelfImpactImpulse				= 800;
	parrySelfVerticalImpulse 			= 800;
	parryProgressiveKnockback			= 8 SPC 8;
};
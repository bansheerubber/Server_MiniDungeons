datablock AudioProfile(GladiusTelegraph1Sound) {
	filename    = "./sounds/gladius telegraph1.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(GladiusTelegraph2Sound) {
	filename    = "./sounds/gladius telegraph2.wav";
	description = AudioClose3d;
	preload = true;
};

datablock ItemData(GladiusSwordItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/gladius.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Offis Badius";
	iconName = "./";
	doColorShift = false;

	sword = GladiusSwordArmor;
	canDrop = true;
};

datablock TSShapeConstructor(GladiusSwordDTS) {
	baseShape = "./shapes/gladius.dts";
};

datablock PlayerData(GladiusSwordArmor : PlayerStandardArmor)  {
	shapeFile = GladiusSwordDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
	item = GladiusSwordItem;

	swordStartMount[0] 					= 0; // start point for raycast/interpolation
	swordEndMount[0] 					= 1; // end point for raycast/interpolation
	swordStepInterpolationCount 		= 3; // how many linear interpolations we do between steps, based on distance
	swordStepTick						= 33; // how fast we do sword stepping

	swordInterpolationDistance[0, 0]	= 1;
	swordInterpolationRadius[0, 0]		= 0.5;

	swordStopSwingOnBrickHit			= false; // whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)


	// GUARD CYCLE: M M H H
	swordMaxCycles = 4;
	
	// middle attack 1
	swordCycle[0] 					= "middle right";
	swordCycleThread[0] 			= "mid2";
	swordCycleThreadSlot[0]			= 1;
	swordCycleDamage[0]				= 15;
	swordCycleImpactImpulse[0]		= 0;
	swordCycleVerticalImpulse[0]	= 0;
	swordCycleHitExplosion[0]		= MidHitProjectile;
	swordCycleHitSound[0]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[0]			= 0.5;
	swordCycleGuardSound[0]			= CycleMidReadySound;
	swordCycleSwingSound[0]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[0]			= 5;
	swordCycleSwingSlot[0]			= 1;
	swordCycleTelegraphSound[0]		= GladiusTelegraph1Sound TAB GladiusTelegraph2Sound;
	swordCycleTelegraphThread[0]	= "mid2";
	swordCycleTelegraphThreadSlot[0] = 1;
	swordCycleTelegraphWaitTime[0]	= 0.2;

	// middle attack 2
	swordCycle[1] 					= "middle left";
	swordCycleThread[1] 			= "mid1";
	swordCycleThreadSlot[1]			= 1;
	swordCycleDamage[1]				= 15;
	swordCycleImpactImpulse[1]		= 0;
	swordCycleVerticalImpulse[1]	= 0;
	swordCycleHitExplosion[1]		= MidHitProjectile;
	swordCycleHitSound[1]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[1]			= 0.1;
	swordCycleGuardSound[1]			= CycleMidReadySound;
	swordCycleSwingSound[1]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[1]			= 5;
	swordCycleSwingSlot[1]			= 2;

	// high attack 1
	swordCycle[2] 					= "high left";
	swordCycleThread[2] 			= "high3";
	swordCycleThreadSlot[2]			= 1;
	swordCycleDamage[2]				= 25;
	swordCycleImpactImpulse[2]		= 200;
	swordCycleVerticalImpulse[2]	= 1000;
	swordCycleHitExplosion[2]		= HighHitProjectile;
	swordCycleHitSound[2]			= HighHit1Sound;
	swordCyclePrepTime[2]			= 0.1;
	swordCycleGuardSound[2]			= CycleHighReadySound;
	swordCycleSwingSound[2]			= HighSwing1Sound TAB HighSwing2Sound TAB HighSwing3Sound;
	swordCycleSwingEmitter[2]		= HighFlareImage;
	swordCycleSwingEmitterTime[2]	= 100;
	swordCycleSwingEmitterSlot[2]	= 1;
	swordCycleSwingSteps[2]			= 5;
	swordCycleSwingSlot[2]			= 1;

	// high attack 2
	swordCycle[3] 					= "high left";
	swordCycleThread[3] 			= "high4";
	swordCycleThreadSlot[3]			= 1;
	swordCycleDamage[3]				= 25;
	swordCycleImpactImpulse[3]		= 200;
	swordCycleVerticalImpulse[3]	= 1000;
	swordCycleHitExplosion[3]		= HighHitProjectile;
	swordCycleHitSound[3]			= HighHit1Sound;
	swordCyclePrepTime[3]			= 0.1;
	swordCycleGuardSound[3]			= CycleHighReadySound;
	swordCycleSwingSound[3]			= HighSwing1Sound TAB HighSwing2Sound TAB HighSwing3Sound;
	swordCycleSwingEmitter[3]		= HighFlareImage;
	swordCycleSwingEmitterTime[3]	= 100;
	swordCycleSwingEmitterSlot[3]	= 1;
	swordCycleSwingSteps[3]			= 5;
	swordCycleSwingSlot[3]			= 2;
};

function GladiusSwordArmor::parryReceive(%this, %colImage, %obj, %slot, %col, %direction, %tangent, %isRadius, %index, %radiusIndex) {
	%obj.swordCurrentCycle[%this] = -1; // reset the cycle
	%obj.setSwordTrigger(%slot, false); // stop firing
	
	Parent::parryReceive(%this, %colImage, %obj, %slot, %col, %direction, %tangent, %isRadius, %index, %radiusIndex);
}
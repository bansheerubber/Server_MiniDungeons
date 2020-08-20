datablock ItemData(AiArmingSwordItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/ai rapier2.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "The Porcupines Sacrifice";
	iconName = "./";
	doColorShift = false;

	sword = AiArmingSwordArmor;
	canDrop = true;
};

datablock TSShapeConstructor(AiArmingSwordDTS) {
	baseShape = "./shapes/ai rapier2.dts";
};

datablock PlayerData(AiArmingSwordArmor : PlayerStandardArmor)  {
	shapeFile = AiArmingSwordDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
	item = AiArmingSwordItem;

	swordStartMount[0] 						= 0; // start point for raycast/interpolation
	swordEndMount[0] 							= 1; // end point for raycast/interpolation
	swordStepInterpolationCount 	= 3; // how many linear interpolations we do between steps, based on distance
	swordStepTick									= 33; // how fast we do sword stepping

	swordInterpolationDistance[0, 0]	= 1;
	swordInterpolationRadius[0, 0]		= 1.5;

	swordStopSwingOnBrickHit			= false; // whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)

	swordMaxCycles = 2;
	
	swordCycle[0] 								= "high right";
	swordCycleThread[0] 					= "high4";
	swordCycleThreadSlot[0]				= 1;
	swordCycleDamage[0]						= 50;
	swordCycleImpactImpulse[0]		= 200;
	swordCycleVerticalImpulse[0]	= 1000;
	swordCycleHitExplosion[0]			= HighHitProjectile;
	swordCycleHitSound[0]					= HighHit1Sound;
	swordCyclePrepTime[0]					= 50;
	swordCycleGuardSound[0]				= CycleHighReadySound;
	swordCycleSwingSound[0]				= HighSwing1Sound TAB HighSwing2Sound TAB HighSwing3Sound;
	swordCycleSwingEmitter[0]			= HighFlareImage;
	swordCycleSwingEmitterTime[0]	= 100;
	swordCycleSwingEmitterSlot[0]	= 1;
	swordCycleSwingSteps[0]				= 5;
	swordCycleSwingSlot[0]				= 2;

	swordCycle[1] 								= "high left";
	swordCycleThread[1] 					= "high3";
	swordCycleThreadSlot[1]				= 1;
	swordCycleDamage[1]						= 50;
	swordCycleImpactImpulse[1]		= 200;
	swordCycleVerticalImpulse[1]	= 1000;
	swordCycleHitExplosion[1]			= HighHitProjectile;
	swordCycleHitSound[1]					= HighHit1Sound;
	swordCyclePrepTime[1]					= 50;
	swordCycleGuardSound[1]				= CycleHighReadySound;
	swordCycleSwingSound[1]				= HighSwing1Sound TAB HighSwing2Sound TAB HighSwing3Sound;
	swordCycleSwingEmitter[1]			= HighFlareImage;
	swordCycleSwingEmitterTime[1]	= 100;
	swordCycleSwingEmitterSlot[1]	= 1;
	swordCycleSwingSteps[1]				= 5;
	swordCycleSwingSlot[1]				= 2;
};

function AiArmingSwordSwordArmor::parryReceive(%this, %colImage, %obj, %slot, %col, %direction, %tangent, %isRadius, %index, %radiusIndex) {
	%obj.swordCurrentCycle[%this] = -1; // reset the cycle
	%obj.setSwordTrigger(%slot, false); // stop firing
	
	Parent::parryReceive(%this, %colImage, %obj, %slot, %col, %direction, %tangent, %isRadius, %index, %radiusIndex);
}
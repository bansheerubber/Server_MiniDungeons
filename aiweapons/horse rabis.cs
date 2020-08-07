datablock TSShapeConstructor(HorseRabisSwordDts) {
	baseShape = "base/data/shapes/empty.dts";
};

datablock PlayerData(HorseRabisSwordArmor : PlayerStandardArmor)  {
	shapeFile = HorseRabisSwordDts.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";

	swordMountSlot = 3;

	swordStartMount[0] 					= 0; // start point for raycast/interpolation
	swordEndMount[0] 					= 0; // end point for raycast/interpolation
	swordStepInterpolationCount 		= 0; // how many linear interpolations we do between steps, based on distance
	swordStepTick						= 33; // how fast we do sword stepping

	swordInterpolationDistance[0, 0]	= 0;
	swordInterpolationRadius[0, 0]		= 2;

	swordStopSwingOnBrickHit			= false; // whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)


	swordMaxCycles = 1;
	
	swordCycle[0] 					= "middle alldirections";
	swordCycleDamage[0]				= 25;
	swordCycleImpactImpulse[0]		= 800;
	swordCycleVerticalImpulse[0]	= 1000;
	swordCycleHitExplosion[0]		= HighHitProjectile;
	swordCycleHitSound[0]			= HighHit1Sound;
	swordCyclePrepTime[0]			= 0.8;
	swordCycleSwingSteps[0]			= 5;
};
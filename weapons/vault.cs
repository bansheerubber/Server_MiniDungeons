datablock ItemData(VaultSwordItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/staff.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Upstick";
	iconName = "./";
	doColorShift = false;

	sword = VaultSwordArmor;
	canDrop = true;
};

datablock TSShapeConstructor(VaultSwordDTS) {
	baseShape = "./shapes/staff.dts";
};

datablock PlayerData(VaultSwordArmor : PlayerStandardArmor)  {
	shapeFile = VaultSwordDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
	item = VaultSwordItem;


	swordStartMount[0] 					= 0; // start point for raycast/interpolation
	swordEndMount[0] 					= 1; // end point for raycast/interpolation
	swordStepInterpolationCount 		= 1; // how many linear interpolations we do between steps, based on distance
	swordStepTick						= 33; // how fast we do sword stepping

	swordInterpolationDistance[0, 0]	= 1;
	swordInterpolationRadius[0, 0]		= 1;

	swordStopSwingOnBrickHit			= false; // whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)


	// GUARD CYCLE: M -> L -> M
	swordMaxCycles = 1;
	swordDoubleHanded = true;
	
	// middle attack 1, just a standard attack
	swordCycle[0] 					= "middle left";
	swordCycleThread[0] 			= "mid4";
	swordCycleThreadSlot[0]			= 1;
	swordCycleDamage[0]				= 60;
	swordCycleImpactImpulse[0]		= 0;
	swordCycleVerticalImpulse[0]	= 0;
	swordCycleHitExplosion[0]		= MidHitProjectile;
	swordCycleHitSound[0]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[0]			= 0.01;
	swordCycleGuardSound[0]			= CycleMidReadySound;
	swordCycleSwingSound[0]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[0]			= 4;

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

	specialMethod						= "startPoleVault";
	specialConditionalMethod			= "canStartPoleVault";
	specialCooldown						= 2000;
	specialDuration						= 1000;
};

function VaultSwordArmor::canStartPoleVault(%this, %obj, %slot) {
	return %obj.isGrounded;
}

function VaultSwordArmor::startPoleVault(%this, %obj, %slot) {
	%obj.playThread(1, "poleVault");
	%obj.schedule(33, stopThread, 1);

	%obj.forceNormalHands = true;
	%obj.swordCycleFrozen = true;
	%obj.client.applyBodyParts();
	%obj.setLookLimits(0.5, 0.5);

	%obj.schedule(100, poleVaultLoop, 10);
}

function Player::poleVaultLoop(%this, %ticks) {
	if(!isObject(%this.antiGravityZone)) {
		%this.antiGravityZone = new PhysicalZone() {
			position = %this.getPosition();
			velocityMod = "1";
			gravityMod = "0";
			extraDrag = "0";
			isWater = "0";
			waterViscosity = "0";
			waterDensity = "0";
			waterColor = "0.200000 0.600000 0.600000 0.300000";
			appliedForce = "0 0 0";
			polyhedron = "0.0 0.0 0.0 1.0 0.0 0.0 0.0 -1.0 0.0 0.0 0.0 1.0";
			owner = %this;
		};
		%this.vaultForwardVector = %this.getForwardVector();
		%this.playThread(1, "poleVault");
	}
	
	if(%ticks > 0) {
		%velocity = vectorAdd(vectorScale(%this.vaultForwardVector, 15), "0 0 14");
		%this.schedule(33, setVelocity, %velocity);
		%this.poleVaultSchedule = %this.schedule(33, poleVaultLoop, %ticks - 1);
	}
	else {
		%this.antiGravityZone.delete();
		%this.schedule(200, stopPoleVault);
	}
}

function Player::stopPoleVault(%this) {
	%this.swordCycleFrozen = false;
	%this.forceNormalHands = false;
	%this.client.applyBodyParts();
	%this.setLookLimits(1, 0);
	%this.sword[0].getDatablock().forceCycleGuard(%this, 0, 0);
}

deActivatePackage(Vaulting);
package Vaulting {
	function Armor::onRemove(%this, %obj) {
		if(isObject(%obj.antiGravityZone)) {
			%obj.antiGravityZone.delete();
		}
		Parent::onRemove(%this, %obj);
	}
};
activatePackage(Vaulting);
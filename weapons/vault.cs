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
	swordCycleCrit[0]				= false;
	swordCycleImpactImpulse[0]		= 0;
	swordCycleVerticalImpulse[0]	= 0;
	swordCycleHitExplosion[0]		= MidHitProjectile;
	swordCycleHitSound[0]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[0]			= 0.7;
	swordCycleGuardSound[0]			= CycleMidReadySound;
	swordCycleSwingSound[0]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[0]			= 4;

	// special attack 1, crits
	swordCycle[1] 					= "middle left";
	swordCycleThread[1] 			= "mid4";
	swordCycleThreadSlot[1]			= 1;
	swordCycleDamage[1]				= 25;
	swordCycleCrit[1]				= false;
	swordCycleImpactImpulse[1]		= 0;
	swordCycleVerticalImpulse[1]	= 0;
	swordCycleHitExplosion[1]		= HighHitProjectile;
	swordCycleHitSound[1]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[1]			= 0.01;
	swordCycleGuardSound[1]			= CycleMidReadySound;
	swordCycleSwingSound[1]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[1]			= 4;

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

	%obj.schedule(150, poleVaultLoop, 10);

	%this.setCycleRange(%obj, %slot, 1, 1);
}

function VaultSwordArmor::poleVaultLoop(%this, %obj, %slot, %ticks) {
	if(!isObject(%obj.antiGravityZone)) {
		%obj.antiGravityZone = new PhysicalZone() {
			position = %obj.getPosition();
			velocityMod = "1";
			gravityMod = "0";
			extraDrag = "0";
			isWater = "0";
			waterViscosity = "0";
			waterDensity = "0";
			waterColor = "0.200000 0.600000 0.600000 0.300000";
			appliedForce = "0 0 0";
			polyhedron = "0.0 0.0 0.0 1.0 0.0 0.0 0.0 -1.0 0.0 0.0 0.0 1.0";
			owner = %obj;
		};
		%obj.vaultForwardVector = %obj.getForwardVector();
		%obj.playThread(1, "poleVault");
	}
	
	if(%ticks > 0) {
		%velocity = vectorAdd(vectorScale(%obj.vaultForwardVector, 15), "0 0 14");
		%obj.schedule(33, setVelocity, %velocity);
		%obj.poleVaultSchedule = %obj.schedule(33, poleVaultLoop, %obj, %slot, %ticks - 1);
	}
	else {
		%obj.antiGravityZone.delete();
		%obj.poleVaultSchedule = %this.schedule(200, stopPoleVault, %obj, %slot);
	}
}

function VaultSwordArmor::stopPoleVault(%this, %obj, %slot) {
	%obj.swordCycleFrozen = false;
	%obj.forceNormalHands = false;
	%obj.client.applyBodyParts();
	%obj.setLookLimits(1, 0);
	%this.forceCycleGuard(%this, 1, 1);

	%this.waitSchedule(100, "resetPoleVault", %obj, %slot, true).addCondition(%obj, "isGrounded", true);
}

function VaultSwordArmor::resetPoleVault(%this, %obj, %slot, %force) {
	%this.setCycleRange(%obj, %slot, 0, 0);
	if(%force) {
		%this.forceCycleGuard(%obj, 0, 0);
	}
}

function VaultSwordArmor::unMount(%this, %obj, %slot) {
	%this.resetPoleVault(%obj, %slot, false);
	cancel(%obj.poleVaultSchedule);
	Parent::unMount(%this, %obj, %slot);
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
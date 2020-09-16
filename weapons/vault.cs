datablock AudioProfile(VaultSound) {
	filename    = "./sounds/vault.ogg";
	description = AudioDefault3d;
	preload = true;
};

datablock AudioProfile(VaultAirSound) {
	filename    = "./sounds/air.ogg";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(VaultLandingSound) {
	filename    = "./sounds/landing.ogg";
	description = AudioClose3d;
	preload = true;
};

datablock ParticleData(VaultFlyParticle) {
	dragCoefficient      = 4;
	gravityCoefficient   = 0.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 700;
	lifetimeVarianceMS   = 200;
	spinSpeed       	 = 0;
	spinRandomMin        = 0;
	spinRandomMax        = 0;

	textureName = "Add-Ons/Player_Beefboy/textures/trail";

	colors[0] = "1.0 1.0 1.0 0.15";
	colors[1] = "1.0 1.0 1.0 0.15";
	colors[2] = "1.0 1.0 1.0 0.0.75";
	colors[3] = "1.0 1.0 1.0 0.0";

	sizes[0] = 3;
	sizes[1] = 3;
	sizes[2] = 3;
	sizes[3] = 3;

	times[0] = 0.0;
	times[1] = 0.05;
	times[2] = 0.5;
	times[3] = 0.9;

	useInvAlpha = false;
};

datablock ParticleEmitterData(VaultFlyEmitter) {
	ejectionPeriodMS = 15;
	periodVarianceMS = 0;
	ejectionVelocity = 0;
	velocityVariance = 0;
	ejectionOffset   = 0;
	thetaMin         = 0;
	thetaMax         = 0;
	phiReferenceVel  = 0;
	phiVariance      = 0;
	overrideAdvance = true;
	orientOnVelocity = false;
	orientParticles = false;

	particles = VaultFlyParticle;
};

datablock ShapeBaseImageData(VaultFlyImage) {
	shapeFile = "base/data/shapes/empty.dts";
	emap = true;
	mountPoint = 9;
	offset = "0 0 0";
	eyeOffset = "0 0 -1000";
	rotation = eulerToMatrix("0 0 0");
	correctMuzzleVector = true;
	className = "WeaponImage";
	item = BeefBoyBeefboySwordItem;

	melee = false;
	armReady = true;

	doColorShift = false;

	stateName[0]		= "Activate";
	stateEmitter[0]		= "VaultFlyEmitter";
	stateEmitterTime[0]	= 9999;
};

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
	swordMaxCycles = 3;
	swordDoubleHanded = true;
	
	// middle attack 1, just a standard attack
	swordCycle[0] 					= "middle left";
	swordCycleThread[0] 			= "mid4";
	swordCycleThreadSlot[0]			= 1;
	swordCycleDamage[0]				= 50;
	swordCycleCrit[0]				= false;
	swordCycleImpactImpulse[0]		= 0;
	swordCycleVerticalImpulse[0]	= 0;
	swordCycleHitExplosion[0]		= MidHitProjectile;
	swordCycleHitSound[0]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[0]			= 0.7;
	swordCycleGuardSound[0]			= CycleMidReadySound;
	swordCycleSwingSound[0]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[0]			= 4;

	// middle attack 2, another standard attack
	swordCycle[1] 					= "middle left";
	swordCycleThread[1] 			= "mid4";
	swordCycleThreadSlot[1]			= 1;
	swordCycleDamage[1]				= 50;
	swordCycleCrit[1]				= false;
	swordCycleImpactImpulse[1]		= 0;
	swordCycleVerticalImpulse[1]	= 0;
	swordCycleHitExplosion[1]		= MidHitProjectile;
	swordCycleHitSound[1]			= BeefBoySwordHit2Sound;
	swordCyclePrepTime[1]			= 0.4;
	swordCycleGuardSound[1]			= CycleMidReadySound;
	swordCycleSwingSound[1]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[1]			= 4;

	// low attack, knock enemies far back
	swordCycle[2] 					= "low left";
	swordCycleThread[2] 			= "";
	swordCycleThreadSlot[2]			= 1;
	swordCycleDamage[2]				= 20;
	swordCycleCrit[2]				= false;
	swordCycleImpactImpulse[2]		= 1500;
	swordCycleVerticalImpulse[2]	= 800;
	swordCycleHitExplosion[2]		= LowHitProjectile;
	swordCycleHitSound[2]			= LowHit1Sound;
	swordCyclePrepTime[2]			= 0.4;
	swordCycleGuardSound[2]			= CycleLowReadySound;
	swordCycleSwingSound[2]			= LowSwingSound;
	swordCycleSwingSteps[2]			= 4;

	// special attack 1, crits
	swordCycle[3] 					= "special left";
	swordCycleThread[3] 			= "mid4";
	swordCycleThreadSlot[3]			= 1;
	swordCycleDamage[3]				= 35;
	swordCycleCrit[3]				= false;
	swordCycleImpactImpulse[3]		= 0;
	swordCycleVerticalImpulse[3]	= 0;
	swordCycleHitExplosion[3]		= HighHitProjectile;
	swordCycleHitSound[3]			= HighHit1Sound;
	swordCyclePrepTime[3]			= 0.01;
	swordCycleGuardSound[3]			= "";
	swordCycleSwingSteps[3]			= 4;

	// parry information
	parryCooldown						= 1500;
	parryDuration						= 300;
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

	specialMethod							= "startPoleVault";
	specialConditionalMethod	= "canStartPoleVault";
	specialCooldown						= 2500;
	specialDuration						= 1000;
};

function VaultSwordArmor::onCycleGuard(%this, %obj, %slot) {
	Parent::onCycleGuard(%this, %obj, %slot);

	%cycle = %obj.swordCurrentCycle[%this] | 0;
	if(%cycle == 2) {
		%obj.playThread(%this.swordCycleThreadSlot[%cycle], "mid4Swing");
		%obj.schedule(33, stopThread, %this.swordCycleThreadSlot[%cycle]);
	}
}

function VaultSwordArmor::onCycleFire(%this, %obj, %slot) {
	%cycle = %obj.swordCurrentCycle[%this] | 0;

	if(%cycle == 2) {
		%obj.playThread(0, "shiftAway");
		%obj.playThread(2, "plant");
	}
	else if(%cycle == 3) {
		%sounds = MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
		serverPlay3d(getField(%sounds, getRandom(0, getFieldCount(%sounds) - 1)), %obj.getPosition(), getRandom(8, 12) / 10);
		%obj.playThread(0, "plant");
	}

	Parent::onCycleFire(%this, %obj, %slot);
}

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

	%this.schedule(150, poleVaultLoop, %obj, %slot, 10);
	%this.schedule(600, stopPoleVault, %obj, %slot);

	%this.setCycleRange(%obj, %slot, 3, 3);
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

		%obj.playAudio(0, VaultSound);
		%obj.playAudio(1, VaultAirSound);
		%obj.mountImage(VaultFlyImage, 3);
	}
	
	if(%ticks > 0) {
		%velocity = vectorAdd(vectorScale(%obj.vaultForwardVector, 15), "0 0 14");
		%obj.schedule(33, setVelocity, %velocity);
		%obj.poleVaultSchedule = %this.schedule(33, poleVaultLoop, %obj, %slot, %ticks - 1);
	}
}

function VaultSwordArmor::stopPoleVault(%this, %obj, %slot) {
	%obj.antiGravityZone.delete();
	%obj.swordCycleFrozen = false;
	%obj.forceNormalHands = false;
	%obj.client.applyBodyParts();
	%obj.setLookLimits(1, 0);

	%this.forceCycleGuard(%obj, %slot, 3);

	%this.waitSchedule(33, "resetPoleVault", %obj, %slot).addCondition(%obj, "isGrounded", true);
}

function VaultSwordArmor::resetPoleVault(%this, %obj, %slot) {
	%this.setCycleRange(%obj, %slot, 0, 2);
	%this.forceCycleGuard(%obj, %slot, 0, true);
	
	%obj.unMountImageSafe(3);
	
	%obj.playAudio(1, VaultLandingSound);
	%obj.playThread(0, "plant");

	if(isObject(%obj.antiGravityZone)) {
		%obj.antiGravityZone.delete();
	}

	%obj.swordCycleFrozen = false;
	%obj.forceNormalHands = false;
	%obj.client.applyBodyParts();
	%obj.setLookLimits(1, 0);
	
	cancel(%obj.poleVaultSchedule);
}

function VaultSwordArmor::unMount(%this, %obj, %slot) {
	// %this.resetPoleVault(%obj, %slot);
	cancel(%obj.poleVaultSchedule);
	Parent::unMount(%this, %obj, %slot);
}

deActivatePackage(MiniDungeonsVaulting);
package MiniDungeonsVaulting {
	function Armor::onRemove(%this, %obj) {
		if(isObject(%obj.antiGravityZone)) {
			%obj.antiGravityZone.delete();
		}
		Parent::onRemove(%this, %obj);
	}
};
activatePackage(MiniDungeonsVaulting);
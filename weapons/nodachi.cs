datablock AudioProfile(NodachiDashSound) {
	filename = "./sounds/nodachi dash.wav";
	description = AudioClose3d;
	preload = true;
};
datablock ItemData(NodachiItem) {
	category = "Weapon";
	className = "Weapon";
	shapeFile = "./shapes/nodachi item.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
	uiName = "The Nodachi";
	iconName = "./";
	doColorShift = false;
	sword = NodachiSwordArmor;
	canDrop = true;
	price = 50;
	description = "Forged using the Honsanmai technique, this sword's power comes not from its blade but from its special dash attack.";
	nameColor = "1 1 0";
};
datablock TSShapeConstructor(NodachiSwordDTS) {
	baseShape = "./shapes/nodachi2.dts";
};
datablock PlayerData(NodachiSwordArmor : PlayerStandardArmor) {
	shapeFile = NodachiSwordDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	uiName = "";
	item = NodachiItem;
	swordStartMount[0] = 0;
	// start point for raycast/interpolation
	swordEndMount[0] = 1;
	// end point for raycast/interpolation
	swordStepInterpolationCount = 5;
	// how many linear interpolations we do between steps, based on distance
	swordStepTick = 33;
	// how fast we do sword stepping
	swordInterpolationDistance[0, 0] = 1;
	swordInterpolationRadius[0, 0] = 0.9;
	swordStopSwingOnBrickHit = false;
	// whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)
	// GUARD CYCLE: M -> L -> M
	swordMaxCycles = 3;
	swordDoubleHanded = true;
	// low attack, sends the opponent flying
	swordCycle[0] = "low right";
	swordCycleThread[0] = "low1";
	swordCycleThreadSlot[0] = 1;
	swordCycleDamage[0] = 35;
	swordCycleImpactImpulse[0] = 1000;
	swordCycleVerticalImpulse[0] = 800;
	swordCycleHitExplosion[0] = LowHitProjectile;
	swordCycleHitSound[0] = LowHit1Sound;
	swordCyclePrepTime[0] = 0.3;
	swordCycleGuardSound[0] = CycleLowReadySound;
	swordCycleSwingSound[0] = LowSwingSound;
	swordCycleSwingEmitter[0] = HighFlareImage;
	swordCycleSwingEmitterTime[0] = 120;
	swordCycleSwingEmitterSlot[0] = 1;
	swordCycleSwingSteps[0] = 5;
	// low attack, sends the opponent flying
	swordCycle[1] = "low left";
	swordCycleThread[1] = "low2";
	swordCycleThreadSlot[1] = 1;
	swordCycleDamage[1] = 35;
	swordCycleImpactImpulse[1] = 1000;
	swordCycleVerticalImpulse[1] = 800;
	swordCycleHitExplosion[1] = LowHitProjectile;
	swordCycleHitSound[1] = LowHit1Sound;
	swordCyclePrepTime[1] = 0.3;
	swordCycleGuardSound[1] = CycleLowReadySound;
	swordCycleSwingSound[1] = LowSwingSound;
	swordCycleSwingSlot[1] = 1;
	swordCycleSwingEmitter[1] = HighFlareImage;
	swordCycleSwingEmitterTime[1] = 120;
	swordCycleSwingEmitterSlot[1] = 1;
	swordCycleSwingSteps[1] = 5;
	swordCycle[2] = "middle right";
	swordCycleThread[2] = "mid4";
	swordCycleThreadSlot[2] = 1;
	swordCycleDamage[2] = 60;
	swordCycleImpactImpulse[2] = 300;
	swordCycleVerticalImpulse[2] = 500;
	swordCycleHitExplosion[2] = MidHitProjectile;
	swordCycleHitSound[2] = BeefBoySwordHit2Sound;
	swordCyclePrepTime[2] = 0.7;
	swordCycleGuardSound[2] = CycleMidReadySound;
	swordCycleSwingSound[2] = MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[2] = 4;
	swordCycle[3] = "special vertical";
	swordCycleThread[3] = "high2";
	swordCycleThreadSlot[3] = 1;
	swordCycleDamage[3] = 120;
	swordCycleImpactImpulse[3] = 1500;
	swordCycleVerticalImpulse[3] = 700;
	swordCycleHitExplosion[3] = HighHitProjectile;
	swordCycleHitSound[3] = BeefBoySwordHit2Sound;
	swordCyclePrepTime[3] = 2.0;
	swordCycleGuardSound[3] = CycleHighReadySound;
	swordCycleSwingEmitter[3] = CritTrailFlareImage;
	swordCycleSwingEmitterTime[3] = 100;
	swordCycleSwingEmitterSlot[3] = 1;
	swordCycleSwingSound[3] = CritImpactShortSound;
	swordCycleSwingSteps[3] = 5;
	swordCycleCrit[3] = true;
	// parry information
	parryCooldown = 1500;
	parryDuration = 700;
	parryWait = 500;
	parryThread = "parry1";
	parryStunDurationSuccess = 700;
	// how long targets are stunned for
	parryRecoverTimeSuccess = 200;
	parryDamageSuccess = 10;
	parrySelfImpactImpulseSuccess = 350;
	parrySelfVerticalImpulseSuccess = 350;
	parryProgressiveKnockbackSuccess = 13 SPC 13;
	parryStunDuration = 0;
	parryDamage = 0;
	parrySelfImpactImpulse = 800;
	parrySelfVerticalImpulse = 800;
	parryProgressiveKnockback = 8 SPC 8;
	specialMethod = "startDash";
	specialConditionalMethod = "canStartDash";
	specialCooldown = 3500;
	specialDuration = 1000;
};
function NodachiSwordArmor::canStartDash(%this, %obj, %slot) {
	return  ! %obj.isGrounded;
}
function NodachiSwordArmor::startDash(%this, %obj) {
	if( ! isObject(%obj.antiGravityZone)) {
		%obj.antiGravityZone = new PhysicalZone() {
			position = %obj.getPosition();
			velocityMod = "1";
			gravityMod = "0";
			extraDrag = "0.5";
			isWater = "0";
			waterViscosity = "0";
			waterDensity = "0";
			waterColor = "0.200000 0.600000 0.600000 0.300000";
			appliedForce = "0 0 0";
			polyhedron = "0.0 0.0 0.0" SPC "4.0 0.0 0.0" SPC "0.0 -4.0 0.0" SPC "0.0 0.0 4.0";
			owner = %obj;
		};
		%obj.nodachiForwardVector = %obj.getForwardVector();
		// %obj.playThread(1, "poleVault");
		// %obj.playAudio(0, VaultSound);
		%obj.playAudio(3, NodachiDashSound);
		%obj.playAudio(1, VaultAirSound);
		%obj.mountImage(VaultFlyImage, 3);
	}
	%obj.setVelocity("0 0 0");
	%obj.addVelocity(vectorScale(%obj.getForwardVector(), 30));
	%obj.nodachiSavedCycle = %this.getCycle(%obj);
	%this.setCycleRange(%obj, 0, 3, 3);
	%this.forceCycleGuard(%obj, 0, 3);
	%this.dashLoop(%obj, 3);
	%this.waitSchedule(33, "landDash", %obj).addCondition(%obj, "isGrounded", true);
	%obj.spawnExplosion(CritPowerProjectile, 0.5);
	commandToClient(%obj.client, 'MD_interpolateFOV', "", "+25", 0.05);
}
function NodachiSwordArmor::dashLoop(%this, %obj, %ticks) {
	if(%ticks < 0) {
		%this.stopDash(%obj);
		return;
	}
	%obj.antiGravityZone.setTransform(vectorAdd(%obj.getPosition(), "-2 2 -2"));
	%obj.dashLoop = %this.schedule(100, dashLoop, %obj, %ticks - 1);
}
function NodachiSwordArmor::landDash(%this, %obj) {
	%this.setCycleRange(%obj, 0, 0, %this.swordMaxCycles - 1);
	%this.forceCycleGuard(%obj, 0, %obj.nodachiSavedCycle, true);
	%obj.playAudio(1, VaultLandingSound);
}
function NodachiSwordArmor::stopDash(%this, %obj) {
	if(isObject(%obj.antiGravityZone)) {
		%obj.antiGravityZone.delete();
	}
	%obj.stopAudio(1);
	%obj.unMountImageSafe(3);
}

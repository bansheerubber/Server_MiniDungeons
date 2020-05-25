$MD::SwordDebugRadiusTime = 1500;
$MD::SwordDebugLineTime = 1500;
$MD::SwordDebug = false;
$MD::SwordRadiusCheck = true;
$MD::ParryLeewayFrames = 2;

function Armor::startSwing(%this, %obj, %slot, %steps) {
	// default to -1 (inifinte steps)
	if(%steps $= "") {
		%steps = -1;
	}
	%this.stepSwing(%obj, %slot, getRandom(0, 100000), %steps);
}

function Armor::endSwing(%this, %obj, %slot) {
	cancel(%obj.stepSwingSchedule);
}

function Armor::stepSwing(%this, %obj, %slot, %id, %stepsRemaining) {
	if(!isObject(%obj) || %stepsRemaining == 0) {
		%this.onSwingEnd(%obj, %slot, %id);
		return;
	}
	
	%time = getRealTime();
	%index = 0;
	while((%startMount = %this.swordStartMount[%index]) !$= "") {
		%endMount = %this.swordEndMount[%index];

		%start = %obj.sword[%slot].swordMount[%startMount].getPosition();
		%end = %obj.sword[%slot].swordMount[%endMount].getPosition();
		%direction = vectorNormalize(vectorSub(%end, %start));
		%distance = vectorDist(%start, %end);

		if(%obj.tempSwordLastStart[%id] !$= "") {
			%obj.lastInterpolationStart[%id] = %obj.tempSwordLastStart[%id];
			if(%this.swordStepInterpolationCount != 0 && %this.swordStepInterpolationCount !$= "") {
				for(%i = 0; %i < %this.swordStepInterpolationCount; %i++) {
					%percent = (%i + 1) / (%this.swordStepInterpolationCount + 1);
					%this.interpolationSwing(%obj, %slot, %start, %end, %distance, %direction, %percent, %index, %id);
				}
			}
			else if(%this.swordStepInterpolationDistance != 0 && %this.swordStepInterpolationDistance !$= "" && %obj.tempSwordLastStart[%id] !$= "") {
				%distance2 = vectorDist(%obj.tempSwordLastStart[%id], %start);
				for(%i = 0; %i < %distance2; %i += %this.swordStepInterpolationDistance) {
					%percent = %i / %distance2;
					%this.interpolationSwing(%obj, %slot, %start, %end, %distance, %direction, %percent, %index, %id);
				}
			}
		}
		else {
			%obj.swordTangent[%id] = %direction;
		}

		%obj.swordTangent[%id] = vectorNormalize(vectorSub(%start, %obj.tempSwordLastStart[%id]));
		if(!%this.raycastSwing(%obj, %slot, %id, %index, %start, %end)) {
			return;
		}
		%obj.tempSwordLastStart[%id] = %start;
		%obj.tempSwordLastDirection[%id] = %direction;
		%obj.tempSwordLastDistance[%id] = %distance;

		%index++;
	}

	%obj.stepSwingSchedule = %this.schedule(%this.swordStepTick, stepSwing, %obj, %slot, %id, %stepsRemaining - 1);
}

function Armor::interpolationSwing(%this, %obj, %slot, %start, %end, %distance, %direction, %percent, %index, %id) {
	%distanceInterpolation = (%distance * (1 - %percent)) + (%obj.tempSwordLastDistance[%id] * %percent);
	%directionInterpolation = vectorNormalize(vectorLerp(%obj.tempSwordLastDirection[%id], %direction, %percent));
	%startInterpolation = vectorLerp(%obj.tempSwordLastStart[%id], %start, %percent);
	%endInterpolation = vectorAdd(%startInterpolation, vectorScale(%directionInterpolation, %distanceInterpolation));

	%obj.swordTangent[%id] = vectorNormalize(vectorSub(%startInterpolation, %obj.lastInterpolationStart[%id]));
	%obj.lastInterpolationStart[%id] = %startInterpolation;

	if(!%this.raycastSwing(%obj, %slot, %id, %index, %startInterpolation, %endInterpolation)) {
		return;
	}
}

function Armor::raycastSwing(%this, %obj, %slot, %id, %index, %start, %end) {
	%masks = $TypeMasks::FxBrickObjectType | $TypeMasks::StaticShapeObjectType | $TypeMasks::PlayerObjectType;

	%raycast = containerRaycast(%start, %end, %masks, %obj);
	if(isObject(%raycast)) {
		%col = getWord(%raycast, 0);

		if(!%obj.tempSwordHit[%id, %col]) {
			// only do leeway frames on bots
			if(%obj.getClassName() $= "AiPlayer") {
				%this.schedule(33 * $MD::ParryLeewayFrames, checkSwingHit, %obj, %slot, %col, %obj.swordTangent[%id], false, %index, -1);
			}
			else {
				%this.checkSwingHit(%obj, %slot, %col, %obj.swordTangent[%id], false, %index, -1);
			}
		}
		
		if(isObject(%col) && !(%col.getType() & $TypeMasks::PlayerObjectType) && %this.swordStopSwingOnBrickHit) {
			%this.setImageAmmo(%slot, false);
			return false;
		}

		%obj.tempSwordHit[%id, %col] = true;
	}
	
	if($MD::SwordDebug) {
		drawDebugLine(%start, %end, 0.2, "1 0 0 1", $MD::SwordDebugLineTime);
	}

	%interpolationIndex = 0;
	while((%percent = %this.swordInterpolationDistance[%index, %interpolationIndex]) !$= "") {
		%position = vectorLerp(%start, %end, %percent);
		%radius = %this.swordInterpolationRadius[%index, %interpolationIndex];
		initContainerRadiusSearch(%position, %radius, %masks);
		while(%col = containerSearchNext()) {
			if(isFunction(%col.getClassName(), "getDatablock")) {
				%distance = vectorDist(%col.getPosition(), %position);

				if(%col.getType() & $TypeMasks::PlayerObjectType) {
					%radiusCheck = doesBoundsIntersectRadius(%col.getPosition(), %col.getObjectBox(), %position, %radius);
				}
				else {
					%radiusCheck = ((!$MD::SwordRadiusCheck || %distance < %radius) || %col.getDatablock().forceSwingRadius);
				}

				if(%radiusCheck && !%obj.tempSwordHit[%id, %col] && %col != %obj) {
					// only do leeway frames on bots
					if(%obj.getClassName() $= "AiPlayer") {
						%this.schedule(33 * $MD::ParryLeewayFrames, checkSwingHit, %obj, %slot, %col, %obj.swordTangent[%id], true, %index, %interpolationIndex);
					}
					else {
						%this.checkSwingHit(%obj, %slot, %col, %obj.swordTangent[%id], true, %index, %interpolationIndex);
					}

					%obj.tempSwordHit[%id, %col] = true;
				}
			}
		}

		if($MD::SwordDebug) {
			drawDebugSphere(%position, %radius, "0 0 1 1", $MD::SwordDebugRadiusTime);
		}
		
		%interpolationIndex++;
	}
	return true;
}

// checks to see if we hit the guy (if he was in parry, etc)
function Armor::checkSwingHit(%this, %obj, %slot, %col, %tangent, %isRadius, %index, %radiusIndex) {
	if(isObject(%col) && miniGameCanDamage(%obj, %col) == true) {
		if(!%col.isParrying) {
			%this.onSwingHit(%obj, %slot, %col, %tangent, true, %index, %interpolationIndex);
		}
		else {
			%this.parryReceive(%col.sword[0].getDatablock(), %obj, %slot, %col, %direction, %tangent, %isRadius, %index, %radiusIndex);
			%col.sword[0].getDatablock().parrySuccess(%this, %obj, %slot, %col, %direction, %tangent, %isRadius, %index, %radiusIndex);
		}
	}
}

// for supporting other kinds of attacks
function Armor::doParry(%this, %obj, %enemyImage, %enemy, %slot, %direction, %tangent) {
	%enemyImage.parryReceive(%this, %enemy, %slot, %obj, %direction, %tangent, false, -1, -1);
	%this.parrySuccess(%enemyImage, %enemy, %slot, %obj, %direction, %tangent, false, -1, -1);
}

// %obj is the person who successfully completed the parry
function Armor::parrySuccess(%this, %enemyImage, %enemy, %slot, %obj, %direction, %tangent, %isRadius, %index, %radiusIndex) {
	// get the direction of the swing
	%swingDirection = getWord(%enemy.swordCurrentCycleName, 1);

	%parryImage = (%shield = %obj.getMountedImage(1)).parryIsShield ? %shield : %this;
	
	%obj.playThread(0, "plant");
	%obj.setActionThread("shiftUp", false);
	%knockbackVector = vectorScale(%obj.getForwardVector(), -1);
	if(%swingDirection !$= %direction) {
		%obj.applyImpulse(%obj.getPosition(), vectorAdd(vectorScale(%knockbackVector, %parryImage.parrySelfImpactImpulseSuccess), "0 0" SPC %parryImage.parrySelfVerticalImpulseSuccess));
		%obj.playAudio(3, ParryHeavySound);

		if(!%parryImage.parryIsShield) {
			%obj.schedule(100, setParry, false, "", true); // reset parry w/ instant cycle recovery
			%obj.playThread(1, "parryLand");
		}
	}
	else {
		%obj.applyImpulse(%obj.getPosition(), vectorAdd(vectorScale(%knockbackVector, %parryImage.parrySelfImpactImpulse), "0 0" SPC %parryImage.parrySelfVerticalImpulse));
		%obj.playAudio(3, ParryLightSound);

		if(!%parryImage.parryIsShield) {
			%obj.schedule(100, setParry, false, "", false); // reset parry, no instant cycle recovery
			%obj.playThread(1, "parryLand");
		}
	}

	%obj.setInvulnerbilityTime(0.2); // give breif invulnerability
}

// %obj is the person who got parried
function Armor::parryReceive(%this, %colImage, %obj, %slot, %col, %direction, %tangent, %isRadius, %index, %radiusIndex) {
	// get the direction of the swing
	%swingDirection = getWord(%obj.swordCurrentCycleName, 1);

	%parryImage = (%shield = %col.getMountedImage(1)).parryIsShield ? %shield : %colImage;

	if(%parryImage.parryIsShield) {
		%cycle = %obj.swordCurrentCycle[%this];
		%col.shieldHP[%parryImage] -= %this.swordCycleDamage[%cycle]; // deal damage to the shield
	}

	(new Projectile() {
		datablock = ParryProjectile;
		initialPosition = vectorAdd(vectorLerp(%obj.getHackPosition(), %col.getHackPosition(), 0.5), "0 0 0.2");
		initialVelocity = vectorScale(vectorNormalize(vectorSub(%obj.getHackPosition(), %col.getHackPosition())), 15);
		sourceObject = 0;
		sourceSlot = 0;
	}).explode();
	
	%knockbackVector = %col.getForwardVector();
	if(%swingDirection !$= %direction || %parryImage.parryIsShield) {
		%knockback = %parryImage.parryProgressiveKnockbackSuccess;
		%obj.progressiveKnockback(vectorScale(%knockbackVector, getWord(%knockback, 0)), getWord(%knockback, 1), %parryImage.parryStunDurationSuccess);
		%obj.damage(%col, %obj.getPosition(), %parryImage.parryDamageSuccess, 2);

		(%p = new Projectile() {
			datablock = HeavyParryProjectile;
			initialPosition = %obj.getHackPosition();
			initialVelocity = "0 0 15";
			sourceObject = 0;
			sourceSlot = 0;
		}).explode();
	}
	else {
		%knockback = %parryImage.parryProgressiveKnockback;
		%obj.progressiveKnockback(vectorScale(%knockbackVector, getWord(%knockback, 0)), getWord(%knockback, 1), %parryImage.parryStunDuration);
		%obj.damage(%col, %obj.getPosition(), %parryImage.parryDamage, 2);

		(%p = new Projectile() {
			datablock = LightParryProjectile;
			initialPosition = %obj.getHackPosition();
			initialVelocity = "0 0 15";
			sourceObject = 0;
			sourceSlot = 0;
		}).explode();
	}
}

function Armor::onSwingHit(%this, %obj, %slot, %col, %tangent, %isRadius, %index, %radiusIndex) {
	// default implementation for sword cycles
	if(%this.swordCycle[0] !$= "") {
		if(%col.getType() & $TypeMasks::PlayerObjectType) {
			%cycle = %obj.swordCurrentCycle[%this];
			%damage = %this.swordCycleDamage[%cycle];
			%impactImpulse = %this.swordCycleImpactImpulse[%cycle];
			%verticalImpulse = %this.swordCycleVerticalImpulse[%cycle];

			if(%impactImpulse !$= "" && %verticalImpulse !$= "") {
				%col.applyImpulse(%col.getPosition(), vectorAdd(vectorScale(%obj.getForwardVector(), %impactImpulse), "0 0" SPC %verticalImpulse)); // impulse
			}
			else {
				%knockback = %this.swordProgressiveKnockback[%cycle];
				%col.progressiveKnockback(vectorScale(%obj.getForwardVector(), getWord(%knockback, 0)), getWord(%knockback, 1), %this.swordProgressiveKnockbackDuration[%cycle] * 1000);
			}

			if(isObject(%explosion = %this.swordCycleHitExplosion[%cycle])) {
				(%p = new Projectile() {
					datablock = %explosion;
					initialPosition = %col.getHackPosition();
					initialVelocity = vectorScale(%tangent, -5);
					sourceObject = 0;
					sourceSlot = 0;
				}).explode();
			}

			%sounds = %this.swordCycleHitSound[%cycle];
			serverPlay3d(getField(%sounds, getRandom(0, getFieldCount(%sounds) - 1)), %col.getPosition());

			%col.damage(%obj, %col.getPosition(), %damage, 3); // damage
		}
		else if((%col.getType() & $TypeMasks::FxBrickObjectType) && %col.brickHp !$= "") {
			%cycle = %obj.swordCurrentCycle[%this];
			%damage = %this.swordCycleDamage[%cycle];
			%col.getDatablock().brickDamage(%col, %obj, %col.getPosition(), %damage, 3);
		}
		else if(isFunction(%col.getClassName(), "getDatablock") && %col.getDatablock().isBreakable) {
			%col.getDatablock().onBreak(%col, %obj);
		}
	}
	else {
		echo("\c2" @ %this.getName() @ "::onSwingHit not implemented.");
	}
}

function Player::cleanUpSwords(%this) {
	for(%i = 0; %i < 100; %i++) {
		if(isObject(%this.swordMount[%i])) {
			%this.swordMount[%i].delete();
		}

		if(isObject(%this.sword[%i])) {
			%this.sword[%i].delete();
		}
	}

	if(isObject(%this.swordMount)) {
		%this.swordMount.delete();
	}

	if(isObject(%this.swordHands)) {
		%this.swordHands.delete();
	}
}

// when supplied a player's bounding box, it scales down the bounding box and checks if a radius intersects it
function doesBoundsIntersectRadius(%objectPosition, %objectBox, %radiusPosition, %radius) {
	%worldBoxCorner1 = vectorAdd(vectorScale(getWords(%objectBox, 0, 2), 1 / 4), %objectPosition);
	%worldBoxCorner2 = vectorAdd(vectorScale(getWords(%objectBox, 3, 6), 1 / 4), %objectPosition);

	%radiusX = getWord(%radiusPosition, 0);
	%radiusY = getWord(%radiusPosition, 1);
	%radiusZ = getWord(%radiusPosition, 2);

	%distanceSquared = mPow(%radius, 2);

	// S.X < C1.X
	if(%radiusX < (%x = getWord(%worldBoxCorner1, 0))) {
		%distanceSquared -= mPow(%radiusX - %x, 2);
	}
	// S.X > C2.X
	else if(%radiusX > (%x = getWord(%worldBoxCorner2, 0))) {
		%distanceSquared -= mPow(%radiusX - %x, 2);
	}

	// S.X < C1.Y
	if(%radiusY < (%y = getWord(%worldBoxCorner1, 1))) {
		%distanceSquared -= mPow(%radiusY - %y, 2);
	}
	// S.X > C2.Y
	else if(%radiusY > (%y = getWord(%worldBoxCorner2, 1))) {
		%distanceSquared -= mPow(%radiusY - %y, 2);
	}

	// S.Z < C1.Z
	if(%radiusZ < (%z = getWord(%worldBoxCorner1, 2))) {
		%distanceSquared -= mPow(%radiusZ - %z, 2);
	}
	// S.Z > C2.Z
	else if(%radiusZ > (%z = getWord(%worldBoxCorner2, 2))) {
		%distanceSquared -= mPow(%radiusZ - %z, 2);
	}

	return %distanceSquared > 0;
}

deActivatePackage(MiniDungeonsSwords);
package MiniDungeonsSwords {
	function Player::setDatablock(%this, %datablock) {
		%this.cleanUpSwords();
		Parent::setDatablock(%this, %datablock);
	}

	function Armor::onRemove(%this, %obj) {
		%obj.cleanUpSwords();
		Parent::onRemove(%this, %obj);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc) {
		%killingPlayer = %sourceObject.getClassName() $= "Projectile" ? %sourceObject.sourceObject : %sourceObject;
		if(%killingPlayer.getClassName() $= "AiPlayer" && %killingPlayer.name !$= "") {
			%client.disableDeathMessage = true;
			%botKill = true;
		}
		
		Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
		
		if(%botKill) {
			%client.disableDeathMessage = false;
			%message = $DeathMessage_Murder[%damageType];
			messageAllExcept(%client, -1, 'MsgClientKilled', %message, %client.getPlayerName(), %killingPlayer.name);
			messageClient(%client, 'MsgYourDeath', %message, %client.getPlayerName(), %killingPlayer.name, $Game::MinRespawnTime);
		}
	}

	function messageClient(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13) {
		if(%client.disableDeathMessage) {
			return;
		}
		
		Parent::messageClient(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
	}

	function messageAllExcept(%client, %team, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13) {
		if(%client.disableDeathMessage) {
			return;
		}
		
		Parent::messageAllExcept(%client, %team, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
	}

	function miniGameCanDamage(%obj1, %obj2) {
		if(isObject(%obj1) && isObject(%obj2)) {
			if(%obj1.brickHp !$= "" || %obj2.brickHp !$= "" || %obj1.getDatablock().isBreakable || %obj2.getDatablock().isBreakable) {
				if(%obj1.sourceObject.isEnemyAi || %obj1.isEnemyAi
					|| %obj2.sourceObject.isEnemyAi || %obj2.isEnemyAi)
				{
					return false;
				}
				else {
					return true;
				}
			}
			else if(%obj1.sourceObject.isEnemyAi && !%obj2.isEnemyAi) {
				if(%obj1.sourceObject.getState() $= "Dead") {
					return false;
				}
				else {
					return true;
				}
			}
			else if(!%obj1.isEnemyAi && %obj2.sourceObject.isEnemyAi) {
				if(%obj2.sourceObject.getState() $= "Dead") {
					return false;
				}
				else {
					return true;
				}
			}
			else if(%obj1.isEnemyAi && !%obj2.isEnemyAi) {
				if(%obj1.getState() $= "Dead") {
					return false;
				}
				else {
					return true;
				}
			}
			else if(!%obj1.isEnemyAi && %obj2.isEnemyAi) {
				if(%obj2.getState() $= "Dead") {
					return false;
				}
				else {
					return true;
				}
			}
			else {
				return Parent::miniGameCanDamage(%obj1, %obj2);
			}
		}
		else {
			return Parent::miniGameCanDamage(%obj1, %obj2);
		}
	}
};
activatePackage(MiniDungeonsSwords);
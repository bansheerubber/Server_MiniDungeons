function Player::progressiveKnockback(%this, %velocity, %wallSpeed, %time, %started) {
	if(!%started) {
		cancel(%this.progressiveKnockbackSchedule);
		%this.progressiveKnockbackStart = getSimTime();
		%this.setStunlocked(true, %time);
		%this.mountImage(BeefboyGuard2SkidImage, 2);
	}

	if(getSimTime() - %this.progressiveKnockbackStart < %time) {
		%this.setVelocity(%velocity);

		// this is crashy
		// if(%wallSpeed !$= "") {
		// 	%masks = $TypeMasks::FxBrickObjectType | $TypeMasks::StaticShapeObjectType;
		// 	%position = %this.getHackPosition();
		// 	%normals = "0 0 0";
		// 	%count = 0;
		// 	initContainerRadiusSearch(%position, 1, %masks);
		// 	while(%col = containerSearchNext()) {
		// 		if(%col != sunLight.getId() && !%col.isDebug) {
		// 			%normals = vectorAdd(%normals, getWords(containerRaycast(%position, %col.getPosition(), %masks, false), 4, 6));
		// 			%count++;
		// 		}
		// 	}

		// 	if(%count != 0) {
		// 		%averageNormal = vectorNormalize(vectorScale(%normals, 1 / %scale));
		// 		%this.setVelocity(vectorAdd(vectorScale(%averageNormal, getWord(%wallSpeed, 0)), "0 0" SPC getWord(%wallSpeed, 1)));
		// 		%this.unMountImageSafe(2);
		// 		return;
		// 	}
		// }

		if(getSimTime() - %this.lastProgressiveKnockbackPlant > 200) {
			%this.lastProgressiveKnockbackPlant = getSimTime();
			serverPlay3d(BeefboyGuard2SlowDownSound, %this.getPosition());
		}

		%this.progressiveKnockbackSchedule = %this.schedule(33, progressiveKnockback, %velocity, %wallSpeed, %time, true);
	}
	else {
		%this.unMountImageSafe(2);
	}
}

function Player::isStunlocked(%this) {
	return isEventPending(%this.stunlockedSchedule);
}

function Player::setStunlocked(%this, %bool, %time) {
	if(%bool) {
		cancel(%this.stunlockedSchedule);
		%this.stunlockedSchedule = %this.schedule(%time, setStunlocked, false);
		%this.setImageLoaded(0, false); // for swords
		%this.playThread(1, "stunlock");
	}
	else {
		if(isObject(%this.sword[0])) {
			%sword = %this.sword[0].getDatablock();
			%cycle = %this.swordCurrentCycle[%sword] | 0;
			%this.playThread(%sword.swordCycleThreadSlot[%cycle], %sword.swordCycleThread[%cycle] @ "Swing"); // play cycle animation
			%this.stopThreadSchedule = %this.schedule(33, stopThread, %sword.swordCycleThreadSlot[%cycle]);
		}
		else {
			%this.playThread(1, "root");
		}
	}
}

deActivatePackage(MiniDungeonsProgressiveKnockback);
package MiniDungeonsProgressiveKnockback {
	function Armor::onRemove(%this, %obj) {
		cancel(%obj.progressiveKnockbackSchedule);
		Parent::onRemove(%this, %obj);
	}
};
activatePackage(MiniDungeonsProgressiveKnockback);
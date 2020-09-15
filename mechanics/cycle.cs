// there are guard cycles, where you go from one guard to another to another. 
deActivatePackage(MiniDungeonsCycles);
package MiniDungeonsCycles {
	function WeaponImage::onMount(%this, %obj, %slot) {
		if(%this.swordCycle[0] !$= "") {
			%obj.setImageLoaded(%slot, false);
			%obj.swordLastSwing[%this.getId()] = getSimTime(); // make it so we have to wait for every guard
			if(%obj.swordCurrentCycle[%this.getId()] $= "") {
				%obj.swordCurrentCycle[%this.getId()] = 0;
			}
		}
		
		if(%this.parryIsShield) {
			%obj.setImageLoaded(%slot, false);
			%obj.playThread(2, "armReadyLeft");
			
			if(%obj.shieldHP[%this.getID()] $= "") {
				%obj.shieldHP[%this.getID()] = %this.parryShieldHealth;
			}

			%obj.shieldTick();
		}
		
		Parent::onMount(%this, %obj, %slot);
	}

	function WeaponImage::onUnMount(%this, %obj, %slot) {
		Parent::onUnMount(%this, %obj, %slot);
	}
};
activatePackage(MiniDungeonsCycles);

// function WeaponImage::onCycleCheck(%this, %obj, %slot) {
// 	%cycle = %obj.swordCurrentCycle[%this];

// 	// if we're below the prep time or we're parrying, then do not guard
// 	if((getSimTime() - %obj.swordLastSwing[%this]) < (%this.swordCyclePrepTime[%cycle] * 1000) || %obj.isParrying || %obj.isStunlocked()) {
// 		%obj.setImageLoaded(%slot, false);
// 	}
// 	else {
// 		%obj.setImageLoaded(%slot, true);
// 	}
// }

// implemented in Armor::onTrigger() in parry.cs, underneath all the parrying code
function Player::setSwordTrigger(%this, %slot, %boolean) {
	if(isObject(%this.sword[%slot])) {
		%cycle = %obj.swordCurrentCycle[%this] | 0;

		// only trigger if the conditions are met
		if((getSimTime() - %this.swordLastSwing[%this]) >= (%this.swordCyclePrepTime[%cycle] * 1000) && !%this.isParrying && !%this.swordCycleFrozen && !%this.isStunlocked() && %this.swordCycleState[%this.sword[%slot].getDatablock()] == 1 && %boolean) {
			%this.sword[%slot].getDatablock().prepareCycleFire(%this, %slot);
		}

		%this.swordTrigger = %boolean;
	}
}

// lets us decide which cycles within a range to use. useful for specials/switching up guard patterns
function Armor::setCycleRange(%this, %obj, %slot, %start, %end) {
	for(%i = %start; %i <= %end; %i++) {
		%cycle[%i] = %this.swordCycle[%i];
	}
	%obj.swordCycleStart[%this.getId()] = %start;
	%obj.swordCycleEnd[%this.getId()] = %end;

	%this.setCycleUI(%obj, %slot);
}

function Armor::setCycleUI(%this, %obj, %slot) {
	%start = %obj.swordCycleStart[%this.getId()];
	%end = %obj.swordCycleEnd[%this.getId()];
	
	for(%i = %start; %i <= %end; %i++) {
		%cycle[%i - %start] = %this.swordCycle[%i];
	}
	
	commandToClient(%obj.client, 'MD_SetUpParryMap');
	commandToClient(%obj.client, 'MD_LoadGuardCycles', getCycleLetterFromName(%cycle[0]), getCycleLetterFromName(%cycle[1]), getCycleLetterFromName(%cycle[2]), getCycleLetterFromName(%cycle[3]), getCycleLetterFromName(%cycle[4]), getCycleLetterFromName(%cycle[5]));
}

function Armor::setCycleActiveUI(%this, %obj, %slot) {
	commandToClient(%obj.client, 'MD_SetActiveCycle', %obj.swordCurrentCycle[%this] - %obj.swordCycleStart[%this.getId()]);
}

function Armor::forceCycleGuard(%this, %obj, %slot, %cycle) {
	if(!isObject(%obj)) {
		return;
	}
	
	// only force when we're not firing
	if(%obj.swordCycleState[%this] == 2) {
		return %this.schedule(33, forceCycleGuard, %obj, %slot);
	}
	else {
		if(isObject(%obj.swordPrepSchedule)) {
			%obj.swordPrepSchedule.delete();
		}

		%obj.swordCycleState[%this] = 0;
		%obj.swordCurrentCycle[%this] = %cycle;
		%this.schedule(33, onCycleGuard, %obj, %slot);
	}
}

function Armor::onCycleGuard(%this, %obj, %slot) {
	if(!isObject(%obj) || !isObject(%obj.sword[%slot])) {
		return;
	}
	
	if(isObject(%obj.swordPrepSchedule)) {
		%obj.swordPrepSchedule.delete();
	}

	%cycle = %obj.swordCurrentCycle[%this] | 0;

	%obj.swordCycleState[%this] = 1;

	// automatically fire if we keep the trigger held down
	if(%obj.swordTrigger) {
		%this.prepareCycleFire(%obj, %slot);
	}
	else {
		%obj.playThread(%this.swordCycleThreadSlot[%cycle], %this.swordCycleThread[%cycle] @ "Swing"); // play cycle animation
		%obj.stopThreadSchedule = %obj.schedule(33, stopThread, %this.swordCycleThreadSlot[%cycle]);
		%obj.sword[%slot].playThread(%this.swordCycleThreadSlot[%cycle], %this.swordCycleThread[%cycle]);
		%obj.playThread(0, "plant");
		%obj.playAudio(1, %this.swordCycleGuardSound[%cycle]);
	}

	%this.setCycleActiveUI(%obj, %slot);
}

function Armor::prepareCycleFire(%this, %obj, %slot) {
	if(!isObject(%obj)) {
		return;
	}
	
	cancel(%obj.stopThreadSchedule);
	
	%cycle = %obj.swordCurrentCycle[%this] | 0;
	%obj.swordCycleState[%this] = 2;

	if(%this.swordCycleTelegraphWaitTime[%cycle] !$= "") {
		%obj.playThread(%this.swordCycleTelegraphThreadSlot[%cycle], %this.swordCycleTelegraphThread[%cycle] @ "Telegraph");
		%obj.sword[%slot].playThread(%this.swordCycleTelegraphThreadSlot[%cycle], %this.swordCycleTelegraphThread[%cycle] @ "Telegraph");

		%sounds = %this.swordCycleTelegraphSound[%cycle];
		%obj.playAudio(3, getField(%sounds, getRandom(0, getFieldCount(%sounds) - 1)));

		%this.schedule(%this.swordCycleTelegraphWaitTime[%cycle] * 1000, onCycleFire, %obj, %slot);
	}
	else {
		%this.onCycleFire(%obj, %slot);
	}
}

function Armor::onCycleFire(%this, %obj, %slot) {
	if(!isObject(%obj)) {
		return;
	}
	
	if(isObject(%obj.swordPrepSchedule)) {
		%obj.swordPrepSchedule.delete();
	}
	
	%obj.swordCycleState[%this] = 2;
	
	%cycle = %obj.swordCurrentCycle[%this] | 0;

	%obj.playThread(%this.swordCycleThreadSlot[%cycle], %this.swordCycleThread[%cycle] @ "Swing");
	%obj.sword[%slot].playThread(%this.swordCycleThreadSlot[%cycle], %this.swordCycleThread[%cycle] @ "Swing");

	%sounds = %this.swordCycleSwingSound[%cycle];
	%audioSlot = %this.swordCycleSwingSlot[%cycle] $= "" ? 0 : %this.swordCycleSwingSlot[%cycle];
	%obj.playAudio(%audioSlot, getField(%sounds, getRandom(0, getFieldCount(%sounds) - 1)));

	%this.startSwing(%obj, %slot, %this.swordCycleSwingSteps[%cycle]);
	%obj.swordLastSwing[%this] = getSimTime();

	if(isObject(%emitterImage = %this.swordCycleSwingEmitter[%cycle])) {
		%obj.sword[%slot].swordMount[%this.swordCycleSwingEmitterSlot[%cycle]].mountImage(%emitterImage, 0);
		%obj.sword[%slot].swordMount[%this.swordCycleSwingEmitterSlot[%cycle]].schedule(%this.swordCycleSwingEmitterTime[%cycle], unMountImageSafe, 0);
	}

	%obj.swordCurrentCycleName = %this.swordCycle[%cycle];
}

function Armor::onSwingEnd(%this, %obj, %slot) {
	if(!isObject(%obj)) {
		return;
	}
	
	if(%this.swordCycle[0] !$= "") {
		%start = %obj.swordCycleStart[%this];
		%end = %obj.swordCycleEnd[%this];
		
		%cycle = (%obj.swordCurrentCycle[%this] + 1);

		if(%cycle > %end) {
			%cycle = %start;
		}

		%obj.swordCurrentCycle[%this] = %cycle;
	}
	
	%this.waitForCycleGuard(%obj, %slot);
}

function Armor::waitForCycleGuard(%this, %obj, %slot) {
	if(!isObject(%obj)) {
		return;
	}

	if(isObject(%obj.swordPrepSchedule)) {
		%obj.swordPrepSchedule.delete();
	}
	
	%cycle = %obj.swordCurrentCycle[%this];
	
	%obj.swordCycleState[%this] = 0;
	
	%obj.swordLastSwing[%this] = getSimTime();
	%obj.swordPrepSchedule = %this.waitSchedule(%this.swordCyclePrepTime[%cycle] * 1000, "onCycleGuard", %obj, %slot)
		.addCondition(%obj, "isParrying", false)
		.addCondition(%obj, "swordCycleFrozen", false)
		.addMethodCondition(%obj, "isStunLocked", false);
}

function Armor::transitionToCycleGuard(%this, %obj, %slot) {
	if(!isObject(%obj)) {
		return;
	}

	if(%obj.swordCycleState[%this] != 1) {
		%obj.swordCycleState[%this] = 0;
		%this.onCycleGuard(%obj, %slot);
	}
}
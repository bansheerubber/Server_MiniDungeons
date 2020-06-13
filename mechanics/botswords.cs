datablock TSShapeConstructor(MiniDungeonsHandsDTS) {
	baseShape = "base/data/shapes/player/minidungeonshands.dts";
};

datablock PlayerData(MiniDungeonsHandsArmor : PlayerStandardArmor)  {
	shapeFile = MiniDungeonsHandsDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
};

function getCycleLetterFromName(%cycle) {
	return strUpr(getSubStr(%cycle, 0, 1));
}

function Armor::mount(%this, %obj, %slot) {
	%this = %this.getId();
	
	%obj.mountSword(%this, %slot);

	%obj.playThread(1, "armReadyRight");

	if(%obj.swordCurrentCycle[%this] $= "") {
		if(isObject(%obj.sword[%slot])) {
			%this.setCycleRange(%obj, %slot, 0, %this.swordMaxCycles - 1);
			%obj.swordCurrentCycle[%this] = 0;
			%this.waitForCycleGuard(%obj, %slot);
		}
	}
	else {
		if(%obj.swordCycleState[%this] == 2) {
			%obj.swordCurrentCycle[%this]++;
		}
		%this.waitForCycleGuard(%obj, %slot);
	}

	%obj.dungeonsFixAppearance(%obj.client);
	
	%this.setCycleActiveUI(%obj, %slot);
	%this.setCycleUI(%obj, %slot);
}

function Armor::unMount(%this, %obj, %slot) {
	%this = %this.getId();
	
	if(isObject(%obj.swordPrepSchedule)) {
		%obj.swordPrepSchedule.delete();
	}
	
	if(%obj.sword[%slot]) {
		%obj.sword[%slot].delete();
	}
	%obj.dungeonsFixAppearance(%obj.client);

	commandToClient(%obj.client, 'MD_LoadGuardCycles', "", "", "", "", "", "");
}

function Player::mountSword(%this, %swordData, %slot) {
	// create the sword mount on the player's hand (is this needed?)
	if(!isObject(%this.swordMount)) {
		%this.swordMount = createMountPoint();
		%this.mountObject(%this.swordMount, 0);
	}

	if(!isObject(%this.swordHands)) {
		%this.swordHands = new AiPlayer() {
			datablock = MiniDungeonsHandsArmor;
			position = "0 0 -10";
			rotation = "0 0 0 1";
			isStaticFX = true;
		};
		%this.swordHands.kill();
		%this.swordMount.mountObject(%this.swordHands, 0);
	}

	if(isObject(%this.sword)) {
		%this.sword.delete();
	}

	if(isObject(%swordData)) {
		// create the sword player
		%this.sword[%slot] = new AiPlayer() {
			datablock = %swordData;
			position = "0 0 -10";
			rotation = "0 0 0 1";
			isStaticFX = true;
		};
		%this.sword[%slot].kill();

		%this.swordMount.mountObject(%this.sword[%slot], 0);

		%this.sword[%slot].setNodeColor("ALL", "1 1 1 1");

		if(%this.sword[%slot].isNodeVisible("trail")) {
			%this.sword[%slot].setNodeColor("trail", "0 0 0 1");
		}
		%this.sword[%slot].hideNode("trail");

		// mount bots to the sword so we can do raycasts
		%index = 0;
		while((%startMount = %swordData.swordStartMount[%index]) !$= "") {
			%endMount = %swordData.swordEndMount[%index];

			if(!isObject(%this.sword[%slot].swordMount[%startMount])) {
				%this.sword[%slot].swordMount[%startMount] = createMountPoint();
				%this.sword[%slot].mountObject(%this.sword[%slot].swordMount[%startMount], %startMount);
			}

			if(!isObject(%this.sword[%slot].swordMount[%endMount])) {
				%this.sword[%slot].swordMount[%endMount] = createMountPoint();
				%this.sword[%slot].mountObject(%this.sword[%slot].swordMount[%endMount], %endMount);
			}
			
			%index++;
		}
	}

	if(%this.getClassName() $= "AiPlayer") {
		%this.swordHands.setControlObject(%this.swordHands);
		%this.swordMount.setControlObject(%this.swordMount);
		%this.setControlObject(%this);
	}
}

deActivatePackage(MiniDungeonsBotSwords);
package MiniDungeonsBotSwords {
	function serverCmdUseTool(%this, %slot) {
		%obj = %this.player;

		if(isObject(%lastSword = %obj.tool[%obj.currTool].sword)) {
			%lastSword.getId().unMount(%obj, 0);
		}

		if(isObject(%obj) && isObject(%sword = %obj.tool[%slot].sword)) {
			%obj.setSwordTrigger(0, false);
			
			%obj.unMountImageSafe(0);
			%sword.getId().mount(%obj, 0);
		}

		Parent::serverCmdUseTool(%this, %slot);
	}

	function serverCmdDropTool(%this, %slot) {
		%obj = %this.player;
		if(isObject(%lastSword = %obj.tool[%obj.currTool].sword)) {
			%obj.setSwordTrigger(0, false);
			%lastSword.getId().unMount(%obj, 0);
		}

		Parent::serverCmdDropTool(%this, %slot);
	}

	function serverCmdUnUseTool(%this) {
		%obj = %this.player;
		if(isObject(%lastSword = %obj.tool[%obj.currTool].sword)) {
			%obj.setSwordTrigger(0, false);
			%lastSword.getId().unMount(%obj, 0);
		}

		Parent::serverCmdUnUseTool(%this, %slot);
	}

	function Player::setScale(%this, %scale) {
		if(isObject(%this.swordHands)) {
			%this.swordHands.setScale(%scale);
		}

		for(%i = 0; %i < 5; %i++) {
			if(isObject(%this.sword[%i])) {
				%this.sword[%i].setScale(%scale);
			}
		}
		
		Parent::setScale(%this, %scale);
	}

	function Player::playDeathCry(%this) {
		Parent::playDeathCry(%this);
	}

	function Armor::onDisabled(%this, %obj) {
		// tell the sword that it cannot dismount 
		%obj.swordMount.tempDisableDismount = true;

		Parent::onDisabled(%this, %obj);
	}

	function Armor::doDismount(%this, %obj, %force) {
		if(%obj.tempDisableDismount) {
			%obj.tempDisableDismount = false;
		}
		else {
			Parent::doDismount(%this, %obj, %force);
		}
	}

	function serverPlay3d(%data, %position) {
		if(%data.getId() != playerMountSound.getId()) {
			Parent::serverPlay3d(%data, %position);
		}
	}
};
activatePackage(MiniDungeonsBotSwords);
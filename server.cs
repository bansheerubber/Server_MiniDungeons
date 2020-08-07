forceRequiredAddOn("Player_BeefBoy");
forceRequiredAddOn("Brick_MiniDungeons");

exec("./playertype.cs");
exec("./sounds.cs");

exec("./lib/global.cs");
exec("./lib/brick.cs");
exec("./lib/debug.cs");
exec("./lib/math.cs");
exec("./lib/player.cs");
exec("./lib/projectile.cs");
exec("./lib/simobject.cs");
exec("./lib/sound.cs");
exec("./lib/vector.cs");
exec("./lib/waitSchedule.cs");

exec("./mechanics/knockback.cs");
exec("./mechanics/botswords.cs");
exec("./mechanics/cycle.cs");
exec("./mechanics/swings.cs");
exec("./mechanics/parry.cs");
exec("./mechanics/currency.cs");
exec("./mechanics/interactable.cs");
exec("./mechanics/events.cs");
exec("./mechanics/health.cs");
exec("./mechanics/notifications.cs");
exec("./mechanics/aim prediction.cs");
exec("./mechanics/crit.cs");
exec("./mechanics/specials.cs");
exec("./mechanics/compass.cs");
exec("./mechanics/pearl compass.cs");

exec("./weapons/high flare.cs");
exec("./weapons/low explosion.cs");
exec("./weapons/one handed.cs");
exec("./weapons/double handed.cs");
exec("./weapons/wood shield.cs");
exec("./weapons/vault.cs");

exec("./generator/builder.cs");
exec("./generator/hallway interpreter.cs");
exec("./generator/hallway builder.cs");
exec("./generator/dungeon interpreter.cs");
exec("./generator/room builder.cs");
exec("./generator/room.cs");
exec("./generator/hallway.cs");
exec("./generator/ghost util.cs");
exec("./generator/one way door.cs");

exec("./ai/rooms.cs");
exec("./ai/avatars.cs");
exec("./ai/states.cs");
exec("./ai/seek.cs");
exec("./ai/idle.cs");
exec("./ai/attack.cs");
exec("./ai/finder.cs");
exec("./ai/pathfinding.cs");
exec("./ai/bard.cs");
exec("./ai/mouse.cs");
exec("./ai/mace.cs");
exec("./ai/gladius.cs");
exec("./ai/bow.cs");
exec("./ai/cannon.cs");
exec("./ai/shopkeep.cs");
exec("./ai/name generator.cs");
exec("./ai/spawn.cs");
exec("./ai/horse.cs");

exec("./aiweapons/mace.cs");
exec("./aiweapons/gladius.cs");
exec("./aiweapons/bow.cs");
exec("./aiweapons/cannon.cs");
exec("./aiweapons/horse rabis.cs");

parseAvatarFile("Lanky", "Add-Ons/Player_Beefboy/lanky/avatar.txt");
parseAvatarFile("Bull", "Add-Ons/Player_Beefboy/bull/avatar.txt");
parseAvatarFile("Bard", "Add-Ons/Server_MiniDungeons/ai/avatars/bard.txt");
parseAvatarFile("Mace", "Add-Ons/Server_MiniDungeons/ai/avatars/mace.txt");
parseAvatarFile("Gladius", "Add-Ons/Server_MiniDungeons/ai/avatars/gladius.txt");
parseAvatarFile("Shopkeep", "Add-Ons/Server_MiniDungeons/ai/avatars/shopkeep.txt");
parseAvatarFile("Horse", "Add-Ons/Server_MiniDungeons/ai/avatars/horse.txt");

exec("./setpieces/swordsmanship angel.cs");

function serverCmdefs(%client, %input1, %input2, %input3, %input4) {
	if(%client.isSuperAdmin) {
		for(%fileName = findFirstFile("Add-Ons/Server_MiniDungeons/*.cs"); %fileName !$= ""; %fileName = findNextFile("Add-Ons/Server_MiniDungeons/*.cs")) {
			if(fileBase(%fileName) $= trim(%input1 SPC %input2 SPC %input3 SPC %input4)) {
				exec(%fileName);
				commandToClient(%client, 'serverMessage', '', "\c5Executed '" @ %fileName @ "'.");
			}
		}
	}
}

function serverCmdeds(%client, %input1, %input2, %input3, %input4) {
	if(%client.isSuperAdmin) {
		for(%fileName = findFirstFile("Add-Ons/Server_MiniDungeons/*.cs"); %fileName !$= ""; %fileName = findNextFile("Add-Ons/Server_MiniDungeons/*.cs")) {
			if(strPos(strLwr(%fileName), "/" @ strLwr(trim(%input1 SPC %input2 SPC %input3 SPC %input4)) @ "/") != -1) {
				exec(%fileName);
				commandToClient(%client, 'serverMessage', '', "\c5Executed '" @ %fileName @ "'.");
			}
		}
	}
}

datablock TSShapeConstructor(MountPointDTS) {
	baseShape = "./shapes/mountpoint.dts";
};

datablock PlayerData(MountPointArmor : PlayerStandardArmor)  {
	shapeFile = "./shapes/mountpoint.dts";
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
};

function createMountPoint() {
	%mount = new AiPlayer() {
		datablock = MountPointArmor;
		position = "0 0 -100";
		rotation = "0 0 0 1";
		isStaticFX = true;
	};
	%mount.kill();
	return %mount;
}

deActivatePackage(MountPoint);
package MountPoint {
	function AiPlayer::playDeathCry(%obj) {
		cancel(%obj.aiSchedule);
		if(%obj.isStaticFX) {
			return;
		}
		Parent::playDeathCry(%obj);
	}
	
	function Player::removeBody(%obj) {
		if(%obj.isStaticFX) {
			return;
		}
		Parent::removeBody(%obj);
	}

	function GameConnection::autoAdminCheck(%this) {
		schedule(1000, 0, commandToClient, %this, 'MD_Handshake');
		
		return Parent::autoAdminCheck(%this);
	}

	function Player::damage(%this, %col, %position, %damage, %damageType) {
		Parent::damage(%this, %col, %position, %damage, %damageType);
		
		if(%this.client) {
			%this.client.updateHealth();
		}
	}

	function Player::setHealth(%this, %health) {
		Parent::setHealth(%this, %health);

		if(%this.client) {
			%this.client.updateHealth();
		}
	}

	function Player::addHealth(%this, %health) {
		Parent::addHealth(%this, %health);

		if(%this.client) {
			%this.client.updateHealth();
		}
	}

	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);

		%this.lastHealthbarPercent = 1;
		if(isObject(%this.player)) {
			%this.updateHealth();

			MiniDugneonsTargetSet.add(%this.player);

			commandToClient(%this, 'MD_HandleSpawn');
		}
	}

	function Armor::onDisabled(%this, %obj) {
		if(MiniDugneonsTargetSet.isMember(%obj)) {
			MiniDugneonsTargetSet.remove(%obj);	
		}
		Parent::onDisabled(%this, %obj);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc) {
		%client.updateHealth(0);
		Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}
};
activatePackage(MountPoint);

function GameConnection::updateHealth(%this, %forceHealth) {
	%health = %forceHealth $= "" ? %this.player.getHealth() : %forceHealth;
	commandToClient(%this, 'MD_SetHealthbar', %health, %this.player.getMaxHealth());
	%percent = %health / %this.player.getMaxHealth();

	if(%this.lastHealthbarPercent > %percent) {
		%delta = %this.lastHealthbarPercent - %percent;
		%shakeX = mClamp(%delta * 50, 0, 20);
		%shakeY = mFloor(%shakeX / 2);
		commandToClient(%this, 'MD_VibrateHealthbar', 12 + mFloor(%delta * 20), %shakeX SPC %shakeY);
	}
	%this.lastHealthbarPercent = %percent;
}

function serverCmdMD_Handshake_Ack(%this, %version) {
	%this.miniDungeonsClientVersion = %version;
	%this.hasClientAddOn = true;
}
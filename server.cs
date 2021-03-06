forceRequiredAddOn("Player_BeefBoy");
forceRequiredAddOn("Brick_MiniDungeons");

exec("./sounds.cs");
exec("./playertype.cs");

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
exec("./lib/items.cs");
exec("./lib/ntname.cs");
exec("./lib/mount point.cs");
exec("./lib/brick text.cs");
exec("./lib/message.cs");

exec("./mechanics/client mod.cs");
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
exec("./mechanics/pearl compass.cs");
exec("./mechanics/potion.cs");
exec("./mechanics/chests.cs");
exec("./mechanics/large chest.cs");
exec("./mechanics/small chest.cs");
exec("./mechanics/pickup.cs");
exec("./mechanics/projectile detect.cs");
exec("./mechanics/is grounded.cs");
exec("./mechanics/shop.cs");

exec("./items/health flask.cs");
exec("./items/steak.cs");
exec("./items/compass.cs");

exec("./weapons/high flare.cs");
exec("./weapons/low explosion.cs");
exec("./weapons/one handed.cs");
exec("./weapons/double handed.cs");
exec("./weapons/wood shield.cs");
exec("./weapons/vault.cs");
exec("./weapons/alligator slayer.cs");
exec("./weapons/nodachi.cs");

exec("./generator/builder.cs");
exec("./generator/hallway interpreter.cs");
exec("./generator/hallway builder.cs");
exec("./generator/dungeon interpreter.cs");
exec("./generator/room builder.cs");
exec("./generator/room.cs");
exec("./generator/hallway.cs");
exec("./generator/ghost util.cs");
exec("./generator/one way door.cs");
exec("./generator/test.cs");
exec("./generator/stats.cs");

exec("./ai/on damage.cs");
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
exec("./ai/arming sword.cs");
// exec("./ai/horse.cs");

exec("./aiweapons/mace.cs");
exec("./aiweapons/gladius.cs");
exec("./aiweapons/bow.cs");
exec("./aiweapons/cannon.cs");
exec("./aiweapons/horse rabis.cs");
exec("./aiweapons/arming sword.cs");

parseAvatarFile("Lanky", "Add-Ons/Player_Beefboy/lanky/avatar.txt");
parseAvatarFile("Bull", "Add-Ons/Player_Beefboy/bull/avatar.txt");
parseAvatarFile("Bard", "Add-Ons/Server_MiniDungeons/ai/avatars/bard.txt");
parseAvatarFile("Mace", "Add-Ons/Server_MiniDungeons/ai/avatars/mace.txt");
parseAvatarFile("Gladius", "Add-Ons/Server_MiniDungeons/ai/avatars/gladius.txt");
parseAvatarFile("Shopkeep", "Add-Ons/Server_MiniDungeons/ai/avatars/shopkeep.txt");
parseAvatarFile("Horse", "Add-Ons/Server_MiniDungeons/ai/avatars/horse.txt");
parseAvatarFile("Duelist", "Add-Ons/Server_MiniDungeons/ai/avatars/duelist.txt");

exec("./setpieces/swordsmanship angel.cs");

function lineCount() {
	%totalCount = 0;
	for(%fileName = findFirstFile("Add-Ons/Server_MiniDungeons/*.cs"); %fileName !$= ""; %fileName = findNextFile("Add-Ons/Server_MiniDungeons/*.cs")) {
		%file = new FileObject();
		%file.openForRead(%fileName);
		%count = 0;
		while(!%file.isEOF()) {
			%file.readLine();
			%count++;
		}

		echo(fileBase(%fileName) SPC %count SPC "lines");
		%totalCount += %count;

		%file.close();
		%file.delete();
	}
	echo(%totalCount SPC "lines" SPC "in total");
}

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

function serverCmdBuildMode(%this) {
	if(%this.isAdmin) {
		%this.buildmode = !%this.buildmode;
		messageClient(%this, '', "\c6Buildmode is" SPC (%this.buildmode ? "on" : "off"));
		
		if(%this.hasSpawnedOnce) {
			%this.spawnPlayer();
		}
	}
}
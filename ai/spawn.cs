if( ! isObject(MiniDungeonsBotSpawnSet)) {
	new SimSet(MiniDungeonsBotSpawnSet);
}
datablock fxDTSBrickData(BrickAiSpawnData) {
	brickFile = "Add-ons/Bot_Hole/4xSpawn.blb";
	category = "Special";
	subCategory = "DUNGEONS!!!!!!!!";
	uiName = "Ai Hole";
	iconName = "Add-Ons/Bot_Blockhead/icon_blockhead";
	brickType = 2;
	canCover = 0;
	orientationFix = 1;
	indestructable = 1;
};
function BrickAiSpawnData::onPlant(%this, %obj) {
	Parent::onPlant(%this, %obj);
	MiniDungeonsBotSpawnSet.add(%obj);
}
function BrickAiSpawnData::onLoadPlant(%this, %obj) {
	Parent::onLoadPlant(%this, %obj);
	MiniDungeonsBotSpawnSet.add(%obj);
}
function BrickAiSpawnData::spawnBot(%this, %obj) {
	%botName = getSubStr(%obj.getName(), 1, strLen(%obj.getName()));
	%transform = vectorAdd(%obj.getSpawnPoint(), "0 0 1.2");
	%function = "create" @ %botName @ "Ai";
	%roomIndex = 0;
	if(isFunction(%function)) {
		%bot = 0;
		eval("%bot = " @ %function @ "(\"" @ %transform @ "\", " @ %roomIndex @ ");");
		// create the bot
		%obj.bot = %bot;
		%bot.spawn = %obj;
		if(isObject(%obj.room)) {
			%obj.room.roomOnBotSpawned(%bot);
		}
		return %bot;
	}
	else {
		return 0;
	}
}
function Player::spawnNearbyBots(%this) {
	%count = MiniDungeonsBotSpawnSet.getCount();
	for(%i = 0; %i < %count; %i++) {
		%brick = MiniDungeonsBotSpawnSet.getObject(%i);
		if(vectorDist(%this.getPosition(), %brick.getPosition()) < 25 &&  ! isObject(%brick.bot)) {
			%brick.getDatablock().spawnBot(%brick);
		}
	}
}
function spawnAllBots() {
	%count = MiniDungeonsBotSpawnSet.getCount();
	for(%i = 0; %i < %count; %i++) {
		%brick = MiniDungeonsBotSpawnSet.getObject(%i);
		if( ! isObject(%brick.bot)) {
			%brick.getDatablock().spawnBot(%brick);
		}
	}
}

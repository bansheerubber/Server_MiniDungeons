if(!isObject(MiniDungeonsBotSpawnSet)) {
	new SimSet(MiniDungeonsBotSpawnSet);
}

datablock fxDTSBrickData(BrickAiSpawnData) {
	brickFile = "Add-ons/Bot_Hole/4xSpawn.blb";
	category = "Special";
	subCategory = "DUNGEONS!!!!!!!!";
	uiName = "Bard Hole";
	iconName = "Add-Ons/Server_MiniDungeons/ai/shapes/bard icon";

	bricktype = 2;
	cancover = 0;
	orientationfix = 1;
	indestructable = 1;
};

function BrickAiSpawnData::onPlant(%this, %obj) {
	Parent::onPlant(%this, %obj);
	MiniDungeonsBotSpawnSet.add(%obj);
}

function BrickAiSpawnData::spawnBot(%this, %obj) {
	%botName = getSubStr(%obj.getName(), 1, strLen(%obj.getName());
	%transform = %obj.getSpawnPoint();
	%function = "create" @ %botName @ "Ai";
	%roomIndex = 0;

	if(isFunction(%function)) {
		%bot = 0;
		eval("%bot = " @ %function @ "(" @ %transform @ ", " @ %roomIndex @ ");"); // create the bot
		%obj.bot = %bot;
		%bot.spawn = %obj;
	}
}
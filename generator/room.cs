// save files as: size_type_difficulty_index.bls
// i.e. shop is 2x2_1_0_0.bls, spawn is 1x1_1_0_0.bls, battle room might be 4x5_0_2_3.bls
function createRoom(%position, %size, %type, %difficulty, %index) {
	%roomSet = new SimSet();
	$MD::DungeonRoomSet.add(%roomSet);
	%difficulty = 0;
	%xo = getWord(%position, 0);
	%yo = getWord(%position, 1);
	%width = getWord(%size, 0);
	%height = getWord(%size, 1);
	%roomSet.position = %position;
	%roomSet.worldPosition = vectorScale(%roomSet.position SPC 20, 8);
	%roomSet.worldPosition = getWords(%roomSet.worldPosition, 0, 1) SPC mFloor((getWord(%roomSet.worldPosition, 2) + 0.1) / 2.4) * 2.4 - 0.1;
	%roomSet.width = %width;
	%roomSet.height = %height;
	%roomSet.difficulty = %difficulty;
	%roomSet.type = %type;
	%roomSet.index = %index !$= "" ? %index : mClamp(getRandom(0, $MD::RoomIndicesCount[%width, %height, %type, %difficulty] - 1), 0, 100000);
	%roomSet.ghostedPlayers = new SimSet();
	%roomSet.botBricks = new SimSet();
	%roomSet.doorBricks = new SimSet();
	%roomSet.oneWayDoors = new SimSet();
	%roomSet.bots = new SimSet();
	%roomSet.hallways = new SimSet();
	%roomSet.botScope = new SimSet();
	%roomSet.pathfindingBricks = new SimSet();
	%roomSet.chestSpawn = 0;
	%roomSet.randomId = getRandom(1000000, 9999999);
	for(%x = 0; %x < %width; %x++) {
		for(%y = 0; %y < %height; %y++) {
			$MD::DungeonRoom[%xo + %x, %yo + %y] = %roomSet;
		}
	}
	return %roomSet;
}
function rePlaceBattleRooms() {
	%count = $MD::DungeonRoomSet.getCount();
	for(%i = 0; %i < %count; %i++) {
		%room = $MD::DungeonRoomSet.getObject(%i);
		if(%room.areBattleBotsDead) {
			%room.roomBuild(true);
			echo("Built room" SPC %room.randomId);
		}
	}
}
function SimSet::roomResetBattle(%this) {
	%this.areBattleBotsDead = false;
	%this.hasSpawnedBots = false;
}
function SimSet::roomOnBotSpawned(%this, %bot) {
	%this.bots.add(%bot);
	%bot.room = %this;
}
function SimSet::roomOnBotKilled(%this, %bot) {
	%this.bots.remove(%bot);
	if(%this.isBattleRoom && %this.bots.getCount() == 0) {
		%this.roomEndBattle(true);
	}
}
function SimSet::roomEndBattle(%this, %victory) {
	if(%victory) {
		%this.areBattleBotsDead = true;
		%this.roomSpawnChest();
	}
	%this.roomUnLockDoors();
}
function SimSet::roomSpawnChest(%this) {
	if(isObject(%this.chestSpawn)) {
		%position = vectorAdd(%this.chestSpawn.getPosition(), "0 0 5");
		%this.bots.add(SmallChestArmor.spawnChest(%position));
	}
}
function SimSet::roomLockDoors(%this) {
	%this.isLocked = true;
	%this.oneWayDoors.deleteAll();
	%count = %this.doorBricks.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.oneWayDoors.add(createOneWayDoor(%this.doorBricks.getObject(%i)));
	}
}
function SimSet::roomUnLockDoors(%this) {
	%this.isLocked = false;
	%this.oneWayDoors.deleteAll();
	%count = %this.ghostedPlayers.getCount();
	for(%i = 0; %i < %count; %i++) {
		%player = %this.ghostedPlayers.getObject(%i);
		%count2 = %this.doorBricks.getCount();
		for(%j = 0; %j < %count2; %j++) {
			%this.doorBricks.getObject(%j).reGhost(%player.client);
		}
	}
}
function SimSet::roomHandleBotSpawning(%this) {
	cancel(%this._roomHandleBotSpawning);
	// essentially debounce noise
	%this._roomHandleBotSpawning = %this.schedule(33, _roomHandleBotSpawning);
}
function SimSet::_roomHandleBotSpawning(%this) {
	// spawn bots if there's players inside the room's bot scope
	if(%this.botScope.getCount() > 0) {
		%this.roomSpawnBots();
	}
	else {
		%this.roomUnSpawnBots();
	}
}
function SimSet::roomSpawnBots(%this) {
	if( ! %this.hasSpawnedBots && ( ! %this.isBattleRoom || (%this.isBattleRoom &&  ! %this.areBattleBotsDead))) {
		%count = %this.botBricks.getCount();
		for(%i = 0; %i < %count; %i++) {
			%bot = BrickAiSpawnData.spawnBot(%this.botBricks.getObject(%i));
			%this.bots.add(%bot);
			%bot.room = %this;
		}
		%this.hasSpawnedBots = true;
	}
}
function SimSet::roomUnSpawnBots(%this) {
	%this.bots.deleteAll();
	%this.hasSpawnedBots = false;
}
function SimSet::roomAddNeighbor(%this, %neighbor) {
	%this.neighbor[%this.neighborCount | 0] = %neighbor;
	%this.neighborCount++;
}
function getOffsetFromOrientation(%orientation) {
	switch(%orientation) {
		case 0:
			return "-4 -4 0";

		case 1:
			return "-4 4 0";

		case 2:
			return "4 4 0";

		case 3:
			return "4 -4 0";

	}
}
function SimSet::roomBuild(%this, %isRePlace) {
	%this.areBattleBotsDead = false;
	// create spawn room
	if(%this.width == 1 && %this.height == 1) {
		%orientation = 0;
		for(%i = 0; %i < 4; %i++) {
			// test vectors
			%vector = cornerVectorHelper(%i);
			%x = getWord(%vector, 0) + getWord(%this.position, 0);
			%y = getWord(%vector, 1) + getWord(%this.position, 1);
			if($MD::DungeonHallwaySet[%x, %y]) {
				%orientation = %i;
				break;
			}
		}
		plantRoom("1x1_2_0_0", vectorAdd(%this.worldPosition, getOffsetFromOrientation(%orientation)), %orientation, %this, %this.randomId, %isRePlace);
	}
	else if(%this.width == 10 && %this.height == 10) {
		plantRoom("test_finalboss", vectorAdd(%this.worldPosition, getOffsetFromOrientation(0)), 0, %this, %this.randomId, %isRePlace);
	}
	else {
		plantRoom(%this.width @ "x" @ %this.height @ "_" @ %this.type @ "_" @ %this.difficulty @ "_" @ %this.index, vectorAdd(%this.worldPosition, getOffsetFromOrientation(0)), 0, %this, %this.randomId, %isRePlace);
		%this.isBattleRoom = %this.type == 0;
	}
}
function SimSet::roomDebugHighlight(%this, %time) {
	%count = %this.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.getObject(%i).setColorFx(3);
		%this.getObject(%i).schedule(%time, setColorFx, 0);
	}
}
function SimSet::roomFindClosestType(%this, %type) {
	deleteVariables("$MD::TempDungeonFindIter*");
	deleteVariables("$MD::TempDungeonFindQueue*");
	$MD::TempDungeonFindIter[%this] = true;
	$MD::TempDungeonFindQueue[$MD::TempDungeonFindQueueCount | 0] = %this;
	$MD::TempDungeonFindQueueCount++;
	$MD::TempDungeonFindQueueStart = 0;
	%count = 0;
	while(%room = $MD::TempDungeonFindQueue[$MD::TempDungeonFindQueueStart]) {
		// dequeue the root
		$MD::TempDungeonFindQueueStart++;
		if(%count > 5000) {
			return;
		}
		if(%room.type == %type) {
			return %room;
		}
		for(%i = 0; %i < %room.neighborCount; %i++) {
			%neighbor = %room.neighbor[%i];
			if( ! $MD::TempDungeonFindIter[%neighbor]) {
				$MD::TempDungeonFindIter[%neighbor] = true;
				// enqueue the neighbor
				$MD::TempDungeonFindQueue[$MD::TempDungeonFindQueueCount | 0] = %neighbor;
				$MD::TempDungeonFindQueueCount++;
			}
		}
	}
	return 0;
}
function SimSet::roomDebugHighlightNeighbors(%this, %time, %depth, %start) {
	if(%start) {
		deleteVariables("$MD::TempDungeonIter*");
	}
	$MD::TempDungeonIter[%this] = true;
	%this.roomDebugHighlight(%time);
	for(%i = 0; %i < %this.neighborCount; %i++) {
		%neighbor = %this.neighbor[%i];
		if( ! $MD::TempDungeonIter[%neighbor] && %depth > 0) {
			%neighbor.schedule(200, roomDebugHighlightNeighbors, %time, %depth - 1);
		}
	}
}
function SimSet::roomOnReGhostedToPlayer(%this, %player) {
	%this.ghostedPlayers.add(%player);
	if(%player.client.ghostDebug) {
		messageClient(%player.client, '', "Room" SPC %this SPC "ghosted");
	}
}
function SimSet::roomGhostToPlayer(%this, %client) {
	%count = %this.getCount();
	for(%i = 0; %i < %total; %i++) {
		%this.getObject(%i).reGhost(%client);
	}
}
function SimSet::roomOnUnGhostedToPlayer(%this, %player) {
	if(%this.ghostedPlayers.isMember(%player)) {
		%this.ghostedPlayers.remove(%player);
	}
	if(%player.client.ghostDebug) {
		messageClient(%player.client, '', "Room" SPC %this SPC "unghosted");
	}
}
function SimSet::roomOnPlayerEnter(%this, %player) {
	%this.botScope.add(%player);
	%this.roomHandleBotSpawning();
	if(%this.isBattleRoom && %this.bots.getCount() > 0 &&  ! %this.areBattleBotsDead &&  ! %this.isLocked) {
		%this.roomLockDoors();
	}
	if(%this.isLocked) {
		%count = %this.doorBricks.getCount();
		for(%i = 0; %i < %count; %i++) {
			%this.doorBricks.getObject(%i).unGhost(%player.client);
		}
		%count = %this.oneWayDoors.getCount();
		for(%i = 0; %i < %count; %i++) {
			%this.oneWayDoors.getObject(%i).reGhost(%player.client);
		}
	}
}
function SimSet::roomOnPlayerLeave(%this, %player) {
	%this.botScope.remove(%player);
	%this.roomHandleBotSpawning();
}
function Player::testGhosting(%this) {
	%x = mFloorMultipleCenter(getWord(%this.getPosition(), 0), 8) / 8;
	%y = mFloorMultipleCenter(getWord(%this.getPosition(), 1), 8) / 8;
	%room = %this.getCurrentRoom();
	%hallway = %this.getCurrentHallway();
	if(isObject(%room)) {
		%this.client.bottomPrint(%x SPC %y SPC %room SPC %room.neighborCount SPC %room.getCount() @ "<br>" @ %hallway, 1);
	}
	else {
		%this.client.bottomPrint(%x SPC %y SPC %hallway SPC "hallway?");
	}
	%this.schedule(33, testGhosting);
}
function Player::getCurrentRoom(%this) {
	return getRoomFromPosition(%this.getPosition());
}
function getRoomFromPosition(%position) {
	return $MD::DungeonRoom[mFloorMultipleCenter(getWord(%position, 0), 8) / 8, mFloorMultipleCenter(getWord(%position, 1), 8) / 8];
}
deActivatePackage(MiniDungeonsRooms);
package MiniDungeonsRooms {
	function GameConnection::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc) {
		%room = %this.player.getCurrentRoom();
		addRoomDeath(%room.width, %room.height, %room.type, %room.difficulty, %room.index);
		Parent::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc);
	}
	function GameConnection::getSpawnPoint(%this) {
		if($MD::DungeonSpawnPoints.getCount() == 0) {
			return "0 0 0 0 0 0 1";
		}
		else {
			return $MD::DungeonSpawnPoints.getObject(getRandom(0, $MD::DungeonSpawnPoints.getCount() - 1)).getTransform();
		}
	}
	function SimSet::onRemove(%this) {
		if(isObject(%this.ghostedPlayers)) {
			%this.ghostedPlayers.delete();
		}
		if(isObject(%this.botBricks)) {
			%this.botBricks.delete();
		}
		if(isObject(%this.bots)) {
			%this.bots.deleteAll();
			%this.bots.delete();
		}
		if(isObject(%this.hallways)) {
			%this.hallways.deleteAll();
			%this.hallways.delete();
		}
		if(isObject(%this.botScope)) {
			%this.botScope.delete();
		}
		if(isObject(%this.doorBricks)) {
			%this.doorBricks.delete();
		}
		if(isObject(%this.oneWayDoors)) {
			%this.oneWayDoors.deleteAll();
			%this.oneWayDoors.delete();
		}
		if(isObject(%this.pathfindingBricks)) {
			%this.pathfindingBricks.delete();
		}
		Parent::onRemove(%this);
	}
};
activatePackage(MiniDungeonsRooms);

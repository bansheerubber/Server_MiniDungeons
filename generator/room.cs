function createRoom(%position, %size) {
	%roomSet = new SimSet();
	$MD::DungeonRoomSet.add(%roomSet);
	
	%xo = getWord(%position, 0);
	%yo = getWord(%position, 1);

	%width = getWord(%size, 0);
	%height = getWord(%size, 1);

	%roomSet.position = %position;
	%roomSet.worldPosition = vectorScale(%roomSet.position SPC 20, 8);
	%roomSet.worldPosition = getWords(%roomSet.worldPosition, 0, 1)
		SPC mFloor(
			(getWord(%roomSet.worldPosition, 2) + 0.1) / 2.4
		) * 2.4 - 0.1;
	%roomSet.width = %width;
	%roomSet.height = %height;
	%roomSet.ghostedPlayers = new SimSet();
	%roomSet.botBricks = new SimSet();
	%roomSet.bots = new SimSet();
	%roomSet.hallways = new SimSet();
	%roomSet.botScope = new SimSet();
	%roomSet.type = "Room";
	
	for(%x = 0; %x < %width; %x++) {
		for(%y = 0; %y < %height; %y++) {
			$MD::DungeonRoom[%xo + %x, %yo + %y] = %roomSet;
		}
	}
	return %roomSet;
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
	if(!%this.hasSpawnedBots) {
		%count = %this.botBricks.getCount();
		for(%i = 0; %i < %count; %i++) {
			%this.bots.add(BrickAiSpawnData.spawnBot(%this.botBricks.getObject(%i)));
		}
	}

	%this.hasSpawnedBots = true;
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

function SimSet::roomBuild(%this) {
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
		plantRoom("spawn", vectorAdd(%this.worldPosition, getOffsetFromOrientation(%orientation)), %orientation, %this);
	}
	else if(%this.width == 2 && %this.height == 2) {
		plantRoom("test_shop", vectorAdd(%this.worldPosition, getOffsetFromOrientation(0)), 0, %this);
	}
	else if(%this.width == 10 && %this.height == 10) {
		plantRoom("test_finalboss", vectorAdd(%this.worldPosition, getOffsetFromOrientation(0)), 0, %this);
	}
	else {
		plantRoom("test_" @ %this.width @ "x" @ %this.height, vectorAdd(%this.worldPosition, getOffsetFromOrientation(0)), 0, %this);
	}
}

function SimSet::roomDebugHighlight(%this, %time) {
	%count = %this.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.getObject(%i).setColorFx(3);
		%this.getObject(%i).schedule(%time, setColorFx, 0);
	}
}

function SimSet::roomDebugHighlightNeighbors(%this, %time, %depth, %start) {
	if(%start) {
		deleteVariables("$MD::TempDungeonIter*");
	}
	
	$MD::TempDungeonIter[%this] = true;
	%this.roomDebugHighlight(%time);
	for(%i = 0; %i < %this.neighborCount; %i++) {
		%neighbor = %this.neighbor[%i];
		if(!$MD::TempDungeonIter[%neighbor] && %depth > 0) {
			%neighbor.schedule(200, roomDebugHighlightNeighbors, %time, %depth - 1);
		}
	}
}

function SimSet::roomOnReGhostedToPlayer(%this, %player) {
	%this.ghostedPlayers.add(%player);
}

function SimSet::roomOnUnGhostedToPlayer(%this, %player) {
	if(%this.ghostedPlayers.isMember(%player)) {
		%this.ghostedPlayers.remove(%player);
	}
}

function SimSet::roomOnPlayerEnter(%this, %player) {
	%this.botScope.add(%player);
	%this.roomHandleBotSpawning();
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
		%this.client.bottomPrint(
			%x SPC %y SPC %room SPC %room.neighborCount SPC %room.getCount() @ "<br>"
			@ %hallway, 1);
	}
	else {
		%this.client.bottomPrint(%x SPC %y SPC %hallway SPC "hallway?");
	}
	
	%this.schedule(33, testGhosting);
}

function Player::getCurrentRoom(%this) {
	return $MD::DungeonRoom[
		mFloorMultipleCenter(
			getWord(%this.getPosition(), 0),
			8
		) / 8,
		mFloorMultipleCenter(
			getWord(%this.getPosition(), 1),
			8
		) / 8
	];
}

deActivatePackage(MiniDungeonsRooms);
package MiniDungeonsRooms {
	function GameConnection::getSpawnPoint(%this) {
		return $MD::DungeonSpawnPoints.getObject(getRandom(0, $MD::DungeonSpawnPoints.getCount() - 1)).getTransform();
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

		Parent::onRemove(%this);
	}
};
activatePackage(MiniDungeonsRooms);
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
	
	for(%x = 0; %x < %width; %x++) {
		for(%y = 0; %y < %height; %y++) {
			$MD::DungeonRoom[%xo + %x, %yo + %y] = %roomSet;
		}
	}
	return %roomSet;
}

function SimSet::roomAddNeighbor(%this, %neighbor) {
	%this.neighbor[%this.neighborCount | 0] = %neighbor;
	%this.neighborCount++;
}

function SimSet::roomBuild(%this) {
	if(%this.width == 2 && %this.height == 2) {
		plantRoom("test_shop", vectorAdd(%this.worldPosition, "-4 -4 0"), 0, %this);
	}
	else {
		plantRoom("test_" @ %this.width @ "x" @ %this.height, vectorAdd(%this.worldPosition, "-4 -4 0"), 0, %this);
	}
}

function SimSet::roomUnGhost(%this, %client) {
	%count = %this.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.getObject(%i).unGhost(%client);
	}
}

function SimSet::roomReGhost(%this, %client) {
	%count = %this.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.getObject(%i).reGhost(%client);
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

function SimSet::roomReGhostBatch(%this, %client, %depth, %start) {
	if(%start) {
		%client.player.createGhostUtilObject(%this);
		deleteVariables("$MD::TempDungeonIter*");
	}
	
	$MD::TempDungeonIter[%this] = true;
	for(%i = 0; %i < %this.neighborCount; %i++) {
		%neighbor = %this.neighbor[%i];
		if(!$MD::TempDungeonIter[%neighbor] && %depth > 0) {
			%client.player.createGhostUtilObject(%neighbor);
			%neighbor.schedule(1, roomReGhostBatch, %client, %depth - 1); // delay it so first depth neighbors are ghosted first
		}
	}
}

function SimSet::roomReGhostQuick(%this, %client, %depth, %start) {
	if(%start) {
		deleteVariables("$MD::TempDungeonIter*");
	}
	
	$MD::TempDungeonIter[%this] = true;
	%this.roomReGhost(%client);
	for(%i = 0; %i < %this.neighborCount; %i++) {
		%neighbor = %this.neighbor[%i];
		if(!$MD::TempDungeonIter[%neighbor] && %depth > 0) {
			%neighbor.schedule(500, roomReGhostQuick, %client, %depth - 1);
		}
	}
}

function SimSet::roomUnGhostQuick(%this, %client, %depth, %start) {
	if(%start) {
		deleteVariables("$MD::TempDungeonIter*");
	}
	
	$MD::TempDungeonIter[%this] = true;
	%this.roomUnGhost(%client);
	for(%i = 0; %i < %this.neighborCount; %i++) {
		%neighbor = %this.neighbor[%i];
		if(!$MD::TempDungeonIter[%neighbor] && %depth > 0) {
			%neighbor.schedule(500, roomUnGhostQuick, %client, %depth - 1);
		}
	}
}

function GameConnection::unGhostAllRooms(%this) {
	%count = $MD::DungeonRoomSet.getCount();
	for(%i = 0; %i < %count; %i++) {
		$MD::DungeonRoomSet.getObject(%i).roomUnGhost(%this);
	}
}

function Player::testGhosting(%this) {
	%x = mFloorMultipleCenter(getWord(%this.getPosition(), 0), 8) / 8;
	%y = mFloorMultipleCenter(getWord(%this.getPosition(), 1), 8) / 8;
	%room = %this.getCurrentRoom();

	if(isObject(%room)) {
		%this.client.bottomPrint(%x SPC %y SPC %room SPC %room.neighborCount SPC %room.getCount(), 1);
	}
	else {
		%this.client.bottomPrint(%x SPC %y SPC "hallway?");
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
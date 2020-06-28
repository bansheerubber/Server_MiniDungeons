function createHallway(%startPosition, %endPosition, %isHorizontal) {
	%hallwaySet = new SimSet();
	$MD::DungeonHallwaySet.add(%hallwaySet);

	%xo = getWord(%startPosition, 0);
	%yo = getWord(%startPosition, 1);
	%xe = getWord(%endPosition, 0);
	%ye = getWord(%endPosition, 1);

	%hallwaySet.startPosition = %startPosition;
	%hallwaySet.endPosition = %endPosition;
	%hallwaySet.isHorizontal = %isHorizontal;
	%hallwaySet.type = "Hallway";

	%hallwaySet.worldStartPosition = vectorScale(%hallwaySet.startPosition SPC 20, 8);
	%hallwaySet.worldStartPosition = getWords(%hallwaySet.worldStartPosition, 0, 1)
		SPC mFloor(
			(getWord(%hallwaySet.worldStartPosition, 2) + 0.1) / 2.4
		) * 2.4 - 0.1;
	
	%hallwaySet.worldEndPosition = vectorScale(%hallwaySet.endPosition SPC 20, 8);
	%hallwaySet.worldEndPosition = getWords(%hallwaySet.worldEndPosition, 0, 1)
		SPC mFloor(
			(getWord(%hallwaySet.worldEndPosition, 2) + 0.1) / 2.4
		) * 2.4 - 0.1;
	
	%hallwaySet.ghostedPlayers = new SimSet();

	// iterate through the hallway's extent
	%dist = vectorDist(%startPosition, %endPosition);
	for(%i = 0; %i <= %dist; %i++) {
		%position = vectorLerpUnit(%startPosition, %endPosition, %i);
		%x = getWord(%position, 0);
		%y = getWord(%position, 1);
		$MD::DungeonHallwaySet[mCeil(%x), mCeil(%y)] = %hallwaySet;
	}

	// look up the rooms that own this hallway
	%room1 = $MD::DungeonRoom[%xo, %yo];
	if(isObject(%room1)) {
		%room1.hallways.add(%hallwaySet);
		%hallwaySet.room1 = %room1;
	}
	else {
		error("Could not find room at position (" @ %startPosition @ ")");
	}

	%room2 = $MD::DungeonRoom[%xe, %ye];
	if(isObject(%room2)) {
		%room2.hallways.add(%hallwaySet);
		%hallwaySet.room2 = %room2;
	}
	else {
		error("Could not find room at position (" @ %endPosition @ ")");
	}
}

function SimSet::hallwayOnPlayerEnter(%this, %player) {
	if(isObject(%this.room1)) {
		%this.room1.botScope.add(%player);
		%this.room1.roomHandleBotSpawning();
	}

	if(isObject(%this.room2)) {
		%this.room2.botScope.add(%player);
		%this.room2.roomHandleBotSpawning();
	}
}

function SimSet::hallwayOnPlayerLeave(%this, %player) {
	if(isObject(%this.room1)) {
		%this.room1.botScope.remove(%player);
		%this.room1.roomHandleBotSpawning();
	}

	if(isObject(%this.room2)) {
		%this.room2.botScope.remove(%player);
		%this.room2.roomHandleBotSpawning();
	}
}

function SimSet::hallwayBuild(%this) {
	buildHallway("test", %this.worldStartPosition, %this.worldEndPosition, getRandom(0, 10000));
}

function Player::getCurrentHallway(%this) {
	return $MD::DungeonHallwaySet[
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

deActivatePackage(MiniDungeonsHallways);
package MiniDungeonsHallways {
	function SimSet::onRemove(%this) {
		// note: ghostedPlayers is deleted in MiniDungeonsRooms package
		Parent::onRemove(%this);
	}
};
activatePackage(MiniDungeonsHallways);
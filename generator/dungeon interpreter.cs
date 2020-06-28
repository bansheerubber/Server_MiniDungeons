function readDungeonFile(%fileName) {
	// cleanup
	if(isObject($MD::DungeonRoomSet)) {
		$MD::DungeonRoomSet.deleteAll();
	}

	if(isObject($MD::DungeonHallwaySet)) {
		$MD::DungeonHallwaySet.deleteAll();
	}
	
	deleteVariables("$MD::Dungeon*");

	if(!isObject($MD::DungeonRoomSet)) {
		$MD::DungeonRoomSet = new SimSet();
	}

	if(!isObject($MD::DungeonHallwaySet)) {
		$MD::DungeonHallwaySet = new SimSet();
	}

	if(!isFile(%fileName)) {
		error("Could not find file" SPC %fileName);
		return;
	}
	
	%file = new FileObject();
	%file.openForRead(%fileName);

	%mode = 0;
	while(!%file.isEOF()) {
		%line = %file.readLine();

		if(%line $= "") {
			%mode++;
			continue;
		}

		// parse room types
		if(%mode == 0) {
			$MD::DungeonRoomType[$MD::DungeonRoomTypeCount | 0] = %line;
			$MD::DungeonRoomTypeCount++;
		}
		// parse rooms
		else if(%mode == 1) {
			$MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "position"] = getWords(%line, 1, 2);
			$MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "size"] = $MD::DungeonRoomType[getWord(%line, 0)];

			createRoom(
				$MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "position"],
				$MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "size"]
			);

			$MD::DungeonRoomsCount++;
		}
		// parse hallways
		else if(%mode == 2) {
			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "start"] = getWords(%line, 0, 1);

			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "end"] = getWords(%line, 2, 3);
			
			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "isHorizontal"] = getWord(%line, 4);
			
			// used for determining what kind of doorway to place
			$MD::DungeonHallways[getWords(%line, 0, 1)] = getWord(%line, 4);
			$MD::DungeonHallways[getWords(%line, 2, 3)] = getWord(%line, 4);

			createHallway(
				$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "start"],
				$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "end"],
				$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "isHorizontal"]
			);

			$MD::DungeonHallwaysCount++;
		}
		// parse neighbors
		else if(%mode == 3) {
			if(%neighborLineIndex $= "") {
				%neighborLineIndex = 0;
			}

			%room = $MD::DungeonRoomSet.getObject(%neighborLineIndex);
			%count = getWordCount(%line);
			for(%i = 0; %i < %count; %i++) {
				%neighbor = $MD::DungeonRoomSet.getObject(getWord(%line, %i));
				%room.roomAddNeighbor(%neighbor);
			}

			%neighborLineIndex++;
		}
	}

	%file.close();
	%file.delete();
}

function testRooms() {
	%count = $MD::DungeonRoomSet.getCount();
	for(%i = 0; %i < %count; %i++) {
		$MD::DungeonRoomSet.getObject(%i).schedule(100 * %i, roomBuild);
	}
}

function testHallways() {
	%count = $MD::DungeonHallwaySet.getCount();
	for(%i = 0; %i < %count; %i++) {
		$MD::DungeonHallwaySet.getObject(%i).schedule(10 * %i, hallwayBuild);
	}
}
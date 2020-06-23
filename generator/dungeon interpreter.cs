function readDungeonFile(%fileName) {
	deleteVariables("$MD::Dungeon*");

	if(!isObject($MD::DungeonRoomSet)) {
		$MD::DungeonRoomSet = new SimSet();
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
			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "start"] = vectorScale(getWords(%line, 0, 1) SPC 20, 8);
			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "start"] = 
				getWords($MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "start"], 0, 1)
				SPC mFloor(
					(getWord($MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "start"], 2) + 0.1) / 2.4
				) * 2.4 - 0.1;

			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "end"] = vectorScale(getWords(%line, 2, 3) SPC 20, 8);
			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "end"] = 
				getWords($MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "end"], 0, 1)
				SPC mFloor(
					(getWord($MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "end"], 2) + 0.1) / 2.4
				) * 2.4 - 0.1;
			
			$MD::DungeonHallways[$MD::DungeonHallwaysCount | 0, "isHorizontal"] = getWord(%line, 4);
			
			$MD::DungeonHallways[getWords(%line, 0, 1)] = getWord(%line, 4);
			$MD::DungeonHallways[getWords(%line, 2, 3)] = getWord(%line, 4);

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
	for(%i = 0; %i < $MD::DungeonHallwaysCount; %i++) {
		%start = $MD::DungeonHallways[%i, "start"];
		%end = $MD::DungeonHallways[%i, "end"];
		buildHallway("test", %start, %end, getRandom(0, 10000));
	}
}
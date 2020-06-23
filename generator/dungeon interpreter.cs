function readDungeonFile(%fileName) {
	deleteVariables("$MD::Dungeon*");

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
			$MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "position"] = vectorScale(getWords(%line, 1, 2) SPC 20, 8);
			$MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "position"] = 
				getWords($MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "position"], 0, 1)
				SPC mFloor(
					(getWord($MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "position"], 2) + 0.1) / 2.4
				) * 2.4 - 0.1;
			
			$MD::DungeonRooms[$MD::DungeonRoomsCount | 0, "size"] = $MD::DungeonRoomType[getWord(%line, 0)];
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
	}

	%file.close();
	%file.delete();
}

function testRooms() {
	%count = $MD::DungeonRoomsCount;
	for(%i = 0; %i < %count; %i++) {
		%width = getWord($MD::DungeonRooms[%i, "size"], 0);
		%height = getWord($MD::DungeonRooms[%i, "size"], 1);
		%position = $MD::DungeonRooms[%i, "position"];

		if(%width == 2 && %height == 2) {
			schedule(100 * %i, 0, plantRoom, "test_shop", vectorAdd(%position, "-4 -4 0"), 0);
		}
		else {
			schedule(100 * %i, 0, plantRoom, "test_" @ %width @ "x" @ %height, vectorAdd(%position, "-4 -4 0"), 0);
		}

		// for(%x = 0; %x < %width; %x++) {
		// 	for(%y = 0; %y < %height; %y++) {
		// 		%brick = new fxDTSBrick() {
		// 			datablock = brick16x16FData;
		// 			position = vectorAdd(
		// 				%position,
		// 				vectorScale(
		// 					%x SPC %y SPC 0,
		// 					8
		// 				)
		// 			);
		// 			angleId = 0;
		// 			colorID = 0;
		// 			isBasePlate = 1;
		// 			rotation = "0 0 0 1";
		// 			sacle = "1 1 1";
		// 			client = 0;
					
		// 			isPlanted = 1;
		// 			isHackBrick = 1;
		// 		};
		// 		BrickGroup_999999.add(%brick);
		// 		%error = %brick.plant();

		// 		%brick.onPlant();
		// 		%brick.setTrusted(true);
		// 	}
		// }
	}
}

function testHallways() {
	for(%i = 0; %i < $MD::DungeonHallwaysCount; %i++) {
		%start = $MD::DungeonHallways[%i, "start"];
		%end = $MD::DungeonHallways[%i, "end"];
		buildHallway("test", %start, %end, getRandom(0, 10000));
	}
}
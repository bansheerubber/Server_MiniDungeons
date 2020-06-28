function loadWallFiles() {
	deleteVariables("$MD::Room*");
	
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_s_d_b.bls", "wall_s_d_b", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_s_b.bls", "wall_s_b", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_s_m.bls", "wall_s_m", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_s_t.bls", "wall_s_t", "Room");

	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c1_d_b.bls", "wall_c1_d_b", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c1_b.bls", "wall_c1_b", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c1_m.bls", "wall_c1_m", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c1_t.bls", "wall_c1_t", "Room");

	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c2_d_b.bls", "wall_c2_d_b", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c2_b.bls", "wall_c2_b", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c2_m.bls", "wall_c2_m", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_wall_c2_t.bls", "wall_c2_t", "Room");

	loadDungeonBLS("config/NewDuplicator/Saves/testceiling1.bls", "ceiling", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/room_floor1.bls", "floor1", "Room");

	for(%width = 3; %width < 6; %width++) {
		for(%height = 3; %height < 6; %height++) {
			if(!%set[%width @ %height]) {
				loadDungeonBLS("config/NewDuplicator/Saves/test_" @ %width @ "x" @ %height @ ".bls", "test_" @ %width @ "x" @ %height, "Room");
				%set[%width @ %height] = true;
			}
		}
	}

	loadDungeonBLS("config/NewDuplicator/Saves/test_shop.bls", "test_shop", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/spawn.bls", "spawn", "Room");
	loadDungeonBLS("config/NewDuplicator/Saves/test_finalboss.bls", "test_finalboss", "Room");
}

function getWallFlags(%datablock, %color, %isDoor) {
	switch$(%datablock) {
		case "BrickStraightSketchData":
			%flags = "wall_" @ "s";
		
		case "BrickCorner1SketchData":
			%flags = "wall_" @ "c1";
		
		case "BrickCorner2SketchData":
			%flags = "wall_" @ "c2";
		
		case "BrickFloorSketchData":
			return "floor1";
		
		case "BrickCeilingSketchData":
			return "ceiling";
	}

	if(%color == 0) {
		if(%isDoor) {
			%flags = %flags @ "_" @ "d";
		}
		
		%flags = %flags @ "_" @ "b";
	}
	else if(%color == 1) {
		%flags = %flags @ "_" @ "m";
	}
	else if(%color == 2) {
		%flags = %flags @ "_" @ "t";
	}
}

function getWallPosition(%datablock, %position) {
	switch$(%datablock) {
		case "BrickStraightSketchData" or "BrickCorner1SketchData" or "BrickCorner2SketchData":
			return vectorAdd(%position, "0 0 -3");
		
		case "BrickCeilingSketchData":
			return vectorAdd(%position, "0 0 -7.5");
		
		default:
			return %position;
	}
}

function plantRoom(%name, %position, %orientation, %simSet) {
	%position = getWords(%position, 0, 1)
		SPC mFloor(
			(getWord(%position, 2) + 0.1) / 0.2
		) * 0.2 - 0.1; // round position to nearest plate on z axis
	
	if($MD::RoomCount $= "") {
		$MD::RoomCount = 0;
	}
	
	for(%i = 0; %i < $MD::Room[%name, "brickCount"]; %i++) {
		%brickPosition = $MD::Room[%name, %i, "position"];
		%rotation = "0 0" SPC -90 * %orientation;
		%brickPosition = vectorRotateEuler(%brickPosition, %rotation);
		%brickPosition = vectorAdd(%position, %brickPosition);
		%colorId = $MD::Room[%name, %i, "colorId"];
		%angleId = ($MD::Room[%name, %i, "angleId"] + %orientation) % 4;
		
		if(strPos($MD::Room[%name, %i, "datablock"].getName(), "SketchData") != -1) {
			%x = mFloorMultipleCenter(getWord(%brickPosition, 0), 8) / 8;
			%y = mFloorMultipleCenter(getWord(%brickPosition, 1), 8) / 8;
			%isDoor = false;
			if($MD::DungeonHallways[%x SPC %y] !$= "") {
				if(
					((%angleId == 0 || %angleId == 2) && $MD::DungeonHallways[%x SPC %y] == 1)
					|| ((%angleId == 1 || %angleId == 3) && $MD::DungeonHallways[%x SPC %y] == 0)
				) {
					%isDoor = true;
				}
			}
			
			plantRoom(
				getWallFlags(
					$MD::Room[%name, %i, "datablock"].getName(),
					$MD::Room[%name, %i, "colorId"],
					%isDoor
				),
				getWallPosition($MD::Room[%name, %i, "datablock"].getName(), %brickPosition),
				%angleId,
				%simSet
			);
		}
		else {
			%brick = new fxDTSBrick() {
				datablock = $MD::Room[%name, %i, "datablock"];
				position = %brickPosition;
				angleId = %angleId;
				colorID = %colorId;
				colorFxID = $MD::Room[%name, %i, "colorFXID"];
				isBasePlate = 1;
				printID = $printNameTable[$MD::Room[%name, %i, "print"]];
				rotation = getRotFromAngleID(%angleId);
				sacle = "1 1 1";
				shapeFxID = $MD::Room[%name, %i, "shapeFXID"];
				client = 0;
				
				isPlanted = 1;
				isHackBrick = 1;
			};
			BrickGroup_999999.add(%brick);

			if(isObject(%simSet)) {
				%simSet.add(%brick);

				if($MD::Room[%name, %i, "datablock"].getName() $= "BrickAiSpawnData") {
					%simSet.botBricks.add(%brick);
				}

				%brick.setNetFlag(6, 1); // all bricks start off unghosted
			}

			%error = %brick.plant();

			if(%error == 1) {
				%brick.delete();
			}
			else {
				%brick.onPlant();
				%brick.setTrusted(true);
				
				%brick.setRaycasting($MD::Room[%name, %i, "isRaycasting"]);
				%brick.setColliding($MD::Room[%name, %i, "isColliding"]);
				%brick.setRendering($MD::Room[%name, %i, "isRendering"]);

				if($MD::Room[%name, %i, "light"] !$= "") {
					%brick.setLight($MD::Room[%name, %i, "light"]);
				}

				if($MD::Room[%name, %i, "emitter"] !$= "") {
					%brick.setEmitter($MD::Room[%name, %i, "emitter"]);
					%brick.setEmitterDirection($MD::Room[%name, %i, "emitterDirection"]);
				}

				%brick.numEvents = 0;
				for(%j = 0; %j < $MD::Room[%name, %i, "eventCount"]; %j++) {
					%brick.handleEventLine($MD::Room[%name, %i, "event", %j], $MD::RoomCount);
				}

				if($MD::Room[%name, %i, "name"] !$= "") {
					%brickName = strReplace($MD::Room[%name, %i, "name"], "ID", $MD::RoomCount);
					%brick.setNTObjectName(%brickName);

					if(strPos(%brickName, "spawnpoint") != -1) {
						$MD::DungeonSpawnPoints.add(%brick);
					}
				}
			}
		}
	}

	$MD::RoomCount++;
}

function unGhostGroup(%group, %client) {
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++) {
		%group.getObject(%i).unGhost(%client);
	}
}

function reGhostGroup(%group, %client) {
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++) {
		%group.getObject(%i).reGhost(%client);
	}
}
function plantRoom(%name, %position, %orientation) {
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
			}
		}
	}

	$MD::RoomCount++;
}
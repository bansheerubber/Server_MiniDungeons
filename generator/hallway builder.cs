function plantHallway(%hallwayCollectionName, %name, %position, %orientation, %dependentPartName, %dependentRecolorName, %recolorName) {
	%position = getWords(%position, 0, 1) SPC mFloor((getWord(%position, 2) + 0.1) / 0.2) * 0.2 - 0.1; // round position to nearest plate on z axis
	
	%alias = %name;
	if($MD::Hallway[%hallwayCollectionName, %name, "recolorAlias"] !$= "") {
		%alias = $MD::Hallway[%hallwayCollectionName, %name, "recolorAlias"];
	}
	
	if($MD::HallwayCount $= "") {
		$MD::HallwayCount = 0;
	}
	
	for(%i = 0; %i < $MD::Hallway[%name, "brickCount"]; %i++) {
		%brickPosition = $MD::Hallway[%name, %i, "position"];
		%rotation = "0 0" SPC -90 * %orientation;
		%brickPosition = vectorRotateEuler(%brickPosition, %rotation);
		%brickPosition = vectorAdd(%position, %brickPosition);
		%colorId = $MD::Hallway[%name, %i, "colorId"];
		%angleId = ($MD::Hallway[%name, %i, "angleId"] + %orientation) % 4;

		// see if we have a valid recolor
		if($MD::Hallway[%hallwayCollectionName, %alias, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, %colorId] !$= "") {
			%colorId = $MD::Hallway[%hallwayCollectionName, %alias, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, %colorId];
		}

		%brick = new fxDTSBrick() {
			datablock = $MD::Hallway[%name, %i, "datablock"];
			position = %brickPosition;
			angleId = %angleId;
			colorID = %colorId;
			colorFxID = $MD::Hallway[%name, %i, "colorFXID"];
			isBasePlate = 1;
			printID = $printNameTable[$MD::Hallway[%name, %i, "print"]];
			rotation = getRotFromAngleID(%angleId);
			sacle = "1 1 1";
			shapeFxID = $MD::Hallway[%name, %i, "shapeFXID"];
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
			
			%brick.setRaycasting($MD::Hallway[%name, %i, "isRaycasting"]);
			%brick.setColliding($MD::Hallway[%name, %i, "isColliding"]);
			%brick.setRendering($MD::Hallway[%name, %i, "isRendering"]);

			if($MD::Hallway[%name, %i, "light"] !$= "") {
				%brick.setLight($MD::Hallway[%name, %i, "light"]);
			}

			if($MD::Hallway[%name, %i, "emitter"] !$= "") {
				%brick.setEmitter($MD::Hallway[%name, %i, "emitter"]);
				%brick.setEmitterDirection($MD::Hallway[%name, %i, "emitterDirection"]);
			}

			%brick.numEvents = 0;
			for(%j = 0; %j < $MD::Hallway[%name, %i, "eventCount"]; %j++) {
				%brick.handleEventLine($MD::Hallway[%name, %i, "event", %j], $MD::HallwayCount);
			}

			if($MD::Hallway[%name, %i, "name"] !$= "") {
				%brickName = strReplace($MD::Hallway[%name, %i, "name"], "ID", $MD::HallwayCount);
				%brick.setNTObjectName(%brickName);
			}
		}
	}

	$MD::HallwayCount++;
}

function getRandomHallwayPart(%hallwayCollectionName, %slot) {
	%random = getRandom(0, 10000);
	for(%i = 0; %i < $MD::HallwayCollection[%hallwayCollectionName, %slot, "count"]; %i++) {
		%partName = $MD::HallwayCollection[%hallwayCollectionName, %slot, %i];

		if($MD::HallwayCollection[%hallwayCollectionName, %slot, %i, "collectiveProbability"] * 10000 >= %random) {
			return %partName;
		}
	}
	return %partName;
}

function buildHallway(%hallwayCollectionName, %startPosition, %endPosition, %runId) {
	if(getWord(%startPosition, 2) < getWord(%endPosition, 2)) {
		return buildHallway(%hallwayCollectionName, %endPosition, %startPosition, %runId);
	}
	
	// ronud heights to staircase step height
	%startPosition = getWords(%startPosition, 0, 1) SPC mFloor((getWord(%startPosition, 2) + 0.1) / 2.4) * 2.4 - 0.1;
	%endPosition = getWords(%endPosition, 0, 1) SPC mFloor((getWord(%endPosition, 2) + 0.1) / 2.4) * 2.4 - 0.1;
	%xyDirection = vectorNormalize(vectorSub(getWords(%endPosition, 0, 1), getWords(%startPosition, 0, 1)));
	%direction = vectorNormalize(vectorSub(%endPosition, %startPosition));
	%distance = vectorDist(getWords(%endPosition, 0, 1), getWords(%startPosition, 0, 1));

	if(vectorDot(%direction, "0 1 0") > 0.9) {
		%orientation = 0;
	}
	else if(vectorDot(%direction, "0 -1 0") > 0.9) {
		%orientation = 2;
	}
	else if(vectorDot(%direction, "1 0 0") > 0.9) {
		%orientation = 1;
	}
	else if(vectorDot(%direction, "0 -1 0") > 0.9) {
		%orientation = 3;
	}

	%oldPosition = %startPosition;
	for(%i = 8; %i < %distance; %i += 8) {
		%position = vectorAdd(%startPosition, vectorScale(%xyDirection, %i));
		%position = vectorAdd(%position, "0 0" SPC getWord(vectorScale(%direction, %i), 2));
		%x = mFloor(getWord(%position, 0) / 0.5) * 0.5;
		%y = mFloor(getWord(%position, 1) / 0.5) * 0.5;
		%z = mFloor((getWord(%position, 2) + 0.1) / 2.4) * 2.4 - 0.1;
		%position = %x SPC %y SPC %z;

		// means a staircase is in order
		if(getWord(%position, 2) != getWord(%oldPosition, 2)) {
			schedule(%i / 8 * 33, 0, generateHallwayCollection, %hallwayCollectionName, vectorAdd(%position, "0 0 2.4"), %orientation, true, %runId);
		}
		else {
			schedule(%i / 8 * 33, 0, generateHallwayCollection, %hallwayCollectionName, %position, %orientation, false, %runId);
		}
		%oldPosition = %position;
	}
}

function testStaircase(%hallwayCollectionName, %startPosition, %direction, %count, %isDown, %runId) {
	if(!%isDown) {
		%startPosition = vectorAdd(%startPosition, vectorScale(%direction, %count * 8));
		%direction = vectorScale(%direction, -1);
		return testStaircase(%hallwayCollectionName, %startPosition, %direction, %count, true, %runId);
	}
	
	BrickGroup_999999.deleteAll();
	
	if(getWord(%direction, 1) == 1) {
		%orientation = 0;
	}
	else if(getWord(%direction, 1) == -1) {
		%orientation = 2;
	}
	else if(getWord(%direction, 0) == 1) {
		%orientation = 1;
	}
	else if(getWord(%direction, 0) == -1) {
		%orientation = 3;
	}

	%staircaseAdd = "0 0 -2.4";
	for(%i = 0; %i < %count; %i++) {
		schedule(%i * 33, 0, generateHallwayCollection, %hallwayCollectionName, vectorAdd(%startPosition, vectorScale(%direction, %i * 8)), %orientation, false, %runId);
		%startPosition = vectorAdd(%startPosition, %staircaseAdd);
	}
}

function testHallway(%hallwayCollectionName, %startPosition, %direction, %count, %orientation, %runId) {
	BrickGroup_999999.deleteAll();
	for(%i = 0; %i < %count; %i++) {
		schedule(%i * 33, 0, generateHallwayCollection, %hallwayCollectionName, vectorAdd(%startPosition, vectorScale(%direction, %i * 8)), %orientation, false, %runId);
	}
}

function generateHallwayCollection(%hallwayCollectionName, %position, %orientation, %isStaircase, %runId) {
	for(%i = 0; %i < $MD::Hallway[%hallwayCollectionName, "slot", "count"]; %i++) {
		%slot = $MD::Hallway[%hallwayCollectionName, "slot", %i];
		%partName = getRandomHallwayPart(%hallwayCollectionName, %slot);

		// make sure none of the slot's tags are forbidden
		if($MD::Hallway[%hallwayCollectionName, %slot, "tag", "count"] !$= "") {
			for(%j = 0; %j < $MD::Hallway[%hallwayCollectionName, %slot, "tag", "count"]; %j++) {
				%tag = $MD::Hallway[%hallwayCollectionName, %slot, "tag", %j];
				if(%forbiddenTag[%tag]) {
					break;
				}
			}

			if(%forbiddenTag[%tag]) {
				continue;
			}
		}

		// make sure the slot isn't forbidden
		if(%forbidden[%slot] || (%isStaircase && !$MD::Hallway[%hallwayCollectionName, %slot, "canStaircase"]) || (!%isStaircase && $MD::Hallway[%hallwayCollectionName, %slot, "onlyStaircase"])) {
			continue;
		}

		// for staircases
		if(%isStaircase && $MD::Hallway[%hallwayCollectionName, %partName, "staircase"] !$= "") {
			%partName = $MD::Hallway[%hallwayCollectionName, %partName, "staircase"];
		}

		if(%partName $= "") {
			continue;
		}

		%alias = %partName;
		if($MD::Hallway[%hallwayCollectionName, %partName, "recolorAlias"] !$= "") {
			%alias = $MD::Hallway[%hallwayCollectionName, %partName, "recolorAlias"];
		}

		// forbid upcoming slots
		if($MD::Hallway[%hallwayCollectionName, %partName, "forbid", "count"] !$= "") {
			for(%j = 0; %j < $MD::Hallway[%hallwayCollectionName, %partName, "forbid", "count"]; %j++) {
				%forbidden[$MD::Hallway[%hallwayCollectionName, %partName, "forbid", %j]] = true;
			}
		}

		// forbid upcoming tags
		if($MD::Hallway[%hallwayCollectionName, %partName, "forbidTag", "count"] !$= "") {
			for(%j = 0; %j < $MD::Hallway[%hallwayCollectionName, %partName, "forbidTag", "count"]; %j++) {
				%forbiddenTag[$MD::Hallway[%hallwayCollectionName, %partName, "forbidTag", %j]] = true;
			}
		}
		
		// determine a recolor to apply
		if($MD::Hallway[%hallwayCollectionName, %alias, "recolor"]) {
			if(!$MD::Hallway[%hallwayCollectionName, %alias, "recolor", "isDependent"]) {
				%index = getRandomRecolorIndex(0, $MD::Hallway[%hallwayCollectionName, %alias, "recolor", "count"] - 1, %runId, %alias);
				%fields = $MD::Hallway[%hallwayCollectionName, %alias, "recolor", %index];
				%dependentPartName = getWord(%fields, 0);
				%dependentRecolorName = getWord(%fields, 1);
				%recolorName = getWord(%fields, 2);

				%recolor[%alias] = %recolorName;

				// talk(%partName SPC "(aka" SPC %alias @ ") got random recolor" SPC %dependentPartName SPC %dependentRecolorName SPC %recolorName);
			}
			else {
				%fields = $MD::Hallway[%hallwayCollectionName, %alias, "recolor", 0];
				%dependentPartName = getWord(%fields, 0);
				%dependentRecolorName = %recolor[%dependentPartName];
				// get random recolor based off of dependence
				%index = getRandomRecolorIndex(0, $MD::Hallway[%hallwayCollectionName, %alias, "recolor", %dependentPartName, %dependentRecolorName, "count"] - 1, %runId, %alias);
				%fields = $MD::Hallway[%hallwayCollectionName, %alias, "recolor", %dependentPartName, %dependentRecolorName, %index];
				%recolorName = getWord(%fields, 2);

				// talk(%partName SPC "(aka" SPC %alias @ ") got random dependent recolor based off of" SPC %dependentPartName SPC %dependentRecolorName @ "," SPC %recolorName);
			}
		}

		if(%partName !$= "nothing") {
			%newOrientation = %orientation;
			if(($MD::Hallway[%hallwayCollectionName, %partName, "canMirror"] && getRandom(1, 2) == 1) || $MD::Hallway[%hallwayCollectionName, %partName, "forceName"]) {
				%newOrientation = (%orientation + 2) % 4;

				if($MD::Hallway[%hallwayCollectionName, %partName, "forbidIfMirror", "count"] !$= "") {
					for(%j = 0; %j < $MD::Hallway[%hallwayCollectionName, %partName, "forbidIfMirror", "count"]; %j++) {
						%forbidden[$MD::Hallway[%hallwayCollectionName, %partName, "forbidIfMirror", %j]] = true;
					}
				}

				if($MD::Hallway[%hallwayCollectionName, %partName, "forbidTagsIfMirror", "count"] !$= "") {
					for(%j = 0; %j < $MD::Hallway[%hallwayCollectionName, %partName, "forbidTagsIfMirror", "count"]; %j++) {
						%forbiddenTag[$MD::Hallway[%hallwayCollectionName, %partName, "forbidTagsIfMirror", %j]] = true;
					}
				}
			}

			plantHallway(%hallwayCollectionName, %partName, %position, %newOrientation, %dependentPartName, %dependentRecolorName, %recolorName);
		}
	}
}

function getRandomRecolorIndex(%min, %max, %index, %partName) {
	if(%index $= "") {
		return getRandom(%min, %max);
	}
	
	if($MD::HallwayRecolorRun[%partName, %index] $= "") {
		$MD::HallwayRecolorRun[%partName, %index] = getRandom(%min, %max);
	}
	
	return $MD::HallwayRecolorRun[%partName, %index];
}
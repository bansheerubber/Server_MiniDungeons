function loadHallwayBLS(%fileName, %name) {
	if($MD::Hallway[%name, "brickCount"] $= "") {
		%file = new FileObject();
		%file.openForRead(%fileName);
		%brickCount = 0;
		%origin = "0 0 1000";
		while(!%file.isEOF()) {
			%line = %file.readLine();

			// handle brick names
			if(getWord(%line, 0) $= "+-NTOBJECTNAME") {
				%objectName = getWord(%line, 1);
				$MD::Hallway[%name, %brickCount - 1, "name"] = %objectName;
				// detect the origin so we can handle it
				if(%objectName $= "origin") {
					%position = $MD::Hallway[%name, %brickCount - 1, "position"];
					if(getWord(%position, 2) < getWord(%origin, 2)) {
						%origin = %position;
					}

					%brickCount--;
					$MD::Hallway[%name, "brickCount"] = %brickCount;
				}
			}
			// handle events
			else if(getWord(%line, 0) $= "+-EVENT") {
				%eventCount = $MD::Hallway[%name, %brickCount - 1, "eventCount"] | 0;
				$MD::Hallway[%name, %brickCount - 1, "event", %eventCount] = %line;
				$MD::Hallway[%name, %brickCount - 1, "eventCount"]++;
			}
			else if(getWord(%line, 0) $= "+-LIGHT") {
				%linething = getWords(%line, 1, getWordCount(%line));
				%light = $uiNameTable_Lights[getSubStr(%linething, 0, strPos(%linething, "\""))];
				$MD::Hallway[%name, %brickCount - 1, "light"] = %light;
			}
			else if(getWord(%line, 0) $= "+-EMITTER") {
				%linething = getWords(%line, 1, getWordCount(%line));
				%emitter = $uiNameTable_Emitters[getSubStr(%linething, 0, strPos(%linething, "\""))];
				%direction = getSubStr(%linething, strPos(%linething, "\"") + 2, strLen(%linething));
				$MD::Hallway[%name, %brickCount - 1, "emitter"] = %emitter;
				$MD::Hallway[%name, %brickCount - 1, "emitterDirection"] = %direction;
			}
			else if((%index = strPos(%line, "\"")) != -1) {
				// position, angleid, isBasePlate, colorID, print, colorFXID, shapeFXID, isRaycasting, isColliding, isRendering
				//    0-2,      3,         4,         5,      6,       7,         8,           9,           10,          11
				
				%datablockName = getSubStr(%line, 0, %index);
				%datablock = $uiNameTable[%datablockName];

				// handle all bricks
				if(isObject(%datablock)) {
					%rest = getSubStr(%line, %index + 2, strLen(%line));
					if(getWord(%rest, 6) !$= "2x2f/arrow") { // only save bricks that do not have the arrow print
						%rest = getSubStr(%line, %index + 2, strLen(%line));
						$MD::Hallway[%name, %brickCount, "datablock"] = %datablock;
						$MD::Hallway[%name, %brickCount, "position"] = getWords(%rest, 0, 2);
						$MD::Hallway[%name, %brickCount, "angleId"] = getWord(%rest, 3);
						$MD::Hallway[%name, %brickCount, "isBasePlate"] = getWord(%rest, 4);
						$MD::Hallway[%name, %brickCount, "colorId"] = getWord(%rest, 5);
						$MD::Hallway[%name, %brickCount, "print"] = getWord(%rest, 6);
						$MD::Hallway[%name, %brickCount, "colorFXID"] = getWord(%rest, 7);
						$MD::Hallway[%name, %brickCount, "shapeFXID"] = getWord(%rest, 8);
						$MD::Hallway[%name, %brickCount, "isRaycasting"] = getWord(%rest, 9);
						$MD::Hallway[%name, %brickCount, "isColliding"] = getWord(%rest, 10);
						$MD::Hallway[%name, %brickCount, "isRendering"] = getWord(%rest, 11);
						%brickCount++;
						$MD::Hallway[%name, "brickCount"] = %brickCount;
					}
				}
			}
		}
		%file.close();
		%file.delete();

		// go through all bricks and apply the origin modifier
		for(%i = 0; %i < $MD::Hallway[%name, "brickCount"]; %i++) {
			if(getWord(%origin, 2) == 7.5) {
				%origin = setWord(%origin, 2, 0.1);
			}
			
			$MD::Hallway[%name, %i, "position"] = vectorSub($MD::Hallway[%name, %i, "position"], %origin SPC "0");
		}
	}
}

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

function fxDTSBrick::handleEventLine(%this, %line, %specialID) {
	%enabled = getField(%line, 2);
	%inputName = getField(%line, 3);
	%delay = getField(%line, 4);
	%targetName = getField(%line, 5);
	%NT = getField(%line, 6);
	%outputName = getField(%line, 7);
	%par1 = getField(%line, 8);
	%par2 = getField(%line, 9);
	%par3 = getField(%line, 10);
	%par4 = getField(%line, 11);

	%inputIdx = inputEvent_GetInputEventIdx(%inputName);

	%targetIdx = inputEvent_GetTargetIndex("FxDTSBrick", %inputIdx, %targetName);

	if(%targetName == -1) {
		%targetClass = "FxDTSBrick";
	}
	else {
		%field = getField($InputEvent_TargetList["FxDTSBrick", %inputIdx], %targetIdx);
		%targetClass = getWord(%field, 1);
	}

	%outputIdx = outputEvent_GetOutputEventIdx(%targetClass, %outputName);

	for(%j = 1; %j < 5; %j++) {
		%field = getField($OutputEvent_ParameterList[%targetClass, %outputIdx], %j - 1);
		%dataType = getWord(%field, 0);

		if(%dataType $= "Datablock" && %par[%j] !$= "-1") {
			%par[%j] = nameToId(%par[%j]);

			if(!isObject(%par[%j])) {
				%par[%j] = 0;
			}
		}
	}

	%j = %this.numEvents | 0;

	%this.eventEnabled[%j] = %enabled;
	%this.eventDelay[%j] = %delay;

	%this.eventInput[%j] = %inputName;
	%this.eventInputIdx[%j] = %inputIdx;

	if(%targetIdx == -1) {
		%this.eventNT[%j] = strReplace(%NT, "ID", %specialID);
	}

	%this.eventTarget[%j] = %targetName;
	%this.eventTargetIdx[%j] = %targetIdx;

	%this.eventOutput[%j] = %outputName;
	%this.eventOutputIdx[%j] = %outputIdx;
	%this.eventOutputAppendClient[%j] = $OutputEvent_AppendClient["FxDTSBrick", %outputIdx];

	//Why does this need to be so complicated?
	if(%targetIdx >= 0) {
		%targetClass = getWord($InputEvent_TargetListfxDtsBrick_[%inputIdx], %targetIdx * 2 + 1);
	}
	else {
		%targetClass = "FxDTSBrick";
	}

	%paramList = $OutputEvent_ParameterList[%targetClass, %outputIdx];
	%paramCount = getFieldCount(%paramList);

	%this.eventOutputParameter[%j, 1] = %par1;
	%this.eventOutputParameter[%j, 2] = %par2;
	%this.eventOutputParameter[%j, 3] = %par3;
	%this.eventOutputParameter[%j, 4] = %par4;

	%this.numEvents++;
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
	for(%i = 0; %i < %distance; %i += 8) {
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
function loadDungeonBLS(%fileName, %name, %globalPrefix) {
	if(
		getMDGlobal(
			%globalPrefix,
			%name,
			%brickCount
		) $= ""
	) {
		%file = new FileObject();
		%file.openForRead(%fileName);
		%brickCount = 0;
		%origin = "0 0 1000";
		while(!%file.isEOF()) {
			%line = %file.readLine();

			// handle brick names
			if(getWord(%line, 0) $= "+-NTOBJECTNAME") {
				%objectName = getWord(%line, 1);
				setMDGlobal(
					%objectName,
					%globalPrefix,
					%name,
					%brickCount - 1,
					"name"
				);
				// detect the origin so we can handle it
				if(%objectName $= "origin") {
					%position = getMDGlobal(
						%globalPrefix,
						%name,
						%brickCount - 1,
						"position"
					);

					if(getWord(%position, 2) < getWord(%origin, 2)) {
						%origin = %position;
					}

					%brickCount--;
					setMDGlobal(
						%brickCount,
						%globalPrefix,
						%name,
						"brickCount"
					);
				}
			}
			// handle events
			else if(getWord(%line, 0) $= "+-EVENT") {
				%eventCount = $MD::Hallway[%name, %brickCount - 1, "eventCount"] | 0;
				setMDGlobal(
					%line,
					%globalPrefix,
					%name,
					%brickCount - 1,
					"event",
					%eventCount
				);
				setMDGlobal(
					getMDGlobal(
						%globalPrefix,
						%name,
						%brickCount - 1,
						"eventCount"
					) + 1,
					%globalPrefix,
					%name,
					%brickCount - 1,
					"eventCount"
				);
			}
			else if(getWord(%line, 0) $= "+-LIGHT") {
				%linething = getWords(%line, 1, getWordCount(%line));
				setMDGlobal(
					$uiNameTable_Lights[
						getSubStr(
							%linething,
							0,
							strPos(%linething, "\"")
						)
					],
					%globalPrefix,
					%name,
					%brickCount - 1,
					"light"
				);
			}
			else if(getWord(%line, 0) $= "+-EMITTER") {
				%linething = getWords(%line, 1, getWordCount(%line));
				%emitter = $uiNameTable_Emitters[
					getSubStr(
						%linething,
						0,
						strPos(%linething, "\"")
					)
				];
				%direction = getSubStr(
					%linething,
					strPos(%linething, "\"") + 2,
					strLen(%linething)
				);

				setMDGlobal(
					%emitter,
					%globalPrefix,
					%name,
					%brickCount - 1,
					"emitter"
				);
				setMDGlobal(
					%direction,
					%globalPrefix,
					%name,
					%brickCount - 1,
					"emitterDirection"
				);
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

						setMDGlobal(
							%datablock,
							%globalPrefix,
							%name,
							%brickCount,
							"datablock"
						);
						setMDGlobal(
							getWords(%rest, 0, 2),
							%globalPrefix,
							%name,
							%brickCount,
							"position"
						);
						setMDGlobal(
							getWord(%rest, 3),
							%globalPrefix,
							%name,
							%brickCount,
							"angleId"
						);
						setMDGlobal(
							getWord(%rest, 4),
							%globalPrefix,
							%name,
							%brickCount,
							"isBasePlate"
						);
						setMDGlobal(
							getWord(%rest, 5),
							%globalPrefix,
							%name,
							%brickCount,
							"colorId"
						);
						setMDGlobal(
							getWord(%rest, 6),
							%globalPrefix,
							%name,
							%brickCount,
							"print"
						);
						setMDGlobal(
							getWord(%rest, 7),
							%globalPrefix,
							%name,
							%brickCount,
							"colorFXID"
						);
						setMDGlobal(
							getWord(%rest, 8),
							%globalPrefix,
							%name,
							%brickCount,
							"shapeFXID"
						);
						setMDGlobal(
							getWord(%rest, 9),
							%globalPrefix,
							%name,
							%brickCount,
							"isRaycasting"
						);
						setMDGlobal(
							getWord(%rest, 10),
							%globalPrefix,
							%name,
							%brickCount,
							"isColliding"
						);
						setMDGlobal(
							getWord(%rest, 11),
							%globalPrefix,
							%name,
							%brickCount,
							"isRendering"
						);

						%brickCount++;
						setMDGlobal(
							%brickCount,
							%globalPrefix,
							%name,
							"brickCount"
						);
					}
				}
			}
		}
		%file.close();
		%file.delete();

		// go through all bricks and apply the origin modifier
		%count = getMDGlobal(
			%globalPrefix,
			%name,
			"brickCount"
		);
		for(%i = 0; %i < %count; %i++) {
			if(getWord(%origin, 2) == 7.5) {
				%origin = setWord(%origin, 2, 0.1);
			}
			
			setMDGlobal(
				vectorSub(
					getMDGlobal(
						%globalPrefix,
						%name,
						%i,
						"position"
					),
					%origin SPC "0"
				),
				%globalPrefix,
				%name,
				%i,
				"position"
			);
		}
	}
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
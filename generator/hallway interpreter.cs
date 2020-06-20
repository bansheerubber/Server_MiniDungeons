// interpret all hall files in the hallways directory
function loadAllHallFiles() {
	deleteVariables("$MD::Hallway*");
	for(%i = findFirstFile("Add-Ons/Server_MiniDungeons/generator/hallways/*.hall"); %i !$= ""; %i = findNextFile("Add-Ons/Server_MiniDungeons/generator/hallways/*.hall")) {
		interpretHallFile(%i);
	}
}

function resetHallwayParts(%hallwayCollectionName) {
	deleteVariables("$MD::HallwayCollection" @ %hallwayCollectionName @ "_*");
	deleteVariables("$MD::Hallway" @ %hallwayCollectionName @ "_*");
}

// slot 0 = walls
// slot 1 = floors
// slot 2 = ceiling
function addHallwayPart(%hallwayCollectionName, %partName, %slot, %odds) {
	%count = $MD::HallwayCollection[%hallwayCollectionName, %slot, "count"] | 0;
	$MD::HallwayCollection[%hallwayCollectionName, %slot, %count] = %partName;
	$MD::HallwayCollection[%hallwayCollectionName, %slot, %count, "odds"] = %odds;
	$MD::HallwayCollection[%hallwayCollectionName, %slot, "count"]++;
}

function addHallwayRecolor(%hallwayCollectionName, %partName, %dependentPartName, %dependentRecolorName, %recolorName, %originalColor, %replaceColor) {
	$MD::Hallway[%hallwayCollectionName, %partName, "recolor"] = true;
	
	if(!$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %recolorName, "isRegistered"]) {
		%count = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", "count"] | 0;
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %count] = %dependentPartName SPC %dependentRecolorName SPC %recolorName;
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %recolorName, "isRegistered"] = true;
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", "count"]++;
	}

	if(!$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, "isRegistered"]) {
		%count = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, "count"] | 0;
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %count] = %dependentPartName SPC %dependentRecolorName SPC %recolorName;
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, "isRegistered"] = true;
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, "count"]++;
	}

	$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, %originalColor] = %replaceColor;
	// keep a set
	%count = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, "count"] | 0;
	$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, %count] = %originalColor;
	$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, "count"]++;
}

// hoo boy
function deleteHallwayRecolor(%hallwayCollectionName, %partName, %recolorName) {
	// unregister the color
	$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %recolorName, "isRegistered"] = false;
	
	// find the index where the recolor name is
	%index = -1;
	%count = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", "count"];
	for(%i = 0; %i < %count; %i++) {
		%fields = $MD::Hallway[%hallwayCollectionName, %name, "recolor", %recolorName, %i];
		%color = getField(%fields, 2);
		if(%color $= %recolorName) {
			%dependentPartName = getField(%fields, 0);
			%dependentRecolorName = getField(%fields, 1);
			%index = %i; // we found our index
			break;
		}
	}

	// now we have to shift the entire array downwards to cover up that index
	if(%index != -1) {
		for(%i = %index; %i < %count - 1; %i++) {
			$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %recolorName, %i] = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %recolorName, %i + 1];
		}
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", "count"]--;
	}

	// unregister the combination
	%index = -1;
	%count = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, "count"];
	for(%i = 0; %i < %count; %i++) {
		%fields = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %i];
		%color = getField(%fields, 2);			
		if(%color $= %recolorName) {
			%index = %i; // we found our index
			break;
		}
	}

	// now we have to shift the entire array downwards to cover up that index
	if(%index != -1) {
		for(%i = %index; %i < %count - 1; %i++) {
			$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %i] = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %i + 1];
		}
		$MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, "count"]--;
	}

	// delete the originalColor: replacementColor map
	deleteVariables("$MD::Hallway" @ %hallwayCollectionName @ "_" @ %dependentPartName @ "_" @ %dependentRecolorName @ "_" @ %recolorName @ "*");
}

function copyDependentHallwayRecolor(%hallwayCollectionName, %partName, %oldDependentPartName, %oldDependentRecolorName, %newDependentPartName, %newDependentRecolorName) {
	// copy over dependent part names by finding every single recolorName that is a part of the oldDependentPartName and the oldDependentRecolorName set
	%dependentCount = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %oldDependentPartName, %oldDependentRecolorName, "count"];
	for(%i = 0; %i < %dependentCount; %i++) {
		// get the parent's recolor name, and copy it over by modifying that name so it is unique
		%fields = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %dependentPartName, %dependentRecolorName, %i];
		%oldRecolorName = getField(%fields, 2);
		%newRecolorName = %oldRecolorName @ "_" @ %newDependentPartName @ "_" @ %newDependentRecolorName;

		// copy over the color replacements
		%colorCount = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %oldDependentPartName, %oldDependentRecolorName, %oldRecolorName, "count"] | 0;
		for(%j = 0; %j < %colorCount; %j++) {
			// get the parent's color replacements
			%originalColor = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %oldDependentPartName, %oldDependentRecolorName, %oldRecolorName, %j];
			%replaceColor = $MD::Hallway[%hallwayCollectionName, %partName, "recolor", %oldDependentPartName, %oldDependentRecolorName, %oldRecolorName, %originalColor];

			addHallwayRecolor(%hallwayCollectionName, %partName, %newDependentPartName, %newDependentRecolorName, %newRecolorName, %originalColor, %replaceColor);
		}
	}
}

function copyHallwayRecolor(%hallwayCollectionName, %oldPartName, %newPartName) {
	%count = $MD::Hallway[%hallwayCollectionName, %oldPartName, "recolor", "count"];
	for(%i = 0; %i < %count; %i++) {
		%fields = $MD::Hallway[%hallwayCollectionName, %oldPartName, "recolor", %i];
		%dependentPartName = getField(%fields, 0);
		%dependentRecolorName = getField(%fields, 1);
		%recolorName = getField(%fields, 2);

		%colorCount = $MD::Hallway[%hallwayCollectionName, %oldPartName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, "count"] | 0;
		for(%j = 0; %j < %colorCount; %j++) {
			// get the parent's color replacements
			%originalColor = $MD::Hallway[%hallwayCollectionName, %oldPartName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, %j];
			%replaceColor = $MD::Hallway[%hallwayCollectionName, %oldPartName, "recolor", %dependentPartName, %dependentRecolorName, %recolorName, %originalColor];

			addHallwayRecolor(%hallwayCollectionName, %newPartName, %dependentPartName, %dependentRecolorName, %recolorName, %originalColor, %replaceColor);
		}
	}
}

function setHallwayRecolorAlias(%hallwayCollectionName, %partName, %aliasPartName) {
	if($MD::Hallway[%hallwayCollectionName, %aliasPartName, "recolorAlias"] !$= "") {
		%aliasPartName = $MD::Hallway[%hallwayCollectionName, %aliasPartName, "recolorAlias"];
	}
	
	$MD::Hallway[%hallwayCollectionName, %partName, "recolorAlias"] = %aliasPartName;
}

// finishes up everything by calculating probabilities
function completeHallwayAddition(%hallwayCollectionName) {
	echo(" ");
	echo(%hallwayCollectionName @ ".hall");
	
	for(%i = 0; %i < $MD::Hallway[%hallwayCollectionName, "slot", "count"]; %i++) {
		%slot = $MD::Hallway[%hallwayCollectionName, "slot", %i];
		for(%j = 0; %j < $MD::HallwayCollection[%hallwayCollectionName, %slot, "count"]; %j++) {
			%partName = $MD::HallwayCollection[%hallwayCollectionName, %slot, %j];
			%total[%slot] += $MD::HallwayCollection[%hallwayCollectionName, %slot, %j, "odds"];
		}
	}

	for(%i = 0; %i < $MD::Hallway[%hallwayCollectionName, "slot", "count"]; %i++) {
		%slot = $MD::Hallway[%hallwayCollectionName, "slot", %i];
		
		if($MD::HallwayCollection[%hallwayCollectionName, %slot, "count"] !$= "") {
			echo("Slot" SPC %slot SPC "Statistics:");
		}
		for(%j = 0; %j < $MD::HallwayCollection[%hallwayCollectionName, %slot, "count"]; %j++) {
			%partName = $MD::HallwayCollection[%hallwayCollectionName, %slot, %j];
			$MD::HallwayCollection[%hallwayCollectionName, %slot, %j, "probability"] = $MD::HallwayCollection[%hallwayCollectionName, %slot, %j, "odds"] / %total[%slot];
			$MD::HallwayCollection[%hallwayCollectionName, %slot, %j, "collectiveProbability"] = $MD::HallwayCollection[%hallwayCollectionName, %slot, %j - 1, "collectiveProbability"] + $MD::HallwayCollection[%hallwayCollectionName, %slot, %j, "odds"] / %total[%slot];
			echo("  -" SPC %partName SPC "probability:" SPC mFloatLength($MD::HallwayCollection[%hallwayCollectionName, %slot, %j, "probability"] * 100, 2) @ "%");

			if($MD::Hallway[%hallwayCollectionName, %partName, "recolor", "count"]) {
				echo("  -" SPC %partName SPC "has" SPC $MD::Hallway[%hallwayCollectionName, %partName, "recolor", "count"] SPC "recolors");
			}
		}
	}
}

$MD::DontCareacter[0] = ",";
$MD::DontCareacter[1] = ":";
$MD::DontCareacter[2] = ";";
$MD::DontCareacter[3] = "-";
$MD::DontCareacter[4] = ">";
$MD::DontCareacter[5] = "<";
$MD::DontCareacter[6] = "(";
$MD::DontCareacter[7] = ")";
$MD::DontCareacterCount = 8;

$MD::CommentCharacter = "#";

function interpretHallFile(%fileName) {
	%file = new FileObject();
	%file.openForRead(%fileName);
	%name = fileBase(%fileName);
	resetHallwayParts(%name);

	%lineCount = 0;
	while(!%file.isEOF()) {
		%line = %file.readLine();
		for(%i = 0; %i < $MD::DontCareacterCount; %i++) { // remove characters that we dont care about
			%line = strReplace(%line, $MD::DontCareacter[%i], "");
		}

		if((%position = strPos(%line, $MD::CommentCharacter)) != -1) {
			%line = getSubStr(%line, 0, %position - 1);
		}

		%slot = interpretHallLine(%name, %line, %slot, %lineCount);
		%lineCount++;
	}

	completeHallwayAddition(%name);

	%file.close();
	%file.delete();
}

function interpretHallLine(%name, %line, %slot, %lineCount) {
	%command = getWord(%line, 0);
	switch$(%command) {
		case "lots": // allows any command to repeat itself from a start index to (and including) and end index
			%start = getWord(%line, 1);
			%end = getWord(%line, 2);
			%line = getWords(%line, 3, getWordCount(%line));
			for(%i = %start; %i <= %end; %i++) {
				%newLine = strReplace(%line, "%index%", %i); // replace special declaration
				%slot = interpretHallLine(%name, %newLine, %slot, %lineCount); // repeat the command
			}
		
		case "slot":
			%slot = getWord(%line, 1);
			$MD::Hallway[%name, "slot", $MD::Hallway[%name, "slot", "count"] | 0] = %slot;
			$MD::Hallway[%name, "slot", "count"]++;
		
		case "tags":
			%count = getWordCount(%line);
			for(%i = 1; %i < %count; %i++) {
				%tag = getWord(%line, %i);

				$MD::Hallway[%name, %slot, "tag", %i - 1] = %tag;
				$MD::Hallway[%name, %slot, "tag", %tag] = true;
			}
			$MD::Hallway[%name, %slot, "tag", "count"] = %count - 1;
		
		case "add":
			%partName = getWord(%line, 1);
			loadDungeonBLS("config/NewDuplicator/Saves/" @ %partName @ ".bls", %partName, "Hallway");
			addHallwayPart(%name, %partName, %slot, getWord(%line, 2));
		
		case "recolor":
			%affectedPartName = getWord(%line, 1);
			%recolorName = getWord(%line, 2);
			%count = getWordCount(%line);
			for(%i = 3; %i < %count; %i += 2) {
				%originalColor = getWord(%line, %i);
				%replaceColor = getWord(%line, %i + 1);
				addHallwayRecolor(%name, %affectedPartName, -1, -1, %recolorName, %originalColor, %replaceColor);
			}
		
		case "deprecolor":
			%affectedPartName = getWord(%line, 1);
			%recolorName = getWord(%line, 2);
			%dependentPartName = getWord(%line, 3);
			%dependentRecolorName = getWord(%line, 4);
			%count = getWordCount(%line);
			for(%i = 5; %i < %count; %i += 2) {
				%originalColor = getWord(%line, %i);
				%replaceColor = getWord(%line, %i + 1);
				addHallwayRecolor(%name, %affectedPartName, %dependentPartName, %dependentRecolorName, %recolorName, %originalColor, %replaceColor);
				$MD::Hallway[%name, %affectedPartName, "recolor", "isDependent"] = true;
			}

		// copies all recolor data from one part name to another
		// copyrecolor old part name, new part name
		case "copyrecolor":
			%affectedPartName = getWord(%line, 1);
			%oldDependentPartName = getWord(%line, 2);
			copyHallwayRecolor(%name, %affectedPartName, %oldDependentPartName);
		
		// copies all recolor data from one recolorName/dependentPartName/dependentRecolorName combination to a new combination
		// depcopyrecolor part name: old dependent part name, old dependent recolor name; new dependent part name, new dependent recolor name
		case "depcopyrecolor":
			%affectedPartName = getWord(%line, 1);
			%oldDependentPartName = getWord(%line, 2);
			%oldDependentRecolorName = getWord(%line, 3);
			%newDependentPartName = getWord(%line, 4);
			%newDependentRecolorName = getWord(%line, 5);
			copyDependentHallwayRecolor(%name, %affectedPartName, %oldDependentPartName, %oldDependentRecolorName, %newDependentPartName, %newDependentRecolorName);

		// deletes recolor data based on a recolorName/dependentPartName/dependentRecolorName combination
		// pdeleterecolor part name: recolor name
		case "deleterecolor":
			%affectedPartName = getWord(%line, 1);
			%recolorName = getWord(%line, 2);
			deleteHallwayRecolor(%name, %affectedPartName, %recolorName);
		
		// allows this part to reuse the partName of the specified part, so we can have wall variations without affecting ceiling/floor/etc colors
		case "setrecoloralias":
			%originalPartName = getWord(%line, 1);
			%aliasPartName = getWord(%line, 2);
			setHallwayRecolorAlias(%name, %originalPartName, %aliasPartName);
		
		// forbids the selected slots from being loaded when the specified part name is loaded
		case "forbid":
			%partName = getWord(%line, 1);
			%count = getWordCount(%line);
			for(%i = 2; %i < %count; %i++) {
				$MD::Hallway[%name, %partName, "forbid", %i - 2] = getWord(%line, %i);
			}
			$MD::Hallway[%name, %partName, "forbid", "count"] = %count - 2;
		
		case "forbidtags":
			%partName = getWord(%line, 1);
			%count = getWordCount(%line);
			for(%i = 2; %i < %count; %i++) {
				$MD::Hallway[%name, %partName, "forbidTag", %i - 2] = getWord(%line, %i);
			}
			$MD::Hallway[%name, %partName, "forbidTag", "count"] = %count - 2;
		
		// tells the engine that the part can be mirrored
		case "canmirror":
			%partName = getWord(%line, 1);
			$MD::Hallway[%name, %partName, "canMirror"] = true;
		
		// tells the engine to force a mirror
		case "forcemirror":
			%partName = getWord(%line, 1);
			$MD::Hallway[%name, %partName, "forceMirror"] = true;
		
		// tells the engine to forbid the specified slots only if we mirror
		case "forbidifmirror":
			%partName = getWord(%line, 1);
			%count = getWordCount(%line);
			for(%i = 2; %i < %count; %i++) {
				$MD::Hallway[%name, %partName, "forbidIfMirror", %i - 2] = getWord(%line, %i);
			}
			$MD::Hallway[%name, %partName, "forbidIfMirror", "count"] = %count - 2;
		
		case "forbidtagsifmirror": 
			%partName = getWord(%line, 1);
			%count = getWordCount(%line);
			for(%i = 2; %i < %count; %i++) {
				$MD::Hallway[%name, %partName, "forbidTagsIfMirror", %i - 2] = getWord(%line, %i);
			}
			$MD::Hallway[%name, %partName, "forbidTagsIfMirror", "count"] = %count - 2;
		
		// registers a staircase part
		case "staircase":
			%partName = getWord(%line, 1);
			%staircasePartName = getWord(%line, 2);
			loadDungeonBLS("config/NewDuplicator/Saves/" @ %staircasePartName @ ".bls", %staircasePartName, "Hallway");
			$MD::Hallway[%name, %partName, "staircase"] = %staircasePartName;
			setHallwayRecolorAlias(%name, %staircasePartName, %partName); // set recolor alias for all staircases
		
		// says that the current slot is able to handle staircases
		case "canstaircase":
			$MD::Hallway[%name, %slot, "canStaircase"] = true;
		
		// says the current slot is only able to handle staircases
		case "onlystaircase":
			$MD::Hallway[%name, %slot, "canStaircase"] = true;
			$MD::Hallway[%name, %slot, "onlyStaircase"] = true;
		
		default:
			if(trim(%command) !$= "") {
				echo("\c3Hallway Interpreter: Unknown command" SPC %command SPC "@" SPC %name @ "#" @ %lineCount);
			}
	}

	return %slot;
}
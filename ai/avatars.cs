function parseAvatarFile(%globalName, %fileName) {
	deleteVariables("$MD::Avatars" @ %globalName @ "*");
	
	%file = new FileObject();
	%file.openForRead(%fileName);

	$MD::Avatars[%globalName, "nodeCount"] = 0;

	while(!%file.isEOF()) {
		%line = trim(%file.readLine());
		%line = strReplace(%line, ",", "");

		if(%line !$= "") {
			%node = getWord(%line, 0);
			%argument = getWord(%line, 1);

			switch$(%argument) {
				case "clone":
					$MD::Avatars[%globalName, %node, "clone"] = getWord(%line, 2);

					$MD::Avatars[%globalName, $MD::Avatars[%globalName, "nodeCount"]] = %node;
					$MD::Avatars[%globalName, "nodeCount"]++;
				
				case "sameindex":
					$MD::Avatars[%globalName, %node, "sameindex"] = getWord(%line, 2);

					%count = getWordCount(%line);
					%colorCount = 0;
					for(%i = 3; %i < %count; %i += 3) {
						%color = getWords(%line, %i, %i + 2);
						$MD::Avatars[%globalName, %node, %colorCount] = %color;
						%colorCount++;
					}
					$MD::Avatars[%globalName, %node, "count"] = %colorCount;

					$MD::Avatars[%globalName, $MD::Avatars[%globalName, "nodeCount"]] = %node;
					$MD::Avatars[%globalName, "nodeCount"]++;
				
				default:
					switch$(%node) {
						case "headup":
							$MD::Avatars[%globalName, "headup"] = getWord(%line, 1);
						
						case "roll" or "rolldependent":
							%count = getWordCount(%line);
							%rollCount = 0;

							if(%node $= "rolldependent") {
								%start = 3;
								$MD::Avatars[%globalName, "rolls", $MD::Avatars[%globalName, "rollCount"] | 0] = getWord(%line, 2);
								$MD::Avatars[%globalName, "rolls", $MD::Avatars[%globalName, "rollCount"] | 0, "dependent"] = getWord(%line, 1);
							}
							else {
								%start = 2;
								$MD::Avatars[%globalName, "rolls", $MD::Avatars[%globalName, "rollCount"] | 0] = getWord(%line, 1);
							}

							for(%i = %start; %i < %count; %i++) {
								%node = getWord(%line, %i);
								$MD::Avatars[%globalName, "rolls", $MD::Avatars[%globalName, "rollCount"] | 0, %rollCount] = %node;
								%rollCount++;
							}
							$MD::Avatars[%globalName, "rolls", $MD::Avatars[%globalName, "rollCount"] | 0, "count"] = %rollCount;
							$MD::Avatars[%globalName, "rollCount"]++;
						
						case "facename" or "decalname":
							%count = getWordCount(%line);
							%nodeCount = 0;
							for(%i = 1; %i < %count; %i++) {
								%color = getWord(%line, %i);
								$MD::Avatars[%globalName, %node, %nodeCount] = %color;
								%nodeCount++;
							}
							$MD::Avatars[%globalName, %node, "count"] = %nodeCount;

							$MD::Avatars[%globalName, $MD::Avatars[%globalName, "nodeCount"]] = %node;
							$MD::Avatars[%globalName, "nodeCount"]++;
						
						default:
							%count = getWordCount(%line);
							%colorCount = 0;
							for(%i = 1; %i < %count; %i += 3) {
								%color = getWords(%line, %i, %i + 2);
								$MD::Avatars[%globalName, %node, %colorCount] = %color;
								%colorCount++;
							}
							$MD::Avatars[%globalName, %node, "count"] = %colorCount;

							$MD::Avatars[%globalName, $MD::Avatars[%globalName, "nodeCount"]] = %node;
							$MD::Avatars[%globalName, "nodeCount"]++;
					}
			}
		}
	}

	%file.close();
	%file.delete();
}

function Player::setAvatar(%this, %globalName) {
	%this.hideNode("ALL");
	%this.setHeadUp($MD::Avatars[%globalName, "headup"]);
	%count = $MD::Avatars[%globalName, "nodeCount"];
	for(%i = 0; %i < %count; %i++) {
		%node = $MD::Avatars[%globalName, %i];

		if((%node $= "lhand" || %node $= "rhand") && isObject(%this.swordHands)) {
			%applyTo = %this.swordHands;
		}
		else {
			%applyTo = %this;
		}

		if(%node $= "facename") {
			%randomFaceIndex = getRandom(0, $MD::Avatars[%globalName, %node, "count"] - 1);
			%randomFace = $MD::Avatars[%globalName, %node, %randomFaceIndex];
			%applyTo.setFaceName(%randomFace);
		}
		else if(%node $= "decalname") {
			%randomDecalIndex = getRandom(0, $MD::Avatars[%globalName, %node, "count"] - 1);
			%randomDecal = $MD::Avatars[%globalName, %node, %randomDecalIndex];
			%applyTo.setDecalName(%randomDecal);
		}
		else {
			if($MD::Avatars[%globalName, %node, "clone"] $= "") {
				if((%sameIndexNode = $MD::Avatars[%globalName, %node, "sameindex"]) $= "") {
					%randomColorIndex = getRandom(0, $MD::Avatars[%globalName, %node, "count"] - 1);
				}
				else {
					%randomColorIndex = %applyTo.nodeColorIndex[%sameIndexNode];
				}
				%randomColor = $MD::Avatars[%globalName, %node, %randomColorIndex];
				%applyTo.setNodeColor(%node, %randomColor SPC "1");
				%applyTo.nodeColor[%node] = %randomColor;
				%applyTo.nodeColorIndex[%node] = %randomColorIndex;
			}
			else {
				%applyTo.setNodeColor(%node, %this.nodeColor[$MD::Avatars[%globalName, %node, "clone"]] SPC "1");
			}
			%applyTo.unHideNode(%node);
		}
	}

	// go through random chances
	%rollCount = $MD::Avatars[%globalName, "rollCount"];
	for(%i = 0; %i < %rollCount; %i++) {
		%chance = $MD::Avatars[%globalName, "rolls", %i];
		%subCount = $MD::Avatars[%globalName, "rolls", %i, "count"];

		%randomNodeIndex = getRandom(0, %subCount - 1);
		%randomNode = $MD::Avatars[%globalName, "rolls", %i, %randomNodeIndex];
		for(%j = 0; %j < $MD::Avatars[%globalName, "rolls", %i, "count"]; %j++) {
			%this.hideNode($MD::Avatars[%globalName, "rolls", %i, %j]);
		}

		%random = getRandom(0, 100);
		%randomChance = %chance * 100;
		%dependent = $MD::Avatars[%globalName, "rolls", %i, "dependent"];
		
		if(%dependent $= "" || (%dependent !$= "" && %this.isNodeVisible(%dependent))) {
			if(%randomChance <= %random) {
				%this.unHideNode(%randomNode);
			}
			else {
				%this.hideNode(%randomNode);
			}
		}
	}
}
function clearPathfinding() {
	deleteVariables("$MD::Node*");
}

datablock fxDTSBrickData(BrickPathfindingNodeData) {
	brickFile = "base/data/bricks/flats/1x1f.blb";
	category = "Special";
	subCategory = "DUNGEONS!!!!!!!!";
	uiName = "Pathfinding Node";
	iconName = "base/client/ui/brickIcons/1x1F";
	isInteractable = true;
};

function BrickPathfindingNodeData::onPlant(%this, %obj) {
	Parent::onPlant(%this, %obj);
	
	if(!%obj.getGroup().pathfindingNodes) {
		%obj.getGroup().pathfindingNodes = new SimSet();
	}

	%obj.getGroup().pathfindingNodes.add(%obj);
}

function BrickPathfindingNodeData::onLoadPlant(%this, %obj) {
	Parent::onLoadPlant(%this, %obj);
	
	if(!%obj.getGroup().pathfindingNodes) {
		%obj.getGroup().pathfindingNodes = new SimSet();
	}

	%obj.getGroup().pathfindingNodes.add(%obj);
}

function BrickPathfindingNodeData::onInteract(%this, %obj, %interactee) {
	if(%interactee.buildmode) {
		%obj.isGraphMode = true;
		if(!isEventPending(%obj.visualizeNeighbors)) {
			if(!isObject(%obj.debugLines)) {
				%obj.debugLines = new SimSet();
			}
			
			%obj.visualizeNeighbors();
		}

		cancel(%obj.stopVisualizeNeighbors);
		%obj.stopVisualizeNeighbors = %obj.schedule(120000, stopVisualizeNeighbors);
	}
}

function BrickPathfindingNodeData::onLook(%this, %obj, %interactee) {
	if(%interactee.buildmode) {
		if(!isEventPending(%obj.visualizeNeighbors)) {
			if(!isObject(%obj.debugLines)) {
				%obj.debugLines = new SimSet();
			}
			
			%obj.visualizeNeighbors();
		}

		if(!%obj.isGraphMode) {
			cancel(%obj.stopVisualizeNeighbors);
			%obj.stopVisualizeNeighbors = %obj.schedule(200, stopVisualizeNeighbors);
		}
	}
}

function BrickPathfindingNodeData::onAdd(%this, %obj) {
	Parent::onAdd(%this, %obj);

	if(!%obj.isPlanted) {
		%obj.debugLines = new SimSet();
		%obj.visualizeNeighbors();
	}
}

function BrickPathfindingNodeData::onRemove(%this, %obj) {
	if(%obj.debugLines) {
		%obj.debugLines.deleteAll();
		%obj.debugLines.delete();
	}
	
	Parent::onRemove(%this, %obj);
}

function fxDTSBrick::stopVisualizeNeighbors(%this) {
	%this.debugLines.deleteAll();
	cancel(%this.visualizeNeighbors);
	%this.lastPosition = "";
	%this.isGraphMode = false;
}

function fxDTSBrick::visualizeNeighbors(%this) {
	cancel(%this.visualizeNeighbors);

	if(%this.lastPosition !$= %this.getPosition()) {
		%this.debugLines.deleteAll();
		
		initContainerRadiusSearch(%this.getPosition(), 14, $TypeMasks::fxBrickObjectType);
		while(%col = containerSearchNext()) {
			%raycast = containerRaycast(%this.getPosition(), %col.getPosition(), $TypeMasks::fxBrickObjectType, %this);
			if(%col != %this && %col.getDatablock().getName() $= "BrickPathfindingNodeData" && getWord(%raycast, 0) == %col) {
				%this.debugLines.add(drawDebugLine(%this.getPosition(), %col.getPosition(), 0.1, "1 0 0 1", -1));
			}
		}
		%this.lastPosition = %this.getPosition();
	}

	%this.visualizeNeighbors = %this.schedule(33, visualizeNeighbors);
}

function testNavmesh(%brickGroup, %id) {
	$MD::PathTCP.send("create_navmesh" SPC %id @ "\n");

	%count = %brickGroup.getCount();
	for(%i = 0; %i < %count; %i++) {
		%brick = %brickGroup.getObject(%i);
		if(%brick.getDatablock().getName() $= "BrickPathfindingNodeData") {
			$MD::PathTCP.send("add_navmesh" SPC %id SPC %brick.getPosition() @ "\n");
		}
	}

	$MD::PathTCP.send("test_navmesh" SPC %id @ "\n");
}

function addNeighbor(%id1, %id2) {
	if(!$MD::NodeNeighbor[%id1, %id2]) {
		$MD::PathTCP.send("neighbor" SPC %id1 SPC %id2 @ "\n");
		$MD::NodeNeighbor[%id1, %id2] = true;
		addNeighbor(%id1, %id2);
	}
}

function fxDTSBrick::searchNeighbors(%this) {
	initContainerRadiusSearch(%this.getPosition(), 14, $TypeMasks::fxBrickObjectType);
	while(%col = containerSearchNext()) {
		%raycast = containerRaycast(%this.getPosition(), %col.getPosition(), $TypeMasks::fxBrickObjectType, %this);
		if(
			%col != %this
			&& (
				%col.getName() $= "_node"
				|| %col.getDatablock().getName() $= "BrickPathfindingNodeData"
			)
			&& getWord(%raycast, 0) == %col
		) {
			addNeighbor(%this.nodeId, %col.nodeId);
			// drawDebugLine(%this.getPosition(), %col.getPosition(), 0.2, "1 0 0 1", 15000);
		}
	}
}

function buildNodesFromBricks(%brickGroup) {
	clearPathfinding();
	
	if($MD::NodeCount $= "") {
		$MD::NodeCount = 0;
	}
	
	%nodes = %brickGroup.getNTObject("_node");
	if(%nodes != -1) {
		%count = getFieldCount(%nodes);
		for(%i = 0; %i < %count; %i++) {
			%brick = getField(%nodes, %i);
			%brick.nodeId = $MD::NodeCount;
			$MD::Node[$MD::NodeCount] = %brick.getPosition();
			$MD::PathTCP.send("add" SPC $MD::NodeCount SPC %brick.getPosition() @ "\n");
			$MD::NodeCount++;
		}

		for(%i = 0; %i < %count; %i++) {
			%brick = getField(%nodes, %i);
			%brick.searchNeighbors();
		}
	}

	// do pathfinding bricks
	%count = %brickGroup.pathfindingNodes.getCount();
	for(%i = 0; %i < %count; %i++) {
		%brick = %brickGroup.pathfindingNodes.getObject(%i);
		talk(%brick SPC %i);
		%brick.nodeId = $MD::NodeCount;
		$MD::Node[$MD::NodeCount] = %brick.getPosition();
		$MD::PathTCP.send("add" SPC $MD::NodeCount SPC %brick.getPosition() @ "\n");
		$MD::NodeCount++;
	}

	for(%i = 0; %i < %count; %i++) {
		%brickGroup.pathfindingNodes.getObject(%i).searchNeighbors();
	}

	$MD::PathTCP.send("simplify\n");
}

function getClosestNode(%position) {
	%nodeId = -1;
	%radius = 20;
	initContainerRadiusSearch(%position, %radius, $TypeMasks::fxBrickObjectType);
	%minDistance = %radius;
	while(%col = containerSearchNext()) {
		%raycast = containerRaycast(%position, %col.getPosition(), $TypeMasks::fxBrickObjectType);
		
		if(
			(
				%col.getName() $= "_node"
				|| %col.getDatablock().getName() $= "BrickPathfindingNodeData"
			)
			&& getWord(%raycast, 0) == %col
			&& (%distance = vectorDist(%col.getPosition(), %position)) < %minDistance
		) {
			%minDistance = %distance;
			%nodeId = %col.nodeId;
		}
	}
	return %nodeId;
}

function createPathfindingClient() {
	%tcp = new TCPObject(MiniDungeonsPathfinding);
	%tcp.connect("localhost:5000");
	return %tcp;
}

function initPathfinding() {
	$MD::PathTCP = createPathfindingClient();
}

function visualizePathfinding() {
	$MD::PathTCP.send("visualize\n");
}

function MiniDungeonsPathfinding::onConnected(%this) {
	echo("MiniDungeonsPathfinding - TCP connected");
}

function MiniDungeonsPathfinding::onDisconnect(%this) {
	echo("MiniDungeonsPathfinding - TCP disconnected");
}

function MiniDungeonsPathfinding::onConnectFailed(%this) {
	echo("MiniDungeonsPathfinding - TCP connection failed");
}

function MiniDungeonsPathfinding::onLine(%this, %data) {
	%command = getWord(%data, 0);
	if(%command $= "path") {
		%id = getWord(%data, 1);
		if(isObject(%bot = $MD::PathIDTable[%id])) {
			%bot.setPath(%data);
		}
	}
	else if(%command $= "error") {
		%id = getWord(%data, 1);
		if(isObject(%bot = $MD::PathIDTable[%id])) {
			%bot.setPath("");
		}
		warn("Failed to get path for bot" SPC $MD::PathIDTable[%id]);
	}
}

function AiPlayer::requestPath(%this, %startNode, %endNode, %started) {
	$MD::PathIDTable[$MD::PathIDTableCount | 0] = %this;
	$MD::PathTCP.send("path" SPC ($MD::PathIDTableCount | 0) SPC %startNode SPC %endNode @ "\n");
	$MD::PathIDTableCount++;
}

function AiPlayer::hasPath(%this) {
	return %this.pathCount != 0;
}

function AiPlayer::setPath(%this, %path) {
	%count = getWordCount(%path);
	%this.pathCount = 0;
	for(%i = 2; %i < %count; %i++) {
		%id = getWord(%path, %i);
		%this.path[%this.pathCount] = $MD::Node[%id];
		%this.pathCount++;
	}
	%this.currentPathIndex = 0;
}

function AiPlayer::resetPath(%this) {
	%this.path = "";
	%this.pathCount = 0;
	%this.currentPathIndex = 0;
}
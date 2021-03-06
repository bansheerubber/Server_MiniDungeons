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
	if( ! %obj.getGroup().pathfindingNodes) {
		%obj.getGroup().pathfindingNodes = new SimSet();
	}
	%obj.getGroup().pathfindingNodes.add(%obj);
}
function BrickPathfindingNodeData::onLoadPlant(%this, %obj) {
	Parent::onLoadPlant(%this, %obj);
	if( ! %obj.getGroup().pathfindingNodes) {
		%obj.getGroup().pathfindingNodes = new SimSet();
	}
	%obj.getGroup().pathfindingNodes.add(%obj);
}
function BrickPathfindingNodeData::onInteract(%this, %obj, %interactee) {
	if(%interactee.client.buildmode) {
		%obj.isGraphMode = true;
		if( ! isEventPending(%obj.visualizeNeighbors)) {
			if( ! isObject(%obj.debugLines)) {
				%obj.debugLines = new SimSet();
			}
			%obj.visualizeNeighbors();
		}
		cancel(%obj.stopVisualizeNeighbors);
		%obj.stopVisualizeNeighbors = %obj.schedule(120000, stopVisualizeNeighbors);
	}
}
function BrickPathfindingNodeData::onLook(%this, %obj, %interactee) {
	if(%interactee.client.buildmode) {
		if( ! isEventPending(%obj.visualizeNeighbors)) {
			if( ! isObject(%obj.debugLines)) {
				%obj.debugLines = new SimSet();
			}
			%obj.visualizeNeighbors();
		}
		if( ! %obj.isGraphMode) {
			cancel(%obj.stopVisualizeNeighbors);
			%obj.stopVisualizeNeighbors = %obj.schedule(200, stopVisualizeNeighbors);
		}
	}
}
function BrickPathfindingNodeData::onAdd(%this, %obj) {
	Parent::onAdd(%this, %obj);
	if( ! %obj.isPlanted) {
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
				%this.debugLines.add(drawDebugLine(%this.getPosition(), %col.getPosition(), 0.1, "1 0 0 1",  - 1));
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
	if( ! $MD::NodeNeighbor[%id1, %id2]) {
		$MD::PathTCP.send("neighbor" SPC %id1 SPC %id2 @ "\n");
		$MD::NodeNeighbor[%id1, %id2] = true;
		addNeighbor(%id1, %id2);
	}
}
function fxDTSBrick::searchNeighbors(%this) {
	%count = %this.room.pathfindingBricks.getCount();
	for(%i = 0; %i < %count; %i++) {
		%brick = %this.room.pathfindingBricks.getObject(%i);
		if(%brick == %this) {
			continue;
		}
		%raycast = containerRaycast(%this.getPosition(), %brick.getPosition(), $TypeMasks::fxBrickObjectType, %this);
		if((%brick.getName() $= "_node" || %brick.getDatablock().getName() $= "BrickPathfindingNodeData") && getWord(%raycast, 0) == %brick && vectorDist(%this.getPosition(), %brick.getPosition()) < 14.5) {
			addNeighbor(%this.nodeId, %brick.nodeId);
			// drawDebugLine(%this.getPosition(), %brick.getPosition(), 0.2, "1 0 0 1", 15000);
		}
	}
}
function buildNodesFromBricks(%brickGroup, %start) {
	if( ! isObject(%brickGroup)) {
		return;
	}
	if(%start $= "") {
		%start = 0;
	}
	if(%start == 0) {
		clearPathfinding();
		echo("Cleared pathfinding");
	}
	if($MD::NodeCount $= "") {
		$MD::NodeCount = 0;
	}
	// do pathfinding bricks
	%count = %brickGroup.pathfindingNodes.getCount();
	for(%i = (%start | 0); (%i < %count) && (%i < %start + 100); %i++) {
		%brick = %brickGroup.pathfindingNodes.getObject(%i);
		%brick.nodeId = $MD::NodeCount;
		%brick.room = getRoomFromPosition(%brick.getPosition());
		$MD::Node[$MD::NodeCount] = %brick.getPosition();
		$MD::PathTCP.send("add" SPC $MD::NodeCount SPC %brick.getPosition() @ "\n");
		$MD::NodeCount++;
	}
	if(%i >= %count) {
		buildNeighborsFromBricks(%brickGroup);
	}
	else {
		schedule(100, 0, buildNodesFromBricks, %brickGroup, %start + 100);
	}
}
function buildNeighborsFromBricks(%brickGroup) {
	if( ! isObject(%brickGroup)) {
		return;
	}
	%count = %brickGroup.pathfindingNodes.getCount();
	for(%i = 0; %i < %count; %i++) {
		%brickGroup.pathfindingNodes.getObject(%i).searchNeighbors();
	}
	$MD::PathTCP.send("simplify\n");
}
function getClosestNode(%position) {
	%nodeId =  - 1;
	%radius = 20;
	%minDistance = %radius;
	%minZDistance = %radius;
	%room = getRoomFromPosition(%position);
	%count = %room.pathfindingBricks.getCount();
	for(%i = 0; %i < %count; %i++) {
		%brick = %room.pathfindingBricks.getObject(%i);
		if((%brick.getName() $= "_node" || %brick.getDatablock().getName() $= "BrickPathfindingNodeData") && (%distance = vectorDist(%brick.getPosition(), %position)) < %minDistance) {
			%raycast = containerRaycast(%position, %brick.getPosition(), $TypeMasks::fxBrickObjectType);
			if(getWord(%raycast, 0) == %brick) {
				%minDistance = %distance;
				%nodeId = %brick.nodeId;
				%closestBrick = %brick;
			}
		}
	}
	return %nodeId;
}
function createPathfindingClient() {
	%file = new FileObject();
	%file.openForRead("Add-Ons/Server_MiniDungeons/pathfinding/server.info");
	%ip = %file.readLine();
	%port = %file.readLine();
	%file.close();
	%tcp = new TCPObject(MiniDungeonsPathfinding);
	%tcp.connect(%ip @ ":" @ %port);
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
function AiPlayer::hasPathOrTimeout(%this) {
	return %this.pathCount != 0 || (getSimTime() > %this.lastPathRequest + 2000);
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

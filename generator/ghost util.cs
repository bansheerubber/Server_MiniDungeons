function Player::createGhostUtilObject(%this, %roomSet) {
	if(!%this.ghostUtilObjects[%roomSet]) {
		%ghostUtilObject = new ScriptObject() {
			class = "GhostUtilObject";
			owner = %this;

			isPendingReGhost = true;
			isPendingUnGhost = false;
			isGhosted = false;
			roomSet = %roomSet;
		};

		%this.ghostUtilObjects[%roomSet] = %ghostUtilObject;

		%this.ghostUtilSet.add(%ghostUtilObject);
		return %ghostUtilObject;
	}
	else {
		return %this.ghostUtilObjects[%roomSet];
	}
}

function Player::deleteGhostUtilObject(%this, %ghostUtilObject) {
	%this.ghostUtilObjects[%ghostUtilObject.roomSet] = "";
	%ghostUtilObject.delete();
}

function Player::deleteAllGhostUtilObjects(%this) {
	%count = %this.ghostUtilSet.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.ghostUtilObjects[%this.ghostUtilSet.getObject(%i).roomSet] = 0;
	}
	%this.ghostUtilSet.deleteAll();
}

function Player::startRoomGhosting(%this) {
	%this.getCurrentRoom().roomHandleGhosting(%this.client, 4, true);
}

function Player::finishRoomGhosting(%this) {
	if(%this.client.ghostDebug) {
		messageClient(%this.client, '', "Finished room ghosting");
	}
	
	cancel(%this.finishGhosting);
	
	// unghost unmarked rooms
	%count = %this.ghostUtilSet.getCount();
	for(%i = 0; %i < %count; %i++) {
		%ghostUtilObject = %this.ghostUtilSet.getObject(%i);
		if(%ghostUtilObject.marked) {
			%ghostUtilObject.marked = false;
			%ghostUtilObject.isPendingUnGhost = false;
			
			// if rooms aren't ghosted, then ghost them
			if(!%ghostUtilObject.isGhosted) {
				%ghostUtilObject.isPendingReGhost = true;
			}
		}
		else {
			%ghostUtilObject.isPendingUnGhost = true;
			%ghostUtilObject.isPendingReGhost = false;
		}
	}
}

// ghosts rooms to client and unghosts rooms that client hasn't seen yet
function SimSet::roomHandleGhosting(%this, %client, %depth, %start) {
	if(%start) {
		%ghostUtilObject = %client.player.createGhostUtilObject(%this);
		%ghostUtilObject.marked = true;
		deleteVariables("$MD::TempDungeonIter" @ %client @ "*");
	}
	
	$MD::TempDungeonIter[%client, %this] = true;
	for(%i = 0; %i < %this.neighborCount; %i++) {
		%neighbor = %this.neighbor[%i];
		if(!$MD::TempDungeonIter[%client, %neighbor] && %depth > 0) {
			%ghostUtilObject = %client.player.createGhostUtilObject(%neighbor);
			%ghostUtilObject.marked = true;
			%neighbor.schedule(1, roomHandleGhosting, %client, %depth - 1); // delay it so first depth neighbors are ghosted first
		}
	}

	cancel(%client.player.finishGhosting);
	%client.player.finishGhosting = %client.player.schedule(1000, finishRoomGhosting);
}

function Player::processGhostUtilObjects(%this, %currentObject) {
	cancel(%this.processGhostUtilObjects);

	if(!isObject(%this.ghostUtilSet)) {
		%this.ghostUtilSet = new SimSet();
	}

	%room = %this.getCurrentRoom();
	%hallway = %this.getCurrentHallway();
	// handle rooms (onEnter is always called last)
	if(isObject(%room)) {
		if(isObject(%this.currentHallway)) {
			%this.onLeaveHallway(%this.currentHallway);
			%this.currentHallway = 0;
		}
		
		if(%this.currentRoom != %room) {
			if(isObject(%this.currentRoom)) {
				%this.onLeaveRoom(%this.currentRoom);
			}
			
			%this.onEnterRoom(%room);
		}
		%this.currentRoom = %room;
	}
	// handle hallways (onEnter is always called last)
	else {
		if(isObject(%this.currentRoom)) {
			%this.onLeaveRoom(%this.currentRoom);
			%this.currentRoom = 0;
		}
		
		if(%this.currentHallway != %hallway) {
			if(isObject(%this.currentHallway)) {
				%this.onLeaveHallway(%this.currentHallway);
			}

			%this.onEnterHallway(%hallway);
		}
		%this.currentHallway = %hallway;
	}
	
	if(%this.debugAbsorbCurrentObject) {
		%this.debugAbsorbCurrentObject = false;
		%currentObject = 0;
	}

	// if we're currently battling, then defer ghosting until later
	if(
		isObject(%this.getCurrentRoom())
		&& %this.getCurrentRoom().isBattleRoom
		&& !%this.getCurrentRoom().areBattleBotsDead
	) {
		if(%this.client.ghostDebug) {
			messageClient(%this.client, '', "Defered ghosting");
		}

		%this.processGhostUtilObjects = %this.schedule(100, processGhostUtilObjects, %currentObject);
		return;
	}
	
	// if there's a pending unghost, check to see if we have any pending reghosts. those always take priority
	if(%currentObject.isPendingUnGhost && !%currentObject.isPendingReGhost && isEventPending(%currentObject.ghostSchedule)) {
		%count = %this.ghostUtilSet.getCount();
		// find new currentObject if our currentObject is pending an unghost
		for(%i = 0; %i < %count; %i++) {
			%ghostUtilObject = %this.ghostUtilSet.getObject(%i);
			
			if(%ghostUtilObject.isPendingReGhost) {
				cancel(%currentObject.ghostSchedule);
				%ghostUtilObject.batchReGhost();
				%currentObject = %ghostUtilObject;
				break;
			}
		}
	}
	else if(
		!%currentObject.isPendingReGhost && !%currentObject.isPendingUnGhost
		|| (
			!isEventPending(%currentObject.ghostSchedule)
			&& (
				%currentObject.isPendingReGhost
				|| %currentObject.isPendingUnGhost
			)
		) // prevents us from getting stuck on an currentObject that has no schedule
	) {
		%currentObject = 0;
		%count = %this.ghostUtilSet.getCount();
		for(%i = 0; %i < %count; %i++) {
			%ghostUtilObject = %this.ghostUtilSet.getObject(%i);

			if(%ghostUtilObject.isPendingUnGhost) {
				%potentialCurrentObject = %ghostUtilObject;
			}
			else if(%ghostUtilObject.isPendingReGhost) {
				%ghostUtilObject.batchReGhost();
				%currentObject = %ghostUtilObject;
				break;
			}
		}

		if(!isObject(%currentObject) && %potentialCurrentObject) {
			%potentialCurrentObject.batchUnGhost();
			%currentObject = %potentialCurrentObject;
		}
	}

	%this.processGhostUtilObjects = %this.schedule(100, processGhostUtilObjects, %currentObject);
}

$MD::GhostAmount = 800; // how much we ghost per tick
$MD::GhostTick = 200; // how often we ghost the above amount

function GhostUtilObject::batchReGhost(%this, %startIndex) {
	cancel(%this.ghostSchedule);
	
	if(%startIndex $= "") {
		%startIndex = 0;
	}
	%total = %this.roomSet.getCount();
	%end = mClamp(%startIndex + $MD::GhostAmount, 0, %total);
	for(%i = %startIndex; %i < %end; %i++) {
		%this.roomSet.getObject(%i).reGhost(%this.owner.client);
	}

	if(%startIndex != %total) {
		%this.ghostSchedule = %this.schedule($MD::GhostTick, batchReGhost, %end);
	}
	else {
		%this.roomSet.roomOnReGhostedToPlayer(%this.owner);
		
		%this.isGhosted = true;
		%this.isPendingReGhost = false;
	}
}

function GhostUtilObject::batchUnGhost(%this, %startIndex) {
	cancel(%this.ghostSchedule);

	%this.isGhosted = false;
	
	if(%startIndex $= "") {
		%startIndex = 0;
	}
	%total = %this.roomSet.getCount();
	%end = mClamp(%startIndex + $MD::GhostAmount, 0, %total);
	for(%i = %startIndex; %i < %end; %i++) {
		%this.roomSet.getObject(%i).unGhost(%this.owner.client);
	}

	if(%startIndex != %total) {
		%this.ghostSchedule = %this.schedule($MD::GhostTick, batchUnGhost, %end);
	}
	else {
		%this.roomSet.roomOnUnGhostedToPlayer(%this.owner);
		
		%this.isGhosted = false;
		%this.isPendingUnGhost = false;
		%this.owner.deleteGhostUtilObject(%this);
	}
}

function Player::onEnterRoom(%this, %room) {
	%this.startRoomGhosting();

	%room.roomOnPlayerEnter(%this);
}

function Player::onLeaveRoom(%this, %room) {
	%room.roomOnPlayerLeave(%this);
}

function Player::onEnterHallway(%this, %hallway) {
	%hallway.hallwayOnPlayerEnter(%this);
}

function Player::onLeaveHallway(%this, %hallway) {
	%hallway.hallwayOnPlayerLeave(%this);
}

deActivatePackage(MiniDungeonsGhostUtil);
package MiniDungeonsGhostUtil {
	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);

		if(isObject(%this.player)) {
			%this.player.processGhostUtilObjects(); // start ghosting rooms to players right away
		}
	}
	
	function Armor::onRemove(%this, %obj) {
		if(isObject(%obj.ghostUtilSet)) {
			%obj.ghostUtilSet.deleteAll();
			%obj.ghostUtilSet.delete();
		}

		if(isObject(%obj.currentRoom)) {
			%obj.onLeaveRoom(%obj.currentRoom);
			%obj.currentRoom = 0;
		}

		if(isObject(%obj.currentHallway)) {
			%obj.onLeaveHallway(%obj.currentHallway);
			%obj.currentHallway = 0;
		}

		Parent::onRemove(%this, %obj);
	}
};
activatePackage(MiniDungeonsGhostUtil);
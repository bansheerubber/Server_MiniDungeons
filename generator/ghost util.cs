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
	%this.ghostUtilObjects[%ghostUtilObject.roomSet] = 0;
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

	%this.schedule(1500, finishRoomGhosting);
}

function Player::finishRoomGhosting(%this) {
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
		deleteVariables("$MD::TempDungeonIter*");
	}
	
	$MD::TempDungeonIter[%this] = true;
	for(%i = 0; %i < %this.neighborCount; %i++) {
		%neighbor = %this.neighbor[%i];
		if(!$MD::TempDungeonIter[%neighbor] && %depth > 0) {
			%ghostUtilObject = %client.player.createGhostUtilObject(%neighbor);
			%ghostUtilObject.marked = true;
			%neighbor.schedule(1, roomHandleGhosting, %client, %depth - 1); // delay it so first depth neighbors are ghosted first
		}
	}
}

function Player::processGhostUtilObjects(%this, %currentObject) {
	cancel(%this.processGhostUtilObjects);

	if(!isObject(%this.ghostUtilSet)) {
		%this.ghostUtilSet = new SimSet();
	}

	%room = %this.getCurrentRoom();
	if(isObject(%room)) {
		if(%this.currentRoom != %room) {
			%this.onEnterRoom(%room);
		}
		%this.currentRoom = %room;
	}
	
	// talk(%currentObject.isPendingUnGhost SPC %currentObject.isPendingReGhost SPC %ghostUtilObject.isGhosted SPC isEventPending(%currentObject.ghostSchedule));
	if(%this.debugAbsorbCurrentObject) {
		%this.debugAbsorbCurrentObject = false;
		%currentObject = 0;
	}
	
	// if there's a pending unghost, check to see if we have any pending reghosts. those always take priority
	if(%currentObject.isPendingUnGhost && !%currentObject.isPendingReGhost && isEventPending(%currentObject.ghostSchedule)) {
		%count = %this.ghostUtilSet.getCount();
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

$MD::GhostAmount = 300; // how much we ghost per tick
$MD::GhostTick = 200; // how often we ghost the above amoujnt

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

deActivatePackage(MiniDungeonsGhostUtil);
package MiniDungeonsGhostUtil {
	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);

		if(isObject(%this.player)) {
			%this.player.processGhostUtilObjects(); // start ghosting rooms to players right away
		}
	}
	
	function Player::onRemove(%this) {
		if(isObject(%this.ghostUtilSet)) {
			%this.ghostUtilSet.deleteAll();
			%this.ghostUtilSet.delete();
		}

		Parent::onRemove(%this);
	}
};
activatePackage(MiniDungeonsGhostUtil);
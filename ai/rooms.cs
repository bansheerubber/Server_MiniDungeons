// handles bots placed in rooms
function MiniDungeonsAiRoomSet::create(%index) {
	$MD::AiRoomSet[%index] = new SimSet(MiniDungeonsAiRoomSet);
	$MD::AiRoomSet[%index].schedule(60000, garbageCollect);
	return $MD::AiRoomSet[%index];
}

function MiniDungeonsAiRoomSet::get(%index) {
	if(isObject($MD::AiRoomSet[%index])) {
		return $MD::AiRoomSet[%index];
	}
	else {
		return MiniDungeonsAiRoomSet::create(%index);
	}
}

// garbage collect rooms not in use
function SimSet::garbageCollect(%this) {
	if(%this.getCount() == 0) {
		%this.delete();
	}
	else {
		%this.schedule(30000, garbageCollect);
	}
}

// set the room into the alert phase. in this phase, bots do not check if walls are in the way when searching for players
function SimSet::alert(%this) {
	%this.alert = true;
}
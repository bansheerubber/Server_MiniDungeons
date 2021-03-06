function Player::addItem(%this, %item) {
	if(!isObject(%item)) {
		return;
	}
	
	%item = %item.getId();
	
	for(%i = 0; %i < %this.getDatablock().maxTools; %i++) {
		if(!isObject(%this.tool[%i])) {
			%this.tool[%i] = %item;
			messageClient(%this.client, 'MsgItemPickup', '', %i, %item);
			return 1;
		}
	}
	
	return 0;
}

function Player::setItem(%this, %item, %index) {
	if(isObject(%item)) {
		%item = %item.getId();
	}
	%this.tool[%index] = %item;
	messageClient(%this.client, 'MsgItemPickup', '', %index, %item);
}

function Player::hasItem(%this, %data) {
	%data = %data.getId();
	
	for(%i = 0; %i < %this.getDatablock().maxTools; %i++) {
		if(%this.tool[%i] == %data) {
			return true;
		}
	}
	return false;
}
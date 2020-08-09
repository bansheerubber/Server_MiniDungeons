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
	%item = %item.getId();
	%this.tool[%index] = %item;
	messageClient(%this.client, 'MsgItemPickup', '', %index, %item);
}
deActivatePackage(MiniDungeonsInteractable);
package MiniDungeonsInteractable {
	function Armor::onTrigger(%this, %obj, %slot, %val) {
		if(%slot == 0 && %val) {
			%obj.interact();
		}
		
		Parent::onTrigger(%this, %obj, %slot, %val);
	}

	function serverCmdActivateStuff(%this) {
		if(isObject(%this.player)) {
			%this.player.interact();
		}

		Parent::serverCmdActivateStuff(%this);
	}

	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);
		%this.player.interactLoop();
	}
};
activatePackage(MiniDungeonsInteractable);

function SimObject::onLook(%this, %obj, %interactee) {

}

function SimObject::onInteract(%this, %obj, %interactee) {

}

function Player::interact(%this) {
	%start = %this.getEyePoint();
	%end = vectorAdd(%start, vectorScale(%this.getEyeVector(), 5));
	%masks = $TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType | $TypeMasks::PlayerObjectType;
	%raycast = containerRaycast(%start, %end, %masks, %this);
	
	if(
		isObject(%obj = getWord(%raycast, 0))
		&& !%this.hasSwordMounted()
		&& (
			!%this.getMountedImage(0)
			|| %this.getMountedImage(0).interactablePassthrough
		)
	) {
		if(
			isFunction(%obj.getClassName(), getDatablock)
			&& %obj.getDatablock().isInteractable
		) {
			%obj.getDatablock().onInteract(%obj, %this);
		}
		else if(%obj.onInteractCall !$= "") {
			%obj.getDatablock().call(%obj.onInteractCall, %obj, %this);
		}
	}
}

// called every tick for interactable viewing
function Player::interactLoop(%this) {
	%start = %this.getEyePoint();
	%end = vectorAdd(%start, vectorScale(%this.getEyeVector(), 5));
	%masks = $TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType | $TypeMasks::PlayerObjectType;
	%raycast = containerRaycast(%start, %end, %masks, %this);

	if(isObject(%obj = getWord(%raycast, 0)) && isFunction(%obj.getClassName(), getDatablock) && %obj.getDatablock().isInteractable) {
		%obj.getDatablock().onLook(%obj, %this);
	}

	%this.interactSchedule = %this.schedule(33, interactLoop);
}
function AiPlayer::attackCleanup(%this) {
	// make it so we aren't suprised if we see the target again shortly after losing tracking
	%this.nextAlarmEmote = getSimTime() + 6000;
}
function AiPlayer::canAttack(%this) {
	if(isObject(%target = %this.target) && %this.isGrounded) {
		return true;
	}
	else {
		return false;
	}
}
// whether or not we can beeline to the player. needs to be an unobstructed path
function AiPlayer::canBeelineToPlayer(%this) {
	if(vectorDist(%targetPosition, %position) > %this.attackRange) {
		return false;
	}
	%raycast = containerRaycast(%this.getEyePoint(), %this.target.getHackPosition(), $TypeMasks::fxBrickObjectType | $TypeMasks::PlayerObjectType, %this);
	if(getWord(%raycast, 0) != %this.target) {
		return false;
	}
	if( ! %this.canWalkCliff()) {
		return false;
	}
	return true;
}

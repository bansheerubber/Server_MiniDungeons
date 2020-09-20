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

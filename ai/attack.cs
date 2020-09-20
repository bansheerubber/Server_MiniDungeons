function AiPlayer::attackCleanup(%this) {
	// make it so we aren't suprised if we see the target again shortly after losing tracking
	%this.nextAlarmEmote = getSimTime() + 6000;
}
function AiPlayer::canAttack(%this) {
	if(isObject(%target = %this.target) && %this.isGrounded && %target.isGrounded) {
		// strict height checks. basically, we don't want bots to start attacking as they're walking up stairs
		if(mAbs(vectorSub((getWord(%this.getPosition(), 2)), (getWord(%target.getPosition(), 2)))) < 1) {
			return true;
		}
		else {
			return false;
		}
	}
	else {
		return false;
	}
}

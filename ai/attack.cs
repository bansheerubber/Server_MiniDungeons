function AiPlayer::attackCleanup(%this) {
	// make it so we aren't suprised if we see the target again shortly after losing tracking
	%this.nextAlarmEmote = getSimTime() + 6000;
}
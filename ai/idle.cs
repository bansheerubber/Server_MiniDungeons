function AiPlayer::idle(%this) {
	%this.setMoveX(0);
	%this.setMoveY(0);

	// search for targets and check if we had a target set manually
	if(%this.findTarget() && isObject(%this.target)) {
		%this.setAiState($MD::AiSeek);
		return;
	}

	if(getSimTime() > %this.nextRotateTime) {
		if(%this.lookAngle $= "") {
			%this.lookAngle = getRandom(0, 360);
		}
		
		%this.lookAngle += getRandom(-75, 75);
		%this.setAimVector(eulerToVector("0 0" SPC %this.lookAngle));
		
		%this.nextRotateTime = getSimTime() + getRandom(3000, 15000);
	}

	%this.ai = %this.schedule(300, idle);
}

// called when we exit idle state
function AiPlayer::idleCleanup(%this) {

}
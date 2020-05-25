datablock AudioProfile(CannonGoAway1Sound) {
	filename    = "./sounds/go away.ogg";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(CannonGoAway2Sound) {
	filename    = "./sounds/go away2.ogg";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(CannonFallSound) {
	filename    = "./sounds/cannon fall.wav";
	description = AudioClose3d;
	preload = true;
};

function createCannonAi(%transform, %roomIndex) {
	%ai = new AiPlayer() {
		datablock = AiCannonPlayer;
		position = getWords(%transform, 0, 2);
		rotation = "0 0 0 1";

		idleFOV = 360;
		idleDrawDistance = 200;
		isEnemyAi = true;
		isBot = true;

		name = generateRandomName();
	};
	%ai.setTransform(%transform);

	%ai.setMaxHealth(50);
	%ai.cannonAttack();

	%dummyPlayer = new AiPlayer() {
		datablock = MiniDungeonsArmor;
		position = "0 0 -500";
		rotation = "0 0 0 1";
		customDeathCry = "GladiusDeath" @ getRandom(1, 3) @ "Sound";
	};
	%ai.mountObject(%dummyPlayer, 0);
	%dummyPlayer.setShapeName(%ai.name, 8564862);
	%dummyPlayer.setShapeNameDistance(50);
	%dummyPlayer.setAvatar("gladius");

	%dummyPlayer.setControlObject(%dummyPlayer);
	%ai.setControlObject(%ai);
	%ai.dummyPlayer = %dummyPlayer;

	%ai.onSpawn(%roomIndex);

	return %ai;
}

// does not follow normal state system, because this does not move
function AiPlayer::cannonAttack(%this) {
	if(getWord(%this.getVelocity() , 2) < -2) {
		if(!%this.cannonFalling) {
			%this.dummyPlayer.playAudio(3, CannonFallSound);
			%this.cannonFalling = true;
			%this.dummyPlayer.talkForTime(50);
			%this.dummyPlayer.setAimObject(%this.target);
		}
		%this.dummyPlayer.playThread(1, "activate2");
	}
	else if(!%this.hasValidTarget()) {
		if(%this.target != 0) {
			%this.loseTarget();
		}
		else {
			%this.findTarget(); // keep searching for a target
		}
	}
	else {
		if(!isEventPending(%this.cannonTrack)) {
			%this.cannonTrackPlayer();
			%this.nextCannonFire = getSimTime() + getRandom(2000, 4000);
		}
		
		%position = %this.getPosition();
		%targetPosition = %this.target.getPosition();
		if(getSimTime() > %this.nextCannonFire) {
			%predictedPosition = %this.target.getPredictedPosition(0.4);

			%masks = $TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType;
			%raycast = containerRaycast(vectorAdd(%predictedPosition, "0 0 5"), vectorAdd(%predictedPosition, "0 0 -20"), %masks, false);

			if(isObject(%raycast)) {
				%predictedPosition = getWords(%raycast, 1, 3);
			}
			else {
				%predictedPosition = %targetPosition;
			}
			%predictedPosition = vectorAdd(%predictedPosition, "0 0 0.1");

			// make sure that the predicted position is not in the air
			%raycast = containerRaycast(%predictedPosition, vectorAdd(%predictedPosition, "0 0 -0.2"), %masks, false);

			if(isObject(%raycast)) {
				// make sure the target point is far away enough
				if(vectorDist(getWords(%predictedPosition, 0, 1), getWords(%position, 0, 1)) > 9) {
					%this.cannonFire(%predictedPosition);
					%this.nextCannonFire = getSimTime() + 5000;
				}
				else if(getSimTime() > %this.cannonNextWorry) {
					
					if((%rand = getRandom(1, 2)) == 1) {
						%this.dummyPlayer.talkForTime(0.5);
					}
					else {
						%this.dummyPlayer.talkForTime(1.1);
					}
					%this.dummyPlayer.playAudio(0, "CannonGoAway" @ %rand @ "Sound");
					%this.cannonNextWorry = getSimTime() + 3000 + getRandom(0, 2000);
				}
			}
		}
	}

	%this.ai = %this.schedule(200, cannonAttack);
}
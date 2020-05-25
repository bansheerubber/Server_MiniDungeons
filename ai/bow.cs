function createBowAi(%transform, %roomIndex) {
	%ai = new AiPlayer() {
		datablock = MiniDungeonsArmor;
		position = getWords(%transform, 0, 2);
		rotation = "0 0 0 1";

		attackRange = 35;
		idle = "idle";
		seek = "seek";
		attack = "bowAttack";

		idleCleanup = "idleCleanup";
		seekCleanup = "bowSeekCleanup";
		attackCleanup = "bowAttackCleanup";

		isEnemyAi = true;
		isBot = true;

		name = generateRandomName();
	};
	%ai.setTransform(%transform);
	%ai.setAIState($MD::AiIdle);
	%ai.setMaxSideSpeed(MiniDungeonsArmor.maxForwardSpeed);

	%ai.setShapeName(%ai.name, 8564862);
	%ai.setShapeNameDistance(50);

	%ai.setMaxHealth(50);

	%ai.mountImage(AiBowImage, 1);

	%ai.setAvatar("gladius");

	%ai.onSpawn(%roomIndex);

	return %ai;
}

function AiPlayer::bowAttack(%this) {
	if(!%this.hasValidTarget()) {
		%this.loseTarget();
		%this.setAiState($MD::AiIdle);
		return;
	}

	%position = %this.getPosition();
	%targetPosition = %this.target.getPosition();

	if(vectorDist(%targetPosition, %position) < 10) {
		if(%this.bowAlarm) {
			%this.playThread(1, "armReadyLeft");
			%this.emote(AlarmProjectile);
			%this.bowAlarm = false;
		}

		%this.bowAnimation = true;

		%this.setAimVector(vectorNormalize(getWords(vectorSub(%position, %targetPosition), 0, 1)));
		%this.setMoveY(0.5);
		
		%this.playThread(0, "activate2");

		%this.nextBowFire = getSimTime() + 1000;
	}
	else {
		if(%this.bowAnimation) {
			%this.playThread(1, "bow1Fire");
			%this.bowAnimation = false;
		}
		
		%this.bowAlarm = true;

		%this.setMoveY(0);

		%aimVector = vectorNormalize(vectorSub(%targetPosition, %this.getEyePoint()));
		%leftVector = vectorNormalize(vectorCross(%aimVector, "0 0 1"));
		%this.setAimVector(%leftVector);
		
		if(getSimTime() > %this.nextBowFire && %this.target.isGrounded) {
			%this.setImageTrigger(1, true);
			%this.schedule(500, setImageTrigger, 1, false);
			
			%this.nextBowFire = getSimTime() + 2000;

			%this.nextBowCheck = getSimTime() + 1000;
		}

		if(getSimTime() > %this.nextBowCheck) {
			%raycast = containerRaycast(%this.getEyePoint(), %this.target.getHackPosition(), $TypeMasks::fxBrickObjectType | $TypeMasks::PlayerObjectType, %this);
			if(vectorDist(%targetPosition, %position) > %this.attackRange || getWord(%raycast, 0) != %this.target) {
				%this.setAiState($MD::AiSeek);
				return;
			}
		}
	}

	%this.ai = %this.schedule(100, bowAttack);
}

function AiPlayer::bowAttackCleanup(%this, %state) {
	%this.setMoveX(0);
	%this.setMoveY(0);

	%this.playThread(1, "armReadyLeft");

	%this.attackCleanup();
}

function AiPlayer::bowSeekCleanup(%this, %state) {
	if(getSimTime() > %this.nextBowFire) {
		%this.nextBowFire = getSimTime() + 1000;
	}

	%this.playThread(1, "bow1Fire");

	%this.seekCleanup();
}
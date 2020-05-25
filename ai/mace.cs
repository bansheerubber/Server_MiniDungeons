function createMaceAi(%transform, %roomIndex) {
	%ai = new AiPlayer() {
		datablock = MiniDungeonsArmor;
		position = getWords(%transform, 0, 2);
		rotation = "0 0 0 1";

		idleDrawDistance = 200;
		idleFOV = 360;
		attackRange = 15;
		idle = "idle";
		seek = "seek";
		attack = "maceAttack";

		idleCleanup = "idleCleanup";
		seekCleanup = "maceSeekCleanup";
		attackCleanup = "maceAttackCleanup";

		seekHeightCheck = true;

		isEnemyAi = true;
		isBot = true;

		name = generateRandomName();
	};
	%ai.setTransform(%transform);
	%ai.setAIState($MD::AiIdle);
	%ai.setScale("1.3 1.3 1.3");
	%ai.setSpeedFactor(0.8);

	%ai.setShapeName(%ai.name, 8564862);
	%ai.setShapeNameDistance(50);

	MaceSwordArmor.mount(%ai, 0);

	%ai.setAvatar("mace");

	%ai.setMaxHealth(250);

	%ai.onSpawn(%roomIndex);

	return %ai;
}

function AiPlayer::maceAttack(%this) {
	if(!%this.hasValidTarget()) {
		%this.loseTarget();
		%this.setAiState($MD::AiIdle);
		return;
	}

	%targetPosition = %this.target.getPosition();
	%position = %this.getPosition();

	
	if(getSimTime() > %this.nextMaceAttack) {
		%this.stop();
		%this.setMoveX(0);
		%this.setMoveY(0);
		%this.setSwordTrigger(0, true);
		%this.setSwordTrigger(0, false);

		%this.playThread(0, "plant");

		%this.schedule(200, clearAim);

		%this.nextMaceAttack = getSimTime() + 3500;
		%this.nextMaceMove = getSimTime() + 1200;
	}

	if(getSimTime() > %this.nextMaceMove) {
		if(vectorDist(%targetPosition, %position) < 1.9 && getSimTime() > %this.nextKickTime) { // kick super close players
			%this.stop();
			%this.setMoveX(0);
			%this.setMoveY(0);

			%this.playThread(0, "plant");

			%this.nextMaceAttack += 600;
			%this.nextMaceMove = getSimTime() + 300;
			%this.nextKickTime = getSimTime() + 800;

			if(%this.target.isParrying) {
				%this.target.sword[0].getDatablock().doParry(%this.target, %this.sword[0].getDatablock(), %this, 0, %this.target.parryDirection, %this.target.getForwardVector());
			}
			else {
				%projectilePosition = vectorAdd(vectorLerp(%position, %targetPosition, 0.5), "0 0 2");
				(%p = new Projectile() {
					datablock = LightParryProjectile;
					initialPosition = %projectilePosition;
					initialVelocity = "0 0 15";
					sourceObject = 0;
					sourceSlot = 0;
				}).explode();

				(new Projectile() {
					datablock = ParryProjectile;
					initialPosition = %projectilePosition;
					initialVelocity = "0 0 15";
					sourceObject = 0;
					sourceSlot = 0;
				}).explode();

				%this.target.applyImpulse(%targetPosition, vectorAdd(vectorScale(%this.getForwardVector(), 1700), "0 0 700"));
				%this.playAudio(1, MaceKickSound);
			}
		}
		else {
			%raycast = containerRaycast(%this.getEyePoint(), %this.target.getHackPosition(), $TypeMasks::fxBrickObjectType | $TypeMasks::PlayerObjectType, %this);
			if(vectorDist(%targetPosition, %position) > %this.attackRange || getWord(%raycast, 0) != %this.target) {
				%this.setAiState($MD::AiSeek);
				return;
			}
			
			%this.setAimObject(%this.target);

			if(vectorDist(%targetPosition, %position) < 7) {
				%this.setMoveX(%this.maceRandomStrafe);

				if(vectorDist(%targetPosition, %position) > 3) {
					%this.setMoveY(0.5);
				}
				else {
					%this.setMoveY(0);
				}
			}
			else {
				%this.setMoveX(0);
				%this.setMoveY(1);
				%this.maceRandomStrafe = getRandom(0, 1) ? -1 : 1;
			}
		}
	}

	%this.ai = %this.schedule(100, maceAttack);
}

function AiPlayer::maceAttackCleanup(%this, %state) {
	%this.setSwordTrigger(0, false); // stop firing
	// %this.setMoveX(0);
	// %this.setMoveY(0);

	%this.attackCleanup();
}

function AiPlayer::maceSeekCleanup(%this, %state) {
	if(getSimTime() > %this.nextMaceAttack && %this.aiSuprise) {
		%this.nextMaceAttack = getSimTime() + 1000; // delay the attack by 1 second
		%this.nextMaceMove = getSimTime(); // imeditally begin moving again
	}

	%this.seekCleanup();
}
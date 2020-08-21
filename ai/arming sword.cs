datablock AudioProfile(ArmingSword1Sound) {
	filename    = "./sounds/arming sword1.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(ArmingSword2Sound) {
	filename    = "./sounds/arming sword2.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(ArmingSword3Sound) {
	filename    = "./sounds/arming sword3.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(ArmingSword4Sound) {
	filename    = "./sounds/arming sword4.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(ArmingSwordDeath1Sound) {
	filename    = "./sounds/arming sword death1.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(ArmingSwordDeath2Sound) {
	filename    = "./sounds/arming sword death2.wav";
	description = AudioClose3d;
	preload = true;
};

function createArmingSwordAi(%transform, %roomIndex) {
	%ai = new AiPlayer() {
		datablock = MiniDungeonsArmor;
		position = getWords(%transform, 0, 2);
		rotation = "0 0 0 1";

		idleDrawDistance = 200;
		idleThirdSenseDistance = 5;
		idleFOV = 360;
		attackRange = 10;
		idle = "idle";
		seek = "seek";
		attack = "armingSwordAttack";

		idleCleanup = "idleCleanup";
		seekCleanup = "armingSwordSeekCleanup";
		attackCleanup = "armingSwordAttackCleanup";

		reward = 2;

		seekHeightCheck = true;

		customDeathCry = "ArmingSwordDeath" @ getRandom(1, 2) @ "Sound";

		isEnemyAi = true;
		isBot = true;

		name = generateRandomName();
	};
	%ai.setTransform(%transform);
	%ai.setAIState($MD::AiIdle);
	%ai.setMaxSideSpeed(MiniDungeonsArmor.maxForwardSpeed);

	%ai.setShapeName(%ai.name, 8564862);
	%ai.setShapeNameDistance(50);

	AiArmingSwordArmor.mount(%ai, 0);
	AiArmingSwordArmor.schedule(100, forceCycleGuard, %ai, 0, getRandom(0, 1));

	%ai.setMaxHealth(150);
	%ai.setAvatar("duelist");

	%ai.onSpawn(%roomIndex);

	return %ai;
}

function AiPlayer::armingSwordAttack(%this) {
	if(!%this.hasValidTarget()) {
		%this.loseTarget();
		%this.setAiState($MD::AiIdle);
		return;
	}

	%targetPosition = %this.target.getPosition();
	%position = %this.getPosition();

	%this.setAimObject(%this.target);

	if(vectorDist(%targetPosition, %position) > %this.attackRange) {
		%this.setAiState($MD::AiSeek);
		return;
	}

	if(getSimTime() > %this.nextXMove) {
		%this.nextXMove = getSimTime() + getRandom(1000, 2000);
		%this.setMoveX(getRandom(0, 1) ? -1 : 1);
	}
	
	%distance = getSimTime() > %this.nextAttack
		? 5
		: 8;

	if(vectorDist(%targetPosition, %position) < %distance) {
		%this.setMoveY(-0.5);
	}
	else {
		%this.setMoveY(0.7);
	}

	if(
		getSimTime() > %this.nextAttack
		&& vectorDist(%targetPosition, %position) < 7
		&& %this.target.isGrounded
		&& %this.isGrounded
	) {
		%this.nextAttack = getSimTime() + 2500;

		%this.playAudio(0, BeefboyGuard2ChargeReadySound);
		%this.emote(winStarProjectile, 1);

		%this.setMoveX(0);
		%this.setMoveY(0);

		%this.schedule(200, armingSwordDashToTarget, 30);

		%this.schedule(300, setSwordTrigger, 0, true);
		%this.schedule(400, setSwordTrigger, 0, false);
		
		%this.schedule(600, armingSwordDashToTarget, -30);

		%this.ai = %this.schedule(700, armingSwordFinishAttack);
		return;
	}
	else if(
		getSimTime() > %this.nextDash
		&& vectorDist(
			getWords(%targetPosition, 0, 1),
			getWords(%position, 0, 1)
		) < 5
	) {
		%this.setMoveX(0);
		%this.setMoveY(0);

		%scalar = -25;
		if(getRandom(0, 1) == 1 || !%this.target.isGrounded) {
			%useRightVector = true;
			%scalar *= getRandom(0, 1) ? -1 : 1;
		}
		else {
			%useRightVector = false;
		}
		
		%this.armingSwordDashToTarget(%scalar, %useRightVector);
		%this.nextDash = getSimTime() + 3000;

		%this.ai = %this.schedule(400, armingSwordAttack);
		return;
	}
	else if(getRandom(1, 10) == 5) {
		%this.armingSwordDashToTarget(25 * (getRandom(0, 1) ? -1 : 1), true);
		%this.nextDash = getSimTime() + 3000;
		%this.ai = %this.schedule(400, armingSwordAttack);
		return;
	}

	%this.ai = %this.schedule(100, armingSwordAttack);
}

function AiPlayer::armingSwordFinishAttack(%this) {
	if(!%this.isStunlocked()) {
		AiArmingSwordArmor.schedule(600, forceCycleGuard, %this, 0, getRandom(0, 1));
		%this.ai = %this.schedule(600, armingSwordAttack);
	}
	else {
		%this.schedule(600, setMoveY, -0.2);
		AiArmingSwordArmor.schedule(1000 + getRandom(-200, 200), forceCycleGuard, %this, 0, getRandom(0, 1));
		%this.ai = %this.schedule(1000 + getRandom(-200, 200), armingSwordAttack);
	}
}

function AiPlayer::armingSwordDashToTarget(%this, %scalar, %useRightVector) {
	if(%this.isStunlocked() || %this.getState() $= "Dead") {
		return;
	}
	
	serverPlay3d("ArmingSword" @ getRandom(1, 4) @ "Sound", %this.getPosition());
	
	%this.mountImage(BeefboyGuard2SkidImage, 2);
	%this.schedule(200, unMountImageSafe, 2);

	%normalized = vectorNormalize(
		vectorSub(
			getWords(%this.target.getPosition(), 0, 1),
			getWords(%this.getPosition(), 0, 1)
		)
	);

	if(%useRightVector) {
		%normalized = vectorNormalize(
			vectorCross(
				%normalized,
				"0 0 1"
			)
		);
	}

	%velocity = vectorScale(
		%normalized,
		%scalar
	);
	%this.setVelocity(%velocity);

	serverPlay3d(BeefboyGuard2SlowDownSound, %this.getPosition());
}

function AiPlayer::armingSwordAttackCleanup(%this, %state) {
	%this.attackCleanup();
}

function AiPlayer::armingSwordSeekCleanup(%this, %state) {
	%this.seekCleanup();
}
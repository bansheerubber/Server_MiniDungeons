datablock AudioProfile(HorseAttackSound) {
	filename    = "./sounds/horse.ogg";
	description = AudioCloseLooping3d;
	preload = true;
};

datablock TSShapeConstructor(MiniDungeonsHorseDts) {
	baseShape  = "./shapes/no seater horse.dts";
	sequence0  = "Add-Ons/Vehicle_Horse/h_root.dsq root";

	sequence1  = "Add-Ons/Vehicle_Horse/h_run.dsq run";
	sequence2  = "Add-Ons/Vehicle_Horse/h_run.dsq walk";
	sequence3  = "Add-Ons/Vehicle_Horse/h_back.dsq back";
	sequence4  = "Add-Ons/Vehicle_Horse/h_side.dsq side";

	sequence5  = "Add-Ons/Vehicle_Horse/h_root.dsq crouch";
	sequence6  = "Add-Ons/Vehicle_Horse/h_run.dsq crouchRun";
	sequence7  = "Add-Ons/Vehicle_Horse/h_back.dsq crouchBack";
	sequence8  = "Add-Ons/Vehicle_Horse/h_side.dsq crouchSide";

	sequence9  = "Add-Ons/Vehicle_Horse/h_look.dsq look";
	sequence10 = "Add-Ons/Vehicle_Horse/h_root.dsq headside";
	sequence11 = "Add-Ons/Vehicle_Horse/h_root.dsq headUp";

	sequence12 = "Add-Ons/Vehicle_Horse/h_jump.dsq jump";
	sequence13 = "Add-Ons/Vehicle_Horse/h_jump.dsq standjump";
	sequence14 = "Add-Ons/Vehicle_Horse/h_root.dsq fall";
	sequence15 = "Add-Ons/Vehicle_Horse/h_root.dsq land";

	sequence16 = "Add-Ons/Vehicle_Horse/h_armAttack.dsq armAttack";
	sequence17 = "Add-Ons/Vehicle_Horse/h_root.dsq armReadyLeft";
	sequence18 = "Add-Ons/Vehicle_Horse/h_root.dsq armReadyRight";
	sequence19 = "Add-Ons/Vehicle_Horse/h_root.dsq armReadyBoth";
	sequence20 = "Add-Ons/Vehicle_Horse/h_spearReady.dsq spearready";  
	sequence21 = "Add-Ons/Vehicle_Horse/h_root.dsq spearThrow";

	sequence22 = "Add-Ons/Vehicle_Horse/h_root.dsq talk";  

	sequence23 = "Add-Ons/Vehicle_Horse/h_death1.dsq death1"; 

	sequence24 = "Add-Ons/Vehicle_Horse/h_shiftUp.dsq shiftUp";
	sequence25 = "Add-Ons/Vehicle_Horse/h_shiftDown.dsq shiftDown";
	sequence26 = "Add-Ons/Vehicle_Horse/h_shiftAway.dsq shiftAway";
	sequence27 = "Add-Ons/Vehicle_Horse/h_shiftTo.dsq shiftTo";
	sequence28 = "Add-Ons/Vehicle_Horse/h_shiftLeft.dsq shiftLeft";
	sequence29 = "Add-Ons/Vehicle_Horse/h_shiftRight.dsq shiftRight";
	sequence30 = "Add-Ons/Vehicle_Horse/h_rotCW.dsq rotCW";
	sequence31 = "Add-Ons/Vehicle_Horse/h_rotCCW.dsq rotCCW";

	sequence32 = "Add-Ons/Vehicle_Horse/h_root.dsq undo";
	sequence33 = "Add-Ons/Vehicle_Horse/h_plant.dsq plant";

	sequence34 = "Add-Ons/Vehicle_Horse/h_root.dsq sit";

	sequence35 = "Add-Ons/Vehicle_Horse/h_root.dsq wrench";

	sequence36 = "Add-Ons/Vehicle_Horse/h_root.dsq activate";
	sequence37 = "Add-Ons/Vehicle_Horse/h_root.dsq activate2";

	sequence38 = "Add-Ons/Vehicle_Horse/h_root.dsq leftrecoil";
};

datablock PlayerData(MiniDungeonsHorseArmor : HorseArmor)  {
	shapeFile = MiniDungeonsHorseDts.baseShape;
	uiName = "Mini Dungeons Horse";
	minImpactSpeed = 500;
	canJet = false;

	rideable = false;
	numMountPoints = 0;

	maxHealth = 200;
};

function createHorseAi(%transform, %roomIndex) {
	%ai = new AiPlayer() {
		datablock = MiniDungeonsHorseArmor;
		position = getWords(%transform, 0, 2);
		rotation = "0 0 0 1";

		idleDrawDistance = 200;
		idleThirdSenseDistance = 5;
		idleFOV = 360;
		attackRange = 7;
		idle = "idle";
		seek = "seek";
		attack = "horseAttack";

		idleCleanup = "horseIdleCleanup";
		seekCleanup = "horseSeekCleanup";
		attackCleanup = "horseAttackCleanup";

		seekHeightCheck = true;

		isEnemyAi = true;
		isBot = true;

		name = "Horse" SPC generateRandomName();
	};
	%ai.setTransform(%transform);
	%ai.setAIState($MD::AiIdle);

	%ai.setShapeName(%ai.name, 8564862);
	%ai.setShapeNameDistance(50);

	HorseRabisSwordArmor.mount(%ai, 0);

	%ai.setAvatar("horse");

	%ai.onSpawn(%roomIndex);

	return %ai;
}

function AiPlayer::horseAttack(%this) {
	if(!%this.hasValidTarget()) {
		%this.loseTarget();
		%this.setAiState($MD::AiIdle);
		return;
	}

	%targetPosition = %this.target.getPosition();
	%position = %this.getPosition();

	%this.setMoveY(0);
	%this.setMoveX(0);

	if(getSimTime() > %this.nextHorseStrafe) {
		%this.setMoveX(getRandom(0, 1) ? -1 : 1);
		%this.nextHorseStrafe = getSimTime() + getRandom(3000, 7000);
	}

	if(!%this.nextHorseJump) {
		%this.nextHorseJump = getSimTime() + getRandom(5000, 10000);
	}

	if(getSimTime() > %this.nextHorseJump) {
		%this.setJumping(true);
		%this.schedule(33, setJumping, false);
		%this.nextHorseJump = getSimTime() + getRandom(5000, 10000);
	}

	%raycast = containerRaycast(%this.getEyePoint(), %this.target.getHackPosition(), $TypeMasks::fxBrickObjectType | $TypeMasks::PlayerObjectType, %this);
	if(vectorDist(%targetPosition, %position) > %this.attackRange || getWord(%raycast, 0) != %this.target) {
		%this.setAiState($MD::AiSeek);
		return;
	}

	if(vectorDist(%targetPosition, %position) > 3) {
		%this.setMoveY(0.75);
	}
	else {
		%this.setMoveY(-0.5);
		%this.setSwordTrigger(0, true);
	}

	%this.ai = %this.schedule(100, horseAttack);
}

function AiPlayer::horseIdleCleanup(%this, %nextState) {
	%this.playAudio(1, HorseAttackSound);
	%this.mountImage(HorseRabisImage, 0);
	%this.idleCleanup(%nextState);
}

function AiPlayer::horseSeekCleanup(%this, %nextState) {
	if(%nextState == $MD::AiIdle) {
		%this.stopAudio(1);
		%this.unMountImageSafe(0);
	}

	%this.seekCleanup(%nextState);
}

function AiPlayer::horseAttackCleanup(%this, %nextState) {
	if(%nextState == $MD::AiIdle) {
		%this.stopAudio(1);
		%this.unMountImageSafe(0);
		%this.setSwordTrigger(0, false);
	}
	
	%this.nextHorseStrafe = 0;
	%this.nextHorseJump = 0;
	%this.attackCleanup(%nextState);
}

datablock ParticleData(HorseRabisParticle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 0.5;
	inheritedVelFactor   = 1.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 800;
	lifetimeVarianceMS   = 300;
	spinSpeed       	 = 0;
	spinRandomMin        = 0;
	spinRandomMax        = 0;

	textureName		= "base/data/particles/chunk.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]	= "1.0 1.0 1.0 0.7";
	colors[1]	= "1.0 1.0 1.0 0.7";
	colors[2]	= "1.0 1.0 1.0 0.3";
	colors[3]	= "1.0 1.0 1.0 0.0";

	sizes[0]	= 0.5;
	sizes[1]	= 0.4;
	sizes[2]	= 0.3;
	sizes[3]	= 0.1;

	times[0]	= 0.0;
	times[1]	= 0.2;
	times[2]	= 0.5;
	times[3]	= 0.6;

	useInvAlpha = false;
};

datablock ParticleEmitterData(HorseRabisEmitter) {
	ejectionPeriodMS = 25;
	periodVarianceMS = 0;
	ejectionVelocity = 2;
	velocityVariance = 1;
	ejectionOffset   = 0.2;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;

	orientOnVelocity = false;
	orientParticles = false;

	particles = HorseRabisParticle;
};

datablock ShapeBaseImageData(HorseRabisImage) {
	shapeFile = "base/data/shapes/empty.dts";
	emap = true;
	mountPoint = 3;
	offset = "0 0 0";
	eyeOffset = "0 0 0";
	rotation = eulerToMatrix("0 0 0");
	correctMuzzleVector = true;
	className = "WeaponImage";
	item = "";

	melee = false;
	armReady = false;

	doColorShift = false;

	stateName[0]		= "Activate";
	stateEmitter[0]		= "HorseRabisEmitter";
	stateEmitterTime[0]	= 9999;
};
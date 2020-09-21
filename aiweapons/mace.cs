datablock AudioProfile(MaceDirectHitSound) {
	filename    = "./sounds/mace hit.ogg";
	description = AudioDefault3d;
	preload = true;
};

datablock AudioProfile(SpikesSinkSound) {
	filename    = "./sounds/sink2.ogg";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(MaceLandSound) {
	filename    = "./sounds/mace land2.ogg";
	description = AudioDefault3d;
	preload = true;
};

datablock AudioProfile(MaceTelegraph1Sound) {
	filename    = "./sounds/mace telegraph1.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(MaceTelegraph2Sound) {
	filename    = "./sounds/mace telegraph2.wav";
	description = AudioClose3d;
	preload = true;
};

datablock StaticShapeData(SpikesStatic) {
	shapeFile = "./shapes/spikes10.dts";
};

datablock ItemData(MaceItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/mace4.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "AI Mace";
	iconName = "./";
	doColorShift = false;

	sword = MaceSwordArmor;
	canDrop = true;
};

datablock TSShapeConstructor(MaceSwordDTS) {
	baseShape = "./shapes/mace4.dts";
};

datablock PlayerData(MaceSwordArmor : PlayerStandardArmor)  {
	shapeFile = MaceSwordDTS.baseShape;
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
	item = MaceItem;


	swordStartMount[0] 					= 0; // start point for raycast/interpolation
	swordEndMount[0] 					= 1; // end point for raycast/interpolation
	swordStepInterpolationCount 		= 3; // how many linear interpolations we do between steps, based on distance
	swordStepTick						= 33; // how fast we do sword stepping

	swordInterpolationDistance[0, 0]	= 1;
	swordInterpolationRadius[0, 0]		= 1;

	swordStopSwingOnBrickHit			= false; // whether or not we want to stop our sword swing when we hit a brick wall (and setImageLoaded to false)


	swordMaxCycles = 1;
	swordDoubleHanded = true;
	
	// repeating high attack
	swordCycle[0] 					= "high leftright";
	swordCycleThread[0] 			= "high2";
	swordCycleThreadSlot[0]			= 1;
	swordCycleDamage[0]				= 65;
	swordCycleImpactImpulse[0]		= 2500;
	swordCycleVerticalImpulse[0]	= 800;
	swordCycleHitExplosion[0]		= HighHitProjectile;
	swordCycleHitSound[0]			= MaceDirectHitSound;
	swordCyclePrepTime[0]			= 1;
	swordCycleGuardSound[0]			= "";
	swordCycleSwingSound[0]			= MidSwing1Sound TAB MidSwing2Sound TAB MidSwing3Sound;
	swordCycleSwingSteps[0]			= 5;
	swordCycleTelegraphSound[0]		= MaceTelegraph1Sound TAB MaceTelegraph2Sound;
	swordCycleTelegraphThread[0]	= "high2";
	swordCycleTelegraphThreadSlot[0] = 1;
	swordCycleTelegraphWaitTime[0]	= 0.2;
};

function MaceSwordArmor::startSwing(%this, %obj, %slot, %steps) {
	%displacement = getWord(%obj.getScale(), 2) * 4;
	
	%vector = %obj.getForwardVector();
	for(%i = 0; %i < 10; %i++) {
		%this.schedule(%i * 100, spawnSpike, vectorAdd(%obj.getPosition(), vectorScale(%vector, %i * 2 + %displacement)), %i == 0 ? "" : %obj);
	}

	%this.schedule(100, spawnLandExplosion, vectorAdd(%obj.getPosition(), vectorScale(%vector, %displacement)));
	
	Parent::startSwing(%this, %obj, %slot, %steps);
}

function MaceSwordArmor::onSwingEnd(%this, %obj, %slot, %id) {
	Parent::onSwingEnd(%this, %obj, %slot, %id);
}

function MaceSwordArmor::spawnSpike(%this, %position, %owner) {
	%raycast = containerRaycast(vectorAdd(%position, "0 0 3"), vectorAdd(%position, "0 0 -3"), $TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType, false);

	if(isObject(%raycast)) {
		%position = getWords(%raycast, 1, 3);
		
		%spike = new StaticShape() {
			datablock = SpikesStatic;
			position = %position;
			rotation = "0 0 0 1";
		};
		%spike.setTransform(%position SPC eulerToAxis("0 0" SPC getRandom(0, 360)));
		%spike.playThread(0, "rise");

		schedule(130, 0, serverPlay3dTimescale, StalagmiteBreakSound, %position, getRandom(8, 12) / 10);
		%this.schedule(130, spawnSpikeExplosion, %position, %owner);

		%spike.schedule(2000, playThread, 0, "sink");
		schedule(2100, 0, serverPlay3dTimescale, SpikesSinkSound, %position, getRandom(7, 14) / 10);
		%spike.schedule(2100, delete);
		%this.schedule(2100, spawnSpikeBreakExplosion, %position);
	}
}

function MaceSwordArmor::spawnSpikeExplosion(%this, %position, %owner) {
	(new Projectile() {
		datablock = MaceSpikesProjectile;
		initialPosition = %position;
		initialVelocity = "0 0 10";
		sourceObject = %owner;
		sourceSlot = 0;
		canReflect = true;
	}).explode();
}

function MaceSwordArmor::spawnSpikeBreakExplosion(%this, %position, %owner) {
	(new Projectile() {
		datablock = MaceSpikesBreakProjectile;
		initialPosition =  %position;
		initialVelocity = "0 0 10";
		sourceObject = %owner;
		sourceSlot = 0;
	}).explode();
}

function MaceSwordArmor::spawnLandExplosion(%this, %position) {
	(new Projectile() {
		datablock = MaceLandProjectile;
		initialPosition = %position;
		initialVelocity = "0 0 10";
		sourceObject = 0;
		sourceSlot = 0;
	}).explode();
}

datablock ParticleData(MaceSpikesParticle) {
	dragCoefficient      = 0;
	gravityCoefficient   = -1.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 900;
	lifetimeVarianceMS   = 400;
	spinSpeed       	 = 0;
	spinRandomMin        = -600;
	spinRandomMax        = 600;

	textureName		= "base/data/particles/cloud.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]	= "0.3 0.3 0.3 1.0";
	colors[1]	= "0.3 0.3 0.3 0.9";
	colors[2]	= "0.2 0.2 0.2 0.0";
	colors[3]	= "0.2 0.2 0.2 0.0";

	sizes[0]	= 3.0;
	sizes[1]	= 4.3;
	sizes[2]	= 5.3;
	sizes[3]	= 5.3;

	times[0]	= 0.0;
	times[1]	= 0.2;
	times[2]	= 0.5;
	times[3]	= 0.6;

	useInvAlpha = true;
};

datablock ParticleEmitterData(MaceSpikesEmitter) {
	ejectionPeriodMS = 15;
	periodVarianceMS = 0;
	ejectionVelocity = 3;
	velocityVariance = 0;
	ejectionOffset   = 1;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;

	particles = MaceSpikesParticle;
};

datablock ParticleData(MaceSpikes2Particle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 1.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 900;
	lifetimeVarianceMS   = 400;
	spinSpeed       	 = 0;
	spinRandomMin        = -600;
	spinRandomMax        = 600;

	textureName		= "base/data/particles/chunk.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]	= "0.2 0.2 0.2 0.9";
	colors[1]	= "0.2 0.2 0.2 0.9";
	colors[2]	= "0.1 0.1 0.1 0.0";
	colors[3]	= "0.1 0.1 0.1 0.0";

	sizes[0]	= 0.8;
	sizes[1]	= 1.2;
	sizes[2]	= 0.2;
	sizes[3]	= 0.0;

	times[0]	= 0.0;
	times[1]	= 0.2;
	times[2]	= 0.5;
	times[3]	= 0.6;

	useInvAlpha = true;
};

datablock ParticleEmitterData(MaceSpikes2Emitter) {
	ejectionPeriodMS = 10;
	periodVarianceMS = 0;
	ejectionVelocity = 5;
	velocityVariance = 9;
	ejectionOffset   = 1;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;

	particles = MaceSpikes2Particle;
};

datablock ExplosionData(MaceSpikesExplosion) {
	shakeCamera = false;

	emitter[0] = MaceSpikesEmitter;
	emitter[1] = MaceSpikes2Emitter;

	lifeTimeMS = 100;

	shakeCamera = true;
	camShakeFreq = "2 2 2";
	camShakeAmp = "2 2 2";
	camShakeDuration = 1;
	camShakeRadius = 20;

	soundProfile = "";

	impulseForce = 700;
	impulseRadius = 4;

	radiusDamage = 30;
	damageRadius = 3;

	debris = Stalagmite1Part4BrokenDebris;
	debrisNum = 1;
	debrisNumVariance = 0;
	debrisPhiMin = 0;
	debrisPhiMax = 360;
	debrisThetaMin = 0;
	debrisThetaMax = 60;
	debrisVelocity = 15;
	debrisVelocityVariance = 3;
};

datablock ProjectileData(MaceSpikesProjectile) {
	explosion = MaceSpikesExplosion;
};

datablock ExplosionData(MaceSpikesBreakExplosion) {
	shakeCamera = false;

	emitter[0] = MaceSpikesEmitter;
	emitter[1] = MaceSpikes2Emitter;

	lifeTimeMS = 100;

	shakeCamera = true;
	camShakeFreq = "0.5 0.5 0.5";
	camShakeAmp = "0.5 0.5 0.5";
	camShakeDuration = 1;
	camShakeRadius = 20;

	soundProfile = "";

	debris = Stalagmite1Part1BrokenDebris;
	debrisNum = 1;
	debrisNumVariance = 0;
	debrisPhiMin = 0;
	debrisPhiMax = 360;
	debrisThetaMin = 0;
	debrisThetaMax = 60;
	debrisVelocity = 5;
	debrisVelocityVariance = 0;
};

datablock ProjectileData(MaceSpikesBreakProjectile) {
	explosion = MaceSpikesBreakExplosion;
};

datablock ParticleData(MaceSpikesRingParticle) {
	dragCoefficient      = 2;
	gravityCoefficient   = 0.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 900;
	lifetimeVarianceMS   = 400;
	spinSpeed       	 = 0;
	spinRandomMin        = -600;
	spinRandomMax        = 600;

	textureName		= "base/data/particles/chunk.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]	= "0.1 0.1 0.1 0.9";
	colors[1]	= "0.1 0.1 0.1 0.9";
	colors[2]	= "0.05 0.05 0.05 0.0";
	colors[3]	= "0.05 0.05 0.05 0.0";

	sizes[0]	= 1.8;
	sizes[1]	= 2.2;
	sizes[2]	= 2.2;
	sizes[3]	= 0.0;

	times[0]	= 0.0;
	times[1]	= 0.2;
	times[2]	= 0.5;
	times[3]	= 0.6;

	useInvAlpha = true;
};

datablock ParticleEmitterData(MaceSpikesRingEmitter) {
	ejectionPeriodMS = 5;
	periodVarianceMS = 0;
	ejectionVelocity = 5;
	velocityVariance = 9;
	ejectionOffset   = 1;
	thetaMin         = 89;
	thetaMax         = 90;
	phiReferenceVel  = 36000;
	phiVariance      = 360;
	overrideAdvance = false;

	particles = MaceSpikesRingParticle;
};

datablock ExplosionData(MaceLandExplosion) {
	shakeCamera = false;

	explosionShape = "./shapes/mace hit sphere3.dts";

	emitter[0] = MaceSpikesRingEmitter;

	lifeTimeMS = 200;

	shakeCamera = true;
	camShakeFreq = "0.5 0.5 0.5";
	camShakeAmp = "0.5 0.5 0.5";
	camShakeDuration = 1;
	camShakeRadius = 20;

	soundProfile = "MaceLandSound";
};

datablock ProjectileData(MaceLandProjectile) {
	explosion = MaceLandExplosion;
};
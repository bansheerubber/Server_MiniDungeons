datablock AudioProfile(CannonFireSound) {
	filename    = "./sounds/cannon fire3.ogg";
	description = AudioDefault3d;
	preload = true;
};

datablock StaticShapeData(CannonRadiusStatic) {
	shapeFile = "./shapes/cannon radius.dts";
};

datablock PlayerData(AiCannonPlayer : PlayerStandardArmor) {
	cameraVerticalOffset = 3;
	shapefile = "./shapes/cannon.dts";
	minImpactSpeed = 0;
	canJet = 0;
	mass = 200000;
	drag = 1;
	density = 5;
	runSurfaceAngle = 1;
	jumpSurfaceAngle = 0;
	maxForwardSpeed = 0;
	maxBackwardSpeed = 0;
	maxBackwardCrouchSpeed = 0;
	maxForwardCrouchSpeed = 0;
	maxSideSpeed = 0;
	maxSideCrouchSpeed = 0;
	maxStepHeight = 0;
	maxUnderwaterSideSpeed = 0;

	uiName = "Ai Cannon";
	showEnergyBar = false;

	jumpForce = 0;
	jumpEnergyDrain = 10000;
	minJumpEnergy = 10000;
	jumpDelay = 127;
	minJumpSpeed = 0;
	maxJumpSpeed = 0;

	rideable = true;
	canRide = true;
	paintable = true;

	boundingBox			= vectorScale("3.75 3.75 1.85", 4);
	crouchBoundingBox	= vectorScale("3.75 3.75 1.85", 4);

	lookUpLimit = $pi / 2;
	lookDownLimit = -$pi / 2;

	numMountPoints = 1;
	mountThread[0] = "root";

	upMaxSpeed = 1;
	upResistSpeed = 1;
	upResistFactor = 1;
	maxdamage = 300;
	minlookangle = -$pi;
	maxlookangle = $pi;

	useCustomPainEffects = true;
	PainHighImage = "";
	PainMidImage  = "";
	PainLowImage  = "";
	painSound     = "";
	deathSound    = "";
};

datablock ParticleData(AiCannonTrailParticle) {
	dragCoefficient      = 5;
	gravityCoefficient   = 0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 5000;
	lifetimeVarianceMS   = 0;
	spinSpeed       	 = 0;
	spinRandomMin        = 0;
	spinRandomMax        = 0;

	textureName		= "base/data/particles/ring.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]     = "1.0 0.9 0.6 0.1";
	colors[1]     = "1.0 0.9 0.6 0.1";
	colors[2]     = "1.0 0.9 0.6 0.05";
	colors[3]     = "1.0 0.9 0.6 0.0";
	times[0] = 0.0;
	times[1] = 0.2;
	times[2] = 0.5;
	times[3] = 0.6;

	useInvAlpha = false;
};

datablock ParticleEmitterData(AiCannonTrailEmitter) {
	ejectionPeriodMS = 35;
	periodVarianceMS = 0;
	ejectionVelocity = 0;
	velocityVariance = 0;
	ejectionOffset   = 0;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;

	orientOnVelocity = false;
	orientParticles = false;

	particles = AiCannonTrailParticle;
};

datablock ExplosionData(AiCannonExplosion) {
	explosionShape = "";
	soundProfile = rocketExplodeSound;

	lifeTimeMS = 150;

	debris = tankShellDebris;
	debrisNum = 30;
	debrisNumVariance = 10;
	debrisPhiMin = 0;
	debrisPhiMax = 360;
	debrisThetaMin = 0;
	debrisThetaMax = 180;
	debrisVelocity = 140;
	debrisVelocityVariance = 50;

	particleEmitter = gravityRocketExplosionEmitter;
	particleDensity = 10;
	particleRadius = 0.2;

	emitter[0] = gravityRocketExplosionRingEmitter;
	emitter[1] = gravityRocketExplosionChunkEmitter;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = true;
	camShakeFreq = "10.0 11.0 10.0";
	camShakeAmp = "3.0 10.0 3.0";
	camShakeDuration = 0.5;
	camShakeRadius = 20.0;

	lightStartRadius = 5;
	lightEndRadius = 20;
	lightStartColor = "1 1 0 1";
	lightEndColor = "1 0 0 0";

	damageRadius = 6;
	radiusDamage = 80;

	impulseRadius = 8;
	impulseForce = 5000;

	playerBurnTime = 5000;
};

datablock ProjectileData(AiCannonProjectile) {
	projectileShapeName = "./shapes/cannoncube.dts";
	directDamage        = 100;
	directDamageType = $DamageType::CannonBallDirect;
	radiusDamageType = $DamageType::CannonBallRadius;
	impactImpulse	   = 1000;
	verticalImpulse	   = 1000;
	explosion           = AiCannonExplosion;
	particleEmitter     = AiCannonTrailEmitter;

	sound = WhistleLoopSound;

	muzzleVelocity      = 120;
	velInheritFactor    = 1.0;

	armingDelay         = 0;
	lifetime            = 45000;
	fadeDelay           = 45000;
	bounceElasticity    = 0.5;
	bounceFriction      = 0.20;
	isBallistic         = true;
	gravityMod = 1.0;

	hasLight    = false;
	lightRadius = 5.0;
	lightColor  = "1 0.5 0.0";

	explodeOnDeath = 1;
};

function AiPlayer::cannonTrackPlayer(%this) {
	if(!%this.hasValidTarget()) {
		return;
	}

	%time = 2.5;
	%targetPosition = %this.target.getPosition();
	%position = %this.getMuzzlePoint(2);
	%this.setAimVector(vectorNormalize(calculateVelocityFromTime(%position, %targetPosition, %time, 1)));
	%this.setVelocity(%this.getVelocity());

	%this.cannonTrack = %this.schedule(66, cannonTrackPlayer);
}

function AiPlayer::cannonFire(%this, %targetPosition) {
	%this.dummyPlayer.playThread(0, "jump");
	
	%position = %this.getMuzzlePoint(2);
	%time = 2.5;
	%velocity = calculateVelocityFromTime(%position, %targetPosition, %time, 1);
	%this.setAimVector(vectorNormalize(%velocity));
	%this.setVelocity(%this.getVelocity());

	%shape = new StaticShape() {
		datablock = CannonRadiusStatic;
		position = %targetPosition;
		rotation = "0 0 0 1";
	};
	%shape.playThread(0, "increase");
	%shape.schedule(2500, delete);
	%shape.setNodeColor("ALL", "1 0 0 1");
	%shape.setScale((6 / 8) SPC (6 / 8) SPC (6 / 8));

	new Projectile() {
		datablock = AiCannonProjectile;
		initialPosition = %position;
		initialVelocity = %velocity;
		sourceObject = %this;
		sourceSlot = 0;
		cannonRadius = %shape;
	};

	(new Projectile() {
		datablock = AiCannonFireProjectile;
		initialPosition = %position;
		initialVelocity = %velocity;
		sourceObject = 0;
		sourceSlot = 0;
	}).explode();

	serverPlay3dTimescale(CannonFireSound, %position, getRandom(8, 12) / 10);

	%this.playThread(1, "fire");
}

function AiCannonProjectile::onExplode(%this, %obj, %position) {
	if(isObject(%obj.cannonRadius)) {
		%obj.cannonRadius.delete();
	}
	Parent::onExplode(%this, %obj, %position);
}



datablock ParticleData(AiCannonFireParticle) {
	dragCoefficient      = 6;
	gravityCoefficient   = -0.5;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 1500;
	lifetimeVarianceMS   = 500;
	spinSpeed       	 = 0;
	spinRandomMin        = -200;
	spinRandomMax        = 200;

	textureName		= "base/data/particles/cloud.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]     = "1.0 0.5 0.0 0.8";
	colors[1]     = "0.0 0.0 0.0 0.6";
	colors[2]     = "0.0 0.0 0.0 0.2";
	colors[3]     = "0.0 0.0 0.0 0.0";

	sizes[0]      = 2.8;
	sizes[1]      = 2.8;
	sizes[2]      = 3.1;
	sizes[3]      = 5.1;

	times[0] = 0.0;
	times[1] = 0.15;
	times[2] = 0.6;
	times[3] = 1.0;

	useInvAlpha = true;
};

datablock ParticleEmitterData(AiCannonFireEmitter) {
	ejectionPeriodMS = 1;
	periodVarianceMS = 0;
	ejectionVelocity = 35;
	velocityVariance = 35;
	ejectionOffset   = 0;
	thetaMin         = 0;
	thetaMax         = 25;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;

	orientOnVelocity = false;
	orientParticles = false;

	particles = AiCannonFireParticle;
};

datablock ExplosionData(AiCannonFireExplosion) {
	shakeCamera = false;

	emitter[0] = AiCannonFireEmitter;

	lifeTimeMS = 100;

	shakeCamera = true;
	camShakeFreq = "2 2 2";
	camShakeAmp = "4 4 4";
	camShakeDuration = 1;
	camShakeRadius = 80;

	soundProfile = "";
};

datablock ProjectileData(AiCannonFireProjectile) {
	explosion = AiCannonFireExplosion;
};

datablock DebrisData(AiCannonDebris) {
	shapeFile = "./shapes/cannon debris2.dts";
	lifetime = 3.5;
	minSpinSpeed = -300.0;
	maxSpinSpeed = 300.0;
	elasticity = 0.5;
	friction = 0.2;
	numBounces = 1;
	staticOnMaxBounce = true;
	snapOnMaxBounce = false;
	fade = true;

	gravModifier = 2;
};

datablock ExplosionData(AiCannonDebrisExplosion) {
	lifeTimeMS = 150;

	soundProfile = vehicleExplosionSound;

	emitter[0] = vehicleExplosionEmitter;
	emitter[1] = vehicleExplosionEmitter2;

	debris = AiCannonDebris;
	debrisNum = 1;
	debrisNumVariance = 0;
	debrisPhiMin = 0;
	debrisPhiMax = 360;
	debrisThetaMin = 0;
	debrisThetaMax = 20;
	debrisVelocity = 18;
	debrisVelocityVariance = 3;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = true;
	camShakeFreq = "7.0 8.0 7.0";
	camShakeAmp = "10.0 10.0 10.0";
	camShakeDuration = 0.75;
	camShakeRadius = 15.0;

	lightStartRadius = 0;
	lightEndRadius = 20;
	lightStartColor = "0.45 0.3 0.1";
	lightEndColor = "0 0 0";

	impulseRadius = 15;
	impulseForce = 1000;
	impulseVertical = 2000;

	playerBurnTime = 5000;
};

datablock ProjectileData(AiCannonDebrisProjectile) {
	directDamage        = 0;
	radiusDamage        = 0;
	damageRadius        = 0;
	explosion           = AiCannonDebrisExplosion;

	explodeOnDeath		= 1;

	armingDelay         = 0;
	lifetime            = 10;
};

deActivatePackage(MiniDungeonsCannon);
package MiniDungeonsCannon {
	function AiCannonPlayer::onDisabled(%this, %obj) {
		Parent::onDisabled(%this, %obj);
		
		(new Projectile() {
			datablock = AiCannonDebrisProjectile;
			initialPosition = %obj.getHackPosition();
			initialVelocity = "0 0 10";
			sourceObject = %obj;
			sourceSlot = 0;
		}).explode();
		
		%obj.dummyPlayer.kill();
		%obj.delete();
	}

	function AiCannonPlayer::onRemove(%this, %obj) {
		if(isObject(%obj.dummyPlayer) && %obj.dummyPlayer.getState() !$= "Dead") {
			%obj.dummyPlayer.delete();
		}
		Parent::onRemove(%this, %obj);
	}
};
activatePackage(MiniDungeonsCannon);
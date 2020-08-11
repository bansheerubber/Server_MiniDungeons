datablock AudioProfile(CritSound) {
	filename = "./sounds/crit.wav";
	description = AudioDefault3d;
	preload = true;
};

datablock AudioProfile(CritActivateSound) {
	filename = "./sounds/crit activate.wav";
	description = AudioDefault3d;
	preload = true;
};

datablock ParticleData(CritParticle) {
	dragCoefficient      = 2;
	gravityCoefficient   = 0.1;
	inheritedVelFactor   = 0.0;
	windCoefficient      = 0;
	constantAcceleration = 0.0;
	lifetimeMS           = 500;
	lifetimeVarianceMS   = 200;
	useInvAlpha          = false;
	textureName          = "./textures/crit";
	spinRandomMin        = -15;
	spinRandomMax        = 15;


	colors[0]     = "1 0 0 1";
	colors[1]     = "0 0 1 1";
	colors[2]     = "0 1 0 1";
	colors[3]     = "1 0 0 0";

	sizes[0]      = 5.5;
	sizes[1]      = 3;
	sizes[2]      = 3;
	sizes[3]      = 3;

	times[0]      = 0.0;
	times[1]      = 0.2;
	times[2]      = 0.6;
	times[3]      = 1.0;
};

datablock ParticleEmitterData(CritEmitter) {
	ejectionPeriodMS = 10;
	periodVarianceMS = 0;
	ejectionVelocity = 0.4;
	velocityVariance = 0;
	ejectionOffset   = 0;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	lifeTimeMS = 100;
	particles = "CritParticle";

	doFalloff = false; //if we do fall off with this emitter it ends up flickering, for most emitters you want this TRUE
};

datablock ExplosionData(CritExplosion) {
	lifeTimeMS = 2000;
	emitter[0] = CritEmitter;
	soundProfile = "";
};

//we cant spawn explosions, so this is a workaround for now
datablock ProjectileData(CritProjectile) {
	explosion           = CritExplosion;
	armingDelay         = 0;
	lifetime            = 10;
	explodeOnDeath		= true;
};

function Player::crit(%this) {
	%p = new Projectile() {
		datablock = CritProjectile;
		initialPosition = vectorAdd(%this.getEyePoint(), "0 0 2.5");
		initialVelocity = "0 0 10";
		sourceObject = 0;
		sourceSlot = 0;
		scale = %this.getScale();
	};
	%p.explode();
	%this.stopAudio(3);
	%this.playAudio(3, CritSound);
}

datablock ParticleData(CritPowerParticle) {
	dragCoefficient      = 2;
	gravityCoefficient   = -1;
	inheritedVelFactor   = 0;
	windCoefficient      = 0;
	constantAcceleration = 0.0;
	lifetimeMS           = 800;
	lifetimeVarianceMS   = 400;
	useInvAlpha          = false;
	textureName          = "base/data/particles/dot";
	spinRandomMin        = -15;
	spinRandomMax        = 15;


	colors[0]     = "1 0 0 0.3";
	colors[1]     = "0 0 1 0.5";
	colors[2]     = "1 0 0 0.5";
	colors[3]     = "0 1 0 0";

	sizes[0]      = 2;
	sizes[1]      = 0.5;
	sizes[2]      = 0.5;
	sizes[3]      = 0.5;

	times[0]      = 0.0;
	times[1]      = 0.2;
	times[2]      = 0.6;
	times[3]      = 1.0;
};

datablock ParticleEmitterData(CritPowerEmitter) {
	ejectionPeriodMS = 3;
	periodVarianceMS = 0;
	ejectionVelocity = 5;
	velocityVariance = 15;
	ejectionOffset   = 0.5;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	particles = CritPowerParticle;

	doFalloff = false;
};

datablock ShapeBaseImageData(CritPowerImage) {
	shapeFile = "base/data/shapes/empty.dts";
	emap = true;
	mountPoint = 10;
	offset = "0 0 1";
	eyeOffset = "0 0 0";
	rotation = eulerToMatrix("0 0 0");
	className = "WeaponImage";

	stateName[0]		= "Activate";
	stateEmitter[0]		= CritPowerEmitter;
	stateEmitterTime[0]	= 9000;
};

function Player::deleteCritPower(%this) {
	if(isObject(%this.critPower)) {
		%this.critPower.delete();

		if(%this.client.hasBBDXClient) {
			commandToClient(%this.client, 'BBDX_toggleRainbowHealthbar', false);
		}
	}
}

function Player::mountCritPower(%this) {
	%this.deleteCritPower();

	%this.critPower = new AIPlayer() {
		datablock = MountPointArmor;
		position = "0 0 -1000";
		rotation = "0 0 0 1";
		isVisible = true;
		isStaticFX = true;
	};
	%this.critPower.kill();
	%this.mountObject(%this.critPower, 31);
	%this.critPower.mountImage(CritPowerImage, 0);
	%this.critPowerLoop();

	%p = new Projectile() {
		datablock = CritPowerProjectile;
		initialPosition = %this.getHackPosition();
		initialVelocity = "0 0 100";
		sourceObject = 0;
		sourceSlot = 0;
	};
	%p.explode();

	if(%this.client.hasBBDXClient) {
		commandToClient(%this.client, 'BBDX_toggleRainbowHealthbar', true);
	}

	%this.playAudio(3, CritActivateSound);
}

function Player::critPowerLoop(%this) {
	if(isObject(%this.critPower)) {
		cancel(%this.critPowerLoop);
		if(%this.isFirstPerson() && %this.critPower.isVisible) {
			%this.critPower.unGhost(%this.client);
			%this.critPower.isVisible = false;
		}
		else if(!%this.isFirstPerson() && !%this.critPower.isVisible) {
			%this.critPower.reGhost(%this.client);
			%this.critPower.isVisible = true;
		}
		%this.critPowerLoop = %this.schedule(100, critPowerLoop);
	}	
}

deActivatePackage(MiniDungeonsCritPowerPackage);
package MiniDungeonsCritPowerPackage {
	function Armor::onRemove(%this, %obj) {
		%obj.deleteCritPower();
		Parent::onRemove(%this, %obj);
	}
};
activatePackage(MiniDungeonsCritPowerPackage);

datablock ParticleData(CritPowerExplosionParticle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 1;
	inheritedVelFactor   = 0;
	windCoefficient      = 0;
	constantAcceleration = 0.0;
	lifetimeMS           = 1500;
	lifetimeVarianceMS   = 800;
	useInvAlpha          = false;
	textureName          = "base/data/particles/dot";
	spinRandomMin        = -1500;
	spinRandomMax        = 1500;


	colors[0]     = "1 0 0 0.5";
	colors[1]     = "0 0 1 0.7";
	colors[2]     = "1 0 0 0.7";
	colors[3]     = "0 1 0 0";

	sizes[0]      = 2;
	sizes[1]      = 0.5;
	sizes[2]      = 0.2;
	sizes[3]      = 0.1;

	times[0]      = 0.0;
	times[1]      = 0.2;
	times[2]      = 0.6;
	times[3]      = 1.0;
};

datablock ParticleEmitterData(CritPowerExplosionEmitter) {
	ejectionPeriodMS = 1;
	periodVarianceMS = 0;
	ejectionVelocity = 5;
	velocityVariance = 25;
	ejectionOffset   = 0.5;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	particles = CritPowerExplosionParticle;
};

datablock ParticleData(CritPowerExplosion2Particle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 1;
	inheritedVelFactor   = 0;
	windCoefficient      = 0;
	constantAcceleration = 0.0;
	lifetimeMS           = 300;
	lifetimeVarianceMS   = 200;
	useInvAlpha          = false;
	textureName          = "./textures/crit star";
	spinRandomMin        = -1500;
	spinRandomMax        = 1500;


	colors[0]     = "1 0 0 0.5";
	colors[1]     = "0 0 1 0.7";
	colors[2]     = "1 0 0 0.7";
	colors[3]     = "0 1 0 0";

	sizes[0]      = 4;
	sizes[1]      = 1.5;
	sizes[2]      = 1.5;
	sizes[3]      = 1.5;

	times[0]      = 0.0;
	times[1]      = 0.2;
	times[2]      = 0.6;
	times[3]      = 1.0;
};

datablock ParticleEmitterData(CritPowerExplosion2Emitter) {
	ejectionPeriodMS = 1;
	periodVarianceMS = 0;
	ejectionVelocity = 5;
	velocityVariance = 25;
	ejectionOffset   = 0.5;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	particles = CritPowerExplosion2Particle;
};

datablock ExplosionData(CritPowerExplosion) {
	lifeTimeMS = 100;
	emitter[0] = CritPowerExplosionEmitter;
	emitter[1] = CritPowerExplosion2Emitter;

	camShakeDuration = 2;
	camShakeRadius = 100;
	camShakeAmp = "1 1 1";
	camShakeFreq = "10 10 10";
	shakeCamera = true;
};

datablock ProjectileData(CritPowerProjectile) {
	explosion = CritPowerExplosion;
};
function Armor::spawnChest(%this, %position) {
	%chest = new AiPlayer() {
		datablock = %this;
		position = %position;
		rotation = "0 0 0 1";
	};

	(new Projectile() {
		datablock = PlayerTeleportProjectile;
		initialPosition = vectorAdd(%position, "0 0 0.4");
		initialVelocity = "0 0 10";
		sourceObject = 0;
		sourceSlot = 0;
	}).explode();

	return %chest;
}

function Armor::onChestInteract(%this, %obj, %interactee) {
	if(getSimTime() > %obj.nextInteract) {
		if(!%obj.isOpen) {
			%obj.playThread(0, "open");
			%obj.isOpen = true;

			serverPlay3dTimescale(CycleLowReadySound, %obj.getPosition(), getRandom(8, 12) / 10);
		}
		else {
			%obj.playThread(0, "close");
			%obj.isOpen = false;

			schedule(250, 0, serverPlay3dTimescale, CycleLowReadySound, %obj.getPosition(), getRandom(8, 12) / 10);
		}

		%obj.nextInteract = getSimTime() + 1000;
	}
}

deActivatePackage(MiniDungeonsChests);
package MiniDungeonsChests {
	function Armor::onImpact(%this, %obj, %collidedObject, %vec, %vecLen) {
		if(%this.isChest) {
			if(!%obj.hasLanded) {
				(new Projectile() {
					datablock = ChestLandProjectile;
					initialPosition = vectorAdd(%obj.getPosition(), "0 0 -0.2");
					initialVelocity = "0 0 10";
					sourceObject = 0;
					sourceSlot = 0;
				}).explode();

				%obj.hasLanded = true;

				%obj.setTransform(%obj.getPosition());
			}
			
			return;
		}
		
		Parent::onImpact(%this, %obj, %collidedObject, %vec, %vecLen);
	}
};
activatePackage(MiniDungeonsChests);

datablock ParticleData(ChestLandParticle) {
	dragCoefficient      = 15;
	gravityCoefficient   = 0.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 600;
	lifetimeVarianceMS   = 0;
	spinSpeed       	 = 0;
	spinRandomMin        = -600;
	spinRandomMax        = 600;

	textureName		= "base/data/particles/cloud.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]	= "1.0 1.0 1.0 0.2";
	colors[1]	= "1.0 1.0 1.0 0.2";
	colors[2]	= "1.0 1.0 1.0 0.0";
	colors[3]	= "1.0 1.0 1.0 0.0";

	sizes[0]	= 1.0;
	sizes[1]	= 0.7;
	sizes[2]	= 0.5;
	sizes[3]	= 0.0;

	times[0]	= 0.0;
	times[1]	= 0.2;
	times[2]	= 0.5;
	times[3]	= 0.6;

	useInvAlpha = false;
};

datablock ParticleEmitterData(ChestLandEmitter) {
	ejectionPeriodMS = 3;
	periodVarianceMS = 0;
	ejectionVelocity = 30;
	velocityVariance = 0;
	ejectionOffset   = 1.2;
	thetaMin         = 90;
	thetaMax         = 91;
	phiReferenceVel  = 36000;
	phiVariance      = 360;
	overrideAdvance = false;

	particles = ChestLandParticle;
};

datablock ExplosionData(ChestLandExplosion) {
	shakeCamera = false;

	emitter[0] = ChestLandEmitter;

	lifeTimeMS = 200;

	shakeCamera = true;
	camShakeFreq = "1.5 1.5 1.5";
	camShakeAmp = "5 5 5";
	camShakeDuration = 1;
	camShakeRadius = 20;

	soundProfile = "MaceKickSound";
};

datablock ProjectileData(ChestLandProjectile) {
	explosion = ChestLandExplosion;
};
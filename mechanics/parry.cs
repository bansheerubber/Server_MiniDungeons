datablock ParticleData(Parry2Particle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 0.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 166;
	lifetimeVarianceMS   = 0;
	spinSpeed       	 = 0;
	spinRandomMin        = 0;
	spinRandomMax        = 0;

	textureName		= "Add-Ons/Player_Beefboy/textures/hit1.png";
	animTexName[0]	= "Add-Ons/Player_Beefboy/textures/hit1.png";
	animTexName[1]	= "Add-Ons/Player_Beefboy/textures/hit2.png";
	animTexName[2]	= "Add-Ons/Player_Beefboy/textures/hit3.png";
	animTexName[3]	= "Add-Ons/Player_Beefboy/textures/hit4.png";
	animTexName[4]	= "Add-Ons/Player_Beefboy/textures/hit5.png";
	animTexName[5]	= "Add-Ons/Player_Beefboy/textures/hit6.png";
	animTexName[6]	= "Add-Ons/Player_Beefboy/textures/hit7.png";
	animTexName[7]	= "Add-Ons/Player_Beefboy/textures/hit8.png";
	animTexName[8]	= "Add-Ons/Player_Beefboy/textures/hit9.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]   = "1.0 1.0 1.0 0.7";
	colors[1]   = "1.0 1.0 1.0 0.7";
	colors[2]   = "1.0 1.0 1.0 0.7";
	colors[3]   = "1.0 1.0 1.0 0.7";

	sizes[0]    = 3.5;
	sizes[1]    = 3.5;
	sizes[2]    = 3.5;
	sizes[3]    = 3.5;

	times[0] 	= 0.0;
	times[1] 	= 0.25;
	times[2] 	= 0.5;
	times[3] 	= 0.75;

	useInvAlpha = false;
};

datablock ParticleEmitterData(Parry2Emitter) {
	ejectionPeriodMS = 5;
	periodVarianceMS = 0;
	ejectionVelocity = 0;
	velocityVariance = 0;
	ejectionOffset   = 0;
	thetaMin         = 0;
	thetaMax         = 0;
	phiReferenceVel  = 0;
	phiVariance      = 0;
	overrideAdvance = false;

	particles = Parry2Particle;
};

datablock ParticleData(ParryParticle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 1.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 300;
	lifetimeVarianceMS   = 300;
	spinSpeed       	 = 0;
	spinRandomMin        = -1000;
	spinRandomMax        = 1000;

	textureName		= "base/data/particles/star1.png";

	colors[0]     = "1.0 1.0 0.0 0.7";
	colors[1]     = "1.0 1.0 0.0 0.7";
	colors[2]     = "1.0 1.0 0.0 0.3";
	colors[3]     = "1.0 1.0 0.0 0.0";

	sizes[0]      = 1.5;
	sizes[1]      = 0.8;
	sizes[2]      = 0.7;
	sizes[3]      = 0.6;

	times[0] = 0.0;
	times[1] = 0.2;
	times[2] = 0.5;
	times[3] = 0.6;

	useInvAlpha = true;
};

datablock ParticleEmitterData(ParryEmitter) {
	ejectionPeriodMS = 25;
	periodVarianceMS = 0;
	ejectionVelocity = 10;
	velocityVariance = 5;
	ejectionOffset   = 1;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;

	lifeTimeMS = 100;

	orientOnVelocity = true;
	orientParticles = true;

	particles = ParryParticle;
};

datablock ExplosionData(ParryExplosion) {
	explosionShape = "";
	
	particleEmitter = Parry2Emitter;
	particleDensity = 1;
	particleRadius = 0;

	emitter[0] = ParryEmitter;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = false;
	camShakeFreq = "2 2 2";
	camShakeAmp = "10 10 10";
	camShakeDuration = 0;
	camShakeRadius = 0;

	lifetimeMS = 400;
};

datablock ProjectileData(ParryProjectile : gunProjectile) {
	explosion = ParryExplosion;
	uiName = "";
};

function serverCmdParryLeft(%client) {
	%client.parry("left");
}

function serverCmdParryRight(%client) {
	%client.parry("right");
}

function GameConnection::parry(%client, %direction) {
	%obj = %client.player;

	if(isObject(%obj) && isObject(%sword = %obj.sword[0])) {
		%cooldown = (%shield = %obj.getMountedImage(1)).parryIsShield ? %shield.parryCooldown : %sword.getDatablock().parryCooldown;
		if(getSimTime() - %obj.lastParryTime > %cooldown && !%obj.isStunlocked() && !%obj.isParrying) {
			%obj.setParry(true, %direction);

			if(!%obj.getMountedImage(1).parryIsShield) {
				%obj.schedule(%sword.getDatablock().parryDuration, setParry, false);
			}
		}
		else {
			%client.play2d(errorSound);
		}
	}
}

function serverCmdStopParry(%client) {
	%obj = %client.player;

	if(isObject(%obj) && isObject(%sword = %obj.sword[0])) {
		if(%obj.getMountedImage(1).parryIsShield && %obj.isParrying) {
			%obj.setParry(false); // stop parrying if we let go of mouse if we have a shied
		}
	}
}

deActivatePackage(MiniDungeonsParry);
package MiniDungeonsParry {
	function serverCmdActivateStuff(%this) {
		if(%this.canParry && isObject(%obj = %this.player) && isObject(%sword = %obj.sword[0])) {
			%cooldown = (%shield = %obj.getMountedImage(1)).parryIsShield ? %shield.parryCooldown : %sword.getDatablock().parryCooldown;
			
			if(getSimTime() - %obj.lastParryTime > %cooldown && !%obj.isStunlocked() && !%obj.isParrying) {
				%obj.parryDuration = getSimTime() + 300;
			}
			else {
				%obj.parryErrorDuration = getSimTime() + 300;
			}
			return;
		}

		Parent::serverCmdActivateStuff(%this);
	}

	function Armor::onTrigger(%this, %obj, %slot, %val) {
		if(isObject(%sword = %obj.sword[0]) && (%sword.getDatablock().parryDuration !$= "" || %obj.getMountedImage(1).parryIsShield) && (%slot == 0 || %slot == 4)) {
			if(%val == 1) {
				if(getSimTime() < %obj.parryDuration) {
					%obj.setParry(true, %slot == 0 ? "left" : "right");

					if(!%obj.getMountedImage(1).parryIsShield) {
						%obj.schedule(%sword.getDatablock().parryDuration, setParry, false);
					}

					%obj.parryDuration = 0;
					return;
				}
				else if(getSimTime() < %obj.parryErrorDuration) {
					%obj.client.play2d(errorSound);
					return;
				}
			}
			else if(%obj.getMountedImage(1).parryIsShield && %obj.isParrying) {
				%obj.setParry(false); // stop parrying if we let go of mouse if we have a shield
				return;
			}
		}

		if(isObject(%obj.sword[0]) && %slot == 0) {
			%obj.setSwordTrigger(%slot, %val);
			return;
		}
		
		Parent::onTrigger(%this, %obj, %slot, %val);
	}

	function ProjectileData::onCollision(%this, %obj, %col, %fade, %position, %normal) {
		if(%obj.canReflect && %col.isParrying && miniGameCanDamage(%obj.sourceObject, %col) == 1 && !%col.getMountedImage(1).parryIsShield) { // reflect the projectile if we can and we're parrying and we don't have a shield (shields can't reflect)
			%speed = vectorLen(%obj.initialVelocity);
			%targetPosition = %obj.sourceObject.getHackPosition();
			%zAngle = calculateProjectileZAngle(%position, %targetPosition, %speed, %this.gravityMod);

			%yDifference = getWord(%targetPosition, 1) - getWord(%position, 1);
			%dot = vectorDot("1 0 0", vectorNormalize(vectorSub(getWords(%targetPosition, 0, 1), getWords(%position, 0, 1))));
			%xyAngle = mACos(%dot) * (%yDifference < 0 ? -1 : 1);
			%xDirection = mCos(%xyAngle) * mCos(%zAngle);
			%yDirection = mSin(%xyAngle) * mCos(%zAngle);
			%zDirection = mSin(%zAngle);
			
			new Projectile() {
				datablock = %this;
				initialPosition = %position;
				initialVelocity = vectorScale(%xDirection SPC %yDirection SPC %zDirection, vectorLen(%obj.initialVelocity));
				sourceObject = %col;
				sourceSlot = 0;
				client = %col.client;

				isReflected = true;
			};

			(new Projectile() {
				datablock = LightParryProjectile;
				initialPosition = vectorAdd(%col.getHackPosition(), vectorScale(%col.getForwardVector(), 2));
				initialVelocity = "0 0 15";
				sourceObject = 0;
				sourceSlot = 0;
			}).explode();

			(new Projectile() {
				datablock = ParryProjectile;
				initialPosition = vectorAdd(%col.getHackPosition(), vectorScale(%col.getForwardVector(), 2));
				initialVelocity = "0 0 15";
				sourceObject = 0;
				sourceSlot = 0;
			}).explode();

			serverPlay3d(ParryLightSound, %obj.getPosition());

			%col.playThread(0, "plant");

			%obj.sourceObject = 0;
		}
		
		Parent::onCollision(%this, %obj, %col, %fade, %position, %normal);
	}

	function ProjectileData::damage(%this, %obj, %col, %fade, %pos, %normal) {
		if(%col.isParrying && miniGameCanDamage(%obj, %col) == 1 && (%obj.canReflect || (%image = %col.getMountedImage(1)).parryIsShield)) {
			(new Projectile() {
				datablock = LightParryProjectile;
				initialPosition = vectorAdd(%col.getHackPosition(), vectorScale(%col.getForwardVector(), 2));
				initialVelocity = "0 0 15";
				sourceObject = 0;
				sourceSlot = 0;
			}).explode();

			(new Projectile() {
				datablock = ParryProjectile;
				initialPosition = vectorAdd(%col.getHackPosition(), vectorScale(%col.getForwardVector(), 2));
				initialVelocity = "0 0 15";
				sourceObject = 0;
				sourceSlot = 0;
			}).explode();

			serverPlay3d(ParryLightSound, %obj.getPosition());

			%col.playThread(0, "plant");

			if(%image.parryIsShield) {
				%col.shieldHP[%image] -= %this.directDamage;
			}

			return;
		}
		
		// double damage for reflected projectiles
		if(%obj.isReflected) {
			%oldDamage = %this.directDamage;
			%this.directDamage = %this.reflectDamage;
		}
		
		Parent::damage(%this, %obj, %col, %fade, %pos, %normal);

		if(%obj.isReflected) {
			%this.directDamage = %oldDamage;
		}
	}

	function ProjectileData::radiusDamage(%this, %obj, %col, %distanceFactor, %pos, %damageAmt) {
		if(%col.isParrying && miniGameCanDamage(%obj, %col) == 1 && (%obj.canReflect || (%image = %col.getMountedImage(1)).parryIsShield)) {
			if(%damageAmt * %distanceFactor > 5) {
				(new Projectile() {
					datablock = LightParryProjectile;
					initialPosition = vectorAdd(%col.getHackPosition(), vectorScale(%col.getForwardVector(), 2));
					initialVelocity = "0 0 15";
					sourceObject = 0;
					sourceSlot = 0;
				}).explode();

				(new Projectile() {
					datablock = ParryProjectile;
					initialPosition = vectorAdd(%col.getHackPosition(), vectorScale(%col.getForwardVector(), 2));
					initialVelocity = "0 0 15";
					sourceObject = 0;
					sourceSlot = 0;
				}).explode();

				serverPlay3d(ParryLightSound, %obj.getPosition());

				%col.playThread(0, "plant");
			}

			if(%image.parryIsShield) {
				%col.shieldHP[%image] -= mClampf(%damageAmt * %distanceFactor, 0, %damageAmt);
			}

			return;
		}
		
		Parent::radiusDamage(%this, %obj, %col, %distanceFactor, %pos, %damageAmt);
	}
};
activatePackage(MiniDungeonsParry);

function GameConnection::enableParry(%this) {
	commandToClient(%this, 'SetPaintingDisabled', true);
	%this.canParry = true;
}

function GameConnection::disableParry(%this) {
	commandToClient(%this, 'SetPaintingDisabled', false);
	%this.canParry = false;
}

function Player::setParry(%this, %bool, %direction, %resetSwing) {
	if(%bool) {
		%this.parryDirection = %direction;
		%this.playAudio(2, ParryStartSround);

		// if we have a shield, play the shield parry animation stuff
		if(%this.getMountedImage(1).parryIsShield) {
			%this.playThread(2, %this.getMountedImage(1).parryThread);
			%this.playThread(1, "armReadyRight");
			%this.setImageLoaded(1, true); // enable first person shield
		}
		else {
			%this.playThread(1, %this.sword[0].getDatablock().parryThread);
		}
	}
	else if(%this.isParrying) {
		%this.playThread(1, "armReadyRight");
		%this.lastParryTime = getSimTime();
		%this.parryDirection = "";

		if(%this.getMountedImage(1).parryIsShield) {
			%this.playThread(2, "armReadyLeft");
			%this.setImageLoaded(1, false); // disable first person shield
		}

		if(%resetSwing) { // instant recovery time
			%this.sword[0].getDatablock().transitionToCycleGuard(%this, 0);
		}
		else {
			%this.sword[0].getDatablock().waitForCycleGuard(%this, 0);
		}
	}
	
	%this.isParrying = %bool;
}

function Player::shieldTick(%this) {
	cancel(%this.shieldSchedule);
	
	%shield = %this.getMountedImage(1);
	if(%shield.parryIsShield) {
		if(%this.isParrying) {
			%this.shieldHP[%shield] = mClampF(%this.shieldHP[%shield] - %shield.parryShieldDecrease, 0, %shield.parryShieldHealth);
			centerPrint(%this.client, "<br><br><br>\c2Shield: " @ mFloor(%this.shieldHP[%shield]) @ " HP", 1);
		}
		else {
			%this.shieldHP[%shield] = mClampF(%this.shieldHP[%shield] + %shield.parryShieldIncrease, 0, %shield.parryShieldHealth);
		}
	}

	%this.shieldSchedule = %this.schedule(100, shieldTick);
}

datablock ExplosionData(LightParryExplosion) {
	explosionShape = "";

	lifeTimeMS = 150;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = true;
	camShakeFreq = "1 1 1";
	camShakeAmp = "1 1 2";
	camShakeDuration = 1;
	camShakeRadius = 3.0;
};

datablock ProjectileData(LightParryProjectile) {
	lifetime = 10;
	fadeDelay = 10;
	explodeondeath = true;
	explosion = LightParryExplosion;
};

datablock ExplosionData(HeavyParryExplosion) {
	explosionShape = "";

	lifeTimeMS = 150;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = true;
	camShakeFreq = "1 1 1";
	camShakeAmp = "4 4 6";
	camShakeDuration = 1;
	camShakeRadius = 3.0;
};

datablock ProjectileData(HeavyParryProjectile) {
	lifetime = 10;
	fadeDelay = 10;
	explodeondeath = true;
	explosion = HeavyParryExplosion;
};
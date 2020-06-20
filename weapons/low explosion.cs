datablock ParticleData(LowHitParticle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 0.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 166;
	lifetimeVarianceMS   = 0;
	spinSpeed       	 = 0;
	spinRandomMin        = 0;
	spinRandomMax        = 0;

	textureName		= "Add-Ons/Player_BeefBoy/textures/hit1.png";
	animTexName[0]	= "Add-Ons/Player_BeefBoy/textures/hit1.png";
	animTexName[1]	= "Add-Ons/Player_BeefBoy/textures/hit2.png";
	animTexName[2]	= "Add-Ons/Player_BeefBoy/textures/hit3.png";
	animTexName[3]	= "Add-Ons/Player_BeefBoy/textures/hit4.png";
	animTexName[4]	= "Add-Ons/Player_BeefBoy/textures/hit5.png";
	animTexName[5]	= "Add-Ons/Player_BeefBoy/textures/hit6.png";
	animTexName[6]	= "Add-Ons/Player_BeefBoy/textures/hit7.png";
	animTexName[7]	= "Add-Ons/Player_BeefBoy/textures/hit8.png";
	animTexName[8]	= "Add-Ons/Player_BeefBoy/textures/hit9.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]     = "1.0 1.0 1.0 0.7";
	colors[1]     = "1.0 1.0 1.0 0.7";
	colors[2]     = "1.0 1.0 1.0 0.7";
	colors[3]     = "1.0 1.0 1.0 0.7";

	sizes[0]      = 4.5;
	sizes[1]      = 4.5;
	sizes[2]      = 4.5;
	sizes[3]      = 4.5;

	times[0] = 0.0;
	times[1] = 0.25;
	times[2] = 0.5;
	times[3] = 0.75;

	useInvAlpha = true;
};

datablock ParticleEmitterData(LowHitEmitter) {
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

	particles = LowHitParticle;
};

datablock ExplosionData(LowHitExplosion) {
	explosionShape = "./shapes/low hit sphere.dts";
	
	particleEmitter = LowHitEmitter;
	particleDensity = 1;
	particleRadius = 0;

	emitter[0] = "";

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = false;
	camShakeFreq = "2 2 2";
	camShakeAmp = "10 10 10";
	camShakeDuration = 0;
	camShakeRadius = 0;

	lightStartRadius 	= 0;
	lightEndRadius 		= 0;
	lightStartColor 	= "1 0.03 0.03 1";
	lightEndColor 		= "0 0 0 1";

	lifetimeMS = 400;
};

datablock ProjectileData(LowHitProjectile : gunProjectile) {
	explosion = LowHitExplosion;
	uiName = "";
};
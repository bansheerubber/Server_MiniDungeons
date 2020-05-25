datablock ParticleData(HighFlareParticle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 1.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 600;
	lifetimeVarianceMS   = 300;
	spinSpeed       	 = 0;
	spinRandomMin        = 0;
	spinRandomMax        = 0;

	textureName		= "base/data/particles/dot.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]	= "1.0 1.0 1.0 0.7";
	colors[1]	= "1.0 1.0 1.0 0.7";
	colors[2]	= "1.0 1.0 1.0 0.3";
	colors[3]	= "1.0 1.0 1.0 0.0";

	sizes[0]	= 0.3;
	sizes[1]	= 0.3;
	sizes[2]	= 0.3;
	sizes[3]	= 0.3;

	times[0]	= 0.0;
	times[1]	= 0.2;
	times[2]	= 0.5;
	times[3]	= 0.6;

	useInvAlpha = false;
};

datablock ParticleEmitterData(HighFlareEmitter) {
	ejectionPeriodMS = 1;
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

	particles = HighFlareParticle;
};

datablock ShapeBaseImageData(HighFlareImage) {
	shapeFile = "base/data/shapes/empty.dts";
	emap = true;
	mountPoint = 8;
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
	stateEmitter[0]		= "HighFlareEmitter";
	stateEmitterTime[0]	= 9999;
};

datablock ExplosionData(HighHitExplosion : BeefboySwordHitExplosion) {
	explosionShape = "./shapes/high hit sphere8.dts";
	lifeTimeMS = 400;
};

datablock ProjectileData(HighHitProjectile : gunProjectile) {
	explosion = HighHitExplosion;
	uiName = "";
};

datablock ExplosionData(MidHitExplosion : BeefboySwordHitExplosion) {
	particleEmitter = "";
	emitter[0] = BeefboySwordHit2Emitter;
};

datablock ProjectileData(MidHitProjectile : gunProjectile) {
	explosion = MidHitExplosion;
	uiName = "";
};
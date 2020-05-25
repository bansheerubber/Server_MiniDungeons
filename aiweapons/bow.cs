datablock ItemData(AiBowItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/bow.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "AI Bow";
	iconName = "./";
	doColorShift = false;

	image = AiBowImage;
	canDrop = true;
};

datablock ShapeBaseImageData(AiBowImage) {
	shapeFile = "./shapes/bow.dts";
	emap = true;
	mountPoint = 1;
	offset = "0 0 0";
	eyeOffset = "0 0 0";
	rotation = eulerToMatrix("0 0 0");
	correctMuzzleVector = true;
	className = "WeaponImage";
	item = WoodShieldItem;

	melee = false;
	armReady = false;

	doColorShift = false;
	
	stateName[0]					= "Activate";
	stateSound[0]					= WeaponSwitchSound;
	stateTimeoutValue[0]			= 0.01;
	stateTransitionOnTimeout[0]		= "Idle";
	
	stateName[1]					= "Idle";
	stateTransitionOnTriggerDown[1]	= "Fire";
	stateSequence[1]				= "reload";
	
	stateName[2]					= "Fire";
	stateSequence[2]				= "noarrow";
	stateTimeoutValue[2]			= 1.0;
	stateTransitionOnTimeout[2]		= "Idle";
	stateScript[2]					= "onFire";
};

function AiBowImage::onMount(%this, %obj, %slot) {
	Parent::onMount(%this, %obj, %slot);

	%obj.playThread(1, "armReadyLeft");
}

function AiBowImage::onFire(%this, %obj, %slot) {
	%obj.playThread(0, "activate");
	%obj.playThread(2, "plant");

	if(%obj.getClassName() !$= "AiPlayer") {
		%rightVector = vectorNormalize(vectorCross(%obj.getMuzzleVector(%slot), "0 0 1"));
		new Projectile() {
			datablock = AiArrowProjectile;
			initialPosition = %obj.getMuzzlePoint(%slot);
			initialVelocity = vectorScale(%rightVector, -75);
			sourceObject = %obj;
			sourceSlot = 0;
			canReflect = true;
		};
	}
	else {
		%position = %obj.getMuzzlePoint(%slot);
		%targetPosition = %obj.target.getHackPosition();
		%speed = 45;

		%time = calculateProjectileFlightTime(%position, %targetPosition, %speed, 0.2);
		%targetPosition = %obj.target.getPredictedPosition(%time);
		
		%zAngle = calculateProjectileZAngle(%position, %targetPosition, %speed, 0.2);
		%yDifference = getWord(%targetPosition, 1) - getWord(%position, 1);
		%dot = vectorDot("1 0 0", vectorNormalize(vectorSub(getWords(%targetPosition, 0, 1), getWords(%position, 0, 1))));
		%xyAngle = mACos(%dot) * (%yDifference < 0 ? -1 : 1);
		%xDirection = mCos(%xyAngle) * mCos(%zAngle);
		%yDirection = mSin(%xyAngle) * mCos(%zAngle);
		%zDirection = mSin(%zAngle);

		new Projectile() {
			datablock = AiArrowProjectile;
			initialPosition = %position;
			initialVelocity = vectorScale(getRandomSpread(%xDirection SPC %yDirection SPC %zDirection, 0.03), %speed);
			sourceObject = %obj;
			sourceSlot = 0;
			canReflect = true;
		};

		%obj.setVelocity(%obj.getVelocity());
	}
}

datablock ParticleData(AiArrowParticle) {
	dragCoefficient      = 5;
	gravityCoefficient   = 0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 600;
	lifetimeVarianceMS   = 300;
	spinSpeed       	 = 0;
	spinRandomMin        = 0;
	spinRandomMax        = 0;

	textureName		= "base/data/particles/ring.png";
	animateTexture	= true;
	framesPerSec	= 54;

	colors[0]     = "1.0 1.0 1.0 0.3";
	colors[1]     = "1.0 1.0 1.0 0.3";
	colors[2]     = "1.0 1.0 1.0 0.1";
	colors[3]     = "1.0 1.0 1.0 0.0";

	sizes[0]      = 0.5;
	sizes[1]      = 0.2;
	sizes[2]      = 0.1;
	sizes[3]      = 0.1;

	times[0] = 0.0;
	times[1] = 0.2;
	times[2] = 0.5;
	times[3] = 0.6;

	useInvAlpha = false;
};

datablock ParticleEmitterData(AiArrowEmitter) {
	ejectionPeriodMS = 5;
	periodVarianceMS = 0;
	ejectionVelocity = 0.5;
	velocityVariance = 0;
	ejectionOffset   = 0;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;

	orientOnVelocity = false;
	orientParticles = false;

	particles = AiArrowParticle;
};

datablock ProjectileData(AiArrowProjectile) {
	projectileShapeName = "./shapes/arrow.dts";
	directDamage        = 10;
	reflectDamage		= 25;
	impactImpulse	    = 200;
	verticalImpulse	    = 200;
	explosion           = ArrowExplosion;
	particleEmitter     = AiArrowEmitter;

	muzzleVelocity      = 50;
	velInheritFactor    = 1;

	armingDelay         = 0;
	lifetime            = 20000;
	fadeDelay           = 19500;
	bounceElasticity    = 0;
	bounceFriction      = 0;
	isBallistic         = true;
	gravityMod 			= 0.2;

	hasLight    = false;
	lightRadius = 3.0;
	lightColor  = "0 0 0.5";
};
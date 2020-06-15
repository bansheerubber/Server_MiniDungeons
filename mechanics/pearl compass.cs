datablock TSShapeConstructor(MiniDungeonsPearlCompassDTS) {
	baseShape = "./shapes/pearl compass2.dts";
};

datablock PlayerData(MiniDungeonsPearlCompassArmor : PlayerStandardArmor)  {
	shapeFile = MiniDungeonsPearlCompassDTS.baseShape;
	emap = "1";
	boundingBox = "4 4 10";
	crouchBoundingBox = "4 4 10";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
};

function createPearlCompass(%position, %aimLocation) {
	(new Projectile() {
		datablock = PlayerTeleportProjectile;
		initialPosition = vectorAdd(%position, "0 0 1.4");
		initialVelocity = "0 0 10";
		sourceObject = 0;
		sourceSlot = 0;
		scale = "0.7 0.7 0.7";
	}).explode();
	
	%pearlCompass = new AiPlayer() {
		datablock = MiniDungeonsPearlCompassArmor;
		position = %position;
		rotation = "0 0 0 1";
		targetLocation = %aimLocation;
	};
	MiniDungeonsPearlCompassArmor.random(%pearlCompass, getRandom(3, 6));

	return %pearlCompass;
}

function MiniDungeonsPearlCompassArmor::random(%this, %obj, %times) {
	if(isObject(%obj)) {
		if(%times > 0) {
			%xyAngle = mDegToRad(getRandom(0, 360));
			%zAngle = mDegToRad(getRandom(0, 360));
			%x = mCos(%xyAngle) * mCos(%zAngle);
			%y = mSin(%xyAngle) * mCos(%zAngle);
			%z = mSin(%zAngle);
			%obj.setAimVector(%x SPC %y SPC %z);

			%obj.emote(WtfImage);

			%this.schedule(1000 + getRandom(-200, 700), random, %obj, %times - 1);
		}
		else {
			(new Projectile() {
				datablock = AlarmProjectile;
				initialPosition = vectorAdd(%obj.getPosition(), "0 0 1");
				initialVelocity = "0 0 10";
				sourceObject = 0;
				sourceSlot = 0;
			}).explode();
			%obj.setAimLocation(vectorAdd(%obj.targetLocation, "0 0 -5"));
		}
	}
}
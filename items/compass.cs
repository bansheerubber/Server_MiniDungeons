datablock TSShapeConstructor(MiniDungeonsCompassDTS) {
	baseShape = "./shapes/compass.dts";
};

datablock PlayerData(MiniDungeonsCompassArmor : PlayerStandardArmor)  {
	shapeFile = MiniDungeonsCompassDTS.baseShape;
	emap = "1";
	boundingBox = "5 5 5";
	crouchBoundingBox = "5 5 5";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
};

datablock ItemData(CompassItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "./shapes/compass.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Compass";
	iconName = "./";
	doColorShift = false;

	image = CompassImage;
	canDrop = true;
};

datablock ShapeBaseImageData(CompassImage) {
	shapeFile = "base/data/shapes/empty.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";
	correctMuzzleVector = true;

	className = "WeaponImage";

	item = HealthFlaskItem;
	ammo = " ";

	melee = false;
	armReady = true;

	doColorShift = false;

	interactablePassthrough = true;
};

function CompassImage::onMount(%this, %obj, %slot) {
	Parent::onMount(%this, %obj, %slot);
	%obj.showCompass();
}

function CompassImage::onUnMount(%this, %obj, %slot) {
	Parent::onUnMount(%this, %obj, %slot);
	%obj.hideCompass();
}

function Player::showCompass(%this) {
	%this.createSwordHands();
	%this.swordHands.unHideNode("rhand");
	%this.swordHands.playThread(0, "holdCompass");
	%this.playThread(1, "armReadyRight");

	if(!isObject(%this.compass)) {
		%this.compass = new AiPlayer() {
			datablock = MiniDungeonsCompassArmor;
			position = "0 0 -100";
			rotation = "0 0 0 1";
			isStaticFX = true;
		};
		%this.swordHands.mountObject(%this.compass, 0);	
	}

	%this.hideRightHand = true;
	%this.client.applyBodyParts();

	%shop = 0;
	if(isObject(%room = %this.getCurrentRoom())) {
		%shop = %room.roomFindClosestType(1);
	}
	else if(isObject(%hallway = %this.getCurrentHallway())) {
		%shop = %hallway.room1.roomFindClosestType(1);
	}

	if(isObject(%shop)) {
		%this.compassTrackPosition(%shop.worldPosition);
	}
}

function Player::hideCompass(%this) {
	if(isObject(%this.compass)) {
		%this.hideRightHand = false;
		%this.swordHands.playThread(0, "root");
		%this.compass.delete();
		%this.client.applyBodyParts();
	}
}

function Player::compassTrackPosition(%this, %position) {
	cancel(%this.compassTrackPosition);

	if(isObject(%this.compass)) {
		%direction = vectorNormalize(
			getWords(vectorSub(%position, %this.getPosition()), 0, 1)
		);

		%this.compassTrackDirection(%direction, true);

		%this.compassTrackPosition = %this.schedule(33, compassTrackPosition, %position);
	}
}

function Player::compassTrackDirection(%this, %direction, %dontSchedule) {
	cancel(%this.compassTrackDirection);
	
	if(isObject(%this.compass)) {
		%angle = mACos(vectorDot(
			%this.getForwardVector(),
			%direction
		));
		%angle2 = mACos(vectorDot(
			vectorNormalize(
				vectorCross(%this.getForwardVector(), "0 0 1")
			),
			%direction
		));

		%correctedAngle = %angle;
		if(%angle2 > $pi / 2) {
			%correctedAngle = -%angle;
		}
		
		%x = mCos(0) * mCos(%correctedAngle / 2);
		%y = mSin(0) * mCos(%correctedAngle / 2);
		%z = mSin(%correctedAngle / 2);
		%this.compass.setAimVector(%x SPC %y SPC %z);

		if(!%dontSchedule) {
			%this.compassTrackDirection = %this.schedule(33, compassTrackDirection, %direction);
		}
	}
}

function Player::compassBlind(%this) {
	cancel(%this.compassBlind);

	if(isObject(%this.compass)) {
		%randomAngle = mDegToRad(getRandom(0, 360));
		%this.compassTrackDirection(
			mCos(%randomAngle) SPC mSin(%randomAngle) SPC 0,
			true
		);
		%this.compassBlind = %this.schedule(150, compassBlind);
	}
}

deActivatePackage(MiniDungeonsCompassPackage);
package MiniDungeonsCompassPackage {
	function Armor::onRemove(%this, %obj) {
		if(isObject(%obj.compass)) {
			%obj.compass.delete();
		}
		Parent::onRemove(%this, %obj);
	}
};
activatePackage(MiniDungeonsCompassPackage);
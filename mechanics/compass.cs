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

function Player::showCompass(%this) {
	%this.swordHands.unHideNode("rhand");
	for(%i = 0; %i < 4; %i++) {
		%this.swordHands.playThread(%i, "holdCompass");	
	}
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
}

function Player::hideCompass(%this) {
	%this.hideRightHand = false;
	for(%i = 0; %i < 4; %i++) {
		%this.swordHands.playThread(%i, "root");	
	}
	%this.compass.delete();
	%this.client.applyBodyParts();
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
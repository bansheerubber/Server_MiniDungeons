datablock TSShapeConstructor(MiniDungeonsCompassDTS) {
	baseShape = "./shapes/compass.dts";
};

datablock PlayerData(MiniDungeonsCompassArmor : PlayerStandardArmor)  {
	shapeFile = MiniDungeonsCompassDTS.baseShape;
	emap = "1";
	boundingBox = "1 1 1";
	crouchBoundingBox = "1 1 1";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
};

function Player::showCompass(%this) {
	%this.swordHands.unHideNode("rhand");
	%this.swordHands.playThread(0, "holdCompass");
	%this.swordHands.playThread(1, "holdCompass");
	%this.swordHands.playThread(2, "holdCompass");
	%this.swordHands.playThread(3, "holdCompass");
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
}

function Player::compassTrack(%this, %position) {
	cancel(%this.compassTrack);

	%angle = mACos(vectorDot(
		%this.getForwardVector(),
		vectorNormalize(
			getWords(vectorSub(%position, %this.getPosition()), 0, 1)
		)
	));
	%angle2 = mACos(vectorDot(
		vectorNormalize(
			vectorCross(%this.getForwardVector(), "0 0 1")
		),
		vectorNormalize(
			getWords(vectorSub(%position, %this.getPosition()), 0, 1)
		)
	));

	%correctedAngle = %angle;
	if(%angle2 > $pi / 2) {
		%correctedAngle = -%angle;
	}
	
	%x = mCos(0) * mCos(%correctedAngle / 2);
	%y = mSin(0) * mCos(%correctedAngle / 2);
	%z = mSin(%correctedAngle / 2);
	%this.compass.setAimVector(%x SPC %y SPC %z);

	%this.compassTrack = %this.schedule(33, compassTrack, %position);
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
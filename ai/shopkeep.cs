function createShopkeepAi(%transform, %roomIndex) {
	%ai = new AiPlayer() {
		datablock = MiniDungeonsArmor;
		position = %position;
		rotation = "0 0 0 1";
		name = "Mooordus";
		onInteractCall = "onMoordusInteract";

		isBot = true;
	};

	%ai.setTransform(%transform);

	%ai.setShapeName(%ai.name, 8564862);
	%ai.setShapeNameDistance(50);

	%ai.setAvatar("shopkeep");

	%ai.shopkeepLoop();

	%ai.onSpawn(%roomIndex);

	return %ai;
}

function AiPlayer::shopkeepLoop(%this) {
	cancel(%this.shopkeepLoop);

	if(getRandom(1, 5) == 3) {
		initContainerRadiusSearch(%this.getPosition(), 15, $TypeMasks::PlayerObjectType);
		while(%col = containerSearchNext()) {
			if(%col != %this) {
				%target = %col;
			}
		}

		%this.setAimObject(%target);
	}
	else {
		%this.clearAim();
		
		%xy = mDegToRad(getRandom(0, 360));
		%z = mDegToRad(getRandom(0, 360));
		%direction = mCos(%xy) * mSin(%z)
			SPC mSin(%xy) * mSin(%z)
			SPC mCos(%z);
		%this.setAimVector(%direction);
	}
	
	%this.shopkeepLoop = %this.schedule(getRandom(2000, 3500), shopkeepLoop);
}

function MiniDungeonsArmor::onMoordusInteract(%this, %obj, %hit) {
	if(getSimTime() > %obj.nextSpeak) {
		%obj.setAimObject(%hit);

		%message[0] = "\c3Moordus\c6: gooooooooooooooooooooooooooooooooood shoppins";
		%message[1] = "\c3Moordus\c6: dooooooooooooooooooooooooooooooooooooooont catch me scammin";
		%message[2] = "\c3Moordus\c6: cooooooooooooooooooooooooooouldnt even believe my eyes and wallets";
		%message[3] = "\c3Moordus\c6: shoooooooooooooouldnt go unprepared";
		%message[4] = "\c3Moordus\c6: doooooooooooooooooooom doom doom doom doom";

		%obj.shopkeepSpeak(%message[getRandom(0, 4)]);
		%obj.nextSpeak = getSimTime() + 5000;
	}
}

function AiPlayer::shopkeepSpeak(%this, %message) {
	%this.talkForTime(strLen(%message) * 50 / 1000);

	messageClientsInArea(%this.getPosition(), 25, '', %message);
}
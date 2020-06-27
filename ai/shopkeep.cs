function createShopkeepAi(%transform, %roomIndex) {
	%ai = new AiPlayer() {
		datablock = MiniDungeonsArmor;
		position = %position;
		rotation = "0 0 0 1";
		name = "Mooordus";

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

function AiPlayer::shopkeepSpeak(%this, %message) {
	%this.playThread(0, "talk");
	%this.talkForTime(strLen(%message) * 50 / 1000);
}
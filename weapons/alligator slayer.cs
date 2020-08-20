datablock AudioProfile(AlligatorSound) {
	filename    = "./sounds/alligator.ogg";
	description = AudioClose3d;
	preload = true;
};

datablock TSShapeConstructor(AlligatorSlayerDTS) {
	baseShape = "./shapes/alligator slayer3.dts";
};

datablock PlayerData(AlligatorSlayerArmor : PlayerStandardArmor)  {
	shapeFile = AlligatorSlayerDTS.baseShape;
	uiName = "";
};

function Player::alligatorSlayerOpen(%this) {
	%this.playThread(0, "demonSlayerOpen");
	

	%this.playAudio(0, BrickChangeSound);
	%this.schedule(830, alligatorSlayerAnimation);
	%this.schedule(2100, playAudio, 0, BrickChangeSound);
}

function Player::alligatorSlayerAnimation(%this) {
	%color = "0.3 0.8 0.5 1.0";
	
	%this.alligatorSlayer = new AiPlayer() {
		datablock = AlligatorSlayerArmor;
		position = "0 0 -10";
		rotation = "0 0 0 1";
	};
	%this.alligatorSlayer.playThread(0, "emerge");
	%this.alligatorSlayer.hideNode("alligatorMouthOpen");
	%this.alligatorSlayer.setNodeColor("ALL", %color);
	%this.mountObject(%this.alligatorSlayer, 8);

	%this.alligatorSlayer.schedule(360, unHideNode, "alligatorMouthOpen");
	%this.alligatorSlayer.schedule(360, hideNode, "alligatorMouthClosed1");
	%this.alligatorSlayer.schedule(360, hideNode, "alligatorMouthClosed2");
	schedule(360, 0, serverPlay3d, AlligatorSound, %this.getPosition());
	
	%this.alligatorSlayer.schedule(800, hideNode, "alligatorMouthOpen");
	%this.alligatorSlayer.schedule(800, unHideNode, "alligatorMouthClosed1");
	%this.alligatorSlayer.schedule(800, unHideNode, "alligatorMouthClosed2");

	%this.alligatorSlayer.schedule(850, delete);
	%this.schedule(850, alligatorSlayerSpawn, %color);
}

function Player::alligatorSlayerSpawn(%this, %color) {
	%position = vectorAdd(
		vectorRelativeShift(
			%this.getAimVector(),
			"0 -5.97362 1.01701"
		),
		%this.getPosition()
	);
	
	%alligator = new AiPlayer() {
		datablock = AlligatorHoleBot;
		position = %position;
		rotation = "0 0 0 1";
		isBot = true;
	};
	%alligator.setTransform(
		%position
		SPC vectorToAxis(
			vectorScale(
				%this.getForwardVector(),
				-1
			)
		)
	);
	%alligator.setNodeColor("ALL", %color);
}
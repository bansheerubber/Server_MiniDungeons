datablock StaticShapeData(PlayerNotificationStatic) {
	shapeFile = "./shapes/notifier2.dts";
};

function Player::placeNotification(%this) {
	%eyePoint = %this.getEyePoint();
	%end = vectorAdd(%eyePoint, vectorScale(%this.getEyeVector(), 500));
	%raycast = containerRaycast(%eyePoint, %end, $TypeMasks::FxBrickObjectType | $TypeMasks::StaticObjectType, false);

	if(isObject(%raycast)) {
		%static = new StaticShape() {
			datablock = PlayerNotificationStatic;
			position = getWords(%raycast, 1, 3);
			rotation = "0 0 0 1";
		};
		%static.playThread(0, "start");
		%static.playThread(1, "spin");
		%static.setNodeColor("ALL", "1 1 0 1");
		%static.schedule(4000, delete);
	}
}
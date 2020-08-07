datablock StaticShapeData(OneWayDoorStatic) {
	shapeFile = "./shapes/one way door2.dts";
};

function createOneWayDoor(%brickDoor) {
	%static = new StaticShape() {
		datablock = OneWayDoorStatic;
		position = vectorAdd(%brickDoor.getPosition(), "0 0" SPC -getWord(%brickDoor.getObjectBox(), 5));
		rotation = getRotFromAngleID(%brickDoor.angleId);
	};
	%static.setNodeColor("door", getColorIdTable(%brickDoor.getColorId()));
	%static.setNodeColor("darker", vectorAdd(
			getColorIdTable(%brickDoor.getColorId()),
			"-0.078431 -0.078431 -0.078431"
		) SPC 1
	);
	// %static.setNetFlag(6, 1); // unghost it by default (fun fact: you spent 20 minutes during initial testing trying to figure out why the door wasn't loading only to discover that you were running the fuckin invisible command on it)

	return %static;
}
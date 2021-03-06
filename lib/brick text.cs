datablock StaticShapeData(BrickTextEmptyShape) {
	shapefile = "base/data/shapes/empty.dts";
};
function fxDtsBrick::setBrickText(%this, %message, %color, %offset) {
	if(isObject(%this.textShape)) {
		%this.textShape.delete();
	}
	if(%message $= "") {
		return;
	}
	%fixedOffset = "0 0" SPC %this.getDatablock().brickSizeZ / 9 + 0.166;
	%this.textShape = new StaticShape() {
		datablock = BrickTextEmptyShape;
		position = vectorAdd(vectorAdd(%this.getPosition(), %fixedOffset), %offset);
		scale = "0.1 0.1 0.1";
	};
	%this.textShape.setShapeName(%message);
	%this.textShape.setShapeNameColor(%color $= "" ? "1 1 1" : %color);
	%this.textShape.setShapeNameDistance(500);
}
deActivatePackage(MiniDungeonsBrickText);
package MiniDungeonsBrickText {
	function fxDTSBrick::onRemove(%this) {
		if(isObject(%this.textShape)) {
			%this.textShape.delete();
		}
		Parent::onRemove(%this);
	}
};
activatePackage(MiniDungeonsBrickText);

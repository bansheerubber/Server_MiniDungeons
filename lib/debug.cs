datablock StaticShapeData(DebugSphereStatic) {
	shapeFile = "./shapes/debugsphere.dts";
};

datablock StaticShapeData(DebugLineStatic) {
	shapeFile = "./shapes/debugline2.dts";
};

datablock StaticShapeData(DebugArrowHeadStatic) {
	shapeFile = "./shapes/debugarrowhead3.dts";
};

function drawDebugSphere(%position, %radius, %color, %time) {
	%sphere = new StaticShape() {
		datablock = DebugSphereStatic;
		position = %position;
		rotation = "0 0 0 1";
		isDebug = true;
	};
	%sphere.setScale(%radius SPC %radius SPC %radius);
	%sphere.setNodeColor("ALL", %color);

	if(%time != -1) {
		%sphere.schedule(%time, delete);
	}

	return %sphere;
}

function drawDebugLine(%start, %end, %scale, %color, %time) {
	%line = new StaticShape() {
		datablock = DebugLineStatic;
		position = "0 0 -10";
		rotation = "0 0 0 1";
		isDebug = true;
	};
	%line.setScale(%scale SPC vectorDist(%start, %end) SPC %scale);
	%euler = vectorToEuler(vectorNormalize(vectorSub(%end, %start)));
	%line.setTransform(%start SPC eulerToAxis(%euler));
	%line.setNodeColor("ALL", %color);

	if(%time != -1) {
		%line.schedule(%time, delete);
	}

	%head = new StaticShape() {
		datablock = DebugArrowHeadStatic;
		position = "0 0 -10";
		rotation = "0 0 0 1";
		isDebug = true;
	};
	%head.setTransform(%end SPC eulerToAxis(%euler));
	%head.setNodeColor("ALL", %color);
	%head.setScale(%scale SPC %scale SPC %scale);
	%line.head = %head;

	if(%time != -1) {
		%head.schedule(%time, delete);
	}

	return %line;
}

deActivatePackage(DebugLines);
package DebugLines {
	function StaticShapeData::onRemove(%this, %obj) {
		if(isObject(%obj.head)) {
			%obj.head.delete();
		}
		
		Parent::onRemove(%this, %obj);
	}
};
activatePackage(DebugLines);
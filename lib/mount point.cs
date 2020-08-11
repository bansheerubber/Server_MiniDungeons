datablock TSShapeConstructor(MountPointDTS) {
	baseShape = "./shapes/mountpoint.dts";
};

datablock PlayerData(MountPointArmor : PlayerStandardArmor)  {
	shapeFile = "./shapes/mountpoint.dts";
	emap = "1";
	boundingBox = "100 100 100";
	crouchBoundingBox = "100 100 100";
	maxDamage = 1;
	canRide = false;
	
	uiName = "";
};

function createMountPoint() {
	%mount = new AiPlayer() {
		datablock = MountPointArmor;
		position = "0 0 -100";
		rotation = "0 0 0 1";
		isStaticFX = true;
	};
	%mount.kill();
	return %mount;
}

deActivatePackage(MountPoint);
package MountPoint {
	function AiPlayer::playDeathCry(%obj) {
		cancel(%obj.aiSchedule);
		if(%obj.isStaticFX) {
			return;
		}
		Parent::playDeathCry(%obj);
	}
	
	function Player::removeBody(%obj) {
		if(%obj.isStaticFX) {
			return;
		}
		Parent::removeBody(%obj);
	}
};
activatePackage(MountPoint);
datablock fxDTSBrickData(BrickBardSpawnData) {
	brickFile = "Add-ons/Bot_Hole/4xSpawn.blb";
	category = "Special";
	subCategory = "DUNGEONS!!!!!!!!";
	uiName = "Bard Hole";
	iconName = "Add-Ons/Server_MiniDungeons/ai/shapes/bard icon";

	bricktype = 2;
	cancover = 0;
	orientationfix = 1;
	indestructable = 1;
};

function BrickBardSpawnData::onPlant(%this, %obj) {
	Parent::onPlant(%this, %obj);
	
	if(%obj.isPlanted) {
		%this.schedule(100, spawnBard, %obj);
	}
}

function BrickBardSpawnData::spawnBard(%this, %obj) {
	if(!isObject(%obj.bard)) {
		%obj.bard = spawnBard(%obj.getPosition(), "0 0" SPC 90 * %obj.angleId);
	}
}

function BrickBardSpawnData::onRemove(%this, %obj) {
	if(isObject(%obj.bard)) {
		%obj.bard.delete();
	}

	Parent::onRemove(%this, %obj);
}

datablock StaticShapeData(BeggarBowlStatic) {
	shapeFile = "./shapes/beggar bowl.dts";
	isInteractable = true;
};

function BeggarBowlStatic::setCoins(%this, %obj, %coins) {
	%obj.hideNode("ALL");
	%obj.unHideNode("bowl");
	%coins = mClamp(%coins, 0, 10);
	for(%i = 1; %i <= %coins; %i++) {
		%obj.unHideNode("coin" @ %i);
	}
	%obj.currentCoins = %coins;
}

function BeggarBowlStatic::onInteract(%this, %obj, %interactee) {
	if(%obj.price <= %interactee.client.getCurrency()) {
		if(%obj.currentCoins != 10) {
			%this.setCoins(%obj, %obj.currentCoins++);

			if(getRandom(1, 6) == 3 && getSimTime() > %obj.beggar.nextSongReset) {
				%obj.beggar.playThread(3, "talk");
				%obj.beggar.schedule(1000, playThread, 3, "root");
				
				messageClient(%interactee.client, '', "\c3" @ %obj.beggar.name @ "\c6: A new one, on the house");
				%obj.beggar.bardSetSong(%obj.beggar.bardHard, %obj.beggar.bardIndex);
				%obj.beggar.hasSpoken = true;

				%obj.beggar.nextSongReset = getSimTime() + 5000;
			}
			else if(!%obj.beggar.hasSpoken) {
				%obj.beggar.playThread(3, "talk");
				%obj.beggar.schedule(1000, playThread, 3, "root");
				
				messageClient(%interactee.client, '', "\c3" @ %obj.beggar.name @ "\c6: You are very kind sir");
				%obj.beggar.hasSpoken = true;
			}

			%interactee.client.purchase("one donation", %obj.price);
		}
	}
	else {
		%interactee.client.displayCantAfford("one donation");
	}
}

function BeggarBowlStatic::onLook(%this, %obj, %interactee) {
	if(%obj.currentCoins != 10) {
		%interactee.client.displayPrice("Each donation", %obj.price);
	}
}

$MD::BardSoftMusic[0] = "musicData_Chain_Gang";
$MD::BardSoftMusic[1] = "musicData_Turn_to_Me";
$MD::BardSoftMusic[2] = "musicData_Cant_Get_Used_To_Losing_You";
$MD::BardSoftMusic[3] = "musicData_Out_Of_Touch";
$MD::BardSoftMusic[4] = "musicData_In_The_Air_Tonight";
$MD::BardSoftMusicCount = 5;

$MD::BardHardMusic[0] = "musicData_Bat_Out_of_Hell";
$MD::BardHardMusic[1] = "musicData_Breakpoint";
$MD::BardHardMusic[2] = "musicData_The_Fateful_Return";
$MD::BardHardMusicCount = 3;

function spawnBard(%position, %euler) {
	%ai = new AiPlayer() {
		datablock = MiniDungeonsArmor;
		position = %position;
		rotation = "0 0 0 1";
		name = generateRandomName() @ ", a Bard";
	};

	%direction = eulerToVector(%euler);
	%ai.beggarsBowl = new StaticShape() {
		datablock = BeggarBowlStatic;
		position = vectorAdd(%position, vectorScale(%direction, 1.5));
		rotation = "0 0 0 1";
		beggar = %ai;
		price = getRandom(1, 10) * 10;
	};
	BeggarBowlStatic.setCoins(%ai.beggarsBowl, getRandom(0, 3));

	%ai.setTransform(%position SPC eulerToAxis(%euler));
	%ai.setAvatar("bard");
	%ai.setActionThread("sit", true);
	%ai.setShapeName(%ai.name, 8564862);
	%ai.setShapeNameDistance(50);
	%ai.bardSetSong();
	%ai.bardLoop();
	return %ai;
}

function AiPlayer::bardDefend(%this, %col) {
	if(!isObject(%this.swordsmanshipAngel)) {
		if(isObject(%col.client)) {
			messageClient(%col.client, '', "\c8Angel\c6: Do not harm the peasant-folk.");
			%this.swordsmanshipAngel = createSwordsmanShipAngel(vectorAdd(%this.getPosition(), vectorAdd(vectorScale(%this.getForwardVector(), 2), "0 0 1.5")) SPC getWords(%this.getTransform(), 3, 6));
			%this.swordsmanshipAngel.schedule(3000, delete);

			(new Projectile() {
				datablock = RocketLauncherProjectile;
				initialPosition = %this.swordsmanshipAngel.getPosition();
				initialVelocity = "0 0 10";
				sourceObject = 0;
				sourceSlot = 0;
			}).explode();

			%col.addVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%col.getPosition(), %this.getPosition())), 20), "0 0 25"));
		}
	}
}

function AiPlayer::bardLoop(%this) {
	if(%this.getState() $= "Dead") {
		return;
	}
	
	if(%this.bardHard == true) {
		%this.playThread(3, "plant");
	}
	else if(getSimTime() > %this.bardNextPlantTime) {
		%this.bardPlants = getRandom(1, 5);
		%this.bardNextPlantTime = getSimTime() + getRandom(2000, 3500);
	}

	if(%this.bardPlants > 0) {
		%this.bardPlants--;
		%this.playThread(3, "plant");
	}
	
	%this.schedule(%this.bardHard ? 100 : 200, bardLoop);
}

function AiPlayer::bardSetSong(%this, %forceHard, %except) {
	// 1/300 chance for disco duck
	if(getRandom(1, 300) == 50) {
		%this.bardMusic = "musicData_Disco_Duck";
	}
	// 5% chance for bard hard
	else if(getRandom(0, 100) < 5 || %forceHard) {
		%this.bardHard = true;
		%this.bardIndex = getRandomExcept(0, $MD::BardHardMusicCount - 1, %except);
		%this.bardMusic = $MD::BardHardMusic[%this.bardIndex];
		%this.playThread(0, "vielleSuperSpin");
	}
	else {
		%this.bardHard = false;
		%this.bardIndex = getRandomExcept(0, $MD::BardSoftMusicCount - 1, %except);
		%this.bardMusic = $MD::BardSoftMusic[%this.bardIndex];
		%this.playThread(0, "vielleSpin");
	}

	%this.playAudio(2, %this.bardMusic);
}

function getRandomExcept(%min, %max, %except) {
	if(%except $= "") {
		return getRandom(%min, %max);	
	}
	else {
		%random = getRandom(%min, %max);
		while(%random == %except) {
			%random = getRandom(%min, %max);
		}
		return %random;
	}
}

deActivatePackage(MiniDungeonsBeggar);
package MiniDungeonsBeggar {
	function Armor::onRemove(%this, %obj) {
		Parent::onRemove(%this, %obj);

		if(isObject(%obj.beggarsBowl)) {
			%obj.beggarsBowl.delete();
		}
	}

	function Player::applyImpulse(%this, %position, %vector) {
		if(isObject(%this.beggarsBowl)) {
			%this.bardDefend(%col);
			return;
		}

		Parent::applyImpulse(%this, %position, %vectoR);
	}

	function Player::damage(%this, %col, %position, %damage, %damageType) {
		if(isObject(%this.beggarsBowl)) {
			if(%damage > 5 && %this != %col && isObject(%col.client)) {
				%this.bardDefend(%col.client.player);
			}
			return;
		}
		
		Parent::damage(%this, %col, %position, %damage, %damageType);
	}
};
activatePackage(MiniDungeonsBeggar);
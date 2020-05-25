function loadFileToGlobal(%fileName, %globalVariable) {
	%file = new FileObject();
	%file.openForRead(%fileName);
	%count = 0;
	while(!%file.isEOF()) {
		%line = %file.readLine();
		eval("$MD::" @ %globalVariable @ "[" @ %count @ "] = \"" @ %line @ "\";");
		%count++;
	}
	eval("$MD::" @ %globalVariable @ "Count = " @ %count @ ";");
	%file.close();
	%file.delete();
}

loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/place.txt", Place);
loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/placeTitleModifier.txt", PlaceTitleModifier);
loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/placeTitle.txt", PlaceTitle);

loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/genericTitle.txt", GenericTitle);
loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/genericTitleModifier.txt", GenericTitleModifier);

loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/status.txt", StatusTitle);
loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/statusModifier.txt", StatusTitleModifier);

loadFileToGlobal("Add-Ons/Server_MiniDungeons/txts/names.txt", Names);



function generateRandomName(%boss, %guaranteeGeneric, %guaranteePlace, %guaranteeStatus) {
	// handle generic title generation
	%genericRandom = %guaranteeGeneric ? 0 : getRandom(0, 100);
	%genericChance = 15;
	%genericTitle = "";
	if(%genericRandom <= %genericChance) {
		%genericModifierRandom = getRandom(0, 100);
		%genericModifierChance = 50;

		if(%genericModifierRandom <= %genericModifierChance) {
			%genericTitle = "The" SPC getRandomGenericModifier() SPC getRandomGeneric();
		}
		else {
			%genericTitle = getRandomGeneric();
		}
	}

	// handle place title generation
	%placeRandom = %guaranteePlace ? 0 : getRandom(0, 100);
	%placeChance = 15;
	while(%placeRandom <= %placeChance) {
		%placeTitleModifierRandom = getRandom(0, 100);
		%placeTitleModifierChance = 15;

		if(%placeTitleModifierRandom <= %placeTitleModifierChance) {
			%tempTitle = "the" SPC getRandomPlaceModifier() SPC getRandomPlaceTitle() SPC "of" SPC getRandomPlace();
		}
		else {
			%tempTitle = getRandomPlaceTitle() SPC "of" SPC getRandomPlace();
		}

		if(%placeTitle $= "") {
			%placeTitle = ", " @ %tempTitle;
		}
		else {
			%placeTitle = %placeTitle @ "," SPC %tempTitle;
		}
		
		%placeRandom = getRandom(0, 100);
		%placeChance *= 0.7; // reduce the chances
	}

	// get another chance at a place title
	if(%placeTitle $= "") {
		%placeTitleRandom = getRandom(0, 100);
		%placeTitleChance = 30;

		if(%placeTitleRandom <= %placeTitleChance) {
			%placeTitle = " of" SPC getRandomPlace();
		}
	}

	%statusRandom = %guaranteeStatus ? 0 : getRandom(0, 100);
	%statusChance = %placeTitle $= "" ? 20 : 10;
	if(%statusRandom <= %statusChance) {
		%statusModifierRandom = getRandom(0, 100);
		%statusModifierChance = 50;

		if(%statusModifierRandom <= %statusModifierChance) {
			%statusTitle = getRandomStatusModifier() SPC getRandomStatus();
		}
		else {
			%statusTitle = getRandomStatus();
		}
	}

	%boss = %boss ? %boss : getRandom(1, 20) == 10;

	return trim(%genericTitle SPC getRandomName(%boss) @ %placeTitle @ (%statusTitle $= "" ? "" : ", a" @ (strPos("aeiouy", strLwr(getSubStr(%statusTitle, 0, 1))) != -1 ? "n " : " ")) @ %statusTitle);
}

function getRandomGenericModifier() {
	return $MD::GenericTitleModifier[getRandom(0, $MD::GenericTitleModifierCount - 1)];
}

function getRandomGeneric() {
	return $MD::GenericTitle[getRandom(0, $MD::GenericTitleCount - 1)];
}

function getRandomPlaceModifier() {
	return $MD::PlaceTitleModifier[getRandom(0, $MD::PlaceTitleModifierCount - 1)];
}

function getRandomPlaceTitle() {
	return $MD::PlaceTitle[getRandom(0, $MD::PlaceTitleCount - 1)];
}

function getRandomPlace() {
	return $MD::Place[getRandom(0, $MD::PlaceCount - 1)];
}

function getRandomStatusModifier() {
	return $MD::StatusTitleModifier[getRandom(0, $MD::StatusTitleModifierCount - 1)];
}

function getRandomStatus() {
	return $MD::StatusTitle[getRandom(0, $MD::StatusTitleCount - 1)];
}

function getRandomName(%boss) {
	%singleRandom = getRandom(0, 100);
	%singleChance = 30;
	if(%boss) {
		%middleInitialRandom = getRandom(0, 100);
		%middleInitialChance = 50;

		if(%middleInitialRandom <= %middleInitialChance) {
			%middleInitial = getSubStr("ABCDEFGHIJKLMNOPQRSTUVWXYZ", getRandom(0, 25), 1) @ ".";
			return $MD::Names[getRandom(0, $MD::NamesCount - 1)] SPC %middleInitial SPC $MD::Names[getRandom(0, $MD::NamesCount - 1)] @ strLwr($MD::Names[getRandom(0, $MD::NamesCount - 1)]);
		}
		else {
			return $MD::Names[getRandom(0, $MD::NamesCount - 1)] SPC $MD::Names[getRandom(0, $MD::NamesCount - 1)] @ strLwr($MD::Names[getRandom(0, $MD::NamesCount - 1)]);
		}
	}
	else if(%singleRandom <= %singleChance) {
		return $MD::Names[getRandom(0, $MD::NamesCount - 1)];
	}
	else {
		return $MD::Names[getRandom(0, $MD::NamesCount - 1)] @ strLwr($MD::Names[getRandom(0, $MD::NamesCount - 1)]);
	}
}

deActivatePackage(MiniDungeonsNames);
package MiniDungeonsNames {
	function Armor::onDisabled(%this, %obj) {
		%obj.setShapeName("", 8564862);
		Parent::onDisabled(%this, %obj);
	}
};
activatePackage(MiniDungeonsNames);
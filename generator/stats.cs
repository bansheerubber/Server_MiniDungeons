function addRoomDeath(%width, %height, %type, %difficulty, %index) {
	if($MD::DeathStatistic[%width, %height, %type, %difficulty, %index] $= "") {
		$MD::DeathStatistic[$MD::DeathStatisticCount | 0] = %width SPC %height SPC %type SPC %difficulty SPC %index;
		$MD::DeathStatisticCount++;
	}
	
	$MD::DeathStatistic[%width, %height, %type, %difficulty, %index]++;
}

function printRoomDeaths() {
	for(%i = 0; %i < $MD::DeathStatisticCount; %i++) {
		%string = $MD::DeathStatistic[%i];
		%width = getWord(%string, 0);
		%height = getWord(%string, 1);
		%type = getWord(%string, 2);
		%difficulty = getWord(%string, 3);
		%index = getWord(%string, 4);
		echo(%width @ "x" @ %height @ "_" @ %type @ "_" @ %difficulty @ "_" @ %index @ ":" SPC $MD::DeathStatistic[%width, %height, %type, %difficulty, %index]);
	}
}
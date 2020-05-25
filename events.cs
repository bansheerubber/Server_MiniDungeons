$MD::Tips[0] = "High guards deal more damage than medium or low guards. I daresay be wary, this be important for combos.";
$MD::Tips[1] = "Low guards disable or knock back enemies. A low guard may ye difference betwixt death and a quick escape!";
$MD::Tips[2] = "Parrying within ye correct direction gives thee bonus \c3" @ $MD::CurrencyName @ "\c6.";
$MD::Tips[3] = "Treasure awaits yon who discover all 72 paintings decorating ye Dungeon's walls.";
$MD::Tips[4] = "Within short, rings give thou wearers increased powers o' varying potential.";
$MD::Tips[5] = "Type \":3\" within ye chat to enhance thyne aim!";
$MD::TipsCount = 5;

$MD::JokeTips[0] = "I see nay reasonable rule society couldst cometh up with that wouldst forbid us from electing Dr. Eggman for President.";
$MD::JokeTipsCount = 1;


function GameConnection::getRandomTip(%this) {
	if($MD::Tips["currentTip", %this.getBLID()] >= $MD::TipsCount) {
		if(getRandom(0, 1)) {
			return $MD::Tips[getRandom(0, $MD::TipsCount - 1)];
		}
		else {
			return $MD::JokeTips[getRandom(0, $MD::JokeTipsCount - 1)];
		}
	}
	
	%tip = $MD::Tips[$MD::Tips["currentTip", %this.getBLID()] | 0];
	$MD::Tips["currentTip", %this.getBLID()]++;

	return %tip;
}

function fxDTSBrick::dungeonsTip(%this, %client) {
	if(%this.tip[%client] $= "") {
		%this.tip[%client] = %client.getRandomTip();
	}

	messageClient(%client, '', "\c1Semibeneficial Sentences Steven: \c6" @ %this.tip[%client]);
}

registerOutputEvent("fxDTSBrick", "dungeonsTip", "", true);
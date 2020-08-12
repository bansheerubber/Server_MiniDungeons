$MD::CurrencyName = "Skill";

function GameConnection::addCurrency(%this, %value) {
	$MD::Currency[%this.getBLID()] += %value;
	if($MD::Currency[%this.getBLID()] <= 0) {
		$MD::Currency[%this.getBLID()] = 0;
	}

	commandToClient(%this, 'MD_SetSkill', $MD::Currency[%this.getBLID()], %value);
}

function GameConnection::setCurrency(%this, %value) {
	$MD::Currency[%this.getBLID()] = %value;
	if($MD::Currency[%this.getBLID()] <= 0) {
		$MD::Currency[%this.getBLID()] = 0;
	}

	commandToClient(%this, 'MD_SetSkill', $MD::Currency[%this.getBLID()]);
}

function GameConnection::getCurrency(%this) {
	return $MD::Currency[%this.getBLID()];
}

function GameConnection::displayPrice(%this, %itemName, %price) {
	if(!isEventPending(%this.blank)) {
		centerPrint(%this, "<br><br>\c6" @ %itemName @ " costs \c3" @ %price SPC $MD::CurrencyName @ "\c6.", 1);
	}
}

function GameConnection::displayCantAfford(%this, %itemName) {
	cancel(%this.blank);
	%this.play2d(errorSound);
	%this.blank = %this.schedule(1000, frog);
	centerPrint(%this, "<br><br>\c0You can't afford" SPC %itemName @ ".", 1);
}

function GameConnection::purchase(%this, %itemName, %price) {
	cancel(%this.blank);
	%this.blank = %this.schedule(1000, frog);
	serverPlay3d(PurchaseSound, %this.player.getPosition());
	centerPrint(%this, "<br><br>\c6You have purchased" SPC %itemName @ ".", 1);
	%this.addCurrency(-1 * %price);
}

deActivatePackage(MiniDungeonsCurrency);
package MiniDungeonsCurrency {
	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);
		%this.setCurrency(0);
	}
};
activatePackage(MiniDungeonsCurrency);
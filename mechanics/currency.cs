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
function GameConnection::canAfford(%this, %amount) {
	return %this.getCurrency() >= %amount;
}
function GameConnection::displayPrice(%this, %itemName, %price, %description) {
	if( ! isEventPending(%this.blank)) {
		centerPrint(%this, "<br><br>\c6" @ (%itemName) @ " costs \c3" @ (%price) @ " " @ ($MD::CurrencyName) @ "\c6.<br><color:ffffff>" @ (%description) @ "", 1);
	}
}
function GameConnection::displayCantAfford(%this, %itemName) {
	cancel(%this.blank);
	%this.blank = %this.schedule(1000, frog);
	%this.play2d(errorSound);
	centerPrint(%this, "<br><br>\c0You can't afford " @ (%itemName) @ ".", 1);
}
function GameConnection::purchase(%this, %itemName, %price) {
	cancel(%this.blank);
	%this.blank = %this.schedule(1000, frog);
	serverPlay3d(PurchaseSound, %this.player.getPosition());
	centerPrint(%this, "<br><br>\c6You have purchased " @ (%itemName) @ ".", 1);
	%this.addCurrency(- 1 * %price);
}
deActivatePackage(MiniDungeonsCurrency);
package MiniDungeonsCurrency {
	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);
		%this.setCurrency(0);
	}
};
activatePackage(MiniDungeonsCurrency);

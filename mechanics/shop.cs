function BrickMiniDungeonsShopPillarData::onLook(%this, %brick, %obj) {
	if(%brick.item && (%data = %brick.item.getDatablock()) && %obj.client) {
		%obj.client.displayPrice(%data.uiName, %data.price, %data.description);
	}
}
function BrickMiniDungeonsShopPillarData::onInteract(%this, %brick, %obj) {
	if(%brick.item && (%data = %brick.item.getDatablock()) && %obj.client) {
		if( ! %obj.hasItem(%data)) {
			if(%obj.client.canAfford(%data.price)) {
				%obj.client.purchase(%data.uiName, %data.price);
				%obj.addItem(%data);
			}
			else {
				%obj.client.displayCantAfford(%data.uiName);
			}
		}
		else {
			centerPrint(%obj.client, "<br><br>\c0You already have " @ (%data.uiName) @ "! You don't need another one.");
			cancel(%obj.client.blank);
			%obj.client.blank = %obj.schedule(1000, frog);
			// prevent UIs from overriding this message
		}
	}
}
deActivatePackage(MiniDungeonsShop);
package MiniDungeonsShop {
	function fxDTSBrick::setItem(%this, %data) {
		Parent::setItem(%this, %data);
		if(%this.getDatablock().getName() $= "BrickMiniDungeonsShopPillarData") {
			if(isObject(%data)) {
				%this.setBrickText("" @ (%data.uiName) @ " - " @ (%data.price) @ " " @ ($MD::CurrencyName) @ "", %data.nameColor);
			}
			else {
				%this.setBrickText("");
			}
		}
	}
	function Armor::onCollision(%this, %obj, %col, %vec, %speed) {
		if((%col.getType() & $TypeMasks::ItemObjectType) && isObject(%col.spawnBrick) && %col.spawnBrick.getDatablock().getName() $= "BrickMiniDungeonsShopPillarData") {
			return;
		}
		else {
			Parent::onCollision(%this, %obj, %col, %vec, %speed);
		}
	}
};
activatePackage(MiniDungeonsShop);

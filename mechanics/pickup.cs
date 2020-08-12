deActivatePackage(MiniDungeonsPickup);
package MiniDungeonsPickup {
	function ItemData::onAdd(%this, %obj) {
		Parent::onAdd(%this, %obj);
		
		if(%this.isPickup) {
			%obj.rotate = true;
		}
	}

	function ItemData::onPickup(%this, %obj, %player, %amount) {
		if(%this.isPickup && isObject(%player.client)) {
			%this.completePickup(%obj, %player);

			if(%this.pickupHealth) {
				%player.addHealthFromPotion(%this.pickupHealth);
			}

			if(isObject(%this.pickupSound)) {
				serverPlay3d(%this.pickupSound, %obj.getPosition());
			}

			%obj.delete();
			return;
		}
		
		Parent::onPickup(%this, %obj, %player, %amount);
	}
};
activatePackage(MiniDungeonsPickup);

function ItemData::completePickup(%this, %obj, %player) {

}

function ItemData::spawnPickup(%this, %position) {
	%item = new Item() {
		datablock = %this;
		position = %position;
		rotation = "0 0 0 1";
		rotate = true;
	};
	%item.setVelocity("0 0 5");

	return %item;
}

function addPickupItem(%itemData) {
	if(!$MD::PickupMap[%itemData]) {
		$MD::Pickup[$MD::PickupCount | 0] = %itemData;
		$MD::PickupMap[%itemData] = true;
		$MD::PickupCount++;
	}
}

function spawnPickup(%position, %force) {
	if(getRandom(1, 3) == 2 || %force) {
		%datablock = $MD::Pickup[getRandom(0, $MD::PickupCount - 1)];
		%datablock.spawnPickup(%position);
	}
}
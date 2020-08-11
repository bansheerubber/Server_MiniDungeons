deActivatePackage(MiniDungeonsClient);
package MiniDungeonsClient {
	function GameConnection::autoAdminCheck(%this) {
		schedule(1000, 0, commandToClient, %this, 'MD_Handshake');
		
		return Parent::autoAdminCheck(%this);
	}

	function Player::damage(%this, %col, %position, %damage, %damageType) {
		Parent::damage(%this, %col, %position, %damage, %damageType);
		
		if(%this.client) {
			%this.client.updateHealth();
		}
	}

	function Player::setHealth(%this, %health) {
		Parent::setHealth(%this, %health);

		if(%this.client) {
			%this.client.updateHealth();
		}
	}

	function Player::addHealth(%this, %health) {
		Parent::addHealth(%this, %health);

		if(%this.client) {
			%this.client.updateHealth();
		}
	}

	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);

		if(!%this.buildmode) {
			%this.player.setDatablock(MiniDungeonsArmor);
		}

		%this.lastHealthbarPercent = 1;
		if(isObject(%this.player)) {
			%this.updateHealth();
			commandToClient(%this, 'MD_HandleSpawn');
		}
	}

	function Player::giveDefaultEquipment(%player) {
		if(%player.client.buildmode) {
			return Parent::giveDefaultEquipment(%player);
		}
		else {
			for(%i = 0; %i < %player.getDatablock().maxTools; %i++) {
				%player.setItem(0, %i);
			}
			
			%player.setItem(DoubleHandedItem, 0);
			return;
		}
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc) {
		%client.updateHealth(0);
		Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}
};
activatePackage(MiniDungeonsClient);

function GameConnection::updateHealth(%this, %forceHealth) {
	%health = %forceHealth $= "" ? %this.player.getHealth() : %forceHealth;
	commandToClient(%this, 'MD_SetHealthbar', %health, %this.player.getMaxHealth());
	%percent = %health / %this.player.getMaxHealth();

	if(%this.lastHealthbarPercent > %percent) {
		%delta = %this.lastHealthbarPercent - %percent;
		%shakeX = mClamp(%delta * 50, 0, 20);
		%shakeY = mFloor(%shakeX / 2);
		commandToClient(%this, 'MD_VibrateHealthbar', 12 + mFloor(%delta * 20), %shakeX SPC %shakeY);
	}

	%this.lastHealthbarPercent = %percent;
}

function serverCmdMD_Handshake_Ack(%this, %version) {
	%this.miniDungeonsClientVersion = %version;
	%this.hasClientAddOn = true;
}
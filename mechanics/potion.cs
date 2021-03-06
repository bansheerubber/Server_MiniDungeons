datablock AudioProfile(PotionGulpSound) {
	filename    = "./sounds/gulp2.ogg";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(PotionSipSound) {
	filename    = "./sounds/sip.ogg";
	description = AudioClose3d;
	preload = true;
};

function Player::showPotionTransformUI(%this, %fluid, %maxFluid) {
	if(
		isObject(%potionImage = %this.getMountedImage(0))
		&& %potionImage.isPotion
	) {
		commandToClient(
			%this.client,
			'MD_SetPotionTransferBar',
			%fluid,
			%maxFluid,
			%this.potionFluid[%potionImage, %this.currTool],
			%potionImage.maxFluid
		);
		return true;
	}
	return false;
}

function Player::refillPotion(%this, %fluid) {
	if(
		isObject(%potionImage = %this.getMountedImage(0))
		&& %potionImage.isPotion
	) {
		%refillFluid = mClamp(
			%potionImage.maxFluid - %this.potionFluid[%potionImage, %this.currTool],
			0,
			%fluid
		);

		%this.potionFluid[%potionImage, %this.currTool] += %refillFluid;

		return %refillFluid;
	}

	return -1;
}

function Player::showPotionUI(%this) {
	if(isObject(%potionImage = %this.getMountedImage(0))) {
		commandToClient(
			%this.client,
			'MD_SetPotionBar',
			%potionImage.sipAmount,
			%potionImage.gulpAmount,
			%this.potionFluid[%potionImage, %this.currTool],
			%potionImage.maxFluid
		);
	}
}

function Player::hidePotionUI(%this) {
	commandToClient(%this.client, 'MD_HidePotionBar');
}

function Player::sipPotion(%this) {
	if(
		isObject(%potionImage = %this.getMountedImage(0))
		&& %potionImage.isPotion
		&& %this.potionFluid[%potionImage, %this.currTool] >= %potionImage.sipAmount
		&& getSimTime() > %this.nextPotionDrink
	) {
		cancel(%this.potionSchedule);
		
		%potionImage.drinkPotion(%this, %potionImage.sipAmount);
		serverPlay3dTimescale(PotionSipSound, %this.getPosition(), getRandom(8, 12) / 10);
		%this.showPotionUI();

		%this.nextPotionDrink = getSimTime() + 250;

		%this.potionSchedule = %this.schedule(350, sipPotion);
	}
}

function Player::gulpPotion(%this) {
	if(
		isObject(%potionImage = %this.getMountedImage(0))
		&& %potionImage.isPotion
		&& %this.potionFluid[%potionImage, %this.currTool] >= %potionImage.gulpAmount
		&& getSimTime() > %this.nextPotionDrink
	) {
		cancel(%this.potionSchedule);
		
		%potionImage.drinkPotion(%this, %potionImage.gulpAmount);
		serverPlay3dTimescale(PotionGulpSound, %this.getPosition(), getRandom(8, 12) / 10);
		%this.showPotionUI();

		%this.nextPotionDrink = getSimTime() + 250;

		%this.potionSchedule = %this.schedule(350, gulpPotion);
	}
}

function Player::addHealthFromPotion(%this, %health) {
	%this.addHealth(%health);
	%this.setWhiteOut(mClampF(%health / 200, 0, 0.7));
}

deActivatePackage(MiniDungeonsPotion);
package MiniDungeonsPotion {
	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);
		
		if(isObject(%this.player)) {
			%this.player.hidePotionUI();
		}
	}
	
	function Armor::onTrigger(%this, %obj, %slot, %val) {
		Parent::onTrigger(%this, %obj, %slot, %val);

		if(
			isObject(%potionImage = %obj.getMountedImage(0))
			&& (
				%potionImage.isPotion
				&& !isEventPending(%obj.hideRefillPotion)
			)
		) {
			if(%val == 1) {
				if(%slot == 0) {
					%obj.sipPotion();
				}
				else if(%slot == 4) {
					%obj.gulpPotion();
				}
			}
			else if(%slot == 0 || %slot == 4) {
				cancel(%obj.potionSchedule);
			}
		}
	}
	
	function WeaponImage::onMount(%this, %obj, %slot) {
		Parent::onMount(%this, %obj, %slot);

		if(%this.isPotion) {
			if(%obj.potionFluid[%this, %obj.currTool] $= "") {
				%obj.potionFluid[%this, %obj.currTool] = %this.maxFluid;
			}
			%obj.showPotionUI();
		}
	}

	function WeaponImage::onUnMount(%this, %obj, %slot) {
		Parent::onUnMount(%this, %obj, %slot);

		if(%this.isPotion) {
			%obj.hidePotionUI();
		}
	}
};
activatePackage(MiniDungeonsPotion);
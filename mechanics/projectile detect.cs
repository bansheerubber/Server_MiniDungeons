deActivatePackage(BotProjectileDetect);
package BotProjectileDetect {
	function Projectile::onAdd(%this) {
		Parent::onAdd(%this);
		%this.projectileDetect();
	}

	function ProjectileData::onExplode(%this, %obj, %position) {
		Parent::onExplode(%this, %obj, %position);
		%obj.exploded = true;
	}
};
activatePackage(BotProjectileDetect);

function Projectile::projectileDetect(%this) {
	%start = %this.getPosition();
	%end = vectorAdd(%start, vectorScale(vectorNormalize(%this.getVelocity()), 150));
	%raycast = containerRaycast(%start, %end, $TypeMasks::fxBrickObjectType | $TypeMasks::PlayerObjectType | $TypeMasks::StaticObjectType, false);

	if(isObject(%raycast) && %raycast.getClassName() $= "AiPlayer" && !%this.botsNotified[getWord(%raycast, 0)] && minigameCanDamage(%this.sourceObject, getWord(%raycast, 0)) == 1) {
		%raycast.onProjectileWillHit(%this, vectorDist(%this.getPosition(), getWords(%raycast, 1, 3)), vectorLen(%this.getVelocity()));
		%this.botsNotified[getWord(%raycast, 0)] = true;
	}

	%this.schedule(100, projectileDetect);
}

function AiPlayer::onProjectileWillHit(%this, %projectile, %distance, %speed) {

}
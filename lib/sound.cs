function serverPlay3dTimescale(%data, %position, %timescale) {
	%old = getTimescale();
	setTimescale(%timescale);
	serverPlay3d(%data, %position);
	setTimescale(%old);
}
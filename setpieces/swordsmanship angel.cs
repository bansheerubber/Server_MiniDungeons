datablock StaticShapeData(BeefboySwordsmanshipAngelStatic) {
	shapeFile = "./shapes/swordsmanship angel.dts";
};

function createSwordsmanShipAngel(%transform) {
	%static = new StaticShape() {
		datablock = BeefboySwordsmanshipAngelStatic;
		position = %position;
		rotation = %rotation;
		scale = "1.25 1.25 1.25";
	};
	%static.setTransform(%transform);

	%static.hideNode("ALL");
	%static.unHideNode("crown");
	%static.unHideNode("crownFlare");
	%static.unHideNode("swords");
	%static.unHideNode("chest");
	%static.unHideNode("headskin");
	%static.unHideNode("larm");
	%static.unHideNode("rarm");
	%static.unHideNode("lhand");
	%static.unHideNode("rhand");
	%static.unHideNode("pants");
	%static.unHideNode("lshoe");
	%static.unHideNode("rshoe");

	%static.setNodeColor("ALL", "1 1 1 1");
	%static.setNodeColor("larm", "1 0.95 0 1");
	%static.setNodeColor("rarm", "1 0.95 0 1");
	%static.setNodeColor("chest", "1 0.95 0 1");
	%static.setNodeColor("pants", "1 0.95 0 1");

	return %static;
}
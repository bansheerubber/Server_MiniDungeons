from server import PathfindingServer

file = open("./server.info", "r")
PathfindingServer(file.readline().strip(), int(file.readline().strip()))
file.close()
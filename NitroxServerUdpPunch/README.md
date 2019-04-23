README

Build and run on Linux:

First publish file with "dotnet publish -c Release"
Then build docker file: sudo docker build -t nitrox_server_udp_punch .

To run in locally: sudo docker run -p 11001:11001/udp nitrox_server_udp_punch

OR:
Just use the "buildDocker.sh" to build the right dockerfile

Further notes: 
	"-t nitrox_server_udp_punch"   <--- gives builded image the "nitrox_server_udp_punch"-tag
	"-p 11001:11001/udp" <--- defines port mapping external:internal/protocol

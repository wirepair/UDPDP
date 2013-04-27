# UDPDP

This solution contains projects for the UDP Decrypting Proxy. It contains 4 projects, the service, a log replayer and two sample decryption dlls. One demonstrates how to modify traffic using a Managed DLL (C#), the other unmanaged (c). It is strongly recommended you log data in *json* format, so you can work with UDPDPReplay. 
Note to successfully build, you'll need to add references to projects from my HackingProjects repo, or just download the Binaries folder which should contain everything.

## Usage
Usage: UDPDP [OPTIONS]
Used proxy and log/decrypt between a UDP client & server.
Possible commands:

  -d, --dest=VALUE           destination (to fwd packets to) ip.ip.ip.ip:port.
  -s, --src=VALUE            source (to bind to) ip.ip.ip.ip:port
      --dll=VALUE            dll to be used to decrypt packets, must export
                               decrypt() and init() functions.
  -o, --output=VALUE         where to write the output to.
  -m, --modify               Allows decrypt() dll function to modify the data.
  -f, --format=VALUE         how to write the data: json|asciibin|dll
  -h, -?, --help             show this message and exit
Example:
UDPDP -s ip.ip.ip.ip:port -d ip.ip.ip.ip:port -o <outfile> -f json --dll=C:\Path\To\SampleDecryptor.dll

### Example
Terminal 1: Start the UDPDP service
UDPDP.exe -d 127.0.0.1:4455 -s 127.0.0.1:5566 -o packets.log -f json -m --dll="C:\GitHub\UDPDP\SampleManagedDecryptor\bin\Debug\SampleManagedDecryptor.dll"

This will listen on port 5566 and forward data to port 4455 logging data to packets.log in json format allowing the dll to modify traffic.

Terminal 2: Start a netcat udp server
nc -u -l -p 4455

Terminal 3: Start a netcat udp client
nc localhost -u 5566


# UDPDP Replay
This allows you to decrypt traffic from the log files of UDPDP for 'after the fact' analysis. Good when you want to test out your decryptor DLL. Only processes data that was saved in JSON format!

## Usage
Usage: UDPDPReplay [OPTIONS]
decrypt traffic of proxied data, after the fact.
Possible commands:

  -d, --dll=VALUE            dll to be used to decrypt packets, must export
                               decrypt() and init() functions.
  -i, --input=VALUE          logfile where raw data was captured.
  -f, --format=VALUE         logfile format for how the data was captured.
  -o, --output=VALUE         Where to store the decrypted output.
      --of, --outputformat=VALUE
                             Output format (json|asciibin).
  -h, -?, --help             show this message and exit
Example:
UDPDPReplay.exe -i <cap.log> -f <format> -o <outfile> --of=<asciinbin> --dll="C:\path\to\Decryptor.dll"
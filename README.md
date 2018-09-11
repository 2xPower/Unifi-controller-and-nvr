# Unifi controller and nvr integration

## Use case
I have some camera's in my home. Those camera's should not record while family members are on premise.
But when we are not at home, the camera's should start recording on motion events.

## Implementation in a nutshell
+ The client list of connected devices is retrieved from the Unifi Controller
+ Presence of familymembers is decided based on wether or not 1 or more of the configured MAC addresses is connected
+ The status for each camera is retrieved from Unif NVR
+ If the status (record on motion) for the configured camera's is not set to the expected value, the camera configuration is updated


## Example console output
![screenshot text](https://raw.githubusercontent.com/2xPower/Unifi-controller-and-nvr/master/Screenshot.PNG "Example console output")


# Deployment
## Download the build for your system

## Complete the settings
The file appsettings.json contains all the settings

```json
{
  "controller": {
    "ControllerSiteDescription": "The description of site in the controller (default is default)",
    "BaseUrl": "https://unifi.example.com",
    "UserName": "the.user", 
    "Password" :  "[write password here]",
    
  },
  "nvr": {
    "BaseUrl": "https://nvr.example.com",
    "UserName": "nvr.user",
    "Password": "[write nvr password here]"
  },
  "presence": {
    "PresenceIndicationMACs": [ "a0:2b:20:5f:08:1a" ],
    "CameraIdsToSetToMotionRecordingIfNoOneIsPresent": [ "5b7dac3ee4b014ad206c0544","get the id from nvr website"]
  }
}
```

For those who require the use of SOCKS proxy OR use an invalid (self-signed) SSL certificate:
```json
{
  "controller": {
    "ControllerSiteDescription": "The description of site in the controller (default is default)",
    "BaseUrl": "https://unifi.example.com",
    "UserName": "the.user", 
    "Password" :  "[write password here]",
    "VerifySsl" :  false,
    "SocksProxy" : "http://10.0.0.45:8887/"
    
  },
  "nvr": {
    "BaseUrl": "https://nvr.example.com",
    "UserName": "nvr.user",
    "Password": "[write nvr password here]",
    "VerifySsl" :  false,
    "SocksProxy" : "http://10.0.0.45:8887/"
  },
  "presence": {
    "PresenceIndicationMACs": [ "a0:2b:20:5f:08:1a" ],
    "CameraIdsToSetToMotionRecordingIfNoOneIsPresent": [ "5b7dac3ee4b014ad206c0544","get the id from nvr website"]
  }
}
```

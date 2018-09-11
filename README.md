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
![alt text][logo]

[logo]: https://github.com/adam-p/markdown-here/raw/master/src/common/images/icon48.png "Logo Title Text 2"

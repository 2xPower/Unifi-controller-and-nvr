# Unifi controller and nvr integration
This application is for use with products from Ubiquiti Networks. The Unifi Controller and Unifi NVR to be more precise.
The API's used to implement needed functionality are not supported for third-party developers, like myself, so they are subject to changed without notice.

## Use case
I have some camera's in my home. Those camera's should not record while family members are on premise.
But when we are not at home, the camera's should start recording motion events.

# Getting started
- [Download](https://github.com/2xPower/Unifi-controller-and-nvr/releases) the build for your system and unzip in a suitable location
- At the command line: 
  - *tup config* to set your configuration
  - *tup update* to update the camera record state.

##### Tip
I use a cronjob to schedule the application to run every 5 minutes on a Raspberry Pi 2 (linux-arm).
```crontab -e
*/5 * * * * /mnt/something/tup update
```

# other options
Run *tup --help* to get more options.
Example
- *tup show clients* shows some information about connected wireless client for the configured controller.


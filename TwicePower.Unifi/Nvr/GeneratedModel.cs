// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var bootstrap = Bootstrap.FromJson(jsonString);

namespace TwicePower.Unifi.Nvr
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class NvrApiResult<T>
    {
        [JsonProperty("data")]
        public T[] Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("serviceState")]
        public string ServiceState { get; set; }

        [JsonProperty("cloudHost")]
        public string CloudHost { get; set; }

        [JsonProperty("nvrName")]
        public string NvrName { get; set; }

        [JsonProperty("isHardwareNvr")]
        public bool IsHardwareNvr { get; set; }

        [JsonProperty("isLoggedIn")]
        public bool IsLoggedIn { get; set; }

        [JsonProperty("isFactoryDefault")]
        public bool IsFactoryDefault { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("userGroups")]
        public object[] UserGroups { get; set; }

        [JsonProperty("alertSchedules")]
        public object[] AlertSchedules { get; set; }

        [JsonProperty("cameraSchedules")]
        public object[] CameraSchedules { get; set; }

        [JsonProperty("cameras")]
        public Camera[] Cameras { get; set; }

        [JsonProperty("servers")]
        public Server[] Servers { get; set; }

        [JsonProperty("maps")]
        public Map[] Maps { get; set; }

        [JsonProperty("liveViews")]
        public object[] LiveViews { get; set; }

        [JsonProperty("settings")]
        public Settings Settings { get; set; }

        [JsonProperty("systemInfo")]
        public DatumSystemInfo SystemInfo { get; set; }

        [JsonProperty("firmwares")]
        public Firmware[] Firmwares { get; set; }

        [JsonProperty("adminUserGroupId")]
        public string AdminUserGroupId { get; set; }

        [JsonProperty("newCriticalAlerts")]
        public long NewCriticalAlerts { get; set; }
    }

    public partial class Camera
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("uptime")]
        public long Uptime { get; set; }

        [JsonProperty("firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [JsonProperty("firmwareBuild")]
        public string FirmwareBuild { get; set; }

        [JsonProperty("protocolVersion")]
        public long ProtocolVersion { get; set; }

        [JsonProperty("systemInfo")]
        public CameraSystemInfo SystemInfo { get; set; }

        [JsonProperty("mac")]
        public string Mac { get; set; }

        [JsonProperty("managed")]
        public bool Managed { get; set; }

        [JsonProperty("managedByOthers")]
        public bool ManagedByOthers { get; set; }

        [JsonProperty("provisioned")]
        public bool Provisioned { get; set; }

        [JsonProperty("unmanagementRequested")]
        public bool UnmanagementRequested { get; set; }

        [JsonProperty("lastSeen")]
        public long LastSeen { get; set; }

        [JsonProperty("internalHost")]
        public string InternalHost { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("disconnectReason")]
        public object DisconnectReason { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("managementToken")]
        public string ManagementToken { get; set; }

        [JsonProperty("controllerHostAddress")]
        public string ControllerHostAddress { get; set; }

        [JsonProperty("controllerHostPort")]
        public long ControllerHostPort { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("lastRecordingId")]
        public string LastRecordingId { get; set; }

        [JsonProperty("lastRecordingStartTime")]
        public long LastRecordingStartTime { get; set; }

        [JsonProperty("deviceSettings")]
        public DeviceSettings DeviceSettings { get; set; }

        [JsonProperty("enableSuggestedVideoSettings")]
        public bool EnableSuggestedVideoSettings { get; set; }

        [JsonProperty("micVolume")]
        public long MicVolume { get; set; }

        [JsonProperty("audioBitRate")]
        public long AudioBitRate { get; set; }

        [JsonProperty("channels")]
        public Channel[] Channels { get; set; }

        [JsonProperty("ispSettings")]
        public IspSettings IspSettings { get; set; }

        [JsonProperty("osdSettings")]
        public OsdSettings OsdSettings { get; set; }

        [JsonProperty("recordingSettings")]
        public CameraRecordingSettings RecordingSettings { get; set; }

        [JsonProperty("scheduleId")]
        public object ScheduleId { get; set; }

        [JsonProperty("zones")]
        public Zone[] Zones { get; set; }

        [JsonProperty("mapSettings")]
        public MapSettings MapSettings { get; set; }

        [JsonProperty("networkStatus")]
        public NetworkStatus NetworkStatus { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("authToken")]
        public AuthToken AuthToken { get; set; }

        [JsonProperty("certSignature")]
        public string CertSignature { get; set; }

        [JsonProperty("hasDefaultCredentials")]
        public bool HasDefaultCredentials { get; set; }

        [JsonProperty("analyticsSettings")]
        public AnalyticsSettings AnalyticsSettings { get; set; }

        [JsonProperty("enableStatusLed")]
        public bool EnableStatusLed { get; set; }

        [JsonProperty("ledFaceAlwaysOnWhenManaged")]
        public bool LedFaceAlwaysOnWhenManaged { get; set; }

        [JsonProperty("enableSpeaker")]
        public bool EnableSpeaker { get; set; }

        [JsonProperty("systemSoundsEnabled")]
        public bool SystemSoundsEnabled { get; set; }

        [JsonProperty("speakerVolume")]
        public long SpeakerVolume { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("authStatus")]
        public string AuthStatus { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class AnalyticsSettings
    {
        [JsonProperty("enableSoundAlert")]
        public bool EnableSoundAlert { get; set; }

        [JsonProperty("soundAlertVolume")]
        public long SoundAlertVolume { get; set; }

        [JsonProperty("minimumMotionSecs")]
        public long MinimumMotionSecs { get; set; }

        [JsonProperty("endMotionAfterSecs")]
        public long EndMotionAfterSecs { get; set; }
    }

    public partial class AuthToken
    {
        [JsonProperty("authToken")]
        public string AuthTokenAuthToken { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("isRtspEnabled")]
        public bool IsRtspEnabled { get; set; }

        [JsonProperty("rtspUris")]
        public string[] RtspUris { get; set; }

        [JsonProperty("isRtmpEnabled")]
        public bool IsRtmpEnabled { get; set; }

        [JsonProperty("rtmpUris")]
        public string[] RtmpUris { get; set; }

        [JsonProperty("isRtmpsEnabled")]
        public bool IsRtmpsEnabled { get; set; }

        [JsonProperty("rtmpsUris")]
        public string[] RtmpsUris { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("fps")]
        public long Fps { get; set; }

        [JsonProperty("bitrate")]
        public long Bitrate { get; set; }

        [JsonProperty("minBitrate")]
        public long MinBitrate { get; set; }

        [JsonProperty("maxBitrate")]
        public long MaxBitrate { get; set; }

        [JsonProperty("fpsValues")]
        public long[] FpsValues { get; set; }

        [JsonProperty("idrInterval")]
        public long IdrInterval { get; set; }

        [JsonProperty("isAdaptiveBitrateEnabled")]
        public bool IsAdaptiveBitrateEnabled { get; set; }
    }

    public partial class DeviceSettings
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("persists")]
        public bool Persists { get; set; }
    }

    public partial class IspSettings
    {
        [JsonProperty("brightness")]
        public long Brightness { get; set; }

        [JsonProperty("contrast")]
        public long Contrast { get; set; }

        [JsonProperty("denoise")]
        public long Denoise { get; set; }

        [JsonProperty("hue")]
        public long Hue { get; set; }

        [JsonProperty("saturation")]
        public long Saturation { get; set; }

        [JsonProperty("sharpness")]
        public long Sharpness { get; set; }

        [JsonProperty("flip")]
        public long Flip { get; set; }

        [JsonProperty("mirror")]
        public long Mirror { get; set; }

        [JsonProperty("gamma")]
        public object Gamma { get; set; }

        [JsonProperty("wdr")]
        public long Wdr { get; set; }

        [JsonProperty("aeMode")]
        public string AeMode { get; set; }

        [JsonProperty("irLedMode")]
        public string IrLedMode { get; set; }

        [JsonProperty("irLedLevel")]
        public long IrLedLevel { get; set; }

        [JsonProperty("focusMode")]
        public string FocusMode { get; set; }

        [JsonProperty("focusPosition")]
        public long FocusPosition { get; set; }

        [JsonProperty("zoomPosition")]
        public long ZoomPosition { get; set; }

        [JsonProperty("icrSensitivity")]
        public long IcrSensitivity { get; set; }

        [JsonProperty("aggressiveAntiFlicker")]
        public long AggressiveAntiFlicker { get; set; }

        [JsonProperty("enable3dnr")]
        public long Enable3Dnr { get; set; }

        [JsonProperty("dZoomStreamId")]
        public long DZoomStreamId { get; set; }

        [JsonProperty("dZoomCenterX")]
        public long DZoomCenterX { get; set; }

        [JsonProperty("dZoomCenterY")]
        public long DZoomCenterY { get; set; }

        [JsonProperty("dZoomScale")]
        public long DZoomScale { get; set; }

        [JsonProperty("lensDistortionCorrection")]
        public long LensDistortionCorrection { get; set; }

        [JsonProperty("enableExternalIr")]
        public long EnableExternalIr { get; set; }

        [JsonProperty("touchFocusX")]
        public long TouchFocusX { get; set; }

        [JsonProperty("touchFocusY")]
        public long TouchFocusY { get; set; }

        [JsonProperty("irOnValBrightness")]
        public long IrOnValBrightness { get; set; }

        [JsonProperty("irOnStsBrightness")]
        public long IrOnStsBrightness { get; set; }

        [JsonProperty("irOnValContrast")]
        public long IrOnValContrast { get; set; }

        [JsonProperty("irOnStsContrast")]
        public long IrOnStsContrast { get; set; }

        [JsonProperty("irOnValDenoise")]
        public long IrOnValDenoise { get; set; }

        [JsonProperty("irOnStsDenoise")]
        public long IrOnStsDenoise { get; set; }

        [JsonProperty("irOnValHue")]
        public long IrOnValHue { get; set; }

        [JsonProperty("irOnStsHue")]
        public long IrOnStsHue { get; set; }

        [JsonProperty("irOnValSaturation")]
        public long IrOnValSaturation { get; set; }

        [JsonProperty("irOnStsSaturation")]
        public long IrOnStsSaturation { get; set; }

        [JsonProperty("irOnValSharpness")]
        public long IrOnValSharpness { get; set; }

        [JsonProperty("irOnStsSharpness")]
        public long IrOnStsSharpness { get; set; }
    }

    public partial class MapSettings
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("mapId")]
        public object MapId { get; set; }

        [JsonProperty("angle")]
        public double Angle { get; set; }

        [JsonProperty("radius")]
        public double Radius { get; set; }

        [JsonProperty("rotation")]
        public double Rotation { get; set; }
    }

    public partial class NetworkStatus
    {
        [JsonProperty("connectionState")]
        public long ConnectionState { get; set; }

        [JsonProperty("connectionStateDescription")]
        public string ConnectionStateDescription { get; set; }

        [JsonProperty("essid")]
        public string Essid { get; set; }

        [JsonProperty("frequency")]
        public long Frequency { get; set; }

        [JsonProperty("quality")]
        public long Quality { get; set; }

        [JsonProperty("qualityMax")]
        public long QualityMax { get; set; }

        [JsonProperty("signalLevel")]
        public long SignalLevel { get; set; }

        [JsonProperty("linkSpeedMbps")]
        public long LinkSpeedMbps { get; set; }

        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
    }

    public partial class OsdSettings
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("overrideMessage")]
        public bool OverrideMessage { get; set; }

        [JsonProperty("enableDate")]
        public long EnableDate { get; set; }

        [JsonProperty("enableLogo")]
        public long EnableLogo { get; set; }

        [JsonProperty("enableStreamerStatsLevel")]
        public long EnableStreamerStatsLevel { get; set; }
    }

    public partial class CameraRecordingSettings
    {
        [JsonProperty("motionRecordEnabled")]
        public bool MotionRecordEnabled { get; set; }

        [JsonProperty("fullTimeRecordEnabled")]
        public bool FullTimeRecordEnabled { get; set; }

        [JsonProperty("channel")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Channel { get; set; }

        [JsonProperty("prePaddingSecs")]
        public long PrePaddingSecs { get; set; }

        [JsonProperty("postPaddingSecs")]
        public long PostPaddingSecs { get; set; }

        [JsonProperty("storagePath")]
        public object StoragePath { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("recordingStatus")]
        public Dictionary<string, RecordingStatus> RecordingStatus { get; set; }

        [JsonProperty("scheduledAction")]
        public object ScheduledAction { get; set; }

        [JsonProperty("remoteHost")]
        public string RemoteHost { get; set; }

        [JsonProperty("remotePort")]
        public long RemotePort { get; set; }
    }

    public partial class RecordingStatus
    {
        [JsonProperty("motionRecordingEnabled")]
        public bool MotionRecordingEnabled { get; set; }

        [JsonProperty("fullTimeRecordingEnabled")]
        public bool FullTimeRecordingEnabled { get; set; }
    }

    public partial class CameraSystemInfo
    {
        [JsonProperty("cpuName")]
        public string CpuName { get; set; }

        [JsonProperty("cpuLoad")]
        public double CpuLoad { get; set; }

        [JsonProperty("memory")]
        public Memory Memory { get; set; }

        [JsonProperty("appMemory")]
        public Memory AppMemory { get; set; }

        [JsonProperty("nics")]
        public Nic[] Nics { get; set; }

        [JsonProperty("disk")]
        public Disk Disk { get; set; }
    }

    public partial class Memory
    {
        [JsonProperty("used")]
        public long Used { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

    public partial class Disk
    {
        [JsonProperty("usedKb")]
        public long UsedKb { get; set; }

        [JsonProperty("totalKb")]
        public long TotalKb { get; set; }

        [JsonProperty("availKb")]
        public long AvailKb { get; set; }

        [JsonProperty("freeKb")]
        public long FreeKb { get; set; }

        [JsonProperty("dirName")]
        public string DirName { get; set; }

        [JsonProperty("devName")]
        public string DevName { get; set; }

        [JsonProperty("usedPercent")]
        public long UsedPercent { get; set; }
    }

    public partial class Nic
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("mac")]
        public string Mac { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("rxBps")]
        public long RxBps { get; set; }

        [JsonProperty("txBps")]
        public long TxBps { get; set; }
    }

    public partial class Zone
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sensitivity")]
        public long Sensitivity { get; set; }

        [JsonProperty("bitmap")]
        public object Bitmap { get; set; }

        [JsonProperty("coordinates")]
        public Coordinate[] Coordinates { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class Coordinate
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public partial class Firmware
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("revision")]
        public object Revision { get; set; }

        [JsonProperty("firmwareCode")]
        public string FirmwareCode { get; set; }

        [JsonProperty("protocolVersion")]
        public long ProtocolVersion { get; set; }

        [JsonProperty("url")]
        public object Url { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class Map
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mapType")]
        public string MapType { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }

        [JsonProperty("googleMapType")]
        public object GoogleMapType { get; set; }

        [JsonProperty("tilt")]
        public long Tilt { get; set; }

        [JsonProperty("zoom")]
        public long Zoom { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class Server
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("uptime")]
        public long Uptime { get; set; }

        [JsonProperty("firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [JsonProperty("firmwareBuild")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long FirmwareBuild { get; set; }

        [JsonProperty("protocolVersion")]
        public object ProtocolVersion { get; set; }

        [JsonProperty("systemInfo")]
        public CameraSystemInfo SystemInfo { get; set; }

        [JsonProperty("cloudSettings")]
        public CloudSettings CloudSettings { get; set; }

        [JsonProperty("livePortSettings")]
        public LivePortSettings LivePortSettings { get; set; }

        [JsonProperty("recordingSettings")]
        public ServerRecordingSettings RecordingSettings { get; set; }

        [JsonProperty("emsFileStats")]
        public object EmsFileStats { get; set; }

        [JsonProperty("systemSettings")]
        public SystemSettings SystemSettings { get; set; }

        [JsonProperty("emailSettings")]
        public EmailSettings EmailSettings { get; set; }

        [JsonProperty("alertSettings")]
        public AlertSettings AlertSettings { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class AlertSettings
    {
        [JsonProperty("motionEmailCoolDownMs")]
        public long MotionEmailCoolDownMs { get; set; }
    }

    public partial class CloudSettings
    {
        [JsonProperty("associated")]
        public bool Associated { get; set; }
    }

    public partial class EmailSettings
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("publicHost")]
        public string PublicHost { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public long Port { get; set; }

        [JsonProperty("useSsl")]
        public bool UseSsl { get; set; }

        [JsonProperty("requiresAuthentication")]
        public bool RequiresAuthentication { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

    public partial class LivePortSettings
    {
        [JsonProperty("rtspEnabled")]
        public bool RtspEnabled { get; set; }

        [JsonProperty("rtspPort")]
        public long RtspPort { get; set; }

        [JsonProperty("rtmpEnabled")]
        public bool RtmpEnabled { get; set; }

        [JsonProperty("rtmpPort")]
        public long RtmpPort { get; set; }

        [JsonProperty("rtmpsEnabled")]
        public bool RtmpsEnabled { get; set; }

        [JsonProperty("rtmpsPort")]
        public long RtmpsPort { get; set; }
    }

    public partial class ServerRecordingSettings
    {
        [JsonProperty("storagePath")]
        public string StoragePath { get; set; }

        [JsonProperty("tempStoragePath")]
        public object TempStoragePath { get; set; }

        [JsonProperty("mbToRetain")]
        public long MbToRetain { get; set; }

        [JsonProperty("timeToRetain")]
        public long TimeToRetain { get; set; }
    }

    public partial class SystemSettings
    {
        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("defaultLanguage")]
        public string DefaultLanguage { get; set; }

        [JsonProperty("disableUpdateCheck")]
        public bool DisableUpdateCheck { get; set; }

        [JsonProperty("disableStatsGathering")]
        public bool DisableStatsGathering { get; set; }

        [JsonProperty("disableDiscovery")]
        public bool DisableDiscovery { get; set; }

        [JsonProperty("useUpnp")]
        public bool UseUpnp { get; set; }

        [JsonProperty("disableAutoCameraUpdate")]
        public bool DisableAutoCameraUpdate { get; set; }

        [JsonProperty("googleMapsApiKey")]
        public object GoogleMapsApiKey { get; set; }

        [JsonProperty("cameraPassword")]
        public string CameraPassword { get; set; }
    }

    public partial class Settings
    {
        [JsonProperty("systemSettings")]
        public SystemSettings SystemSettings { get; set; }

        [JsonProperty("emailSettings")]
        public EmailSettings EmailSettings { get; set; }

        [JsonProperty("alertSettings")]
        public AlertSettings AlertSettings { get; set; }

        [JsonProperty("livePortSettings")]
        public LivePortSettings LivePortSettings { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class DatumSystemInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("buildType")]
        public string BuildType { get; set; }

        [JsonProperty("buildJobName")]
        public string BuildJobName { get; set; }

        [JsonProperty("buildNumber")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long BuildNumber { get; set; }

        [JsonProperty("buildBranch")]
        public string BuildBranch { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("mongoVersion")]
        public object MongoVersion { get; set; }

        [JsonProperty("protocolVersion")]
        public long ProtocolVersion { get; set; }

        [JsonProperty("httpPort")]
        public long HttpPort { get; set; }

        [JsonProperty("httpsPort")]
        public long HttpsPort { get; set; }

        [JsonProperty("liveWsPort")]
        public long LiveWsPort { get; set; }

        [JsonProperty("liveWssPort")]
        public long LiveWssPort { get; set; }

        [JsonProperty("maxExportLimit")]
        public long MaxExportLimit { get; set; }

        [JsonProperty("localAddr")]
        public object LocalAddr { get; set; }
    }

    public partial class User
    {
        [JsonProperty("account")]
        public Account Account { get; set; }

        [JsonProperty("userGroup")]
        public UserGroup UserGroup { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("mobileKey")]
        public string MobileKey { get; set; }

        [JsonProperty("loginToken")]
        public object LoginToken { get; set; }

        [JsonProperty("enableApiAccess")]
        public bool EnableApiAccess { get; set; }

        [JsonProperty("enableLocalAccess")]
        public bool EnableLocalAccess { get; set; }

        [JsonProperty("motionAlertSchedules")]
        public MotionAlertSchedules MotionAlertSchedules { get; set; }

        [JsonProperty("subscribedMotion")]
        public object SubscribedMotion { get; set; }

        [JsonProperty("subscribedServerConnection")]
        public object SubscribedServerConnection { get; set; }

        [JsonProperty("subscribedCameraConnection")]
        public string[] SubscribedCameraConnection { get; set; }

        [JsonProperty("adoptionKey")]
        public string AdoptionKey { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("superAdmin")]
        public bool SuperAdmin { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("enableEmail")]
        public bool EnableEmail { get; set; }

        [JsonProperty("enablePush")]
        public bool EnablePush { get; set; }

        [JsonProperty("sysDisconnectEmailAlert")]
        public bool SysDisconnectEmailAlert { get; set; }

        [JsonProperty("sysDisconnectPushAlert")]
        public bool SysDisconnectPushAlert { get; set; }
    }

    public partial class Account
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("lastIp")]
        public string LastIp { get; set; }

        [JsonProperty("lastLogInTimestamp")]
        public long LastLogInTimestamp { get; set; }

        [JsonProperty("ssoId")]
        public Guid SsoId { get; set; }

        [JsonProperty("inviteId")]
        public object InviteId { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class MotionAlertSchedules
    {
    }

    public partial class UserGroup
    {
        [JsonProperty("groupType")]
        public string GroupType { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }

        [JsonProperty("filteredCount")]
        public long FilteredCount { get; set; }
    }

    public enum Name { Video1, Video2, Video3 };

    public partial class Bootstrap
    {
        public static Bootstrap FromJson(string json) => JsonConvert.DeserializeObject<Bootstrap>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Bootstrap self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                NameConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class NameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "video1":
                    return Name.Video1;
                case "video2":
                    return Name.Video2;
                case "video3":
                    return Name.Video3;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            switch (value)
            {
                case Name.Video1:
                    serializer.Serialize(writer, "video1");
                    return;
                case Name.Video2:
                    serializer.Serialize(writer, "video2");
                    return;
                case Name.Video3:
                    serializer.Serialize(writer, "video3");
                    return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }
}

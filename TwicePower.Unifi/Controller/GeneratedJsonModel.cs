using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace TwicePower.Unifi.Controller
{
    // To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
    //
    //    using QuickType.io generation tool;
    //
    //    var welcome = Welcome.FromJson(jsonString);

    public class UnifiApiResult<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        public static UnifiApiResult<T> FromJson(string json) => JsonConvert.DeserializeObject<UnifiApiResult<T>>(json, Converter.Settings);
        public string ToJson() => JsonConvert.SerializeObject(this, Converter.Settings);

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } },
            };
        }
    }
    public partial class Site
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("attr_hidden_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AttrHiddenId { get; set; }

        [JsonProperty("attr_no_delete", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AttrNoDelete { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("rc")]
        public string Rc { get; set; }
    }
    internal class DecodingChoiceConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return integerValue;
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    long l;
                    if (Int64.TryParse(stringValue, out l))
                    {
                        return l;
                    }
                    break;
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
            serializer.Serialize(writer, value);
            return;
        }

        public static readonly DecodingChoiceConverter Singleton = new DecodingChoiceConverter();
    }

    public partial class Sta
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_is_guest_by_ugw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGuestByUgw { get; set; }

        [JsonProperty("_is_guest_by_usw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGuestByUsw { get; set; }

        [JsonProperty("_last_seen_by_ugw", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastSeenByUgw { get; set; }

        [JsonProperty("_last_seen_by_usw", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastSeenByUsw { get; set; }

        [JsonProperty("_uptime_by_ugw", NullValueHandling = NullValueHandling.Ignore)]
        public long? UptimeByUgw { get; set; }

        [JsonProperty("_uptime_by_usw", NullValueHandling = NullValueHandling.Ignore)]
        public long? UptimeByUsw { get; set; }

        [JsonProperty("assoc_time")]
        public long AssocTime { get; set; }

        [JsonProperty("bytes-r", NullValueHandling = NullValueHandling.Ignore)]
        public long? BytesR { get; set; }

        [JsonProperty("first_seen")]
        public long FirstSeen { get; set; }

        [JsonProperty("fixed_ip", NullValueHandling = NullValueHandling.Ignore)]
        public string FixedIp { get; set; }

        [JsonProperty("gw_mac", NullValueHandling = NullValueHandling.Ignore)]
        public string GwMac { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("is_guest")]
        public bool IsGuest { get; set; }

        [JsonProperty("is_wired")]
        public bool IsWired { get; set; }

        [JsonProperty("last_seen")]
        public long LastSeen { get; set; }

        [JsonProperty("latest_assoc_time")]
        public long LatestAssocTime { get; set; }

        [JsonProperty("mac")]
        public string Mac { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("network")]
        public string Network { get; set; }

        [JsonProperty("network_id")]
        public string NetworkId { get; set; }

        [JsonProperty("noted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Noted { get; set; }

        [JsonProperty("oui")]
        public string Oui { get; set; }

        [JsonProperty("qos_policy_applied", NullValueHandling = NullValueHandling.Ignore)]
        public bool? QosPolicyApplied { get; set; }

        [JsonProperty("rx_bytes", NullValueHandling = NullValueHandling.Ignore)]
        public long? RxBytes { get; set; }

        [JsonProperty("rx_bytes-r", NullValueHandling = NullValueHandling.Ignore)]
        public long? RxBytesR { get; set; }

        [JsonProperty("rx_packets", NullValueHandling = NullValueHandling.Ignore)]
        public long? RxPackets { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("sw_depth", NullValueHandling = NullValueHandling.Ignore)]
        public long? SwDepth { get; set; }

        [JsonProperty("sw_mac", NullValueHandling = NullValueHandling.Ignore)]
        public string SwMac { get; set; }

        [JsonProperty("sw_port", NullValueHandling = NullValueHandling.Ignore)]
        public long? SwPort { get; set; }

        [JsonProperty("tx_bytes", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxBytes { get; set; }

        [JsonProperty("tx_bytes-r", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxBytesR { get; set; }

        [JsonProperty("tx_packets", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxPackets { get; set; }

        [JsonProperty("uptime")]
        public long Uptime { get; set; }

        [JsonProperty("use_fixedip", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseFixedip { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("usergroup_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UsergroupId { get; set; }

        [JsonProperty("wired-rx_bytes", NullValueHandling = NullValueHandling.Ignore)]
        public long? WiredRxBytes { get; set; }

        [JsonProperty("wired-rx_bytes-r", NullValueHandling = NullValueHandling.Ignore)]
        public long? WiredRxBytesR { get; set; }

        [JsonProperty("wired-rx_packets", NullValueHandling = NullValueHandling.Ignore)]
        public long? WiredRxPackets { get; set; }

        [JsonProperty("wired-tx_bytes", NullValueHandling = NullValueHandling.Ignore)]
        public long? WiredTxBytes { get; set; }

        [JsonProperty("wired-tx_bytes-r", NullValueHandling = NullValueHandling.Ignore)]
        public long? WiredTxBytesR { get; set; }

        [JsonProperty("wired-tx_packets", NullValueHandling = NullValueHandling.Ignore)]
        public long? WiredTxPackets { get; set; }

        [JsonProperty("hostname", NullValueHandling = NullValueHandling.Ignore)]
        public string Hostname { get; set; }

        [JsonProperty("_is_guest_by_uap", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGuestByUap { get; set; }

        [JsonProperty("_last_seen_by_uap", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastSeenByUap { get; set; }

        [JsonProperty("_uptime_by_uap", NullValueHandling = NullValueHandling.Ignore)]
        public long? UptimeByUap { get; set; }

        [JsonProperty("ap_mac", NullValueHandling = NullValueHandling.Ignore)]
        public string ApMac { get; set; }

        [JsonProperty("bssid", NullValueHandling = NullValueHandling.Ignore)]
        public string Bssid { get; set; }

        [JsonProperty("ccq", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ccq { get; set; }

        [JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
        public long? Channel { get; set; }

        [JsonProperty("dhcpend_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? DhcpendTime { get; set; }

        [JsonProperty("essid", NullValueHandling = NullValueHandling.Ignore)]
        public string Essid { get; set; }

        [JsonProperty("idletime", NullValueHandling = NullValueHandling.Ignore)]
        public long? Idletime { get; set; }

        [JsonProperty("is_11r", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Is11R { get; set; }

        [JsonProperty("noise", NullValueHandling = NullValueHandling.Ignore)]
        public long? Noise { get; set; }

        [JsonProperty("powersave_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PowersaveEnabled { get; set; }

        [JsonProperty("radio", NullValueHandling = NullValueHandling.Ignore)]
        public string Radio { get; set; }

        [JsonProperty("radio_name", NullValueHandling = NullValueHandling.Ignore)]
        public string RadioName { get; set; }

        [JsonProperty("radio_proto", NullValueHandling = NullValueHandling.Ignore)]
        public string RadioProto { get; set; }

        [JsonProperty("rssi", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rssi { get; set; }

        [JsonProperty("rx_rate", NullValueHandling = NullValueHandling.Ignore)]
        public long? RxRate { get; set; }

        [JsonProperty("satisfaction", NullValueHandling = NullValueHandling.Ignore)]
        public long? Satisfaction { get; set; }

        [JsonProperty("signal", NullValueHandling = NullValueHandling.Ignore)]
        public long? Signal { get; set; }

        [JsonProperty("tx_power", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxPower { get; set; }

        [JsonProperty("tx_rate", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxRate { get; set; }

        [JsonProperty("vlan", NullValueHandling = NullValueHandling.Ignore)]
        public long? Vlan { get; set; }

        [JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
        public string Note { get; set; }
    }

    public partial class SysInfo
    {
        [JsonProperty("autobackup")]
        public bool Autobackup { get; set; }

        [JsonProperty("build")]
        public string Build { get; set; }

        [JsonProperty("data_retention_days")]
        public long DataRetentionDays { get; set; }

        [JsonProperty("data_retention_time_in_hours_for_5minutes_scale")]
        public long DataRetentionTimeInHoursFor5MinutesScale { get; set; }

        [JsonProperty("data_retention_time_in_hours_for_daily_scale")]
        public long DataRetentionTimeInHoursForDailyScale { get; set; }

        [JsonProperty("data_retention_time_in_hours_for_hourly_scale")]
        public long DataRetentionTimeInHoursForHourlyScale { get; set; }

        [JsonProperty("data_retention_time_in_hours_for_monthly_scale")]
        public long DataRetentionTimeInHoursForMonthlyScale { get; set; }

        [JsonProperty("data_retention_time_in_hours_for_others")]
        public long DataRetentionTimeInHoursForOthers { get; set; }

        [JsonProperty("debug_device")]
        public string DebugDevice { get; set; }

        [JsonProperty("debug_mgmt")]
        public string DebugMgmt { get; set; }

        [JsonProperty("debug_sdn")]
        public string DebugSdn { get; set; }

        [JsonProperty("debug_system")]
        public string DebugSystem { get; set; }

        [JsonProperty("default_site_device_auth_password_alert")]
        public bool DefaultSiteDeviceAuthPasswordAlert { get; set; }

        [JsonProperty("facebook_wifi_registered")]
        public bool FacebookWifiRegistered { get; set; }

        [JsonProperty("google_maps_api_key")]
        public string GoogleMapsApiKey { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("image_maps_use_google_engine")]
        public bool ImageMapsUseGoogleEngine { get; set; }

        [JsonProperty("inform_port")]
        public long InformPort { get; set; }

        [JsonProperty("ip_addrs")]
        public string[] IpAddrs { get; set; }

        [JsonProperty("live_chat")]
        public string LiveChat { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("override_inform_host")]
        public bool OverrideInformHost { get; set; }

        [JsonProperty("radius_disconnect_running")]
        public bool RadiusDisconnectRunning { get; set; }

        [JsonProperty("store_enabled")]
        public string StoreEnabled { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("unifi_go_enabled")]
        public bool UnifiGoEnabled { get; set; }

        [JsonProperty("update_available")]
        public bool UpdateAvailable { get; set; }

        [JsonProperty("update_downloaded")]
        public bool UpdateDownloaded { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class UserGroup
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("attr_hidden_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AttrHiddenId { get; set; }

        [JsonProperty("attr_no_delete", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AttrNoDelete { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("qos_rate_max_down")]
        [JsonConverter(typeof(DecodingChoiceConverter))]
        public long QosRateMaxDown { get; set; }

        [JsonProperty("qos_rate_max_up")]
        [JsonConverter(typeof(DecodingChoiceConverter))]
        public long QosRateMaxUp { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }
    }

}

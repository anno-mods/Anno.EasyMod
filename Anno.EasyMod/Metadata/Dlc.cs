using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Anno.EasyMod.Metadata
{
    public enum DlcId
    {
        [EnumMember] Anarchist = 4100010,
        [EnumMember] SunkenTreasures = 410040,
        [EnumMember] Botanica = 410041,
        [EnumMember] ThePassage = 410042,
        [EnumMember] SeatOfPower = 410059,
        [EnumMember] BrightHarvest = 410070,
        [EnumMember] LandOfLions = 410071,
        [EnumMember] Docklands = 410083,
        [EnumMember] Tourism = 410084,
        [EnumMember] Highlife = 410085,
        [EnumMember] SeedsOfChange = 24961,
        [EnumMember] EmpireOfTheSkies = 24962,
        [EnumMember] NewWorldRising = 24963,

        //Cosmetic DLCs
        [EnumMember] Christmas = 116630,
        [EnumMember] AmusementPark = 410079,
        [EnumMember] CityLife = 410081,
        [EnumMember] VehicleSkins = 319,
        [EnumMember] VibrantCity = 522,
        [EnumMember] PedestrianZone = 410100,
        [EnumMember] SeasonalDecorations = 25149,
        [EnumMember] IndustryOrnaments = 24964,
        [EnumMember] OldTown = 24965,
        [EnumMember] DragonGarden = 10114,
        [EnumMember] Fiesta = 21049
    }

    public static class DlcIdExtensions
    {
        public static int GetGuid(this DlcId id) => (int)id;
        public static DlcId FromGuid(int guid) => (DlcId)guid;
    }

    public enum DlcRequirement
    {
        [EnumMember] required,
        [EnumMember] partly,
        [EnumMember] atLeastOneRequired
    }

    public class Dlc
    {
        public Dlc() { }

        [JsonConverter(typeof(StringEnumConverter))]
        public DlcId? DLC { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DlcRequirement? Dependant { get; set; }
    }
}

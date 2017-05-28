﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace PKHeX.Core
{
    public static class GameInfo
    {
        private static readonly string[] ptransp = { "ポケシフター", "Poké Transfer", "Poké Fret", "Pokétrasporto", "Poképorter", "Pokétransfer", "포케시프터", "宝可传送", "寶可傳送", "ポケシフター" };
        private static readonly string[] lang_val = { "ja", "en", "fr", "it", "de", "es", "ko", "zh", "zh2", "pt" };
        private const string DefaultLanguage = "en";
        public static string CurrentLanguage { get; set; } = DefaultLanguage;
        public static int Language(string lang = null) => Array.IndexOf(lang_val, lang ?? CurrentLanguage);
        public static string Language2Char(uint lang) => lang > lang_val.Length ? DefaultLanguage : lang_val[lang];
        private static readonly GameStrings[] Languages = new GameStrings[lang_val.Length];

        // Lazy fetch implementation
        private static int DefaultLanguageIndex => Array.IndexOf(lang_val, DefaultLanguage);
        private static int getLanguageIndex(string lang)
        {
            int l = Array.IndexOf(lang_val, lang);
            return l < 0 ? DefaultLanguageIndex : l;
        }
        public static GameStrings getStrings(string lang)
        {
            int index = getLanguageIndex(lang);
            return Languages[index] ?? (Languages[index] = new GameStrings(lang_val[index]));
        }
        private static string getTransporterName(string lang)
        {
            int index = getLanguageIndex(lang);
            if (index >= ptransp.Length)
                index = DefaultLanguageIndex;
            return ptransp[index];
        }

        // String providing
        public class GameStrings
        {
            // PKM Info
            public readonly string[] specieslist, movelist, itemlist, abilitylist, types, natures, forms,
                memories, genloc, trainingbags, trainingstage, characteristics,
                encountertypelist, gamelanguages, balllist, gamelist, pokeblocks,
                g3coloitems, g3xditems, g3items, g2items, g1items;

            // Met Locations
            public readonly string[] metGSC_00000, metRSEFRLG_00000, metCXD_00000;
            public readonly string[] metHGSS_00000, metHGSS_02000, metHGSS_03000;
            public readonly string[] metBW2_00000, metBW2_30000, metBW2_40000, metBW2_60000;
            public readonly string[] metXY_00000, metXY_30000, metXY_40000, metXY_60000;
            public readonly string[] metSM_00000, metSM_30000, metSM_40000, metSM_60000;

            // Misc
            public readonly string[] wallpapernames, puffs;
            public readonly string eggname;
            private readonly string Language;

            public GameStrings(string l)
            {
                Language = l;
                // Past Generation strings
                g3items = get("ItemsG3");
                // XD and Colosseum
                {
                    g3coloitems = (string[])g3items.Clone();
                    string[] tmp = get("ItemsG3Colosseum");
                    Array.Resize(ref g3coloitems, 500 + tmp.Length);
                    for (int i = g3items.Length; i < g3coloitems.Length; i++)
                        g3coloitems[i] = $"UNUSED {i}";
                    Array.Copy(tmp, 0, g3coloitems, g3coloitems.Length - tmp.Length, tmp.Length);

                    g3xditems = (string[])g3items.Clone();
                    string[] tmp2 = get("ItemsG3XD");
                    Array.Resize(ref g3xditems, 500 + tmp2.Length);
                    for (int i = g3items.Length; i < g3xditems.Length; i++)
                        g3xditems[i] = $"UNUSED {i}";
                    Array.Copy(tmp2, 0, g3xditems, g3xditems.Length - tmp2.Length, tmp2.Length);
                }
                g2items = get("ItemsG2");
                g1items = get("ItemsG1");
                metRSEFRLG_00000 = get("rsefrlg_00000");
                metGSC_00000 = get("gsc_00000");

                metCXD_00000 = get("cxd_00000");
                // Sanitize a little
                var metSanitize = (string[])metCXD_00000.Clone();
                for (int i = 0; i < metSanitize.Length; i++)
                    if (metCXD_00000.Count(r => r == metSanitize[i]) > 1)
                        metSanitize[i] += $" [{i:000}]";
                metCXD_00000 = metSanitize;

                // Current Generation strings
                natures = Util.getNaturesList(l);
                types = get("types");
                abilitylist = get("abilities");

                movelist = get("moves");
                string[] ps = {"P", "S"}; // Distinguish Physical/Special
                for (int i = 622; i < 658; i++)
                    movelist[i] += $" ({ps[i%2]})";

                itemlist = get("items");
                characteristics = get("character");
                specieslist = get("species");
                wallpapernames = get("wallpaper");
                encountertypelist = get("encountertype");
                gamelist = get("games");
                gamelanguages = Util.getNulledStringArray(Util.getStringList("languages"));

                balllist = new string[Legal.Items_Ball.Length];
                for (int i = 0; i < balllist.Length; i++)
                    balllist[i] = itemlist[Legal.Items_Ball[i]];

                pokeblocks = get("pokeblock");
                forms = get("forms");
                memories = get("memories");
                genloc = get("genloc");
                trainingbags = get("trainingbag");
                trainingstage = get("supertraining");
                puffs = get("puff");

                eggname = specieslist[0];
                metHGSS_00000 = get("hgss_00000");
                metHGSS_02000 = get("hgss_02000");
                metHGSS_03000 = get("hgss_03000");
                metBW2_00000 = get("bw2_00000");
                metBW2_30000 = get("bw2_30000");
                metBW2_40000 = get("bw2_40000");
                metBW2_60000 = get("bw2_60000");
                metXY_00000 = get("xy_00000");
                metXY_30000 = get("xy_30000");
                metXY_40000 = get("xy_40000");
                metXY_60000 = get("xy_60000");
                metSM_00000 = get("sm_00000");
                metSM_30000 = get("sm_30000");
                metSM_40000 = get("sm_40000");
                metSM_60000 = get("sm_60000");

                Sanitize();
            }

            private void Sanitize()
            {
                // Gen4 Mail names not stored in future games. No clever solution like for HM's, so improvise.
                for (int i = 137; i <= 148; i++)
                    itemlist[i] = $"Mail #{i - 137 + 1} (G4)";

                // Fix Item Names (Duplicate entries)
                int len = itemlist[425].Length;
                itemlist[426] = itemlist[425].Substring(0, len - 1) + (char)(itemlist[425][len - 1] + 1) + " (G4)";
                itemlist[427] = itemlist[425].Substring(0, len - 1) + (char)(itemlist[425][len - 1] + 2) + " (G4)";
                itemlist[456] += " (HG/SS)"; // S.S. Ticket
                itemlist[736] += " (OR/AS)"; // S.S. Ticket
                itemlist[463] += " (DPPt)"; // Storage Key
                itemlist[734] += " (OR/AS)"; // Storage Key
                itemlist[478] += " (HG/SS)"; // Basement Key
                itemlist[478] += " (OR/AS)"; // Basement Key
                itemlist[621] += " (M)"; // Xtransceiver
                itemlist[626] += " (F)"; // Xtransceiver
                itemlist[629] += " (2)"; // DNA Splicers
                itemlist[637] += " (2)"; // Dropped Item
                itemlist[707] += " (2)"; // Travel Trunk
                itemlist[713] += " (2)"; // Alt Bike
                itemlist[714] += " (2)"; // Holo Caster
                itemlist[729] += " (1)"; // Meteorite
                itemlist[740] += " (2)"; // Contest Costume
                itemlist[751] += " (2)"; // Meteorite
                itemlist[771] += " (3)"; // Meteorite
                itemlist[772] += " (4)"; // Meteorite
                itemlist[842] += " (SM)"; // Fishing Rod

                // Append Z-Crystal flagging
                foreach (var i in Legal.Pouch_ZCrystal_SM)
                    itemlist[i] += " [Z]";

                // Replace the Egg Name with ---; egg name already stored to eggname
                specieslist[0] = "---";

                // Get the met locations... for all of the games...

                // Fix up some of the Location strings to make them more descriptive:
                metHGSS_02000[1] += " (NPC)";         // Anything from an NPC
                metHGSS_02000[2] += " (" + eggname + ")"; // Egg From Link Trade
                metBW2_00000[36] = metBW2_00000[84] + "/" + metBW2_00000[36]; // Cold Storage in BW = PWT in BW2
                metBW2_00000[40] += "(B/W)"; // Victory Road in BW 
                metBW2_00000[134] += "(B2/W2)"; // Victory Road in B2W2

                // BW2 Entries from 76 to 105 are for Entralink in BW
                for (int i = 76; i < 106; i++)
                    metBW2_00000[i] = metBW2_00000[i] + "●";

                // Localize the Poketransfer to the language (30001)
                metBW2_30000[1 - 1] = getTransporterName(Language); // Default to English
                metBW2_30000[2 - 1] += " (NPC)";            // Anything from an NPC
                metBW2_30000[3 - 1] += $" ({eggname})";     // Link Trade (Egg)

                // Zorua/Zoroark events
                metBW2_30000[10 - 1] = specieslist[251] + " (" + specieslist[570] + " 1)"; // Celebi's Zorua Event
                metBW2_30000[11 - 1] = specieslist[251] + " (" + specieslist[570] + " 2)"; // Celebi's Zorua Event
                metBW2_30000[12 - 1] = specieslist[571] + " (" + "1)"; // Zoroark
                metBW2_30000[13 - 1] = specieslist[571] + " (" + "2)"; // Zoroark

                metBW2_60000[3 - 1] += " (" + eggname + ")";  // Egg Treasure Hunter/Breeder, whatever...

                metXY_00000[104] += " (X/Y)";              // Victory Road
                metXY_00000[106] += " (X/Y)";              // Pokémon League
                metXY_00000[202] += " (OR/AS)";            // Pokémon League
                metXY_00000[298] += " (OR/AS)";            // Victory Road
                metXY_30000[0] += " (NPC)";                // Anything from an NPC
                metXY_30000[1] += " (" + eggname + ")";    // Egg From Link Trade

                // Sun/Moon duplicates -- elaborate!
                var metSM_00000_good = (string[])metSM_00000.Clone();
                for (int i = 0; i < metSM_00000.Length; i += 2)
                {
                    var nextLoc = metSM_00000[i + 1];
                    if (!string.IsNullOrWhiteSpace(nextLoc) && nextLoc[0] != '[')
                        metSM_00000_good[i] += $" ({nextLoc})";
                    if (i > 0 && !string.IsNullOrWhiteSpace(metSM_00000_good[i]) && metSM_00000_good.Take(i - 1).Contains(metSM_00000_good[i]))
                        metSM_00000_good[i] += $" ({metSM_00000_good.Take(i - 1).Count(s => s == metSM_00000_good[i]) + 1})";
                }
                metSM_00000_good.CopyTo(metSM_00000, 0);

                metSM_30000[0] += " (NPC)";                // Anything from an NPC
                metSM_30000[1] += " (" + eggname + ")";    // Egg From Link Trade
                for (int i = 2; i <= 5; i++) // distinguish first set of regions (unused) from second (used)
                    metSM_30000[i] += " (-)";

                // Set the first entry of a met location to "" (nothing)
                // Fix (None) tags
                abilitylist[0] = itemlist[0] = movelist[0] = metXY_00000[0] = metBW2_00000[0] = metHGSS_00000[0] = "(" + itemlist[0] + ")";
            }
            private string[] get(string ident)
            {
                string[] data = Util.getStringList(ident, Language);
                if (data == null || data.Length == 0)
                    data = Util.getStringList(ident, DefaultLanguage);

                return data;
            }

            public string[] getItemStrings(int generation, GameVersion game)
            {
                switch (generation)
                {
                    case 0:
                        return new string[0];
                    case 1:
                        return g1items;
                    case 2:
                        return g2items;
                    case 3:
                        switch (game)
                        {
                            case GameVersion.COLO:
                                return g3coloitems;
                            case GameVersion.XD:
                                return g3xditems;
                            default:
                                if (Legal.EReaderBerryIsEnigma)
                                    return g3items;

                                var g3itemsEBerry = (string[])g3items.Clone();
                                g3itemsEBerry[175] = Legal.EReaderBerryDisplayName;
                                return g3itemsEBerry;
                        }
                    default:
                        return itemlist;
                }
            }
        }
        public static GameStrings Strings;

        // DataSource providing
        public static List<ComboItem> ItemDataSource, SpeciesDataSource, BallDataSource, NatureDataSource, AbilityDataSource, VersionDataSource;
        public static List<ComboItem> LegalMoveDataSource, HaXMoveDataSource, MoveDataSource;
        private static List<ComboItem> metGen2, metGen3, metGen3CXD, metGen4, metGen5, metGen6, metGen7;

        public static void InitializeDataSources(GameStrings s)
        {
            int[] ball_nums = { 007, 576, 013, 492, 497, 014, 495, 493, 496, 494, 011, 498, 008, 006, 012, 015, 009, 005, 499, 010, 001, 016, 851 };
            int[] ball_vals = { 007, 025, 013, 017, 022, 014, 020, 018, 021, 019, 011, 023, 008, 006, 012, 015, 009, 005, 024, 010, 001, 016, 026 };
            BallDataSource = Util.getVariedCBList(s.itemlist, ball_nums, ball_vals);
            SpeciesDataSource = Util.getCBList(s.specieslist, null);
            NatureDataSource = Util.getCBList(s.natures, null);
            AbilityDataSource = Util.getCBList(s.abilitylist, null);
            VersionDataSource = Util.getCBList(s.gamelist, Legal.Games_7sm, Legal.Games_6oras, Legal.Games_6xy, Legal.Games_5, Legal.Games_4, Legal.Games_4e, Legal.Games_4r, Legal.Games_3, Legal.Games_3e, Legal.Games_3r, Legal.Games_3s);
            VersionDataSource.AddRange(Util.getCBList(s.gamelist, Legal.Games_7vc1).OrderBy(g => g.Value)); // stuff to end unsorted
            VersionDataSource.AddRange(Util.getCBList(s.gamelist, Legal.Games_7go).OrderBy(g => g.Value)); // stuff to end unsorted

            HaXMoveDataSource = Util.getCBList(s.movelist, null);
            MoveDataSource = LegalMoveDataSource = HaXMoveDataSource.Where(m => !Legal.Z_Moves.Contains(m.Value)).ToList();
            #region Met Locations
            // Gen 2
            {
                var met_list = Util.getCBList(s.metGSC_00000, Enumerable.Range(0, 0x5F).ToArray());
                met_list = Util.getOffsetCBList(met_list, s.metGSC_00000, 00000, new[] { 0x7E, 0x7F });
                metGen2 = met_list;
            }
            // Gen 3
            {
                var met_list = Util.getCBList(s.metRSEFRLG_00000, Enumerable.Range(0, 213).ToArray());
                met_list = Util.getOffsetCBList(met_list, s.metRSEFRLG_00000, 00000, new[] { 253, 254, 255 });
                metGen3 = met_list;

                var cxd_list = Util.getCBList(s.metCXD_00000, Enumerable.Range(0, s.metCXD_00000.Length).ToArray()).Where(c => c.Text.Length > 0).ToList();
                metGen3CXD = cxd_list;
            }
            // Gen 4
            {
                var met_list = Util.getCBList(s.metHGSS_00000, new[] { 0 });
                met_list = Util.getOffsetCBList(met_list, s.metHGSS_02000, 2000, new[] { 2000 });
                met_list = Util.getOffsetCBList(met_list, s.metHGSS_02000, 2000, new[] { 2002 });
                met_list = Util.getOffsetCBList(met_list, s.metHGSS_03000, 3000, new[] { 3001 });
                met_list = Util.getOffsetCBList(met_list, s.metHGSS_00000, 0000, Legal.Met_HGSS_0);
                met_list = Util.getOffsetCBList(met_list, s.metHGSS_02000, 2000, Legal.Met_HGSS_2);
                met_list = Util.getOffsetCBList(met_list, s.metHGSS_03000, 3000, Legal.Met_HGSS_3);
                metGen4 = met_list;
            }
            // Gen 5
            {
                var met_list = Util.getCBList(s.metBW2_00000, new[] { 0 });
                met_list = Util.getOffsetCBList(met_list, s.metBW2_60000, 60001, new[] { 60002 });
                met_list = Util.getOffsetCBList(met_list, s.metBW2_30000, 30001, new[] { 30003 });
                met_list = Util.getOffsetCBList(met_list, s.metBW2_00000, 00000, Legal.Met_BW2_0);
                met_list = Util.getOffsetCBList(met_list, s.metBW2_30000, 30001, Legal.Met_BW2_3);
                met_list = Util.getOffsetCBList(met_list, s.metBW2_40000, 40001, Legal.Met_BW2_4);
                met_list = Util.getOffsetCBList(met_list, s.metBW2_60000, 60001, Legal.Met_BW2_6);
                metGen5 = met_list;
            }
            // Gen 6
            {
                var met_list = Util.getCBList(s.metXY_00000, new[] { 0 });
                met_list = Util.getOffsetCBList(met_list, s.metXY_60000, 60001, new[] { 60002 });
                met_list = Util.getOffsetCBList(met_list, s.metXY_30000, 30001, new[] { 30002 });
                met_list = Util.getOffsetCBList(met_list, s.metXY_00000, 00000, Legal.Met_XY_0);
                met_list = Util.getOffsetCBList(met_list, s.metXY_30000, 30001, Legal.Met_XY_3);
                met_list = Util.getOffsetCBList(met_list, s.metXY_40000, 40001, Legal.Met_XY_4);
                met_list = Util.getOffsetCBList(met_list, s.metXY_60000, 60001, Legal.Met_XY_6);
                metGen6 = met_list;
            }
            // Gen 7
            {
                var met_list = Util.getCBList(s.metSM_00000, new[] { 0 });
                met_list = Util.getOffsetCBList(met_list, s.metSM_60000, 60001, new[] { 60002 });
                met_list = Util.getOffsetCBList(met_list, s.metSM_30000, 30001, new[] { 30002 });
                met_list = Util.getOffsetCBList(met_list, s.metSM_00000, 00000, Legal.Met_SM_0);
                met_list = Util.getOffsetCBList(met_list, s.metSM_30000, 30001, Legal.Met_SM_3);
                met_list = Util.getOffsetCBList(met_list, s.metSM_40000, 40001, Legal.Met_SM_4);
                met_list = Util.getOffsetCBList(met_list, s.metSM_60000, 60001, Legal.Met_SM_6);
                metGen7 = met_list;
            }
            #endregion
        }

        public static void setItemDataSource(bool HaX, int MaxItemID, IEnumerable<ushort> allowed, int generation, GameVersion game, GameStrings s)
        {
            string[] items = s.getItemStrings(generation, game);
            ItemDataSource = Util.getCBList(items, (allowed == null || HaX ? Enumerable.Range(0, MaxItemID) : allowed.Select(i => (int) i)).ToArray());
        }
        public static List<ComboItem> getLocationList(GameVersion Version, int SaveFormat, bool egg)
        {
            if (SaveFormat == 2)
                return metGen2;

            if (egg)
            {
                if (Version < GameVersion.W && SaveFormat >= 5)
                    return metGen4;
            }

            switch (Version)
            {
                case GameVersion.CXD:
                    if (SaveFormat == 3)
                        return metGen3CXD;
                    break;

                case GameVersion.R:
                case GameVersion.S:
                    if (SaveFormat == 3)
                        return metGen3.OrderByDescending(loc => loc.Value <= 87).ToList(); // Ferry
                    break;
                case GameVersion.E:
                    if (SaveFormat == 3)
                        return metGen3.OrderByDescending(loc => loc.Value <= 87 || (loc.Value >= 196 && loc.Value <= 212)).ToList(); // Trainer Hill
                    break;
                case GameVersion.FR:
                case GameVersion.LG:
                    if (SaveFormat == 3)
                        return metGen3.OrderByDescending(loc => loc.Value > 87 && loc.Value < 197).ToList(); // Celadon Dept.
                    break;

                case GameVersion.D:
                case GameVersion.P:
                    if (SaveFormat == 4 || (SaveFormat >= 5 && egg))
                        return metGen4.Take(4).Concat(metGen4.Skip(4).OrderByDescending(loc => loc.Value <= 111)).ToList(); // Battle Park
                    break;

                case GameVersion.Pt:
                    if (SaveFormat == 4 || (SaveFormat >= 5 && egg))
                        return metGen4.Take(4).Concat(metGen4.Skip(4).OrderByDescending(loc => loc.Value <= 125)).ToList(); // Rock Peak Ruins
                    break;

                case GameVersion.HG:
                case GameVersion.SS:
                    if (SaveFormat == 4 || (SaveFormat >= 5 && egg))
                        return metGen4.Take(4).Concat(metGen4.Skip(4).OrderByDescending(loc => loc.Value > 125 && loc.Value < 234)).ToList(); // Celadon Dept.
                    break;

                case GameVersion.B:
                case GameVersion.W:
                    return metGen5;

                case GameVersion.B2:
                case GameVersion.W2:
                    return metGen5.Take(3).Concat(metGen5.Skip(3).OrderByDescending(loc => loc.Value <= 116)).ToList(); // Abyssal Ruins

                case GameVersion.X:
                case GameVersion.Y:
                    return metGen6.Take(3).Concat(metGen6.Skip(3).OrderByDescending(loc => loc.Value <= 168)).ToList(); // Unknown Dungeon

                case GameVersion.OR:
                case GameVersion.AS:
                    return metGen6.Take(3).Concat(metGen6.Skip(3).OrderByDescending(loc => loc.Value > 168 && loc.Value <= 354)).ToList(); // Secret Base

                case GameVersion.SN:
                case GameVersion.MN:

                case GameVersion.GO:
                case GameVersion.RD:
                case GameVersion.BU:
                case GameVersion.GN:
                case GameVersion.YW:
                    return metGen7.Take(3).Concat(metGen7.Skip(3).OrderByDescending(loc => loc.Value < 200)).ToList(); // Secret Base
            }

            // Currently on a future game, return corresponding list for generation
            if (Version <= GameVersion.CXD && SaveFormat == 4)
                return metGen4.Where(loc => loc.Value == 0x37) // Pal Park to front
                    .Concat(metGen4.Take(4))
                    .Concat(metGen4.Skip(4).Where(loc => loc.Value != 0x37)).ToList();

            if (Version < GameVersion.X && SaveFormat >= 5) // PokéTransfer to front
                return metGen5.Where(loc => loc.Value == 30001)
                    .Concat(metGen5.Take(3))
                    .Concat(metGen5.Skip(3).Where(loc => loc.Value != 30001)).ToList();

            return metGen6;
        }

        /// <summary>
        /// Gets Country and Region strings for corresponding IDs and language.
        /// </summary>
        /// <param name="country">Country ID</param>
        /// <param name="region">Region ID</param>
        /// <param name="language">Language ID</param>
        /// <returns></returns>
        public static Tuple<string, string> getCountryRegionText(int country, int region, string language)
        {
            // Get Language we're fetching for
            int lang = Array.IndexOf(new[] { "ja", "en", "fr", "de", "it", "es", "zh", "ko" }, language);
            string c = getCountryString(country, lang);
            string r = getRegionString(country, region, lang);
            return new Tuple<string, string>(c, r); // country, region
        }

        /// <summary>
        /// Gets the Country string for a given Country ID
        /// </summary>
        /// <param name="country">Country ID</param>
        /// <param name="language">Language ID</param>
        /// <returns>Country ID string</returns>
        private static string getCountryString(int country, int language)
        {
            string c;
            // Get Country Text
            try
            {
                string[] inputCSV = Util.getStringList("countries");
                // Set up our Temporary Storage
                string[] unsortedList = new string[inputCSV.Length - 1];
                int[] indexes = new int[inputCSV.Length - 1];

                // Gather our data from the input file
                for (int i = 1; i < inputCSV.Length; i++)
                {
                    string[] countryData = inputCSV[i].Split(',');
                    if (countryData.Length <= 1) continue;
                    indexes[i - 1] = Convert.ToInt32(countryData[0]);
                    unsortedList[i - 1] = countryData[language + 1];
                }

                int countrynum = Array.IndexOf(indexes, country);
                c = unsortedList[countrynum];
            }
            catch { c = "Illegal"; }

            return c;
        }

        /// <summary>
        /// Gets the Region string for a specified country ID.
        /// </summary>
        /// <param name="country">Country ID</param>
        /// <param name="region">Region ID</param>
        /// <param name="language">Language ID</param>
        /// <returns>Region ID string</returns>
        private static string getRegionString(int country, int region, int language)
        {
            // Get Region Text
            try
            {
                string[] inputCSV = Util.getStringList("sr_" + country.ToString("000"));
                // Set up our Temporary Storage
                string[] unsortedList = new string[inputCSV.Length - 1];
                int[] indexes = new int[inputCSV.Length - 1];

                // Gather our data from the input file
                for (int i = 1; i < inputCSV.Length; i++)
                {
                    string[] countryData = inputCSV[i].Split(',');
                    if (countryData.Length <= 1) continue;
                    indexes[i - 1] = Convert.ToInt32(countryData[0]);
                    unsortedList[i - 1] = countryData[language + 1];
                }

                int regionnum = Array.IndexOf(indexes, region);
                return unsortedList[regionnum];
            }
            catch { return "Illegal"; }
        }

        /// <summary>
        /// Gets the location names array for a specified generation.
        /// </summary>
        /// <param name="gen">Generation to get location names for.</param>
        /// <param name="bankID">BankID used to choose the text bank.</param>
        /// <returns>List of location names.</returns>
        public static string[] getLocationNames(int gen, int bankID)
        {
            switch (gen)
            {
                case 2: return Strings.metGSC_00000;
                case 3: return Strings.metRSEFRLG_00000;
                case 4:
                    switch (bankID)
                    {
                        case 0: return Strings.metHGSS_00000;
                        case 2: return Strings.metHGSS_02000;
                        default: return null;
                    }
                case 5:
                    switch (bankID)
                    {
                        case 0: return Strings.metBW2_00000;
                        case 3: return Strings.metBW2_30000;
                        case 4: return Strings.metBW2_40000;
                        case 6: return Strings.metBW2_60000;
                        default: return null;
                    }
                case 6:
                    switch (bankID)
                    {
                        case 0: return Strings.metXY_00000;
                        case 3: return Strings.metXY_30000;
                        case 4: return Strings.metXY_40000;
                        case 6: return Strings.metXY_60000;
                        default: return null;
                    }
                case 7:
                    switch (bankID)
                    {
                        case 0: return Strings.metSM_00000;
                        case 3: return Strings.metSM_30000;
                        case 4: return Strings.metSM_40000;
                        case 6: return Strings.metSM_60000;
                        default: return null;
                    }
                default:
                    return null;
            }
        }
    }
}
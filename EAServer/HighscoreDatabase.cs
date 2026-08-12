using Newtonsoft.Json;
using SSX3_Server.EAClient;
using SSX3_Server.EAClient.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSX3_Server.EAServer
{
    public class HighscoreDatabase
    {
        public List<CourseEntry> courseEntries = new List<CourseEntry>();

        public static Dictionary<int, string> IDToChar { get; } =
new Dictionary<int, string>()
{
                { 0, "Moby" },
                { 1, "Kaori" },
                { 2, "Allegra" },
                { 3, "Mac" },
                { 4, "Zoe" },
                { 5, "Griff" },
                { 6, "Elise" },
                { 7, "Nate" },
                { 8, "Psymon" },
                { 9, "Viggo" },
};

        public static Dictionary<string, string> TrackIDToName { get; } =
new Dictionary<string, string>()
{
                { "0", "Snow Jam" }, //Snow Jam
                { "1", "Metro City" }, //Metro Race
                { "2", "Ruthless Ridge" }, //Ruthless Ridge Race
                { "3", "Intimidator" },
                { "4", "Gravitude" },
                { "5", "R&B" },
                { "6", "Style Mile" },
                { "7", "Kick Doubt" },
                { "8", "Crow's Nest" },
                { "9", "Launch Time" },
                { "10", "Much-2-Much" },
                { "11", "The Junction" },
                { "12", "Schizophrenia" },
                { "13", "Perpendiculous" },
                { "14", "Happiness" },
                { "15", "Ruthless" },
                { "16", "The Throne" },
};

        public static Dictionary<string, string> EventIDToName { get; } =
new Dictionary<string, string>()
{
                { "0", "Race" }, //Snow Jam
                { "1", "Slope Style" }, //Metro Race
                { "2", "Big Air" }, //Ruthless Ridge Race
                { "3", "Super Pipe" },
};

        public static Dictionary<int, string> IDToName { get; } =
        new Dictionary<int, string>()
        {
                { 0, "Overall" },
                { 1, "Null" },
                { 2, "Null" },
                { 3, "Null" },
                { 4, "Snow Jam" },
                { 5, "Metro City" },
                { 6, "Ruthless Ridge" },
                { 7, "Intimidator" },
                { 8, "Gravitude" },
                { 9, "Happiness" }, //Race
                { 10, "Ruthless" }, //Race
                { 11, "The Throne" }, //Race
                { 12, "R&B" },
                { 13, "Style Mile" },
                { 14, "Kick Doubt" },
                { 15, "Null" },
                { 16, "Null" },
                { 17, "Null" },
                { 18, "Null" },
                { 19, "Null" },
                { 20, "Happiness" }, //Score
                { 21, "Ruthless" },//Score
                { 22, "The Throne" }, //Score
                { 23, "Crow's Nest" },
                { 24, "Launch Time" },
                { 25, "Much-2-Much" },
                { 26, "The Junction" },
                { 27, "Schizophrenia" },
                { 28, "Perpendiculous" },
        };

        public static Dictionary<int, string> IDToEvent { get; } =
new Dictionary<int, string>()
{
                { 0, "Overall" },
                { 1, "Null" },
                { 2, "Null" },
                { 3, "Null" },
                { 4, "Race" },
                { 5, "Race" },
                { 6, "Race" },
                { 7, "Race" },
                { 8, "Race" },
                { 9, "Race" }, //Race
                { 10, "Race" }, //Race
                { 11, "Race" }, //Race
                { 12, "Slope Style" },
                { 13, "Slope Style" },
                { 14, "Slope Style" },
                { 15, "Null" },
                { 16, "Null" },
                { 17, "Null" },
                { 18, "Null" },
                { 19, "Null" },
                { 20, "Slope Style" }, //Score
                { 21, "Slope Style" },//Score
                { 22, "Slope Style" }, //Score
                { 23, "Big Air" },
                { 24, "Big Air" },
                { 25, "Big Air" },
                { 26, "Half Pipe" },
                { 27, "Half Pipe" },
                { 28, "Half Pipe" },
};

        public static Dictionary<string, int> RankToHighscore { get; } =
        new Dictionary<string, int>()
        {
                { "0,0", 4 }, //Snow Jam Race
                { "1,0", 5 }, //Metro Race
                { "2,0", 6 }, //Ruthless Ridge Race
                { "3,0", 7 }, //Intimidator Race
                { "4,0", 8 }, //Gravitude Race
                { "14,0", 9 }, //Happiness Race Rival
                { "15,0", 10 }, //Ruthless Race Rival
                { "16,0", 11 }, //The Throne Race Rival
                { "5,1", 12 }, //R&N SlopeStyle
                { "6,1", 13 }, //Style Mile SlopeStyle
                { "7,1", 14 }, //Kick Doubt SlopeStyle
                { "14,1", 20 }, //Happiness SlopeStyle Rival
                { "15,1", 21 }, //Ruthless SlopeStyle Rival
                { "16,1", 22 }, //The Throne SlopeStyle Rival
                { "8,3", 23 }, //Crows Nest Big Air
                { "9,3", 24 }, //Launch Time Big Air
                { "10,3", 25 }, //Much-2-Much Big Air
                { "11,2", 26 }, //The Junction Super Pipe
                { "12,2", 27 }, //Schiophrenia Super Pipe
                { "13,2", 28 }, //Pernendiculous Super Pipe
        };

        public void AddScores(RaceDataFile rankData)
        {
            //Check If Race
            //Note Replace

            var rankDataFile = rankData.raceData0;
            var rankDataFile1 = rankData.raceData1;

            //if (rankDataFile.AUTH!="1")
            //{
            //    return;
            //}

            //Reported 1 Player Didnt Finish the race
            if (rankDataFile.QUIT0 != "0" || rankDataFile.QUIT1 != "0" || rankDataFile1.QUIT0 != "0" || rankDataFile1.QUIT1 != "0")
            {
                return;
            }
            var Client0 = EAServerManager.Instance.GetUserPersona(rankDataFile.NAME0);
            var Client1 = EAServerManager.Instance.GetUserPersona(rankDataFile.NAME1);

            string Player0 = rankDataFile.NAME0;
            string Player1 = rankDataFile.NAME1;
            int Score0 = 0;
            int Score1 = 0;
            string Version0 = EAClientManager.VersionPrefix[Client0.VERS];
            string Version1 = EAClientManager.VersionPrefix[Client1.VERS];

            if (rankDataFile.RACEEVE0=="0")
            {
                Score0 = int.Parse(rankDataFile.RACETIM0);
                Score1 = int.Parse(rankDataFile.RACETIM1);
            }
            else
            {
                Score0 = int.Parse(rankDataFile.SCORE0);
                Score1 = int.Parse(rankDataFile.SCORE1);
            }

            //Add Checking Files
            int HighscoreID = -1;
            if (!RankToHighscore.TryGetValue(rankDataFile.RACETRA0 + "," + rankDataFile.RACEEVE0, out HighscoreID))
            {
                ConsoleManager.WriteLine("Unknown Highscore Entry " + rankDataFile.RACETRA0 + "," + rankDataFile.RACEEVE0);
                return;
            }

            var TempEntry = courseEntries[HighscoreID];

            //Check if Empty
            if(TempEntry.Entries.Count==1)
            {
                if (TempEntry.Entries[0].Name =="Empty")
                {
                    TempEntry.Entries.RemoveAt(0);
                }
            }

            //Add
            int Index0 = -1;
            int Index1 = -1;
            bool Add0 = true;
            bool Add1 = true;

            if(rankDataFile.DNF0=="1")
            {
                Add0 = false;
            }

            if (rankDataFile.DNF1 == "1")
            {
                Add1 = false;
            }

            //Check DSYNC
            //Check DISC

            //Calculate Rank Point





            for (int i = 0; i < TempEntry.Entries.Count; i++)
            {
                if (TempEntry.Entries[i].Name==Player0 && Add0 == true)
                {
                    Index0 = i;

                    if (rankDataFile.RACEEVE0 == "0")
                    {
                        if (Score0 > TempEntry.Entries[i].Score)
                        {
                            Add0 = false;
                        }    
                    }
                    else
                    {
                        if (Score0 < TempEntry.Entries[i].Score)
                        {
                            Add0 = false;
                        }
                    }
                }
                if (TempEntry.Entries[i].Name == Player1 && Add1 == true)
                {
                    Index1 = i;
                    if (rankDataFile.RACEEVE0 == "0")
                    {
                        if (Score1 > TempEntry.Entries[i].Score)
                        {
                            Add1 = false;
                        }
                    }
                    else
                    {
                        if (Score1 < TempEntry.Entries[i].Score)
                        {
                            Add1 = false;
                        }
                    }
                }
                if(Index0!=-1&&Index1!=-1)
                {
                    break;
                }
            }

            if (!Add1&&!Add0)
            {
                return;
            }

            var NewEntry = new ScoreEntry();

            if (Add0)
            {
                NewEntry.Name = Player0;
                NewEntry.GameVersion = Version0;
                NewEntry.Score = Score0;
                NewEntry.RaceDataFile = rankData.GUID;
                NewEntry.When = rankData.raceData0.WHEN;

                if (Index0 == -1)
                {
                    TempEntry.Entries.Add(NewEntry);
                }
                else
                {
                    TempEntry.Entries[Index0] = NewEntry;
                }

                ConsoleManager.WriteLine("Added Highscore Entry " + NewEntry.Name + " " + NewEntry.Score + " " + rankDataFile.RACETRA0 + "," + rankDataFile.RACEEVE0);
            }

            if (Add1)
            {
                NewEntry = new ScoreEntry();

                NewEntry.Name = Player1;
                NewEntry.GameVersion = Version1;
                NewEntry.Score = Score1;
                NewEntry.RaceDataFile = rankData.GUID;
                NewEntry.When = rankData.raceData1.WHEN;

                if (Index1 == -1)
                {
                    TempEntry.Entries.Add(NewEntry);
                }
                else
                {
                    TempEntry.Entries[Index1] = NewEntry;
                }

                ConsoleManager.WriteLine("Added Highscore Entry " + NewEntry.Name + " " + NewEntry.Score + " " + rankDataFile.RACETRA0 + "," + rankDataFile.RACEEVE0);
            }

            //Sort
            if (rankDataFile.RACEEVE0 == "0")
            {
                TempEntry.Entries = TempEntry.Entries.OrderBy(x => x.Score).ToList();
            }
            else
            {
                TempEntry.Entries = TempEntry.Entries.OrderByDescending(x => x.Score).ToList();
            }

            courseEntries[HighscoreID] = TempEntry;

            CreateJson(AppContext.BaseDirectory + "\\Highscore.json");
        }

        public void CreateBlankDatabase()
        {
            for (int i = 0; i < 29; i++)
            {
                CourseEntry courseEntry = new CourseEntry();

                courseEntry.ID = i;

                courseEntry.Name = IDToName[i];
                courseEntry.Type = "Null";
                courseEntry.Event = "Null";

                courseEntry.Entries = new List<ScoreEntry>();

                ScoreEntry TempEntry = new ScoreEntry();
                TempEntry.Name = "Empty";
                TempEntry.Score = 0;
                TempEntry.RaceDataFile = "NULL";

                courseEntry.Entries.Add(TempEntry);

                courseEntries.Add(courseEntry);
            }

            for (int i = 4; i < 9; i++)
            {
                var Temp = courseEntries[i];

                Temp.Type = "Race";
                Temp.Event = "Race";

                courseEntries[i] = Temp;
            }

            for (int i = 9; i < 12; i++)
            {
                var Temp = courseEntries[i];

                Temp.Type = "Race";
                Temp.Event = "Backcountry";

                courseEntries[i] = Temp;
            }

            for (int i = 12; i < 15; i++)
            {
                var Temp = courseEntries[i];

                Temp.Type = "Freestyle";
                Temp.Event = "Slope Style";

                courseEntries[i] = Temp;
            }

            for (int i = 20; i < 23; i++)
            {
                var Temp = courseEntries[i];

                Temp.Type = "Freestyle";
                Temp.Event = "Backcountry";

                courseEntries[i] = Temp;
            }

            for (int i = 23; i < 26; i++)
            {
                var Temp = courseEntries[i];

                Temp.Type = "Freestyle";
                Temp.Event = "Big Air";

                courseEntries[i] = Temp;
            }

            for (int i = 26; i < 29; i++)
            {
                var Temp = courseEntries[i];

                Temp.Type = "Freestyle";
                Temp.Event = "Half Pipe";

                courseEntries[i] = Temp;
            }
        }

        public void CreateJson(string path, bool Inline = false)
        {
            var TempFormating = Formatting.None;
            if (Inline)
            {
                TempFormating = Formatting.Indented;
            }

            var serializer = JsonConvert.SerializeObject(this, TempFormating);
            File.WriteAllText(path, serializer);
        }

        public string CreateJsonText()
        {
            var TempFormating = Formatting.None;

            var serializer = JsonConvert.SerializeObject(this, TempFormating);
            return serializer.ToString().Replace(":null", ":\"NULL\"");
        }

        public string CreateJsonCourseText(int ID)
        {
            var TempFormating = Formatting.None;

            var Entry = courseEntries[ID];

            var serializer = JsonConvert.SerializeObject(Entry, TempFormating);
            return serializer.ToString().Replace(":null", ":\"NULL\"");
        }

        public static HighscoreDatabase Load(string path)
        {
            string paths = path;
            if (File.Exists(paths))
            {
                var stream = File.ReadAllText(paths);
                var container = JsonConvert.DeserializeObject<HighscoreDatabase>(stream);
                return container;
            }
            else
            {
                return null;
            }
        }

        public struct CourseEntry
        {
            public int ID;
            public string Name;
            public string Type;
            public string Event;
            public List<ScoreEntry> Entries;
        }

        public struct ScoreEntry
        {
            public string GameVersion;
            public string Name;
            public int Score;
            public string RaceDataFile;
            public string When;
        }
    }
}

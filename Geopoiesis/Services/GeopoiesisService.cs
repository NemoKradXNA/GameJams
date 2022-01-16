using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Geopoiesis.Services
{

    public delegate void SystemEventFired(SystemEvent evt);

    internal class SaveGame {
        public bool IsPaused { get; set; }

        public float OZone { get; set; }
        public float WaterLevel { get; set; }
        public float LifeLevel { get; set; }
        public float DistanceFromStar { get; set; }
        public float SurfaceTemp { get; set; }

        public float Volcanism { get; set; }
        public float Quakes { get; set; }

        public int Seed { get; set; }

        public int Years { get; set; }

        public List<SystemEvent> LoggedEvents { get; set; }

        public SaveGame() { }
        public SaveGame(GeopoiesisService data)
        {
            IsPaused = data.IsPaused;
            OZone = data.OZone;
            WaterLevel = data.WaterLevel;
            LifeLevel = data.LifeLevel;
            DistanceFromStar = data.DistanceFromStar;
            SurfaceTemp = data.SurfaceTemp;
            Volcanism = data.Volcanism;
            Quakes = data.Quakes;
            Seed = data.Seed;
            Years = data.Years;
            LoggedEvents = data.LoggedEvents;
        }
    }

    public class GeopoiesisService : GameComponent
    {

        protected ICameraService Camera { get { return Game.Services.GetService<ICameraService>(); } }
        protected ICoroutineService coroutineService { get { return Game.Services.GetService<ICoroutineService>(); } }

        public StartType StartType = StartType.None;
        public Epoch CurrentEpoch = Epoch.PlanetForming;

        public float OrbitalDistance = float.MaxValue;
        public int Years = 0;
        public int YearsFullSpeed = 5000000;
        public int SlowestYears = 500000;
        public int CurrentTimeFlow;

        public bool InGame = false; // Needs this to be part of the scene manager really..
        public bool IsPaused = false;

        public float OZone = 0; // 0-1
        public float WaterLevel = 0; // 0-1
        public float LifeLevel = 0; // 0-1
        public float DistanceFromStar = 1; // in AU
        public float SurfaceTemp = -30; // Celsius.

        public float Volcanism = 0; //0-1
        public float Quakes = 0; //0-1

        public int Seed = 1971;

        public event SystemEventFired OnSystemEventFired;

        public List<SystemEvent> LoggedEvents = new List<SystemEvent>();

        public GeopoiesisService(Game game) : base(game)
        {
            CurrentTimeFlow = YearsFullSpeed;

            Game.Services.AddService(typeof(GeopoiesisService), this);
            Game.Components.Add(this);
        }

        public void StartTheMarchOfTime()
        {
            coroutineService.StartCoroutine(TheMarchOfTime());
        }

        public void SaveGame()
        {
            SaveGame thisGame = new SaveGame(this);
            File.WriteAllText("save.json",JsonConvert.SerializeObject(thisGame));
        }

        public void LoadGame()
        {
            string json = File.ReadAllText("save.json");
            SaveGame thisGame = JsonConvert.DeserializeObject<SaveGame>(json);

            IsPaused = thisGame.IsPaused;
            OZone = thisGame.OZone;
            WaterLevel = thisGame.WaterLevel;
            LifeLevel = thisGame.LifeLevel;
            DistanceFromStar = thisGame.DistanceFromStar;
            SurfaceTemp = thisGame.SurfaceTemp;
            Volcanism = thisGame.Volcanism;
            Quakes = thisGame.Quakes;
            Seed = thisGame.Seed;
            Years = thisGame.Years;
            LoggedEvents = thisGame.LoggedEvents;
        }

        IEnumerator TheMarchOfTime()
        {
            Random rnd = new Random();

            int loopCycle = 0;
            InGame = true;
            while (InGame)
            {
                yield return new WaitForSeconds(Game, 1);

                if (!IsPaused)
                {
                    Years += CurrentTimeFlow;

                    // Add float
                    Years += rnd.Next(50000, 4500000);


                    // Water will form over time
                    WaterLevel += (Years / 100000000) * .0001f;

                    float bt = 0;
                    if (DistanceFromStar < .5f)
                        bt += .001f;
                    else if (DistanceFromStar < 1)
                        bt += .0001f;
                    else if (DistanceFromStar == 1)
                        bt = 0;
                    else if (DistanceFromStar > 1)
                        bt += .0001f;
                    else if (DistanceFromStar > 2)
                        bt -= .00001f;
                    else if (DistanceFromStar > 3)
                        bt -= .0001f;
                    else if (DistanceFromStar > 4)
                        bt -= .001f;

                    SurfaceTemp += bt;

                    if (SurfaceTemp > 60)
                        WaterLevel -= .0001f;

                    OZone = Math.Min(1, OZone +  WaterLevel * .001f);

                    float lm = 1;
                    // Based on stats grow what we have....
                    if (SurfaceTemp < -25)
                        lm *= .001f;
                    else if (SurfaceTemp < 0)
                        lm *= .002f;
                    else if (SurfaceTemp < 5)
                        lm *= .003f;
                    else if (SurfaceTemp < 15)
                        lm *= .004f;
                    else if (SurfaceTemp < 25)
                        lm *= .006f;
                    else if (SurfaceTemp < 50)
                        lm += .001f;

                    lm *= OZone;


                    // Set Epoch
                    if (WaterLevel > .1f && CurrentEpoch < Epoch.OceansForm)
                    {
                        CurrentEpoch = Epoch.OceansForm;
                        FireAnEvent(new SystemEvent() { Title = "Oceans Formed!", Description = "Oceans have now formed on your world!", TitleColor = Color.SeaGreen, TextColor = Color.DarkSeaGreen, beepSFX = "Audio/SFX/beep-10" });
                    }

                    if(CurrentEpoch > Epoch.OceansForm) // no life without water.
                        LifeLevel += lm;

                    if (LifeLevel > .01f && CurrentEpoch < Epoch.Prokaryotes)
                    {
                        CurrentEpoch = Epoch.Prokaryotes;
                        FireAnEvent(new SystemEvent() { Title = "Prokaryotes Formed!", Description = "Single-celled organisms, bacteria and cyanobacteria have begun to grow.", TitleColor = Color.SeaGreen, TextColor = Color.DarkSeaGreen, beepSFX = "Audio/SFX/beep-10" });
                    }

                    // Fire Any events.
                    if (loopCycle % 20 == 0)
                    {
                        FireAnEvent(new SystemEvent() { Title="Passage Of Time", Description = "Time passes slowly...", TitleColor = Color.Lime , TextColor = Color.LimeGreen, beepSFX = "Audio/SFX/beep-10" });
                    }

                    // Fire a random event!
                    if (loopCycle % rnd.Next(5, 20) == 0)
                    {
                        int evtIdx = rnd.Next(0, EvenstList.Count);
                        FireAnEvent(EvenstList[evtIdx]);
                    }

                    loopCycle++;
                }
            }
        }

        public void FireAnEvent(SystemEvent evt)
        {
            if (OnSystemEventFired != null)
                OnSystemEventFired(evt);
        }

        public List<SystemEvent> UpcommingEvents { get; set; }

        public List<SystemEvent> EvenstList = new List<SystemEvent>()
        {
            new SystemEvent(){ Title = "Ice Meteor Shower", Description = "A shower of rocks and ICE! +H2O", WaterLevel = .01f, TitleColor = Color.Silver, TextColor = Color.SteelBlue },
            new SystemEvent(){ Title = "Ice Meteor Shower", Description = "A shower of rocks and ICE! +H2O", WaterLevel = .02f, TitleColor = Color.Silver, TextColor = Color.SteelBlue },
            new SystemEvent(){ Title = "Solar Flare!", Description = "A coronal mass ejection has occurred -O3 +c ", OZone = -.01f, SurfaceTemp = .01f, TitleColor = Color.Gold, TextColor = Color.Goldenrod, beepSFX = "Audio/SFX/beep-10"  },
            new SystemEvent(){ Title = "Asteroid Strike", Description = "An small asteroid has stuck! +Life +O3 +c +H2O", LifeLevel = .01f, OZone = .01f, SurfaceTemp=.01f, WaterLevel=.0125f, TitleColor = Color.CornflowerBlue, TextColor = Color.DodgerBlue},
            new SystemEvent(){ Title = "Asteroid Strike", Description = "An medium asteroid has stuck! +Life +O3 +c +H2O", LifeLevel = .015f, OZone = .02f, SurfaceTemp =.01f, WaterLevel = .025f, TitleColor = Color.CornflowerBlue, TextColor = Color.DodgerBlue },
            new SystemEvent(){ Title = "Asteroid Strike", Description = "An large asteroid has stuck! +Life +O3 +c +H2O", LifeLevel = .025f, OZone = .05f, SurfaceTemp = .05f, WaterLevel = .05f, TitleColor = Color.CornflowerBlue, TextColor = Color.DodgerBlue },
            new SystemEvent(){ Title = "Asteroid Strike", Description = "An MASSIVE asteroid has stuck! -Life -O3 +c -H2O", LifeLevel = -.1f, OZone = -.1f, SurfaceTemp = .1f, WaterLevel = -.05f, TitleColor = Color.CornflowerBlue, TextColor = Color.DodgerBlue, beepSFX = "Audio/SFX/beep-10" },
            new SystemEvent(){ Title = "Cosmic Rays", Description = "A near by star has exploded showering us in cosmic rays - Life", LifeLevel = -.001f, TitleColor = Color.Magenta, TextColor = Color.Maroon, beepSFX = "Audio/SFX/beep-10"},
            new SystemEvent(){ Title = "Cosmic Rays", Description = "A near by star has exploded showering us in cosmic rays +Life", LifeLevel = .001f, TitleColor = Color.Magenta, TextColor = Color.Maroon, beepSFX = "Audio/SFX/beep-10" },
        };

        public Texture2D CreateBox(int width, int height, Rectangle thickenss, Color bgColor, Color edgeColor)
        {
            Texture2D boxTexture = new Texture2D(Game.GraphicsDevice, width, height);

            Color[] c = new Color[width * height];

            Color color = new Color(0, 0, 0, 0);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x < thickenss.X || x >= width - thickenss.Width || y < thickenss.Height || y >= height - thickenss.Y)
                        color = edgeColor;
                    else
                        color = bgColor;

                    c[x + y * width] = color;
                }
            }

            boxTexture.SetData(c);

            return boxTexture;
        }
    }

    public class SystemEvent
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public float OZone { get; set; }
        public float WaterLevel { get; set; }
        public float LifeLevel { get; set; }
        public float DistanceFromStar { get; set; }
        public float SurfaceTemp { get; set; }
        
        public int YearArrives { get; set; }

        public Color TitleColor { get; set; }
        public Color TextColor { get; set; }

        public string beepSFX { get; set; }

        public SystemEvent()
        {
            TitleColor = Color.White;
            TextColor = Color.Silver;
            beepSFX = null;
        }
    }
}
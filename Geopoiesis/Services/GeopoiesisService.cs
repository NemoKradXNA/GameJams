using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Services
{
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

        public int Seed = 1971;

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

        IEnumerator TheMarchOfTime()
        {
            Random rnd = new Random();

            InGame = true;
            while (InGame)
            {
                yield return new WaitForSeconds(Game, 1);

                if (!IsPaused)
                {
                    Years += CurrentTimeFlow;

                    // Add float
                    Years += rnd.Next(50000,4500000);
                }
            }
        }
    }
}

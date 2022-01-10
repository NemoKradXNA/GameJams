using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Enums
{
    // This is a bit time based and based on current life value, but life takes time to grow right?
    public enum Epoch // Time are "real" so need to be spread out a bit lol, or compress time as we age?
    {
        PlanetFormed, // 0
        OceansForm, // Took ~500 million years on earth.
        Prokaryotes, // Took 1 billion years to arrive on earth
        Photosynthesis, // 1.6
        Eukaryotes, // 2.6
        MulticellularLife, // 3.6
        SimpleAnimals, // 4 billion
        Arthropods, // 4.13 
        Animals, // 4.15
        ProtoAmphibians, // 4.1
        LandPlants, // 4.15
        Insects, // 4.2
        Amphibians, // 4.24
        Reptiles, // 4.3
        Mammals, // 4.4
        Birds, // 4.45
        Flowers, // 4.47
        DeathOfDinosours, // 4.54
        Sophonts, // 4.54
        StoneAge, // 4.58
        BronzeAge, //
        Civilization, // 4.6        
        AdvancedCivilization,
        HiTechCivilization,
        SpaceExploration,
        YouWin
    }
}

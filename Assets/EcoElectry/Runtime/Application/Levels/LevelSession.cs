using System;
using EcoElectry.Domain.Levels;

namespace EcoElectry.Application.Levels
{
    public sealed class LevelSession
    {
        public LevelSession(LevelRules rules)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public LevelRules Rules { get; }
    }
}

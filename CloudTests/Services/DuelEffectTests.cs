using CloudLogic.Services.Duel.Arena;
using CloudLogic.Services.Duel.Spells;
using FluentAssertions;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DuelEffectTests
    {
        private const int Fireball = 5;
        private const int PoisonGas = 13;

        [TestMethod]
        public void Burn_StartsFreshAfterItExpires()
        {
            var state = new ArenaState(DuelArenaTests.Mage(), DuelArenaTests.Mage(), 1);
            var attacker = state.Fighters[0];
            var defender = state.Fighters[1];
            var fireball = DuelSpellBook.Get(Fireball);

            DuelEffects.Apply(state, attacker, defender, fireball, 500);
            state.Advance(30);
            DuelEffects.Apply(state, attacker, defender, fireball, 50);

            defender.BurnPerTick.Should().BeApproximately(50 * fireball.EffectValue, 0.01f);
        }

        [TestMethod]
        public void Poison_StartsFreshAfterItExpires()
        {
            var state = new ArenaState(DuelArenaTests.Mage(), DuelArenaTests.Mage(), 1);
            var gas = DuelSpellBook.Get(PoisonGas);

            for (int i = 0; i < 4; i++)
            {
                DuelEffects.AddPoison(state, state.Fighters[0], gas);
            }

            state.Advance(30);
            DuelEffects.AddPoison(state, state.Fighters[0], gas);

            state.Fighters[1].PoisonStacks.Should().Be(1);
        }
    }
}

namespace CloudLogic.Services.Duel.Spells
{
    public static class DuelStance
    {
        public const int Auto = 0;
        public const int Close = 1;
        public const int Mid = 2;
        public const int Far = 3;

        public static float? Distance(int stance)
        {
            switch (stance)
            {
                case Close:
                    return 1.4f;
                case Mid:
                    return 5f;
                case Far:
                    return 8f;
                default:
                    return null;
            }
        }

        public static int Clean(int stance)
        {
            return stance >= Auto && stance <= Far ? stance : Auto;
        }
    }
}

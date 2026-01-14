namespace ConsoleApp1
{
    internal class BeforeDamageEventArgs : EventArgs
    {
        public Character attacker;
        public Character defender;
        public NormalBuffer buffer;
        public BeforeDamageEventArgs(Character attacker, Character defender, NormalBuffer buffer)
        {
            this.attacker = attacker;
            this.defender = defender;
            this.buffer = buffer;
        }

    }

    internal class DamageJudgeEventArgs : EventArgs
    {
        public Character attacker;
        public Character defender;

        public DamageJudgeEventArgs(Character attacker, Character defender)
        {
            this.attacker = attacker;
            this.defender = defender;
        }
    }

    internal class AfterDamageEventArgs : EventArgs
    {
        public Character attacker;
        public Character defender;
        public AfterDamageEventArgs(Character attacker, Character defender)
        {
            this.attacker = attacker;
            this.defender = defender;
        }
    }


}

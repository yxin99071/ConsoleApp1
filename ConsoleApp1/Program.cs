namespace ConsoleApp1
{
    public class Character
    {
        public string name;
        private int _damage;
        private int _health;
        private List<NormalBuffer> _buffers = new List<NormalBuffer>();
        public int DamageCorrection;
        public int HealthCorrection;


        public int Damage
        {
            get => _damage >0?_damage:0;
            set => _damage = value;
        }
        public int Health 
        {
            get => _health;
            set => _health = value;
        }
        public List<NormalBuffer>? Buffers{get => _buffers;}

        public void AddBuffer(NormalBuffer buffer)
        {
            DamageCorrection += buffer.damageCorrection;
            HealthCorrection += buffer.healthCorrection;
            if (HealthCorrection < 0)
                HealthCorrection = 0;

            _buffers.Add(buffer);
            Console.WriteLine($"Buffed by {buffer.name} in {buffer.lastRound} round!");
        }
        public void RemoveBuffer(NormalBuffer buffer)
        {
            DamageCorrection -= buffer.damageCorrection;
            HealthCorrection -= buffer.healthCorrection;
            if (HealthCorrection < 0)
                HealthCorrection = 0;

            _buffers.Remove(buffer);
            Console.WriteLine($"\"{buffer.name}\"Buffer Lost");
        }

        public Character(int damage, int health, string name = "UnNamed")
        {
            this.Damage = damage;
            this.Health = health;
            this.name = name;
        }
        public override string ToString()
        {
            return $"Health is :{Health}+{HealthCorrection}, Damage is :{Damage}+{DamageCorrection}, ";
        }

    }

    public class NormalBuffer
    {
        public string name = "TempBuff";
        public int lastRound;
        public int damageCorrection;
        public int healthCorrection;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            int[] ant = [1, 2, 3, 4, 5];

            int length = ant.Length;
            int[] result = new int[length * 2];
            for (int i = 0; i < length; i++)
            {
                result[i] = ant[i];
                result[2 * length - i] = ant[i];
            }

            //Character hero = new Character(5, 20,"Hero");
            //Character monster = new Character(3, 10,"Monster");

            //BattleSystem bs = new BattleSystem();
            //SystemController systemController = new SystemController(bs);
            //systemController.BattleControll(hero, monster);

        }
    }
   
}



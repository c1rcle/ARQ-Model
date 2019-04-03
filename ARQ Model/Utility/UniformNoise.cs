using System;
using System.Collections;

namespace ARQ_Model.Utility
{
    public class UniformNoise
    {
        private readonly Random numberGenerator;

        private double flipProbability;

        public UniformNoise(double flipProbability)
        {
            if (flipProbability > 1.0d || flipProbability < 0.0f)
                throw new ArgumentException("Wrong parameter!");
            this.flipProbability = flipProbability;
            numberGenerator = new Random();
        }

        public double FlipProbability
        {
            set
            {
                if (value > 1.0d || value < 0.0f)
                    throw new ArgumentException("Wrong parameter!");
                flipProbability = value;
            }
        }

        public BitArray GenerateNoise(BitArray packet)
        {
            for (var i = 0; i < packet.Length; i++)
                if (GetRandomWithProbability())
                    packet[i] = !packet[i];
            return packet;
        }

        public bool GetRandomWithProbability(double probability = double.NaN)
        {
            if (double.IsNaN(probability)) return numberGenerator.NextDouble() < flipProbability;

            if (probability > 1.0d || probability < 0.0f)
                throw new ArgumentException("Wrong parameter!");
            return numberGenerator.NextDouble() < probability;
        }
    }
}
using System;
using System.Collections;

namespace ARQ_Model.Utility
{
    public class UniformNoise
    {
        private readonly Random numberGenerator;

        private readonly double flipProbability;

        public UniformNoise(double flipProbability)
        {
            if (flipProbability > 1.0d || flipProbability < 0.0f) 
                throw new ArgumentException("Wrong parameter!");
            this.flipProbability = flipProbability;
            numberGenerator = new Random();
        }
        
        public BitArray GenerateNoise(BitArray packet)
        {
            for (var i = 0; i < packet.Length; i++)
            {
                if (GetRandomWithProbability()) 
                    packet[i] = !packet[i];
            }
            return packet;
        }

        private bool GetRandomWithProbability()
        {
            return numberGenerator.NextDouble() <= flipProbability;
        }
    }
}
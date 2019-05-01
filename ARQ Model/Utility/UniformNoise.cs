using System;
using System.Collections;

namespace ARQ_Model.Utility
{
    /// <summary>
    /// Uniform noise medium simulation class.
    /// </summary>
    public class UniformNoise
    {
        /// <summary>
        /// Random number generator.
        /// </summary>
        private readonly Random numberGenerator;

        /// <summary>
        /// The probability of a bit flip.
        /// </summary>
        private double flipProbability;

        /// <summary>
        /// UniformNoise class constructor.
        /// </summary>
        /// <param name="flipProbability">Initial bit flip probability.</param>
        /// <exception cref="ArgumentException">Probability can be between 0 and 1.</exception>
        public UniformNoise(double flipProbability)
        {
            if (flipProbability > 1.0d || flipProbability < 0.0f)
                throw new ArgumentException("Wrong parameter!");
            this.flipProbability = flipProbability;
            numberGenerator = new Random();
        }

        /// <summary>
        /// Custom setter for bit flip probability.
        /// </summary>
        /// <exception cref="ArgumentException">Probability can be between 0 and 1.</exception>
        public double FlipProbability
        {
            set
            {
                if (value > 1.0d || value < 0.0f)
                    throw new ArgumentException("Wrong parameter!");
                flipProbability = value;
            }
        }

        /// <summary>
        /// Simulates going through a noisy medium.
        /// </summary>
        /// <param name="packet">Packet for simulation.</param>
        /// <returns>Packet after simulation.</returns>
        public BitArray GenerateNoise(BitArray packet)
        {
            var temp = new BitArray(packet);
            for (var i = 0; i < temp.Length; i++)
                if (GetRandomWithProbability())
                    temp[i] = !temp[i];
            return temp;
        }

        /// <summary>
        /// Returns 'true' with given probability.
        /// </summary>
        /// <param name="probability">Probability of returning 'true'.</param>
        /// <returns>true (probability), false (1 - probability).</returns>
        /// <exception cref="ArgumentException">Probability can be between 0 and 1.</exception>
        public bool GetRandomWithProbability(double probability = double.NaN)
        {
            if (double.IsNaN(probability)) return numberGenerator.NextDouble() < flipProbability;

            if (probability > 1.0d || probability < 0.0f)
                throw new ArgumentException("Wrong parameter!");
            return numberGenerator.NextDouble() < probability;
        }
    }
}
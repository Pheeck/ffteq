using System;
using System.Numerics;

namespace ffteq
{
    class FFT
    {
        /// <returns> The lowest power of 2 greater than n </returns>
        private static int LowestNextPowOf2(int n)
        {
            int m = 1;
            while (m < n)
            {
                m *= 2;
            }
            return m;
        }

        /// <returns>
        /// Primitive n-th root of 1 (the one with the smallest angle in its
        /// polar form)
        /// </returns>
        private static Complex PrimitiveNthRootOf1(int n)
        {
            return new Complex(Math.Cos(2 * Math.PI / n), Math.Sin(2 * Math.PI / n));
        }

        /// <summary>
        /// Given a vector with even length return a vector of positions 1, 3,
        /// 5, ...
        /// </summary>
        private static Complex[] OddCoeficients(Complex[] vector)
        {
            int n = vector.Length;
            Complex[] result = new Complex[n / 2];
            for (int k = 0; k < n / 2; k++)
            {
                result[k] = vector[2 * k + 1];
            }
            return result;
        }

        /// <summary>
        /// Given a vector with even length return a vector of positions 0, 2,
        /// 4, ...
        /// </summary>
        private static Complex[] EvenCoeficients(Complex[] vector)
        {
            int n = vector.Length;
            Complex[] result = new Complex[n / 2];
            for (int k = 0; k < n / 2; k++)
            {
                result[k] = vector[2 * k];
            }
            return result;
        }

        /// <summary>
        /// Given n = 2^k, primitive nth root of 1 omega and a complex vector
        /// of length n, compute DFT of vector using FFT.
        /// </summary>
        private static Complex[] _DoFFT(int n, Complex omega, Complex[] vector)
        {
            Complex[] result = new Complex[n];

            if (n == 1)
            {
                result[0] = vector[0];
            }
            else
            {
                Complex newOmega = Complex.Pow(omega, 2);
                Complex[] o = _DoFFT(n / 2, newOmega, OddCoeficients(vector));
                Complex[] e = _DoFFT(n / 2, newOmega, EvenCoeficients(vector));

                Complex powOmega = omega;
                for (int k = 0; k < n / 2; k++)
                {
                    result[k] = o[k] + powOmega * e[k];
                    result[k + n / 2] = o[k] - powOmega * e[k];
                    powOmega *= omega;
                }
            }

            return result;
        }

        /// <summary>
        /// Compute DFT of complex vector using FFT. Length of the vector has
        /// to be power of 2.
        /// </summary>
        public static Complex[] DoFFT(Complex[] vector)
        {
            int n = vector.Length;
            if (n != LowestNextPowOf2(n))
            {
                throw new ApplicationException(
                    "Internal Error: Tried doing DFT on vector with length " +
                    "!= power of 2"
                );
            }
            return _DoFFT(n, PrimitiveNthRootOf1(n), vector);
        }

        /// <summary>
        /// Compute inverse DFT of complex vector using FFT. Length of the
        /// vector has to be power of 2.
        /// </summary>
        public static Complex[] DoIFFT(Complex[] vector)
        {
            int n = vector.Length;
            if (n != LowestNextPowOf2(n))
            {
                throw new ApplicationException(
                    "Internal Error: Tried doing DFT on vector with length " +
                    "!= power of 2"
                );
            }

            // Conjugate primitive root
            Complex omega = PrimitiveNthRootOf1(n);
            omega = Complex.Conjugate(omega);

            // Factor by 1/n
            Complex[] foo = new Complex[n];
            for (int k = 0; k < n; k++)
            {
                foo[k] = vector[k] / n;
            }

            return _DoFFT(n, omega, foo);
        }
    }
}

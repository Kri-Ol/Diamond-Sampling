using System;

namespace diamond
{
    class Program
    {
        public const double SQRT_5 = 2.2360679774997896964091736687313;

        public static double gaussian((double mu, double sigma) N, Random rng) {
            var phi = 2.0 * Math.PI * rng.NextDouble();
            var r   = Math.Sqrt( -2.0 * Math.Log(1.0 - rng.NextDouble()) );
            return N.mu + N.sigma * r * Math.Sin(phi);
        }

        public static double laplace((double mu, double sigma) L, Random rng) {
            var v = - L.sigma * Math.Log(1.0 - rng.NextDouble());
            return L.mu + ((rng.NextDouble() < 0.5) ? v : -v );
        }

        public static double sample_length(double lmax, Random rng) {
            return lmax * rng.NextDouble();
        }

        public static (double, double) move_point((double x, double y) pos, (double wx, double wy) dir, double l) {
            return (pos.x + dir.wx * l, pos.y + dir.wy * l);
        }

        public static (double, double) sample_in_quadrant((double x0, double y0) pos, (double wx, double wy) dir, double lmax, double sigma, Random rng) {
            while (true) {
                var l = sample_length(lmax, rng);
                (double x, double y) = move_point(pos, dir, l);

                var dort = (dir.wy, -dir.wx); // orthogonal to the line direction

                var s = gaussian((0.0, sigma), rng); // could be laplace instead of gaussian

                (x, y) = move_point((x, y), dort, s);
                if (x >= -1.0 && x <= 0.0 && y >= 0.0 && y <= 1.0) // acceptance/rejection
                    return (x, y);
            }
        }

        public static (double, double) sample_in_plane((double x, double y) pos, (double wx, double wy) dir, double lmax, double sigma, Random rng) {
            (double x, double y) = sample_in_quadrant(pos, dir, lmax, sigma, rng);

            if (rng.NextDouble() < 0.25)
                return (x, y);

            if (rng.NextDouble() < 0.5) // reflection over X
                return (x, -y);

            if (rng.NextDouble() < 0.75) // reflection over Y
                return (-x, y);

            return (-x, -y); // reflection over X&Y
        }

        static void Main(string[] args) {
            var rng = new Random(32345);

            var L = 0.5 * SQRT_5 + 0.5 / SQRT_5; // sampling length, BIGGER THAN JUST A SEGMENT IN THE QUADRANT
            (double x0, double y0) pos = (-1.0, 0.0); // initial position
            (double wx, double wy) dir = (2.0 / SQRT_5, 1.0 / SQRT_5); // directional cosines, wx*wx + wy*wy = 1
            double sigma = 0.2; // that's a value to play with

            // last rejection stage
            (double x, double y) pt;
            while(true) {
                pt = sample_in_plane(pos, dir, L, sigma, rng);

                if (pt.x < 0.5) // reject points in the red area, accept otherwise
                    break;
            }
            Console.WriteLine(String.Format("{0} {1}", pt.x, pt.y));
        }
    }
}

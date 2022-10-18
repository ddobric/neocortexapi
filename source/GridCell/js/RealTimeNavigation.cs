using System;
namespace GridCell.js
{
    public class RealTimeNavigation
    {
        public readonly int arenaSize;

        public double posx;
        public double posy;

        public double[] prevPos;

        public bool add = true;

        private int directionX = 2;
        private int directionY = 2;

        public double[] speedvect = { 0, 0 };

        public RealTimeNavigation(int arenaSize)
        {
            this.arenaSize = arenaSize;
            posx = arenaSize / 2;
            posy = arenaSize / 2;

            prevPos = new double[] {
                posx, posy
            };
        }

        public void Move()
        {
            prevPos = new double[] {
                posx, posy
            };

            if (posx < 2)
            {
                directionX = 2;
            }

            if (posx > arenaSize - 2)
            {
                directionX = -2;
            }

            if (posy < 2)
            {
                directionY = 2;
            }

            if (posy > arenaSize - 2)
            {
                directionY = -2;
            }

            posx += (directionX + new Random().NextDouble() * 2 - 2);
            posy += (directionY + new Random().NextDouble() * 2 - 2);

            speedvect = new double[] {
                posx - prevPos[0], posx - prevPos[0]
            };
        }
    }
}


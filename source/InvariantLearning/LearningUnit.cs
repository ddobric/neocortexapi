
namespace InvariantLearning
{
    internal class LearningUnit
    {
        private int v1;
        private int v2;

        public LearningUnit(int v1, int v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        internal void Learn(InvImage sample)
        {
            throw new NotImplementedException();
        }

        internal Dictionary<string, double> Predict(InvImage image)
        {
            throw new NotImplementedException();
        }
    }
}
using YOLO;

internal class InvariantExp
{
    private object invariantSet;
    private RunConfig experimentParams;

    public InvariantExp(object invariantSet, RunConfig experimentParams)
    {
        this.invariantSet = invariantSet;
        this.experimentParams = experimentParams;
    }

    internal object Train()
    {
        throw new NotImplementedException();
    }

    internal object Predict(object v)
    {
        throw new NotImplementedException();
    }
}
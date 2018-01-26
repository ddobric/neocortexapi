using System;

namespace NeoCortexApi
{
    
    public class SpatialPooler //implements Persistable
    {

    /** Default Serial Version  */

    private static /*final*/ long serialVersionUID = 1L;



   

    public SpatialPooler() { }



    /**

     * Initializes the specified {@link Connections} object which contains

     * the memory and structural anatomy this spatial pooler uses to implement

     * its algorithms.

     * 

     * @param c     a {@link Connections} object

     */

    public void init(Connections c)
    {

        if (c.getNumActiveColumnsPerInhArea() == 0 && (c.getLocalAreaDensity() == 0 ||

            c.getLocalAreaDensity() > 0.5))
        {

            throw new InvalidSPParamValueException("Inhibition parameters are invalid");

        }



        c.doSpatialPoolerPostInit();

        initMatrices(c);

        connectAndConfigureInputs(c);

    }
}

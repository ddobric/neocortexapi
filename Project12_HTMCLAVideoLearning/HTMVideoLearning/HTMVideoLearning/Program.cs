namespace HTMVideoLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            // Toogle between the two test Run1 or Run2

            // Run1:
            // Training and Learning Video with HTMClassifier with key as a frame key
            // Testing Procedure:
            // Read the Training dataset
            // Preprocessing the frame into smaller resolution, lower frame rate, color to binarized value
            // Learn the patterns(frames) with SP till reaching newborn stable state
            // Learn the patterns(frames) with SP+TM to generate sequential relation of adjacent frames,
            //      The learning ends when average accuracy is more than 90% and stays for 40 cycles or reaching maxcycles
            //      Calculating Average accuracy:
            //          Get the Predicted cells of the current frames SDR through TM
            //          Use the Predicted cells in HTMClassifier to see if there are learned framekey
            //          If the key of the next frame is found, count increase 1.
            //          The average accuracy is calculated by average of videoset accuracy, 
            //          videoset accuracy is calculated by average of all video accuracy in that set.
            // Testing session start:
            // Drag an Image as input, The trained layer will try to predict the next Frame, then uses the next frame as input to continue 
            // as long as there are predicted cells.
            // The predicted series of Frame after the input frame are made into videos under Run1Experiment/TEST/

            VideoLearning.Run1();

            // Run2:
            // Training and Learning Video with HTMClassifier with key as a serie of framekey
            // Testing Procedure:
            // Read the Training dataset
            // Preprocessing the frame into smaller resolution, lower frame rate, color to binarized value
            // Learn the patterns(frames) with SP till reaching newborn stable state
            // Learn the patterns(serie of frames) with SP+TM,
            // The serie of frames add each framekey respectively untill it reached the videos' framecount lengths:30
            // Then key - serie of frames with current frame as last frame is learned with the Cells index of the current frame.
            //      e.g. current frame circle_vd1_3's cell will be associate with key "circle_vd1_4-circle_vd1_5-circle_vd1_6-...-circle_vd1_29-circle_vd1_0-circle_vd1_1-circle_vd1_2-circle_vd1_3"
            //      through each iteration of frames in a video, the key will be framekey-shifted
            //      a List of Last Predicted Values is saved every frame iteration to be used in the next as validation.
            //          if LastPredictedValue of previous Frame contains the current frame's key, then match increase 1
            //          Accuracy is calculated each iteration of each Videos.
            //          The training ends when accuracy surpasses 80% more than 30 times or reaching max cycle
            // Testing session start:
            // Drag an Image as input, The trained layer will try to predict the next Frame, then uses the next frame label - framekey series
            // to recreate the video under Run2Experiment/TEST/

            //VideoLearning.Run2();
        }
    }
}

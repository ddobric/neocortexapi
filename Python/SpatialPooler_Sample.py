import matplotlib
import numpy as np
import random

matplotlib.use('Agg')
import matplotlib.pyplot as plt
from nupic.algorithms.spatial_pooler import SpatialPooler as SP

def percentOverlap(x1, x2, size):
  """
  Computes the percentage of overlap between vectors x1 and x2.

  @param x1   (array) binary vector
  @param x2   (array) binary vector
  @param size (int)   length of binary vectors

  @return percentOverlap (float) percentage overlap between x1 and x2
  """
  nonZeroX1 = np.count_nonzero(x1)
  nonZeroX2 = np.count_nonzero(x2)
  minX1X2 = min(nonZeroX1, nonZeroX2)
  percentOverlap = 0
  if minX1X2 > 0:
    percentOverlap = float(np.dot(x1, x2))/float(minX1X2)
  return percentOverlap
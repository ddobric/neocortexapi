# Script scrap

## Idea:
The output SDR of the first layer will then be processed ?/ learned by another spatial pooler in the second layer. 
In current setup, the second layer's SP will learned from 4 or 9 region of the image, each region in layer 2 consisted of 4 region in layer 1. This make the SP in layer 2 has an input bit resolution of 4 x SDR layer 1's length. The grouping of adjacent region enable the spatial relationship between near region in layer 1.  

Layer 3 SP will learned input bits as all SDRs from layer 2 combined. The SDRs list from layer 3 trained SP will be saved in an HtmClasifier according to label.  
These saved SDR will be used later in inference mode.

## Note:
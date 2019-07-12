import { Synapse } from './neocortexmodel';
export class Heap {
    heapArray: Array<number> = [];
    heapSize: number;

    left(i: number) {
        return i = i * 2;
    }
    right(i: number) {
        return i = i * 2 + 1;
    }
    parent(i: number) {
        return i = i / 2;

    }
    maxHeapify(heapList = [], i: number) {
        let largest: number;
        let l = this.left(i);
        let r = this.right(i);
        if (l < this.heapSize && heapList[l] > heapList[i]) {
            largest = l;
        }
        else {
            largest = i;
        }
        if (r < this.heapSize && heapList[r] > heapList[largest]) {
            largest = r
        }
        if (largest != i) {
            this.exchange(i, largest);
            this.maxHeapify(this.heapArray, largest);
        }


    }
    exchange(iPos: any, largestPos: any) {
        let holdiPos = this.heapArray[iPos];
        this.heapArray[iPos] = this.heapArray[largestPos];
        this.heapArray[largestPos] = holdiPos;

    }
    buildMaxHeap() {
        this.heapSize = this.heapArray.length;
        for (let index = 0; index < this.heapArray.length / 2; index++) {
            this.maxHeapify(this.heapArray, index);

        }
    }
}
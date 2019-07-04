//import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId } from '../neocortexmodel';
import { Synapse } from './neocortexmodel';
import { rootRenderNodes } from '@angular/core/src/view';


export class Node {
    permanence: Synapse;
    leftChild: Synapse;
    rightChild: Synapse;
    constructor(permanence: Synapse) {
        this.permanence = permanence;
        this.leftChild = null;
        this.rightChild = null;

    }
}
export class BinarySearchTree {
    root: Node;
    perm: Synapse;
    iNOrderResult;
    node;
    preOrderResult;
    constructor() {
        this.root = null;
    }
    insert(permanence: Synapse) {
        this.perm = permanence;
        this.node = this.root;
        if (this.node === null) {
            this.root = new Node(permanence);
        } else {
            this.searchTree(this.node);

        }
    }
    searchTree(node) {
        if (this.perm < node.permanence) {
            if (node.leftChild === null) {
                node.leftChild = new Node(this.perm);
            } else if (node.leftChild !== null) {
                this.searchTree(node.leftChild);

            }
        } else if (this.perm > node.permanence) {
            if (node.rightChild === null) {
                node.rightChild = new Node(this.perm);

            } else if (node.rightChild !== null) {
                this.searchTree(node.rightChild);

            }
        } else {
            null;

        }
    }
    inOrder() {
        if (this.root == null) {
            return null;
        } else {
            this.iNOrderResult = new Array();
            this.traverseInOrder(this.root);
            return this.iNOrderResult;
        };
    }
    traverseInOrder(node) {
        node.leftChild && this.traverseInOrder(node.leftChild);
        this.iNOrderResult.push(node.permanence);
        node.rightChild && this.traverseInOrder(node.rightChild);
    }

    preOrder() {
        if (this.root == null) {
            return null;
        } else {
            this.preOrderResult = new Array();
            this.traversePreOrder(this.root);
            return this.preOrderResult;
        };
    }
    traversePreOrder(node) {
        this.preOrderResult.push(node.permanence);
        node.leftChild && this.traversePreOrder(node.leftChild);
        node.rightChild && this.traversePreOrder(node.rightChild);
    };

}


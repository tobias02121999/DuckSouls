using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_flora_tree_generate : MonoBehaviour
{
    // Initialize the public variables
    public GameObject treeBranch;
    public GameObject leaves;
    public int treeHeightMin;
    public int treeHeightMax;
    public int branchLengthMin;
    public int branchLengthMax;
    public float leavesScaleMin;
    public float leavesScaleMax;
    public int branchChance; // The lower the branchChance value, the higher the chance to instantiate a branch
    public int branchChanceDecrease; // Defines how fast the branchChance value goes up everytime a branch extends
    public int branchHeight;
    public float treeCurve;
    public int maxBranchOffs; // The maximum amount a branch can branch off from the main tree branch

    [HideInInspector]
    public float terrainHighestPoint;

    // Initialize the private variables
    int treeHeight;
    int currentTreeHeight; // Keeps track of how many chunks need to be instatiated before the tree is at its randomized height
    
    // Run this code once at the start (private)
    void Start()
    {
        // Randomly determine the treeHeight based on the given min and max height variables
        treeHeight = Mathf.RoundToInt(Random.Range(treeHeightMin, treeHeightMax));

        // Instantiate the main branch of the tree
        InstantiateMainBranch(transform, transform);

        // Move to the nearest terrain height
        int layerMask = 1 << 12;
        RaycastHit hit;

        if (Physics.Raycast(transform.position + (transform.up * terrainHighestPoint), -transform.up, out hit, Mathf.Infinity, layerMask))
            transform.position = hit.point;
    }

    // Instantiate the main branch of the tree (public)
    public void InstantiateMainBranch(Transform branchTransform, Transform leavesTransform)
    {
        // Check if the treeHeight has not been reached yet
        if (currentTreeHeight < treeHeight)
        {
            // Instantiate the branch
            var branch = Instantiate(treeBranch, branchTransform) as GameObject;
            branch.transform.parent = transform;
            branch.transform.rotation = Quaternion.Euler(branch.transform.eulerAngles.x + Random.Range(-treeCurve, treeCurve), branch.transform.eulerAngles.y + Random.Range(-treeCurve, treeCurve), branch.transform.eulerAngles.z + Random.Range(-treeCurve, treeCurve));

            if (currentTreeHeight >= branchHeight)
                branch.GetComponent<scr_flora_tree_branch>().branchChance = branchChance;
            else
                branch.GetComponent<scr_flora_tree_branch>().branchChance = 1000;

            branch.GetComponent<scr_flora_tree_branch>().isMainBranch = true;

            // Increase the currentTreeHeight variable
            currentTreeHeight++;
        }
        else // This means it's the final branch, instantiate leaves
        {
            var _leaves = Instantiate(leaves, leavesTransform) as GameObject;
            _leaves.transform.parent = transform;
            _leaves.transform.localScale = new Vector3(Random.Range(leavesScaleMin, leavesScaleMax), Random.Range(leavesScaleMin, leavesScaleMax), Random.Range(leavesScaleMin, leavesScaleMax));
        } 
    }

    // Instantiate a tree branch (public)
    public void InstantiateBranch(Transform branchTransform, int currentBranchOffCount)
    {
        // Instantiate the branch
        var branch = Instantiate(treeBranch, branchTransform) as GameObject;
        branch.transform.parent = transform;
        branch.GetComponent<scr_flora_tree_branch>().branchChance += branchChanceDecrease;
        branch.GetComponent<scr_flora_tree_branch>().branchLength = Mathf.RoundToInt(Random.Range(branchLengthMin, branchLengthMax)) - 1;
        branch.GetComponent<scr_flora_tree_branch>().branchOffCount = currentBranchOffCount + 1;

        //branch.transform.rotation = Quaternion.Euler(branchTransform.parent.parent.eulerAngles.x + Random.Range(-45f, 45f), branchTransform.parent.parent.eulerAngles.y + Random.Range(-45f, 45f), branchTransform.parent.parent.eulerAngles.z + Random.Range(-45f, 45f));
        branch.transform.rotation = Quaternion.Euler(Random.Range(-90f, 90f), Random.Range(-90f, 90f), Random.Range(-90f, 90f));

        branch.transform.localScale = branchTransform.parent.parent.transform.localScale * .65f;
    }
}

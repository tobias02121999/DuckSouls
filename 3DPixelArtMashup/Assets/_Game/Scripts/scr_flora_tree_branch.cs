using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_flora_tree_branch : MonoBehaviour
{
    // Initialize the public variables
    public Transform extensionTransform;
    public Transform leavesTransform;
    public Transform[] branchTransform;

    [HideInInspector]
    public int branchChance; // The lower the branchChance value, the higher the chance to instantiate a branch

    [HideInInspector]
    public int branchLength;

    [HideInInspector]
    public int currentBranchLength;

    [HideInInspector]
    public bool isMainBranch;

    //[HideInInspector]
    public int branchOffCount;

    // Initialize the private variables
    scr_flora_tree_generate treeGenerateParent;

    // Run this code once at the start (private)
    void Start()
    {
        // Link the parent treeGenerate script to the variable
        treeGenerateParent = GetComponentInParent<scr_flora_tree_generate>();

        // Instantiate an extensionBranch if the maxHeight hasn't been reached yet
        if (isMainBranch)
            treeGenerateParent.InstantiateMainBranch(extensionTransform, leavesTransform);
        else
            InstantiateMainBranch(extensionTransform);

        // Instantiate a branch
        for (var x = 0; x < branchTransform.Length; x++)
        {
            if (Mathf.RoundToInt(Random.Range(0f, branchChance)) == 0 && branchOffCount < treeGenerateParent.maxBranchOffs)
                treeGenerateParent.InstantiateBranch(branchTransform[x], branchOffCount);

            Destroy(branchTransform[x].gameObject);
        }

        // Destroy its script component
        Destroy(GetComponent<scr_flora_tree_branch>());
    }

    // Instantiate the main branch (public)
    public void InstantiateMainBranch(Transform branchTransform)
    {
        // Check if the treeHeight has not been reached yet
        if (currentBranchLength < branchLength)
        {
            // Instantiate the branch
            var branch = Instantiate(treeGenerateParent.treeBranch, branchTransform) as GameObject;
            branch.transform.parent = treeGenerateParent.transform;
            branch.GetComponent<scr_flora_tree_branch>().branchChance = branchChance;
            branch.GetComponent<scr_flora_tree_branch>().branchLength = branchLength - 1;
            branch.GetComponent<scr_flora_tree_branch>().branchOffCount = branchOffCount;
        }
        else // This means it's the final branch, instantiate leaves
        {
            var leaves = Instantiate(treeGenerateParent.leaves, leavesTransform) as GameObject;
            leaves.transform.parent = treeGenerateParent.transform;

            //leaves.transform.localScale = new Vector3(Random.Range(treeGenerateParent.leavesScaleMin, treeGenerateParent.leavesScaleMax), Random.Range(treeGenerateParent.leavesScaleMin, treeGenerateParent.leavesScaleMax), Random.Range(treeGenerateParent.leavesScaleMin, treeGenerateParent.leavesScaleMax));
            var scale = Random.Range(treeGenerateParent.leavesScaleMin, treeGenerateParent.leavesScaleMax);
            leaves.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}

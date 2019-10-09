using UnityEngine;

interface IBehavior 
{
    void UpdateAnimatorController(GameObject ghost);

    Node ChooseNextNode(GameObject ghost);
}

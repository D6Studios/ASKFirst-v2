using UnityEngine;

public class FacialExpressions : MonoBehaviour
{
    [SerializeField]
    MeshRenderer faceRenderer;
    [SerializeField]
    Material[] expressionMaterials;
    [SerializeField]
    int startingExpressionIndex = 0;
    void Start()
    {
        faceRenderer.material = expressionMaterials[startingExpressionIndex];
    }
    public void IsNeutral()
    {
        faceRenderer.material = expressionMaterials[0];
    }
    public void IsSweating()
    {
        faceRenderer.material = expressionMaterials[1];
    }
    public void IsThinking()
    {
        faceRenderer.material = expressionMaterials[2];
    }
}

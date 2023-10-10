using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurface : MonoBehaviour
{
    public WaterSurface targetSurface = null;

    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    // Update is called once per frame
    void Update()
    {
        if (targetSurface != null)
        {
            // Build the search parameters
            searchParameters.startPositionWS = searchResult.candidateLocationWS;
            searchParameters.targetPositionWS = gameObject.transform.position;
            searchParameters.error = 0.01f;
            searchParameters.maxIterations = 8;

            // Do the search
            if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
            {
                // TODO make the thing work
                // Apply buoyancy exponentially based on depth
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float depth = searchResult.candidateLocationWS.y - searchResult.projectedPositionWS.y;
                    float buoyancy = Mathf.Exp(-depth * 0.5f);
                    // accelerate upwards

                    rb.AddForce(Vector3.up * buoyancy * 9.81f, ForceMode.Acceleration);
                }

            }
        }
    }
}
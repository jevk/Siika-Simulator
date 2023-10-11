using Ditzelgames;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FitToWaterSurface : MonoBehaviour
{
    public WaterSurface targetSurface = null;
    public float AirDrag = 1;
    public float WaterDrag = 10;
    public Transform[] FloatPoints;

    protected Rigidbody Rigidbody;

    protected Vector3[] WaterLinePoints;

    protected Vector3 centerOffset;

    public Vector3 Center { get { return transform.position + centerOffset; } }

    // Internal search params
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.useGravity = false;

        WaterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; i++)
        {
            WaterLinePoints[i] = FloatPoints[i].position;
        }
        centerOffset = PhysicsHelper.GetCenter(WaterLinePoints) - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var newWaterLine = 0f;
        var pointUnderWater = false;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            WaterLinePoints[i] = FloatPoints[i].position;
            WaterLinePoints[i].y = searchParameters.targetPositionWS.y;
            newWaterLine += WaterLinePoints[i].y / FloatPoints.Length;
            if (WaterLinePoints[i].y > searchParameters.targetPositionWS.y)
                pointUnderWater = true;
        }

        var waterLineDelta = newWaterLine - searchParameters.targetPositionWS.y;

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
                // Apply the floating force to the boat
                var waterSurfaceNormal = targetSurface.(searchResult.candidateLocationWS);
                var waterSurfaceRotation = Quaternion.LookRotation(waterSurfaceNormal, transform.up);
                var waterSurfacePointVelocity = Rigidbody.GetPointVelocity(searchResult.candidateLocationWS);
                var waterSurfacePointSpeed = waterSurfacePointVelocity.magnitude;
                var waterDrag = waterSurfacePointSpeed * waterSurfacePointSpeed * WaterDrag;
                var waterResistanceForce = waterSurfacePointVelocity.normalized * -waterDrag;
                var buoyancyForce = -waterSurfaceNormal * Mathf.Abs(Physics.gravity.y) * Rigidbody.mass;
                var force = buoyancyForce + waterResistanceForce;
                var pointUnderWater = searchResult.candidateLocationWS.y > searchParameters.targetPositionWS.y;
                if (pointUnderWater)
                {
                    var forcePosition = searchResult.candidateLocationWS;
                    Rigidbody.AddForceAtPosition(force, forcePosition, ForceMode.Force);
                    Rigidbody.AddForceAtPosition(waterSurfacePointVelocity * -AirDrag, forcePosition, ForceMode.Force);
                    Rigidbody.AddTorque(PhysicsHelper.QuaternionToAngularVelocity(waterSurfaceRotation * Quaternion.Inverse(Rigidbody.rotation)) * 50f * Time.deltaTime, ForceMode.VelocityChange);
                }
                else
                {
                    var forcePosition = searchResult.candidateLocationWS;
                    Rigidbody.AddForceAtPosition(force, forcePosition, ForceMode.Force);
                    Rigidbody.AddForceAtPosition(waterSurfacePointVelocity * -AirDrag, forcePosition, ForceMode.Force);
                    Rigidbody.AddTorque(PhysicsHelper.QuaternionToAngularVelocity(waterSurfaceRotation * Quaternion.Inverse(Rigidbody.rotation)) * 50f * Time.deltaTime, ForceMode.VelocityChange);
                }

            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (FloatPoints == null)
            return;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null)
                continue;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(FloatPoints[i].position.x, searchParameters.targetPositionWS.y, FloatPoints[i].position.z), 0.1f);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, searchParameters.targetPositionWS.y, Center.z), Vector3.one * 1f);
        }
    }
}
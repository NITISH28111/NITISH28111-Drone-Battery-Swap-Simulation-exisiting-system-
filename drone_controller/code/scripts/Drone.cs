using IndiePixel;
using UnityEngine;
using System.IO;

public class Drone : MonoBehaviour
{
    public List<Transform> waypoints;
    public List<Transform> chargingStations;
    public float speed = 7f; // Realistic drone speed is around 10-20 m/s
    public float battery = 100f; // Battery capacity in percentage
    public float payloadPower = 0f; // Power consumed by the payload in watts
    public float batteryDrainRate = 0.15f; // Battery drain rate in percentage per second
    public Rigidbody rb;

    private string Book1 = "C:\\Users\\prade\\OneDrive\\Desktop\\SOA_DATA.csv";

    private int currentWaypointIndex = 0;
    private bool isCharging = false;
    private float heightFromGround; // New variable to store the height from the ground
    private int obstnum = 0;
    private float totalDistanceCovered;


    private void Start()
    {
        Random.InitState(0); // Initialize random number generator with a fixed seed
        battery = 100f;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float time = Time.deltaTime;

        Debug.Log("Time: " + Time.time + ", Battery: " + battery);

        if (battery <= 100f && !isCharging)
        {
            MoveToChargingStation();
        }
        else if (battery < 100f && isCharging)
        {
            ChargeBattery();
        }
        else
        {
            MoveToWaypoint();
        }

        CheckBatteryLevel();

        float powerConsumed = (hoverPower + propulsionPower + payloadPower) * time;
        if (!isCharging)
        {
            battery -= powerConsumed * batteryDrainRate * time;
        }

        AvoidObstacle();

        heightFromGround = CalculateHeightFromGround();
        totalDistanceCovered += Vector3.Distance(transform.position, transform.position - transform.forward * speed * time);

        // Write the time and battery data to the CSV file
        WriteDataToCSV(Time.time, battery, speed, totalDistanceCovered, heightFromGround, obstnum);
    }

    private float CalculateHeightFromGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            return hit.distance;
        }
        return 0;
    }
    private void WriteDataToCSV(float time, float battery, float speed, float totalDistanceCovered, float heightFromGround, int obstnum)
    {
        // Check if the file exists
        if (!File.Exists(Book1))
        {
            // Create a new StreamWriter to write to the CSV file
            using (StreamWriter writer = new StreamWriter(Book1))
            {
                // Write the headers to the CSV file
                writer.WriteLine("Time,Battery,speed,distance,height,obstacles");
            }
        }

        // Append the data to the CSV file
        using (StreamWriter writer = new StreamWriter(Book1, true))
        {
            // Write the time and battery data to the CSV file
            writer.WriteLine(time + "," + battery + "," + speed + "," + totalDistanceCovered + "," + heightFromGround + "," + obstnum);
        }
    }

    private void MoveToWaypoint()
    {
        float time = Time.deltaTime;

        if (currentWaypointIndex >= waypoints.Count)
        {
            Debug.Log("Drone has reached all waypoints.");
            enabled = false;
            return;
        }

        float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
        if (distanceToWaypoint <= 0.1f)
        {
            currentWaypointIndex++;

        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, speed * time);
        }
        AvoidObstacle();
    }

    private void AvoidObstacle()
    {
        float avoidanceForce = 10f;
        float avoidanceAngle = 30f;
        int numRaycasts = 6;


        for (int i = 0; i < numRaycasts; i++)
        {
            float angle = i * (360f / numRaycasts) + avoidanceAngle;
            Vector3 rayDir = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
            if (Physics.Raycast(rb.position, rayDir, out RaycastHit hit, 15f))
            {
                // If we hit an obstacle, adjust the drone's movement
                if (hit.collider.gameObject != gameObject)
                {
                    rb.AddForce(-rayDir * avoidanceForce);
                    Debug.Log("Ray detected object: " + hit.collider.gameObject.name);
                    obstnum++;
                }

            }

            // Visualize the ray
            Debug.DrawRay(transform.position, rayDir * 15f, Color.red);
        }
    }

    private void MoveToChargingStation()
    {
        float time = Time.deltaTime;

        if (currentChargingStationIndex >= chargingStations.Count)
        {
            Debug.Log("Drone has visited all charging stations.");
            return;
        }

        float distanceToChargingStation = Vector3.Distance(transform.position, chargingStations[currentChargingStationIndex].position);
        if (distanceToChargingStation <= 0.1f)
        {
            currentChargingStationIndex++;

            isCharging = true;
            rb.isKinematic = true; // Disable physics simulation (landing)
            hoverPower = 0f; // Stop hover power consumption
            propulsionPower = 0f; // Stop propulsion power consumption
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, chargingStations[currentChargingStationIndex].position, speed * time);
        }
        AvoidObstacle();
    }

   
public class Stopwatch
{
    private float elapsedTime;
    public float startTime;

    public void Start()
    {
        startTime = Time.time;
        elapsedTime = 0;
    }

    public float Elapsed
    {
        get
        {
            return elapsedTime;
        }
    }

    public void Stop()
    {
        elapsedTime += Time.time - startTime;
    }

    public void Reset()
    {
        elapsedTime = 0;
        startTime = 0;
    }

    public bool IsRunning
    {
        get { return startTime > 0; }
    }
}
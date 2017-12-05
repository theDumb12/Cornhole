using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

using Windows.Kinect;

using System.Linq;

public class KinectListener : MonoBehaviour {

    private KinectSensor _sensor;
    private BodyFrameReader _bodyFrameReader;
    private Body[] _bodies = null;

    bool recordReady = false;
    bool recording = false;
    public bool releasedNow = false;
    LinkedList<CameraSpacePoint> recordedPoints = new LinkedList<CameraSpacePoint>();
    CameraSpacePoint prevPoint;

    public static float HAND_SHOULDER_Z_DELTA = 0.05f;
    public static float HAND_ELBOW_Y_DELTA = 0.01f;
    public static float TIME_OF_5_FRAMES = 0.17f;
    public static int NUM_FRAMES_TO_AVG = 5;

    public Transform ball;
    public int score = 0;
    public bool newBall = false;
    public Text scoreText;

    public double releasedXDirection;
    public double releasedYDirection;
    public double releasedZDirection;
    public double releasedYVelocity;
    public double releasedXVelocity;
    public double releasedZVelocity;

    public double handX = 0.0;
    public double handy = 0.0;
    public double handz = 0.0;

    //public GameObject kinectAvailableText;
    //public Text handXText;

    public bool IsAvailable;

    public static KinectListener instance = null;

    public Body[] GetBodies()
    {
        return _bodies;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
        _sensor = KinectSensor.GetDefault();
        if (_sensor != null)
        {
            IsAvailable = _sensor.IsAvailable;

            //kinectAvailableText.SetActive(IsAvailable);

            _bodyFrameReader = _sensor.BodyFrameSource.OpenReader();

            if (!_sensor.IsOpen)
            {
                _sensor.Open();
            }

            _bodies = new Body[_sensor.BodyFrameSource.BodyCount];
        }
        Instantiate(ball, new Vector3(0,0,0),Quaternion.identity);
    }
	
	// Update is called once per frame
	void Update () {
        //IsAvailable = _sensor.IsAvailable;
        scoreText.text = "Score: " + score.ToString();

        if (_bodyFrameReader != null)
        {
            var frame = _bodyFrameReader.AcquireLatestFrame();

            if (frame != null)
            {
                frame.GetAndRefreshBodyData(_bodies);

                foreach (var body in _bodies.Where(b => b.IsTracked))
                {
                    IDictionary<JointType, Windows.Kinect.Joint> joints = body.Joints;
                    Dictionary<JointType, PointF> jointPoints = new Dictionary<JointType, PointF>();

                    Windows.Kinect.Joint rightHand = joints[JointType.HandRight];
                    Windows.Kinect.Joint rightHip = joints[JointType.HipRight];
                    Windows.Kinect.Joint rightElbow = joints[JointType.ElbowRight];
                    Windows.Kinect.Joint shoulderRight = joints[JointType.ShoulderRight];

                    handX = rightHand.Position.X;
                    handy = rightHand.Position.Y;
                    handz = rightHand.Position.Z;

                    recording = inRecordingMode(joints) || recording;
                    releasedNow = isReleased(joints);

                    //recodingModeLabel.Text = recording ? "TRUE" : "FALSE";
                    //isReleasedLabel.Text = releasedNow ? "TRUE" : "FALSE";

                    if (recording)
                        recordedPoints.AddLast(rightHand.Position);

                    if (recording && releasedNow)
                    {
                        CameraSpacePoint startingPoint = getStartingPoint();
                        CameraSpacePoint endingPoint = recordedPoints.ElementAt(recordedPoints.Count - 1);
                        releasedXVelocity = calculateVelocity(startingPoint.X, endingPoint.X);
                        releasedYVelocity = calculateVelocity(startingPoint.Y, endingPoint.Y);
                        releasedZVelocity = calculateVelocity(endingPoint.Z, startingPoint.Z);  //Flipped due to perspective being from Kinect
                                                                                                //releasedXDirection = Math.Atan2(releasedXVelocity, releasedZVelocity);
                                                                                                //releasedYDirection = Math.Atan2(releasedYVelocity, releasedZVelocity);
                        recordedPoints.Clear();
                        recording = false;

                        //xVelocityLabel.Text = releasedXVelocity.ToString("#.##") + " m/s";
                        //zVelocityLabel.Text = releasedZVelocity.ToString("#.##") + " m/s";
                        //yVelocityLabel.Text = releasedYVelocity.ToString("#.##") + " m/s";
                        //xDirectionLabel.Text = releasedXDirection.ToString("#.##") + " degrees";
                        //yDirectionLabel.Text = releasedYDirection.ToString("#.##") + " degrees";
                    }


                    recordedPoints.AddLast(rightHand.Position);

                    //rightHandX.Text = rightHand.Position.X.ToString("#.##");
                    //rightHandY.Text = rightHand.Position.Y.ToString("#.##");
                    //rightHandZ.Text = rightHand.Position.Z.ToString("#.##");

                    //rightHipX.Text = shoulderRight.Position.X.ToString("#.##");
                    //rightHipY.Text = shoulderRight.Position.Y.ToString("#.##");
                    //rightHipZ.Text = shoulderRight.Position.Z.ToString("#.##");

                    //Console.Out.WriteLine($"Hand: {rightHand.Position.X} {rightHand.Position.Y} {rightHand.Position.Z}");
                    //Console.Out.WriteLine($"Hip: {rightHip.Position.X} {rightHip.Position.Y} {rightHip.Position.Z}");
                }

                frame.Dispose();
                frame = null;
            }
        }
    }

    private CameraSpacePoint getStartingPoint()
    {
        int len = recordedPoints.Count - 1;
        CameraSpacePoint csp = recordedPoints.ElementAt(0);

        for (int i = NUM_FRAMES_TO_AVG; i >= 0 && len >= 0; i--)
        {
            if (len - i >= 0)
            {
                csp = recordedPoints.ElementAt(len - i);
                break;
            }
        }

        return csp;
    }

    private double calculateVelocity(double start, double end)
    {
        return (end - start) / TIME_OF_5_FRAMES;
    }

    private bool inRecordingMode(IDictionary<JointType, Windows.Kinect.Joint> joints)
    {
        Windows.Kinect.Joint rightHand = joints[JointType.HandRight];
        Windows.Kinect.Joint rightElbow = joints[JointType.ElbowRight];
        Windows.Kinect.Joint shoulderRight = joints[JointType.ShoulderRight];

        return rightHand.Position.Z > (shoulderRight.Position.Z + HAND_SHOULDER_Z_DELTA)
            && rightHand.Position.Y < (rightElbow.Position.Y + HAND_ELBOW_Y_DELTA);
    }

    private bool isReleased(IDictionary<JointType, Windows.Kinect.Joint> joints)
    {
        Windows.Kinect.Joint rightHand = joints[JointType.HandRight];
        Windows.Kinect.Joint rightElbow = joints[JointType.ElbowRight];
        Windows.Kinect.Joint shoulderRight = joints[JointType.ShoulderRight];
        //Joint hip = joints[JointType.HipRight];

        double dz = shoulderRight.Position.Z - rightHand.Position.Z;
        double dy = shoulderRight.Position.Y - rightHand.Position.Y;
        //dzLabel.Text = dz.ToString("#.##");
        //dyLabel.Text = dy.ToString("#.##");
        bool isReleased = dz >= dy;

        return isReleased;
    }
}

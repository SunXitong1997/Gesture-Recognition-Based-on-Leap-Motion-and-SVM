using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using System;
using System.IO;
using Accord;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.MachineLearning.VectorMachines;
using Accord.Statistics.Kernels;
using System.Linq;

public class GestureRecognition : MonoBehaviour
{
    //训练集 Data Set
    public static string trainedDataPath = @"C:\Users\sunxi\Desktop\LoopWithoutOBJ\mutiARsdk2\Assets\MyTxT\gesturesData.txt";
    public static List<List<double>> dataArrays = new List<List<double>>();
    //标签集 Labels Set
    public static string trainedLabelPath = @"C:\Users\sunxi\Desktop\LoopWithoutOBJ\mutiARsdk2\Assets\MyTxT\gesturesLabel.txt";
    public static List<int> labelArrays = new List<int>();

    //创建一个多种类支持向量机。 Create a Multi-Class SVM
    MulticlassSupportVectorLearning<Linear> teacher = new MulticlassSupportVectorLearning<Linear>()
    {
        Learner = (p) => new SequentialMinimalOptimization<Linear>()
        {
            Complexity = 10000.0 // Create a hard SVM
        }
    };

    MulticlassSupportVectorMachine<Linear> svm;

    //Hand shape tag
    public static string handShape = "Nature";

    //当前手势的标志 current gesture
    public static string currentGesture = null;

    //Leap motion controller

    LeapServiceProvider provider;

    Controller controller;

    //Finger Ray Definition 手指射线

    public LineRenderer laserLineRenderer;
    public float laserWidth = 0.01f;



    // Start is called before the first frame update
    void Start()
    {

        provider = FindObjectOfType<LeapServiceProvider>();

        controller = provider.GetLeapController();

        //支持向量机学习。
        //Using data set and label set for SVM learning

        //读训练集 Read data set
        string[] allLineContents = File.ReadAllLines(trainedDataPath);
        for (int i = 0; i < allLineContents.Length; i++)
        {
            List<double> everyLineNumbers = new List<double>();
            string[] everyLineContents = allLineContents[i].Split(' ');
            for (int j = 0; j < everyLineContents.Length; j++)
            {
                everyLineNumbers.Add(Convert.ToDouble(everyLineContents[j]));
            }
            dataArrays.Add(everyLineNumbers);
        }

        double[][] trainedData = dataArrays.Select(x => x.Select(y => y).ToArray()).ToArray();

        //此时，dataArray内以二维数组的形式存储了trainedData.txt文件中的全部数字。
        //Currently, dataArray contains all data from trainedData.txt


        //读标签集 Read label set
        string[] allLineContentsForLabels = File.ReadAllLines(trainedLabelPath);
        for (int i = 0; i < allLineContentsForLabels.Length; i++)
        {
            labelArrays.Add(Convert.ToInt32(allLineContentsForLabels[i]));
        }

        int[] trainedLabels = labelArrays.ToArray();

        //此时，trainedLabels.txt内的标签全部存储到了labelArrays中。
        //Now, labelArrays contains all data from trainedLabels.txt


        Debug.Log("Learning...");

        // Learn a multi-label SVM using the teacher
        //使用teacher函数进行SVM学习
        svm = teacher.Learn(trainedData, trainedLabels);



        //手指射线初始化
        //Initialize Finger ray
        Vector3[] initialLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        laserLineRenderer.SetPositions(initialLaserPositions);
        laserLineRenderer.startWidth = 0.01f;
        laserLineRenderer.endWidth = 0.01f;


    }

    // Update is called once per frame
    void Update()
    {

        //手势识别
        //Gesture Recognition

        //如果leap motion controller已经连接，则截取帧。
        //If leap motion works normally, we can get frame from it.

        if (controller.IsConnected == true)
        {
            //当前帧
            //Current frame

            //Frame frame = controller.Frame();
            Frame frame = new Frame();

            frame = provider.CurrentFrame;
            //上一帧
            //Last Frame

            Frame previousFrame = controller.Frame(1);

            //从帧内获取手
            //Get Hands from frame

            List<Hand> hands = frame.Hands;

            //单手判断
            //For single hand gesture

            if (hands.Count == 1)
            {

                Hand firstHand = hands[0];

                //获取手掌palm的位置
                //Get palm position
                Leap.Vector handPalmPosition = firstHand.PalmPosition;

                //获取手腕wrist的位置
                //Get wrist position
                Leap.Vector handWristPosition = firstHand.WristPosition;

                //下面获取手指相关参数
                //Fingers related data
                List<Finger> fingers = firstHand.Fingers;

                //拇指
                //Thumb
                Finger thumb = new Finger();

                //食指
                //Index
                Finger index = new Finger();

                //中指
                //Middle
                Finger middle = new Finger();

                //无名指
                //Ring
                Finger ring = new Finger();

                //小指
                //Pinky
                Finger pinky = new Finger();

                //给定义的手指赋予相应的对象。
                //Assign each finger related object.
                foreach (Finger finger in fingers)
                {
                    if (finger.Type == Finger.FingerType.TYPE_THUMB)
                    {
                        thumb = finger;
                    }
                    if (finger.Type == Finger.FingerType.TYPE_INDEX)
                    {
                        index = finger;
                    }
                    if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
                    {
                        middle = finger;
                    }
                    if (finger.Type == Finger.FingerType.TYPE_RING)
                    {
                        ring = finger;
                    }
                    if (finger.Type == Finger.FingerType.TYPE_PINKY)
                    {
                        pinky = finger;
                    }
                }

                //获取手指指尖坐标参数
                //Get Fingertips Position
                Leap.Vector thumbTipPosition = thumb.TipPosition;
                Leap.Vector indexTipPosition = index.TipPosition;
                Leap.Vector middleTipPosition = middle.TipPosition;
                Leap.Vector ringTipPosition = ring.TipPosition;
                Leap.Vector pinkyTipPosition = pinky.TipPosition;

                //下面计算SVM学习所需要的参数
                //Get SVM learning data

                //1.五个指尖到掌中的距离
                //1. Distance between five gingertips and palm
                //拇指
                //Thumb
                double fromThumbToPalm = System.Math.Sqrt((thumbTipPosition.x - handPalmPosition.x) * (thumbTipPosition.x - handPalmPosition.x) +
                                        (thumbTipPosition.y - handPalmPosition.y) * (thumbTipPosition.y - handPalmPosition.y) +
                                         (thumbTipPosition.z - handPalmPosition.z) * (thumbTipPosition.z - handPalmPosition.z));
                float distanceFromThumbToPalm = Convert.ToSingle(fromThumbToPalm);
                //食指
                //Index
                double fromIndexToPalm = System.Math.Sqrt((indexTipPosition.x - handPalmPosition.x) * (indexTipPosition.x - handPalmPosition.x) +
                                                          (indexTipPosition.y - handPalmPosition.y) * (indexTipPosition.y - handPalmPosition.y) +
                                                          (indexTipPosition.z - handPalmPosition.z) * (indexTipPosition.z - handPalmPosition.z));
                float distanceFromIndexToPalm = Convert.ToSingle(fromIndexToPalm);
                //中指
                //Middle
                double fromMiddleToPalm = System.Math.Sqrt((middleTipPosition.x - handPalmPosition.x) * (middleTipPosition.x - handPalmPosition.x) +
                                                           (middleTipPosition.y - handPalmPosition.y) * (middleTipPosition.y - handPalmPosition.y) +
                                                           (middleTipPosition.z - handPalmPosition.z) * (middleTipPosition.z - handPalmPosition.z));
                float distanceFromMiddleToPalm = Convert.ToSingle(fromMiddleToPalm);
                //无名指
                //Ring
                double fromRingToPalm = System.Math.Sqrt((ringTipPosition.x - handPalmPosition.x) * (ringTipPosition.x - handPalmPosition.x) +
                                                         (ringTipPosition.y - handPalmPosition.y) * (ringTipPosition.y - handPalmPosition.y) +
                                                         (ringTipPosition.z - handPalmPosition.z) * (ringTipPosition.z - handPalmPosition.z));
                float distanceFromRingToPalm = Convert.ToSingle(fromRingToPalm);
                //小指
                //Pinky
                double fromPinkyToPalm = System.Math.Sqrt((pinkyTipPosition.x - handPalmPosition.x) * (pinkyTipPosition.x - handPalmPosition.x) +
                                                          (pinkyTipPosition.y - handPalmPosition.y) * (pinkyTipPosition.y - handPalmPosition.y) +
                                                          (pinkyTipPosition.z - handPalmPosition.z) * (pinkyTipPosition.z - handPalmPosition.z));
                float distanceFromPinkyToPalm = Convert.ToSingle(fromPinkyToPalm);


                //2.五个指尖到手腕的距离。
                //2. Distance between five fingertips and wrist
                //拇指
                //Thumb
                double fromThumbToWrist = System.Math.Sqrt((thumbTipPosition.x - handWristPosition.x) * (thumbTipPosition.x - handWristPosition.x) +
                                                          (thumbTipPosition.y - handWristPosition.y) * (thumbTipPosition.y - handWristPosition.y) +
                                                          (thumbTipPosition.z - handWristPosition.z) * (thumbTipPosition.z - handWristPosition.z));
                float distanceFromThumbToWrist = Convert.ToSingle(fromThumbToWrist);
                //食指
                //Index
                double fromIndexToWrist = System.Math.Sqrt((indexTipPosition.x - handWristPosition.x) * (indexTipPosition.x - handWristPosition.x) +
                                                          (indexTipPosition.y - handWristPosition.y) * (indexTipPosition.y - handWristPosition.y) +
                                                          (indexTipPosition.z - handWristPosition.z) * (indexTipPosition.z - handWristPosition.z));
                float distanceFromIndexToWrist = Convert.ToSingle(fromIndexToWrist);
                //中指
                //Middle
                double fromMiddleToWrist = System.Math.Sqrt((middleTipPosition.x - handWristPosition.x) * (middleTipPosition.x - handWristPosition.x) +
                                                           (middleTipPosition.y - handWristPosition.y) * (middleTipPosition.y - handWristPosition.y) +
                                                           (middleTipPosition.z - handWristPosition.z) * (middleTipPosition.z - handWristPosition.z));
                float distanceFromMiddleToWrist = Convert.ToSingle(fromMiddleToWrist);
                //无名指
                //Ring
                double fromRingToWrist = System.Math.Sqrt((ringTipPosition.x - handWristPosition.x) * (ringTipPosition.x - handWristPosition.x) +
                                                         (ringTipPosition.y - handWristPosition.y) * (ringTipPosition.y - handWristPosition.y) +
                                                         (ringTipPosition.z - handWristPosition.z) * (ringTipPosition.z - handWristPosition.z));
                float distanceFromRingToWrist = Convert.ToSingle(fromRingToWrist);
                //小指
                //Pinky
                double fromPinkyToWrist = System.Math.Sqrt((pinkyTipPosition.x - handWristPosition.x) * (pinkyTipPosition.x - handWristPosition.x) +
                                                          (pinkyTipPosition.y - handWristPosition.y) * (pinkyTipPosition.y - handWristPosition.y) +
                                                          (pinkyTipPosition.z - handWristPosition.z) * (pinkyTipPosition.z - handWristPosition.z));
                float distanceFromPinkyToWrist = Convert.ToSingle(fromPinkyToWrist);

                //3.其余四个手指尖到拇指尖的距离
                //3. Distance between the other 4 fingertips and thumb tip.
                //食指
                //Index
                double fromIndexToThumb = System.Math.Sqrt((indexTipPosition.x - thumbTipPosition.x) * (indexTipPosition.x - thumbTipPosition.x) +
                                                          (indexTipPosition.y - thumbTipPosition.y) * (indexTipPosition.y - thumbTipPosition.y) +
                                                          (indexTipPosition.z - thumbTipPosition.z) * (indexTipPosition.z - thumbTipPosition.z));
                float distanceFromIndexToThumb = Convert.ToSingle(fromIndexToThumb);
                //中指
                //Middle
                double fromMiddleToThumb = System.Math.Sqrt((middleTipPosition.x - thumbTipPosition.x) * (middleTipPosition.x - thumbTipPosition.x) +
                                                           (middleTipPosition.y - thumbTipPosition.y) * (middleTipPosition.y - thumbTipPosition.y) +
                                                           (middleTipPosition.z - thumbTipPosition.z) * (middleTipPosition.z - thumbTipPosition.z));
                float distanceFromMiddleToThumb = Convert.ToSingle(fromMiddleToThumb);
                //无名指
                //Ring
                double fromRingToThumb = System.Math.Sqrt((ringTipPosition.x - thumbTipPosition.x) * (ringTipPosition.x - thumbTipPosition.x) +
                                                         (ringTipPosition.y - thumbTipPosition.y) * (ringTipPosition.y - thumbTipPosition.y) +
                                                         (ringTipPosition.z - thumbTipPosition.z) * (ringTipPosition.z - thumbTipPosition.z));
                float distanceFromRingToThumb = Convert.ToSingle(fromRingToThumb);
                //小指
                //Pinky
                double fromPinkyToThumb = System.Math.Sqrt((pinkyTipPosition.x - thumbTipPosition.x) * (pinkyTipPosition.x - thumbTipPosition.x) +
                                                          (pinkyTipPosition.y - thumbTipPosition.y) * (pinkyTipPosition.y - thumbTipPosition.y) +
                                                          (pinkyTipPosition.z - thumbTipPosition.z) * (pinkyTipPosition.z - thumbTipPosition.z));
                float distanceFromPinkyToThumb = Convert.ToSingle(fromPinkyToThumb);

                //将这些数据添加进数组
                //Add distance data to parameters array
                List<float> parameters = new List<float>();
                parameters.Add(distanceFromThumbToPalm);
                parameters.Add(distanceFromIndexToPalm);
                parameters.Add(distanceFromMiddleToPalm);
                parameters.Add(distanceFromRingToPalm);
                parameters.Add(distanceFromPinkyToPalm);

                parameters.Add(distanceFromThumbToWrist);
                parameters.Add(distanceFromIndexToWrist);
                parameters.Add(distanceFromMiddleToWrist);
                parameters.Add(distanceFromRingToWrist);
                parameters.Add(distanceFromPinkyToWrist);

                parameters.Add(distanceFromIndexToThumb);
                parameters.Add(distanceFromMiddleToThumb);
                parameters.Add(distanceFromRingToThumb);
                parameters.Add(distanceFromPinkyToThumb);

                //将参数归一化
                //Normalize all data


                //计算出最大距离值
                //Get maximum distance
                float maxDistance = GetMax(parameters);

                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i] = parameters[i] / maxDistance;
                }

                //进行hand shape预测
                //Hand shape prediction

                List<double> parametersDoubleList = new List<double>();
                for (int i = 0; i < parameters.Count; i++)
                {
                    parametersDoubleList.Add(parameters[i]);
                }

                double[] parametersArray = parametersDoubleList.ToArray();

                double[][] predictData =
                {
                        parametersArray,
                };

                //预测结果
                //Prediction outcome
                int[] answers = svm.Decide(predictData);
                if (answers[0] == 0)
                {
                    handShape = "Nature";
                }

                if (answers[0] == 1)
                {
                    handShape = "Close";//握拳 Fist
                }

                if (answers[0] == 2)
                {
                    handShape = "Point";
                }

                if (answers[0] == 3)
                {
                    handShape = "Grab";
                }

                if (answers[0] == 4)
                {
                    handShape = "Open"; //Open all fingers
                }

                if (answers[0] == 5)
                {
                    handShape = "Deselecting"; //Open all fingers except thumb
                }

                if (answers[0] == 6)
                {
                    handShape = "Shape6";
                }

                if (answers[0] == 7)
                {
                    handShape = "OK";
                }

                if (answers[0] == 8)
                {
                    handShape = "Debug";
                }

                //存储之前的手势。
                string previousGesture = currentGesture;

                //手势定义。在这里，我们基于当前帧的手形hand shape与来自leap motion的一些动态参数，比如手指方向，掌心方向等来进行手势定义与判断。
                //Hand Gesture Definition. Here we based on predicted hand shape in current frame and some dymanic paratemeters from Leap Motion
                //                         to define and recognize hand gesture.




                //手势1：自然状态。静态 
                //Gesture 1. Nature Gesture
                //手势2：右手Point。右手指尖会有手指射线
                //Gesture 2. Pointing Gesture with finger ray from index finger tip
                //手势3：右手drag。使用此手势拖动选中的虚标记。
                //Gesture 3. Dragging Gesture.
                //手势4：SwipeLeft。
                //Gesture 4. Swipe left.




                //手势1 Gesture 1
                if (handShape == "Nature")
                {
                    currentGesture = "Nature";
                }


                //手势2 Gesture 2
                if (handShape == "Point")
                {
                    currentGesture = "Point";
                    //指尖位置
                    //Index tip Position
                    Vector3 indexPosition = new Vector3();
                    indexPosition.x = indexTipPosition.x;
                    indexPosition.y = indexTipPosition.y;
                    indexPosition.z = indexTipPosition.z;

                    //指尖方向
                    //Index tip Direction
                    Leap.Vector indexTipDirection = index.Direction;
                    Vector3 indexDirection = new Vector3();
                    indexDirection.x = indexTipDirection.x;
                    indexDirection.y = indexTipDirection.y;
                    indexDirection.z = indexTipDirection.z;

                    //射线
                    //Ray
                    Ray ray = new Ray(indexPosition, indexDirection);
                    Debug.DrawRay(indexPosition, indexDirection, Color.red);
                    //Debug.Log(indexPosition.x);

                    //利用射线选中物体。
                    //Using ray to select object
                    RaycastHit raycasthit;
                    if (Physics.Raycast(ray, out raycasthit))
                    {
                        /*
                        foreach (GameObject _object in ObjectsList)
                        {
                            if (raycasthit.collider.name == _object.name)
                            {
                                SelectedObjects.Add(raycasthit.transform.gameObject);
                                Debug.Log(_object.name);
                            }
                        }
                        */
                    }

                    //使用LineRender使射线可视化
                    //Using LineRender to make ray visible in Unity Scenery

                    //ShootLaserFromTargetPosition(laserLineRenderer, indexPosition, indexDirection, 5.0f);

                    DrawFingerLaser(laserLineRenderer, indexTipPosition, indexTipDirection, 3.0f);

                    /*
                    laserLineRenderer.SetPosition(0, new Vector3(indexTipPosition.x, indexTipPosition.y, indexTipPosition.z));
                    Vector3 endPosition = indexPosition + new Vector3(indexTipDirection.x, indexTipDirection.y, indexTipDirection.z) * 3.0f;
                    laserLineRenderer.SetPosition(1, endPosition);
                    Debug.Log(indexPosition);
                    */

                    laserLineRenderer.enabled = true;

                }
                if (handShape != "Point")
                {
                    //关闭手指射线
                    //Close Finger ray
                    laserLineRenderer.enabled = false;
                }


                //手势3 Gesture 3
                if (handShape == "Grab")
                {
                    currentGesture = "SingleHandDrag";
                    Leap.Vector palmVelocity = firstHand.PalmVelocity;

                    float palmVelocityValue = palmVelocity.Magnitude;
                    if (palmVelocityValue > 0.04)
                    {

                        //下面关联一个物体让物体实时拥有成比例的运动速度和相同的运动方向。
                        Vector3 itemVelocity = new Vector3();
                        itemVelocity.x = palmVelocity.x / 4;
                        itemVelocity.y = palmVelocity.y / 4;
                        itemVelocity.z = palmVelocity.z / 4;

                        //item1.transform.Translate(itemVelocity.x, itemVelocity.y, 0);

                        //使用Dragging gestue 来移动被手指射线选中的物体
                        //Using this gesture to move objects chosen by Pointing gesture
                        /*
                        foreach (var selectedObject in SelectedObjects)
                        {
                            selectedObject.transform.Translate(-itemVelocity.x, -itemVelocity.z, 0);
                        }
                        */
                    }
                }

                //手势4 Gesture 4
                //虚拟手左右移动时，其速度体现在速度向量的x坐标上，向左移动时，x为负数，向右移动时，x为正数，
                if (handShape == "Open")
                {
                    Leap.Vector palmVelocity = firstHand.PalmVelocity;
                    if (firstHand.PalmNormal.x < -0.75)
                    {
                        if (palmVelocity.x < -1)
                        {
                            currentGesture = "SwipeLeft";
                        }

                    }
                }

                //Swipe Right
                if (handShape == "Open")
                {
                    Leap.Vector palmVelocity = firstHand.PalmVelocity;
                    if (firstHand.PalmNormal.x < -0.75)
                    {
                        if (palmVelocity.x > 0.8)
                        {
                            currentGesture = "SwipeRight";
                        }
                    }
                }

                //如果手势更新，则输出新手势名称。
                //Print out gesture name if it changes.

                if (currentGesture != previousGesture)
                {
                    Debug.Log(currentGesture);
                }


            }


            //双手判断
            //Double-hand gesture
            if (hands.Count == 2)
            {
                /*
                Hand firstHand = hands[0];
                Hand secondHand = hands[1];
                */

                //定义左右手
                //Define left hand and right hand


                Hand rightHand = hands[0];
                Hand leftHand = hands[1];


                for (int i = 0; i < hands.Count; i++)
                {
                    if (hands[i].IsLeft)
                    {
                        leftHand = hands[i];
                    }
                    else
                    {
                        rightHand = hands[i];
                    }
                }


                //获取左手的Hand Shape
                //Left hand shape
                string leftHandShape = getHandShape(leftHand, svm);
                //获取右手的Hand Shape
                //Right hand shape
                string rightHandShape = getHandShape(rightHand, svm);

                //存储之前的手势。
                string previousGesture = currentGesture;

                //双手手势定义
                //Double-hand gesture definition

                //双手clap
                //Clap gesture



                //控制虚拟物体的双手手势：
                //Double-hand gesture to control virtual object

                //放缩手势 Scalling
                if ((leftHandShape == "Grab") && (rightHandShape == "Grab"))
                {
                    currentGesture = "Scale";
                    //放缩操作

                    Leap.Vector leftHandVelocity = leftHand.PalmVelocity;
                    Leap.Vector rightHandVelocity = rightHand.PalmVelocity;



                    float scalingVelocity = (rightHandVelocity.x - leftHandVelocity.x)/20000;
                    //item1.transform.localScale += new Vector3(scalingVelocity, scalingVelocity, scalingVelocity);

                }


                //旋转手势Rotation
                if ((leftHandShape == "Grab") && (rightHandShape == "Open"))
                {
                    currentGesture = "Rotation";
                    //旋转操作

                    Leap.Vector rightHandPalmVelocity = rightHand.PalmVelocity;
                    float rotationVelocity = rightHandPalmVelocity.x / 30;
                    //item1.transform.Rotate(0, 0, rotationVelocity);

                }

                //如果手势更新，则输出新手势名称。
                //Print out getsture name if gesture changed
                if (currentGesture != previousGesture)
                {
                    Debug.Log("手势已更新。");
                    Debug.Log(currentGesture);
                    Debug.Log("===========");
                }

            }


        }

    }

    //最大值函数
    //Maximun Function
    public float GetMax(List<float> _numbers)
    {
        float max = 0f;
        max = _numbers[0];
        for (int i = 0; i < _numbers.Count; i++)
        {
            if (max < _numbers[i])
            {
                max = _numbers[i];
            }
        }
        return max;
    }

    //获取手形函数
    //GetHandShape Function

    public string getHandShape(Hand _hand, MulticlassSupportVectorMachine<Linear> _svm)
    {
        //获取手掌palm的位置
        Leap.Vector _handPalmPosition = _hand.PalmPosition;

        //获取手腕wrist的位置
        Leap.Vector _handWristPosition = _hand.WristPosition;

        //下面获取手指相关参数
        List<Finger> _fingers = _hand.Fingers;

        //拇指
        Finger _thumb = new Finger();

        //食指
        Finger _index = new Finger();

        //中指
        Finger _middle = new Finger();

        //无名指
        Finger _ring = new Finger();

        //小指
        Finger _pinky = new Finger();

        //给定义的手指赋予相应的对象。
        foreach (Finger finger in _fingers)
        {
            if (finger.Type == Finger.FingerType.TYPE_THUMB)
            {
                _thumb = finger;
            }
            if (finger.Type == Finger.FingerType.TYPE_INDEX)
            {
                _index = finger;
            }
            if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
            {
                _middle = finger;
            }
            if (finger.Type == Finger.FingerType.TYPE_RING)
            {
                _ring = finger;
            }
            if (finger.Type == Finger.FingerType.TYPE_PINKY)
            {
                _pinky = finger;
            }
        }

        //获取手指指尖坐标参数
        Leap.Vector _thumbTipPosition = _thumb.TipPosition;
        Leap.Vector _indexTipPosition = _index.TipPosition;
        Leap.Vector _middleTipPosition = _middle.TipPosition;
        Leap.Vector _ringTipPosition = _ring.TipPosition;
        Leap.Vector _pinkyTipPosition = _pinky.TipPosition;

        //下面计算SVM学习所需要的参数
        //1.五个指尖到掌中的距离
        //拇指
        double _fromThumbToPalm = System.Math.Sqrt((_thumbTipPosition.x - _handPalmPosition.x) * (_thumbTipPosition.x - _handPalmPosition.x) +
                                (_thumbTipPosition.y - _handPalmPosition.y) * (_thumbTipPosition.y - _handPalmPosition.y) +
                                 (_thumbTipPosition.z - _handPalmPosition.z) * (_thumbTipPosition.z - _handPalmPosition.z));
        float _distanceFromThumbToPalm = Convert.ToSingle(_fromThumbToPalm);
        //食指
        double _fromIndexToPalm = System.Math.Sqrt((_indexTipPosition.x - _handPalmPosition.x) * (_indexTipPosition.x - _handPalmPosition.x) +
                                                  (_indexTipPosition.y - _handPalmPosition.y) * (_indexTipPosition.y - _handPalmPosition.y) +
                                                  (_indexTipPosition.z - _handPalmPosition.z) * (_indexTipPosition.z - _handPalmPosition.z));
        float _distanceFromIndexToPalm = Convert.ToSingle(_fromIndexToPalm);
        //中指
        double _fromMiddleToPalm = System.Math.Sqrt((_middleTipPosition.x - _handPalmPosition.x) * (_middleTipPosition.x - _handPalmPosition.x) +
                                                   (_middleTipPosition.y - _handPalmPosition.y) * (_middleTipPosition.y - _handPalmPosition.y) +
                                                   (_middleTipPosition.z - _handPalmPosition.z) * (_middleTipPosition.z - _handPalmPosition.z));
        float _distanceFromMiddleToPalm = Convert.ToSingle(_fromMiddleToPalm);
        //无名指
        double _fromRingToPalm = System.Math.Sqrt((_ringTipPosition.x - _handPalmPosition.x) * (_ringTipPosition.x - _handPalmPosition.x) +
                                                 (_ringTipPosition.y - _handPalmPosition.y) * (_ringTipPosition.y - _handPalmPosition.y) +
                                                 (_ringTipPosition.z - _handPalmPosition.z) * (_ringTipPosition.z - _handPalmPosition.z));
        float _distanceFromRingToPalm = Convert.ToSingle(_fromRingToPalm);
        //小指
        double _fromPinkyToPalm = System.Math.Sqrt((_pinkyTipPosition.x - _handPalmPosition.x) * (_pinkyTipPosition.x - _handPalmPosition.x) +
                                                  (_pinkyTipPosition.y - _handPalmPosition.y) * (_pinkyTipPosition.y - _handPalmPosition.y) +
                                                  (_pinkyTipPosition.z - _handPalmPosition.z) * (_pinkyTipPosition.z - _handPalmPosition.z));
        float _distanceFromPinkyToPalm = Convert.ToSingle(_fromPinkyToPalm);


        //2.五个指尖到手腕的距离。
        //拇指
        double _fromThumbToWrist = System.Math.Sqrt((_thumbTipPosition.x - _handWristPosition.x) * (_thumbTipPosition.x - _handWristPosition.x) +
                                                  (_thumbTipPosition.y - _handWristPosition.y) * (_thumbTipPosition.y - _handWristPosition.y) +
                                                  (_thumbTipPosition.z - _handWristPosition.z) * (_thumbTipPosition.z - _handWristPosition.z));
        float _distanceFromThumbToWrist = Convert.ToSingle(_fromThumbToWrist);
        //食指
        double _fromIndexToWrist = System.Math.Sqrt((_indexTipPosition.x - _handWristPosition.x) * (_indexTipPosition.x - _handWristPosition.x) +
                                                  (_indexTipPosition.y - _handWristPosition.y) * (_indexTipPosition.y - _handWristPosition.y) +
                                                  (_indexTipPosition.z - _handWristPosition.z) * (_indexTipPosition.z - _handWristPosition.z));
        float _distanceFromIndexToWrist = Convert.ToSingle(_fromIndexToWrist);
        //中指
        double _fromMiddleToWrist = System.Math.Sqrt((_middleTipPosition.x - _handWristPosition.x) * (_middleTipPosition.x - _handWristPosition.x) +
                                                   (_middleTipPosition.y - _handWristPosition.y) * (_middleTipPosition.y - _handWristPosition.y) +
                                                   (_middleTipPosition.z - _handWristPosition.z) * (_middleTipPosition.z - _handWristPosition.z));
        float _distanceFromMiddleToWrist = Convert.ToSingle(_fromMiddleToWrist);
        //无名指
        double _fromRingToWrist = System.Math.Sqrt((_ringTipPosition.x - _handWristPosition.x) * (_ringTipPosition.x - _handWristPosition.x) +
                                                 (_ringTipPosition.y - _handWristPosition.y) * (_ringTipPosition.y - _handWristPosition.y) +
                                                 (_ringTipPosition.z - _handWristPosition.z) * (_ringTipPosition.z - _handWristPosition.z));
        float _distanceFromRingToWrist = Convert.ToSingle(_fromRingToWrist);
        //小指
        double _fromPinkyToWrist = System.Math.Sqrt((_pinkyTipPosition.x - _handWristPosition.x) * (_pinkyTipPosition.x - _handWristPosition.x) +
                                                  (_pinkyTipPosition.y - _handWristPosition.y) * (_pinkyTipPosition.y - _handWristPosition.y) +
                                                  (_pinkyTipPosition.z - _handWristPosition.z) * (_pinkyTipPosition.z - _handWristPosition.z));
        float _distanceFromPinkyToWrist = Convert.ToSingle(_fromPinkyToWrist);

        //3.其余四个手指尖到拇指尖的距离
        //食指
        double _fromIndexToThumb = System.Math.Sqrt((_indexTipPosition.x - _thumbTipPosition.x) * (_indexTipPosition.x - _thumbTipPosition.x) +
                                                  (_indexTipPosition.y - _thumbTipPosition.y) * (_indexTipPosition.y - _thumbTipPosition.y) +
                                                  (_indexTipPosition.z - _thumbTipPosition.z) * (_indexTipPosition.z - _thumbTipPosition.z));
        float _distanceFromIndexToThumb = Convert.ToSingle(_fromIndexToThumb);
        //中指
        double _fromMiddleToThumb = System.Math.Sqrt((_middleTipPosition.x - _thumbTipPosition.x) * (_middleTipPosition.x - _thumbTipPosition.x) +
                                                   (_middleTipPosition.y - _thumbTipPosition.y) * (_middleTipPosition.y - _thumbTipPosition.y) +
                                                   (_middleTipPosition.z - _thumbTipPosition.z) * (_middleTipPosition.z - _thumbTipPosition.z));
        float _distanceFromMiddleToThumb = Convert.ToSingle(_fromMiddleToThumb);
        //无名指
        double _fromRingToThumb = System.Math.Sqrt((_ringTipPosition.x - _thumbTipPosition.x) * (_ringTipPosition.x - _thumbTipPosition.x) +
                                                 (_ringTipPosition.y - _thumbTipPosition.y) * (_ringTipPosition.y - _thumbTipPosition.y) +
                                                 (_ringTipPosition.z - _thumbTipPosition.z) * (_ringTipPosition.z - _thumbTipPosition.z));
        float _distanceFromRingToThumb = Convert.ToSingle(_fromRingToThumb);
        //小指
        double _fromPinkyToThumb = System.Math.Sqrt((_pinkyTipPosition.x - _thumbTipPosition.x) * (_pinkyTipPosition.x - _thumbTipPosition.x) +
                                                  (_pinkyTipPosition.y - _thumbTipPosition.y) * (_pinkyTipPosition.y - _thumbTipPosition.y) +
                                                  (_pinkyTipPosition.z - _thumbTipPosition.z) * (_pinkyTipPosition.z - _thumbTipPosition.z));
        float _distanceFromPinkyToThumb = Convert.ToSingle(_fromPinkyToThumb);

        //将这些数据添加进数组
        List<float> _parameters = new List<float>();
        _parameters.Add(_distanceFromThumbToPalm);
        _parameters.Add(_distanceFromIndexToPalm);
        _parameters.Add(_distanceFromMiddleToPalm);
        _parameters.Add(_distanceFromRingToPalm);
        _parameters.Add(_distanceFromPinkyToPalm);

        _parameters.Add(_distanceFromThumbToWrist);
        _parameters.Add(_distanceFromIndexToWrist);
        _parameters.Add(_distanceFromMiddleToWrist);
        _parameters.Add(_distanceFromRingToWrist);
        _parameters.Add(_distanceFromPinkyToWrist);

        _parameters.Add(_distanceFromIndexToThumb);
        _parameters.Add(_distanceFromMiddleToThumb);
        _parameters.Add(_distanceFromRingToThumb);
        _parameters.Add(_distanceFromPinkyToThumb);

        //将参数归一化
        //计算出最大距离值
        float _maxDistance = GetMax(_parameters);

        for (int i = 0; i < _parameters.Count; i++)
        {
            _parameters[i] = _parameters[i] / _maxDistance;
        }

        //进行hand shape预测

        List<double> _parametersDoubleList = new List<double>();
        for (int i = 0; i < _parameters.Count; i++)
        {
            _parametersDoubleList.Add(_parameters[i]);
        }

        double[] _parametersArray = _parametersDoubleList.ToArray();

        double[][] _predictData =
        {
                   _parametersArray,
        };

        //预测结果
        int[] _answers = _svm.Decide(_predictData);

        string _handShape = null;

        if (_answers[0] == 0)
        {
            _handShape = "Nature";
        }

        if (_answers[0] == 1)
        {
            _handShape = "Close";//握拳
        }

        if (_answers[0] == 2)
        {
            _handShape = "Point";
        }

        if (_answers[0] == 3)
        {
            _handShape = "Grab";
        }

        if (_answers[0] == 4)
        {
            _handShape = "Open";
        }

        if (_answers[0] == 5)
        {
            _handShape = "Panel";
        }

        if (_answers[0] == 6)
        {
            _handShape = "Deselecting";
        }

        if (_answers[0] == 7)
        {
            _handShape = "OK";
        }

        if (_answers[0] == 8)
        {
            _handShape = "Debug";
        }

        return _handShape;

    }


    //手指射线函数
    //Finger ray function (old version)
    public void ShootLaserFromTargetPosition(LineRenderer _laserLineRenderer, Vector3 _targerPosition, Vector3 _direction, float _length)
    {
        Ray _ray = new Ray(_targerPosition, _direction);
        //RaycastHit _raycastHit;
        Vector3 _endPosition = _targerPosition + (_length * _direction);
        _laserLineRenderer.SetPosition(0, _targerPosition);
        _laserLineRenderer.SetPosition(1, _endPosition);
    }

    //手指射线函数新
    //Finger ray funcion (new version)
    public void DrawFingerLaser(LineRenderer _laserLineRenderer, Leap.Vector _indexTipPosition, Leap.Vector _indexDirection, float _length)
    {
        Ray _ray = new Ray(new Vector3(_indexTipPosition.x, _indexTipPosition.y, _indexTipPosition.z), new Vector3(_indexDirection.x, _indexDirection.y, _indexDirection.z));
        RaycastHit _raycastHit;
        Vector3 endPosition = new Vector3(_indexTipPosition.x, _indexTipPosition.y, _indexTipPosition.z) + new Vector3(_indexDirection.x, _indexDirection.y, _indexDirection.z) * _length;
        if (Physics.Raycast(_ray, out _raycastHit, _length))
        {
            endPosition = _raycastHit.point;
        }

        _laserLineRenderer.SetPosition(0, new Vector3(_indexTipPosition.x, _indexTipPosition.y, _indexTipPosition.z));
        laserLineRenderer.SetPosition(1, endPosition);
    }



}




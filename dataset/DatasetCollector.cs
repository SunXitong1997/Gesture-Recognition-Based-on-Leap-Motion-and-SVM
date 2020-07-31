using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using System;
using System.IO;


public class DatasetCollector : MonoBehaviour
{

    //用于生成手势数据集

    public static string gesturesDataPath = @"C:\Users\sunxi\Desktop\LoopWithoutOBJ\mutiARsdk2\Assets\MyTxT\Gesture8TestData.txt";

    public static string gesturesLabelPath = @"C:\Users\sunxi\Desktop\LoopWithoutOBJ\mutiARsdk2\Assets\MyTxT\Gesture8TestLabels.txt";

    public static int labelNumber = 5;

    int flag = 0;

    Controller controller;

    //public GameObject game;

    // Start is called before the first frame update
    void Start()
    {
        controller = new Controller();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (controller.IsConnected == true)
            {
                //如果leap motion controller已经连接，则截取帧。
                Frame frame = controller.Frame();
                Frame previousFrame = controller.Frame(1);

                //从帧内获取手
                List<Hand> hands = frame.Hands;
                Hand firstHand = hands[0];

                //获取抓取参数
                float grabStrength = firstHand.GrabStrength;

                //获取捏取参数
                float pinchStrength = firstHand.PinchStrength;

                //获取手掌palm的位置
                Leap.Vector handPalmPosition = firstHand.PalmPosition;

                //获取手腕wrist的位置
                Leap.Vector handWristPosition = firstHand.WristPosition;

                //下面获取手指相关参数
                List<Finger> fingers = firstHand.Fingers;

                //拇指
                Finger thumb = new Finger();

                //食指
                Finger index = new Finger();

                //中指
                Finger middle = new Finger();

                //无名指
                Finger ring = new Finger();

                //小指
                Finger pinky = new Finger();

                //给定义的手指赋予相应的对象。
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
                //fingertips parameter
                Leap.Vector thumbTipPosition = thumb.TipPosition;
                Leap.Vector indexTipPosition = index.TipPosition;
                Leap.Vector middleTipPosition = middle.TipPosition;
                Leap.Vector ringTipPosition = ring.TipPosition;
                Leap.Vector pinkyTipPosition = pinky.TipPosition;


                //下面计算SVM学习所需要的参数
                //1.五个指尖到掌中的距离
                //拇指
                double fromThumbToPalm = System.Math.Sqrt((thumbTipPosition.x - handPalmPosition.x) * (thumbTipPosition.x - handPalmPosition.x) +
                                        (thumbTipPosition.y - handPalmPosition.y) * (thumbTipPosition.y - handPalmPosition.y) +
                                         (thumbTipPosition.z - handPalmPosition.z) * (thumbTipPosition.z - handPalmPosition.z));
                float distanceFromThumbToPalm = Convert.ToSingle(fromThumbToPalm);
                //食指
                double fromIndexToPalm = System.Math.Sqrt((indexTipPosition.x - handPalmPosition.x) * (indexTipPosition.x - handPalmPosition.x) +
                                                          (indexTipPosition.y - handPalmPosition.y) * (indexTipPosition.y - handPalmPosition.y) +
                                                          (indexTipPosition.z - handPalmPosition.z) * (indexTipPosition.z - handPalmPosition.z));
                float distanceFromIndexToPalm = Convert.ToSingle(fromIndexToPalm);
                //中指
                double fromMiddleToPalm = System.Math.Sqrt((middleTipPosition.x - handPalmPosition.x) * (middleTipPosition.x - handPalmPosition.x) +
                                                           (middleTipPosition.y - handPalmPosition.y) * (middleTipPosition.y - handPalmPosition.y) +
                                                           (middleTipPosition.z - handPalmPosition.z) * (middleTipPosition.z - handPalmPosition.z));
                float distanceFromMiddleToPalm = Convert.ToSingle(fromMiddleToPalm);
                //无名指
                double fromRingToPalm = System.Math.Sqrt((ringTipPosition.x - handPalmPosition.x) * (ringTipPosition.x - handPalmPosition.x) +
                                                         (ringTipPosition.y - handPalmPosition.y) * (ringTipPosition.y - handPalmPosition.y) +
                                                         (ringTipPosition.z - handPalmPosition.z) * (ringTipPosition.z - handPalmPosition.z));
                float distanceFromRingToPalm = Convert.ToSingle(fromRingToPalm);
                //小指
                double fromPinkyToPalm = System.Math.Sqrt((pinkyTipPosition.x - handPalmPosition.x) * (pinkyTipPosition.x - handPalmPosition.x) +
                                                          (pinkyTipPosition.y - handPalmPosition.y) * (pinkyTipPosition.y - handPalmPosition.y) +
                                                          (pinkyTipPosition.z - handPalmPosition.z) * (pinkyTipPosition.z - handPalmPosition.z));
                float distanceFromPinkyToPalm = Convert.ToSingle(fromPinkyToPalm);


                //2.五个指尖到手腕的距离。
                //拇指
                double fromThumbToWrist = System.Math.Sqrt((thumbTipPosition.x - handWristPosition.x) * (thumbTipPosition.x - handWristPosition.x) +
                                                          (thumbTipPosition.y - handWristPosition.y) * (thumbTipPosition.y - handWristPosition.y) +
                                                          (thumbTipPosition.z - handWristPosition.z) * (thumbTipPosition.z - handWristPosition.z));
                float distanceFromThumbToWrist = Convert.ToSingle(fromThumbToWrist);
                //食指
                double fromIndexToWrist = System.Math.Sqrt((indexTipPosition.x - handWristPosition.x) * (indexTipPosition.x - handWristPosition.x) +
                                                          (indexTipPosition.y - handWristPosition.y) * (indexTipPosition.y - handWristPosition.y) +
                                                          (indexTipPosition.z - handWristPosition.z) * (indexTipPosition.z - handWristPosition.z));
                float distanceFromIndexToWrist = Convert.ToSingle(fromIndexToWrist);
                //中指
                double fromMiddleToWrist = System.Math.Sqrt((middleTipPosition.x - handWristPosition.x) * (middleTipPosition.x - handWristPosition.x) +
                                                           (middleTipPosition.y - handWristPosition.y) * (middleTipPosition.y - handWristPosition.y) +
                                                           (middleTipPosition.z - handWristPosition.z) * (middleTipPosition.z - handWristPosition.z));
                float distanceFromMiddleToWrist = Convert.ToSingle(fromMiddleToWrist);
                //无名指
                double fromRingToWrist = System.Math.Sqrt((ringTipPosition.x - handWristPosition.x) * (ringTipPosition.x - handWristPosition.x) +
                                                         (ringTipPosition.y - handWristPosition.y) * (ringTipPosition.y - handWristPosition.y) +
                                                         (ringTipPosition.z - handWristPosition.z) * (ringTipPosition.z - handWristPosition.z));
                float distanceFromRingToWrist = Convert.ToSingle(fromRingToWrist);
                //小指
                double fromPinkyToWrist = System.Math.Sqrt((pinkyTipPosition.x - handWristPosition.x) * (pinkyTipPosition.x - handWristPosition.x) +
                                                          (pinkyTipPosition.y - handWristPosition.y) * (pinkyTipPosition.y - handWristPosition.y) +
                                                          (pinkyTipPosition.z - handWristPosition.z) * (pinkyTipPosition.z - handWristPosition.z));
                float distanceFromPinkyToWrist = Convert.ToSingle(fromPinkyToWrist);

                //3.其余四个手指尖到拇指尖的距离
                //食指
                double fromIndexToThumb = System.Math.Sqrt((indexTipPosition.x - thumbTipPosition.x) * (indexTipPosition.x - thumbTipPosition.x) +
                                                          (indexTipPosition.y - thumbTipPosition.y) * (indexTipPosition.y - thumbTipPosition.y) +
                                                          (indexTipPosition.z - thumbTipPosition.z) * (indexTipPosition.z - thumbTipPosition.z));
                float distanceFromIndexToThumb = Convert.ToSingle(fromIndexToThumb);
                //中指
                double fromMiddleToThumb = System.Math.Sqrt((middleTipPosition.x - thumbTipPosition.x) * (middleTipPosition.x - thumbTipPosition.x) +
                                                           (middleTipPosition.y - thumbTipPosition.y) * (middleTipPosition.y - thumbTipPosition.y) +
                                                           (middleTipPosition.z - thumbTipPosition.z) * (middleTipPosition.z - thumbTipPosition.z));
                float distanceFromMiddleToThumb = Convert.ToSingle(fromMiddleToThumb);
                //无名指
                double fromRingToThumb = System.Math.Sqrt((ringTipPosition.x - thumbTipPosition.x) * (ringTipPosition.x - thumbTipPosition.x) +
                                                         (ringTipPosition.y - thumbTipPosition.y) * (ringTipPosition.y - thumbTipPosition.y) +
                                                         (ringTipPosition.z - thumbTipPosition.z) * (ringTipPosition.z - thumbTipPosition.z));
                float distanceFromRingToThumb = Convert.ToSingle(fromRingToThumb);
                //小指
                double fromPinkyToThumb = System.Math.Sqrt((pinkyTipPosition.x - thumbTipPosition.x) * (pinkyTipPosition.x - thumbTipPosition.x) +
                                                          (pinkyTipPosition.y - thumbTipPosition.y) * (pinkyTipPosition.y - thumbTipPosition.y) +
                                                          (pinkyTipPosition.z - thumbTipPosition.z) * (pinkyTipPosition.z - thumbTipPosition.z));
                float distanceFromPinkyToThumb = Convert.ToSingle(fromPinkyToThumb);

                //将这些数据添加进数组
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
                //计算出最大距离值
                float maxDistance = GetMax(parameters);
                /*
                Debug.Log("最大距离值为：");
                Debug.Log(maxDistance);
                Debug.Log("-------------------");
                */

                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i] = parameters[i] / maxDistance;
                }



                //存入txt文件中
                //Save data to .txt file
                StreamWriter sw = new StreamWriter(gesturesDataPath, true);
                for (int i = 0; i < parameters.Count; i++)
                {
                    int end = parameters.Count - 1;
                    if (i < end)
                    {
                        sw.Write(parameters[i] + " ");
                    }
                    if (i == end)
                    {
                        sw.WriteLine(parameters[i]);
                    }
                }
                sw.Close();
                sw.Dispose();

                //同时加标签
                //Add related label in the same time
                StreamWriter swForLabels = new StreamWriter(gesturesLabelPath, true);
                swForLabels.WriteLine("8");
                swForLabels.Close();
                swForLabels.Dispose();

                Debug.Log("添加成功 Successfully");

                //计数
                //Counting
                flag = flag + 1;
                Debug.Log(flag);
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

}

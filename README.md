# Gesture-Recognition-Based-on-Leap-Motion-and-SVM / 基于Leap Motion与SVM的手势识别
This is C# script can be used in Unity 2018.4.19f for hand gesture recognition. SVM in this script is based on Accord.NET framework.
这是一个用于Unity2018.4.19f版本的C#脚本，可以进行手势识别。SVM技术用到了Accord.NET框架。

How to use Accord.NET in Unity / 如何在Unity中使用Accord.NET https://unitycoder.com/blog/2016/11/04/using-accord-net-with-unity/

Our hand gesture recognition are composed of 2 steps. First step is to use SVM to predict static hand shape of user's hand. And second step is to combine prediced hand shape and dynamic parameters from Leap Motion API to realize dynamic hand gesture recognition. You can define the second part as you want. I give some examples of second part in the script with finger direction, paml direction and palm velocity. For dynmaic data of user's hand, you can refer to official document https://developer-archive.leapmotion.com/documentation/csharp/index.html.
我们的手势识别分为两步。第一步是使用SVM来预测用户的实时静态手形。第二步是基于预测的手形与来自Leap Motion的一些动态参数来进行动态手势识别。你可以根据需要自己定义第二部分，我在脚本中给出了一些例子，使用到了手指方向，掌心方向，掌心速度等参数。动态参数的获取请参考官方文档https://developer-archive.leapmotion.com/documentation/csharp/index.html。

This script also support double-hand gesture recognition. You can see the related codes in the script.
该脚本同样支持双手手势的定义与识别，你可以在脚本中看到相关的代码。

As for finger ray, you need to place a linerender in the scenery. You can use finger ray to interact with virtual object. For this function, you need to complete related part in the script using Raycast API.
关于手指射线，你需要在场景中提前放置一个Linerender。你可以使用手指射线与虚拟物体进行交互。如若使用这个功能，你需要完善脚本内的相关部分，这里要用到Raycast API。

In my project, this script has some relationship with other script. Therefore I delete some part before uploading it. If you want to use this script noramly, you can manke some modification. I have added some neccessary annotation about them. 
在我的项目中，该脚本与其他脚本有联系，所以在上传该脚本之前，我进行了部分删除，所以该脚本可能需要稍作修改才能使用，但是我对每部分加了必要的注释。

For any questions, you can contact me sunxitong1997@gmail.com.
如果你有什么问题，可以联系我sunxitong1997@gmail.com

Thank you for your support.
感谢你的支持。

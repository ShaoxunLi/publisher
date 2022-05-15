using OPCAutomation;
using System;
using System.IO;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Linq;
using System.Collections.Generic;

//NuGet程序包->输入：Install-Package M2Mqtt -Version 4.3.0 导入引用
namespace ConsoleApplication2
{
    public class OPCInstance
    {
        //1. 声明OPC服务器访问需要的成员
        OPCServer Server;
        OPCGroups Groups;
        OPCGroup Group;
        OPCItems Items;
        OPCItem Item;
        MqttClient client1;
        MqttClient client2;

        private List<string > _VariableNams = new List<string>();

        public void Run()
        {
            Server = new OPCServer();
            tag1:
            try
            {
                Server.Connect(Settings1.Default.OPCAddress);//连接OPC
            }
            catch (Exception e)
            {
                goto tag1;
            }
            

            if (Server.ServerState == Convert.ToInt32(OPCServerState.OPCRunning))
            {
                string clientId = Guid.NewGuid().ToString();
            tag2:
                try
                {
                    client1.Connect(clientId);
                }
                catch (Exception e)
                {
                    goto tag2;
                }


                //显示时间+OPC连接成功说明
                //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"OPC Server connect success.");
                Groups = Server.OPCGroups;
                //数据缓冲区值刷新阈值默认值
                Groups.DefaultGroupDeadband = 0;//设定死区
                //组对象是否活动默认值
                Groups.DefaultGroupIsActive = true;//激活
                //数据缓冲区刷新频率默认值
                Groups.DefaultGroupUpdateRate = 1000;//刷新频率，每1000ms刷新一次
                //添加一个组
                Group = Groups.Add("group1");
                //当前组对象是否订阅DateChange事件
                Group.IsSubscribed = true;
                Items = Group.OPCItems;
                var itemDataNameFile= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataNme2.txt");
                var nameLines = File.ReadAllLines(itemDataNameFile)
                                    .Where(c => c.StartsWith("Applications.Application_1"))
                                    .Distinct()
                                    .ToList();
               _VariableNams = nameLines.Select(c => c.Replace(".", "_")).ToList();

                //int key = 0;

                ////新增：
                //int length=_VariableNams.ToArray().Length;
                //Array handles = new Array[length];
                //int[] h=new int[length];

                ////nameLines.ForEach(c =>{ Items.AddItem(c, key++);});

                //nameLines.ForEach(c =>{ h[key]=Items.AddItem(c, key++).ServerHandle;});
                //handles=(Array)h;
                //Array values = Array.CreateInstance(typeof(string), length);
                //Array Errors = Array.CreateInstance(typeof(Int32), 10);
                //Object cancel=new object();
                //Object Qualities = new object();

                //string host = "localhost";
                //client = new MqttClient(host);


                

                //var sb = new StringBuilder();
                ////第一次先获取所有的值
                //Group.SyncRead((short)OPCAutomation.OPCDataSource.OPCDevice, length, ref handles, out values, out Errors, out Qualities, out cancel);
                //for (int i = 1; i <= length; i++)
                //{
                //    sb.Append("{");
                //    sb.Append(@"""");
                //    sb.Append(_VariableNams[i]);
                //    sb.Append(@""":");
                //    sb.Append(@"""");
                //    sb.Append(values.GetValue(i));
                //    if (i < length) { sb.Append(@""","); }
                //    sb.Append(@"""}");
                //}
                //var str = sb.ToString();
                //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MQTT_test.txt");
                //FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                //using (StreamWriter sw = new StreamWriter(fs))
                //{
                //    sw.WriteLine(str);
                //}
                //string topic = "/DC";
                //MQTT(sb.ToString(), topic);


                //订阅组DatEchange事件
                Group.DataChange+=Group_DataChange;
            }
            else
            {
                //Console.WriteLine("OPC server connect failed.");
            }
        }

        private void MQTT(string content, string topic)
        {

            // 实例化Mqtt客户端
                string strValue = Convert.ToString(content);

                // 发布消息主题 "/DC" ，消息质量 QoS 2 
                client1.Publish(topic, Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Console.WriteLine(string.Format("publisher,topic:{0},content:{1}", topic, content));
                //client.Disconnect();
        }

        //private void MQTT(MqttClient client,string topic,string content)
        //{
        //    string strValue = Convert.ToString(content);

        //    // 发布消息主题 "/DC" ，消息质量 QoS 2 
        //    client.Publish(topic, Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        //    Console.WriteLine(string.Format("publisher,topic:{0},content:{1}", topic, content));
        //}

        private void Group_DataChange(int TransactionID, int NumItems, ref Array
    ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            //MQTT
            string topic = "/DC";
            
            var handleToValueMap = new Dictionary<int,string>();

            Console.ForegroundColor = ConsoleColor.Green;//绿色字体
            for (int i = 1; i <= NumItems; i++)
            {
                int tmpClientHandle = Convert.ToInt32(ClientHandles.GetValue(i));
                string tmpValue = ItemValues.GetValue(i).ToString();
                if(!handleToValueMap.ContainsKey(tmpClientHandle))
                {
                    handleToValueMap.Add(tmpClientHandle,tmpValue);
                }
            }
            if(handleToValueMap.Any())
            {
                var sb = new StringBuilder();
                sb.Append("{");
                int key = handleToValueMap.Last().Key;
                foreach(var handleToValue in handleToValueMap)
                {
                    sb.Append(@"""");
                    sb.Append(_VariableNams[handleToValue.Key]);
                    sb.Append(@""":");
                    sb.Append(@"""");
                    sb.Append(handleToValue.Value);
                    if (handleToValue.Key != key)
                      sb.Append( @""",");
                }
                sb.Append(@"""}");
                var str = sb.ToString();

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MQTT_test.txt");
                FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(str);
                }
                MQTT(sb.ToString(), topic);
            }   
        }

    }
}

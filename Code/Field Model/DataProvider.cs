//**************************************************************//
//**************************************************************//
//*******           Copyright (C) 2010-2018              *******//
//*******           Team: Wuhan University               *******//
//*******           Developer:Xuequan Zhang              *******//
//*******           Email:xuequan_zhang@126.com         *******//
//*******           Address: 129 Luoyu Road, Wuhan,      *******//
//*******                    Hubei, 430079, China        *******//
//**************************************************************//
//**************************************************************//

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using SlimDX;
using SlimDX.Direct3D9;
using Gaea;

namespace FieldModel
{
    public class MeteorDataProvider
    {
        public MeteorMetadata meteorMetaData;
        public MeteorData meteorData;
        private bool isEndRead;
        private bool isStartReading;
        private string _path;
        private string _datasetName;
        public string MeteoDatasetName
        {
            get { return _datasetName; }
        }

        public bool IsEndRead
        {
            get { return isEndRead; }
        }

        public bool StartReading
        {
            get { return isStartReading; }
        }

        public string ConfigPath
        {
            get { return _path; }
        }

        public MeteorDataProvider(string path)
        {
            _path = path;
            isEndRead = false;
            isStartReading = false;
            meteorMetaData.MergeNameList = new List<string>();
            meteorMetaData.varNameList = new List<string>();
            GetMetaData(path); //获取XML文件中的数据
            //GetMeteorData(path, 0);

            meteorData.varList = new List<float[]>();
            meteorData.mergeList = new List<Vector3[]>();
        }

        public void UnloadData()
        {
            //释放内存
            meteorMetaData.MergeNameList.Clear();
            meteorMetaData.MergeNameList.TrimExcess();
            meteorMetaData.varNameList.Clear();
            meteorMetaData.varNameList.TrimExcess();

            meteorData.varList.Clear();
            meteorData.varList.TrimExcess();
            meteorData.mergeList.Clear();
            meteorData.mergeList.TrimExcess();
        }

        /// <summary>
        /// 获取气象数据元数据
        /// </summary>
        /// <param name="path">配置文件路径</param>
        void GetMetaData(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            _datasetName = xmlDoc.DocumentElement.Name;
            if (_datasetName == "MeteorologicalSet")
            {
                XmlNode root = xmlDoc.SelectSingleNode("MeteorologicalSet");
                if (root.FirstChild.Name == "Metadata")     //第一个节点是Metadata
                {
                    XmlNodeList metaList = root.FirstChild.ChildNodes;
                    foreach (XmlElement e in metaList)
                    {
                        if ("title" == e.Name)
                            meteorMetaData.Title = e.InnerText;
                        if ("undef" == e.Name)
                            meteorMetaData.UnDefine = e.InnerText;

                        if ("xdef" == e.Name)  //经度方向
                        {
                            meteorMetaData.nColumn = int.Parse(e.GetAttribute("Number"));
                            meteorMetaData.lons = new GMAngle[meteorMetaData.nColumn];
                            if (e.GetAttribute("Style") == "linear")
                            {
                                meteorMetaData.lonInterval = float.Parse(e.GetAttribute("Interval"));
                                for (int i = 0; i < meteorMetaData.lons.Length; i++)
                                {
                                    meteorMetaData.lons[i] = GMAngle.FromDegrees(float.Parse(e.GetAttribute("Orignal")) + float.Parse(e.GetAttribute("Interval")) * i);
                                }
                            }
                            else
                            {
                                string[] data = e.InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < meteorMetaData.lons.Length; i++)
                                {
                                    meteorMetaData.lons[i] = GMAngle.FromDegrees(float.Parse(data[i]));
                                }
                            }
                        }

                        if ("ydef" == e.Name)  //纬度方向
                        {
                            meteorMetaData.nRow = int.Parse(e.GetAttribute("Number"));
                            meteorMetaData.lats = new GMAngle[meteorMetaData.nRow];
                            if (e.GetAttribute("Style") == "linear")
                            {
                                meteorMetaData.latInterval = float.Parse(e.GetAttribute("Interval"));
                                for (int i = 0; i < meteorMetaData.lats.Length; i++)
                                {
                                    meteorMetaData.lats[i] = GMAngle.FromDegrees(float.Parse(e.GetAttribute("Orignal")) + float.Parse(e.GetAttribute("Interval")) * i);
                                }
                            }
                            else
                            {
                                string[] data = e.InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < meteorMetaData.lats.Length; i++)
                                {
                                    meteorMetaData.lats[i] = GMAngle.FromDegrees(float.Parse(data[i]));
                                }
                            }
                        }

                        if ("zdef" == e.Name)  //高程方向
                        {
                            meteorMetaData.nLevel = int.Parse(e.GetAttribute("Number"));
                            meteorMetaData.levels = new float[meteorMetaData.nLevel];
                            if (e.GetAttribute("Style") == "linear")
                                for (int i = 0; i < meteorMetaData.levels.Length; i++)
                                {
                                    meteorMetaData.levels[i] = float.Parse(e.GetAttribute("Orignal")) + float.Parse(e.GetAttribute("Interval")) * i;
                                }
                            else
                            {
                                string[] data = e.InnerText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < meteorMetaData.levels.Length; i++)
                                {
                                    meteorMetaData.levels[i] = float.Parse(data[i]);
                                }
                            }
                        }

                        if ("tdef" == e.Name)  //时间
                        {
                            meteorMetaData.nTime = int.Parse(e.GetAttribute("Number"));
                            meteorMetaData.times = new string[meteorMetaData.nTime];

                            string[] data = e.GetAttribute("Orignal").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            meteorMetaData.startTime = new DateTime(int.Parse(data[3]), int.Parse(data[2]), int.Parse(data[1]), int.Parse(data[0]),0,0);
                        }

                    }
                }
                if (root.FirstChild.NextSibling.Name == "Data")
                {
                    XmlNodeList dataList = root.FirstChild.NextSibling.ChildNodes;
                    foreach (XmlElement e in dataList)
                    {
                        meteorMetaData.varNameList.Add(e.Name);
                    }
                }
                if (root.LastChild.Name == "MergeData")
                {
                    XmlNodeList mergeList = root.LastChild.ChildNodes;
                    foreach (XmlElement e in mergeList)
                    {
                        meteorMetaData.MergeNameList.Add(e.Name);
                    }
                }
            }

        }

        /// <summary>
        /// 获取气象数据体
        /// </summary>
        /// <param name="path">配置文件路径</param>
        public void GetMeteorData(string path, int Tnum)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            int varNum, d4Num;  //总变量数、四维变量数

            XmlNode root = xmlDoc.SelectSingleNode("MeteorologicalSet");
            if (root.LastChild.Name == "Data") //最后一个节点，即第二个节点，data
            {
                varNum = int.Parse((root.LastChild as XmlElement).GetAttribute("varNum"));
                d4Num = int.Parse((root.LastChild as XmlElement).GetAttribute("Dimension4"));

                XmlNodeList dataList = root.LastChild.ChildNodes;
                foreach (XmlElement element in dataList)
                {
                    string dataDirectory = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)); //数据体文件夹
                    string dataVarDirectory = Path.Combine(dataDirectory, element.Name);  //变量文件夹
                    string tempPath = Path.Combine(dataVarDirectory, element.Name + Tnum.ToString() + ".dat");

                    using (FileStream fs = new FileStream(tempPath, FileMode.Open))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        int length = meteorMetaData.nRow * meteorMetaData.nColumn * int.Parse(element.GetAttribute("levelNum"));
                        float[] dataArr = new float[length];
                        meteorData.varList = new List<float[]>();
                        for (int i = 0; i < length; i++)
                        {
                            dataArr[i] = br.ReadSingle();
                        }
                        //meteorData.varList.Add(dataArr);
                    }
                }
            }
        }

        /// <summary>
        /// 开辟新线程，获取变量某时刻的数据体
        /// </summary>
        /// <param name="para">包含配置文件路径、变量名、时刻三个相关信息</param>
        private void GetVarData(object para)
        {
            lock (typeof(MeteorDataProvider))  //锁定该区域，一次只能有一个线程进入进行读取数据操作
            {
                isEndRead = false;
                isStartReading = true;

                string path = ((MeteorReadPara)para).path;
                string varName = ((MeteorReadPara)para).varName;
                int Tnum = ((MeteorReadPara)para).time;
                string dataDirectory = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)); //数据体文件夹
                string dataVarDirectory = Path.Combine(dataDirectory, varName);  //变量文件夹
                string tempPath = Path.Combine(dataVarDirectory, varName + Tnum.ToString() + ".dat");
                using (FileStream fs = new FileStream(tempPath, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int length = meteorMetaData.nRow * meteorMetaData.nColumn * meteorMetaData.nLevel;
                    if (meteorMetaData.varNameList.Contains(varName))
                    {
                        //float[] dataArr = new float[length];
                        for (int i = 0; i < length; i++)
                        {
                            ((MeteorReadPara)para).floArr[i] = br.ReadSingle();
                        }
                        meteorData.varList.Clear();
                        meteorData.varList.Add(((MeteorReadPara)para).floArr);
                    }
                    else if (meteorMetaData.MergeNameList.Contains(varName))
                    {
                        //Vector3 *dataArr = stackalloc Vector3[length];
                        //System.Runtime.InteropServices.Marshal.Copy();
                        
                        //Vector3[] dataArr = new Vector3[length];
                        for (int i = 0; i < length; i++)
                        {
                            ((MeteorReadPara)para).vecArr[i].X = br.ReadSingle();
                            ((MeteorReadPara)para).vecArr[i].Y = br.ReadSingle();
                            ((MeteorReadPara)para).vecArr[i].Z = br.ReadSingle();
                        }
                        meteorData.mergeList.Clear();
                        meteorData.mergeList.Add(((MeteorReadPara)para).vecArr);
                    }
                }
                isStartReading = false;
                isEndRead = true;
            }
        }

        public void GetDataAsyn(string path, string varName, int Tnum, Vector3[] refdata)
        {
            MeteorReadPara para = new MeteorReadPara();
            para.path = path;
            para.varName = varName;
            para.time = Tnum;
            para.vecArr = refdata;

            Thread thread = new Thread(new ParameterizedThreadStart(GetVarData));
            thread.Start(para);  //para 即传入GetVarData中的参数
        }

        public void GetDataAsynEx(string path, string varName, int Tnum, float[] refdata)
        {
            MeteorReadPara para = new MeteorReadPara();
            para.path = path;
            para.varName = varName;
            para.time = Tnum;
            para.floArr = refdata;

            Thread thread = new Thread(new ParameterizedThreadStart(GetVarData));
            thread.Start(para);  //para 即传入GetVarData中的参数
        }

        public void GetData(string path, string varName, int Tnum)
        {
            lock (typeof(MeteorDataProvider))  // zxq锁定该区域，一次只能有一个线程进入进行读取数据操作
            {
                TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);

                string dataDirectory = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)); //数据体文件夹
                string dataVarDirectory = Path.Combine(dataDirectory, varName);  //变量文件夹
                string tempPath = Path.Combine(dataVarDirectory, varName + Tnum.ToString() + ".dat");
                using (FileStream fs = new FileStream(tempPath, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int length = meteorMetaData.nRow * meteorMetaData.nColumn * meteorMetaData.nLevel;
                    if (meteorMetaData.varNameList.Contains(varName))         //标量
                    {
                        float[] dataArr = new float[length];
                        for (int i = 0; i < length; i++)
                        {
                            dataArr[i] = br.ReadSingle();
                        }
                        meteorData.varList.Clear();
                        meteorData.varList.Add(dataArr);
                    }
                    else if (meteorMetaData.MergeNameList.Contains(varName))   //合成矢量
                    {
                        //Vector3[] dataArr = new Vector3[length];
                        //0508 xjn 内存溢出，强制调用gc
                        Vector3[] dataArr;
                        try
                        {
                            dataArr = new Vector3[length];
                        }
                        catch (Exception)
                        {
                            try
                            {
                                GC.Collect();
                            }
                            catch (Exception)
                            { }

                            dataArr = new Vector3[length];
                        }
                        //------------------------------------------
                        for (int i = 0; i < length; i++)
                        {
                            dataArr[i].X = br.ReadSingle();
                            dataArr[i].Y = br.ReadSingle();
                            dataArr[i].Z = br.ReadSingle();
                        }
                        meteorData.mergeList.Clear();
                        meteorData.mergeList.Add(dataArr);
                    }
                }

                TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan ts = ts2.Subtract(ts1).Duration();
                double t = ts.TotalSeconds;
            }
        }
    }

    public struct MeteorReadPara
    {
        public string path;
        public string varName;
        public int time;

        public Vector3[] vecArr;
        public float[] floArr;
    }

    public struct MeteorMetadata
    {
        public string Title;  //文件名称
        public string UnDefine;  //无效值

        public int nTime;     //帧数
        public int nLevel;    //层数
        public int nRow;      //行数
        public int nColumn;   //列数

        public GMAngle[] lons;
        public GMAngle[] lats;
        public float[] levels;
        public string[] times;
        public DateTime startTime;

        public float lonInterval;
        public float latInterval;

        public List<string> varNameList;  //变量列表
        public List<string> MergeNameList;  //合成变量列表
    }

    public struct MeteorData
    {
        public List<float[]> varList;
        public List<Vector3[]> mergeList;
    }

    /// <summary>
    /// 存储格子的经纬度，以度为单位
    /// </summary>
    public struct CellPostion
    {
        public double Lat;  //纬度  单位： 度
        public double Lon;  //经度  单位： 度
        public double Height; //所处平面 对应高层
    }

    /// <summary>
    /// 当前网格的行列号
    /// </summary>
    public struct CellID
    {
        public int Row;  //行号
        public int Column; //列号
        public int Level; //Z方向的序号 2011-03-27 添加，用来处理z方向的
    }

    /// <summary>
    /// 存储风场符号的位置、大小、角度等有关信息
    /// </summary>
    public struct WindSymbol : IComparable, IComparable<WindSymbol>, IEquatable<WindSymbol>
    {
        public CellID SymbolID;  //符号当前所处网格的行列号
        public CellPostion SymbolPosition;  //符号当前所在的具体位置，用经纬度来表示
        public float SymbolLen;  //符号的长度，同时也表示符号运行的速度
        public int Timing;  //符号所处的时间段
        /// <summary>
        /// 风箭头符号的三个偏向角，分别与NetCDF中的u，v, w 相对应，
        /// 在Gaea球上，u则与经度对应，但方向相反；v为纬度对应 ，w对应高程
        /// </summary>
        public float SymbolUAngle; //箭头符号的x,y,z三个偏向角   单位：弧度
        public float SymbolVAngle;
        public float SymbolWAngle;

        #region IComparable 成员

        public int CompareTo(object obj)
        {
            return CompareTo((WindSymbol)obj);
        }

        #endregion

        #region IComparable<WindSymbol> 成员

        public int CompareTo(WindSymbol other)
        {
            if (this.SymbolID.Column == other.SymbolID.Column && this.SymbolID.Row == other.SymbolID.Row && this.SymbolID.Level == other.SymbolID.Level)
                return 0;
            else
                return 1;
        }

        #endregion

        #region IEquatable<WindSymbol> 成员

        public bool Equals(WindSymbol other)
        {
            if (this.CompareTo(other) == 0)
                return true;
            else
                return false;
        }

        #endregion
    }
   
    public struct NetCDFData
    {
        public float[] vfHumidity;//存储空气湿度信息
        public float[] vfTemperature; //存储温度信息

        public float[] varfloatu;//从wind类中获得数据初始信息
        public float[] varfloatv;
        public float[] varfloatw;
        public int nTime, nLeval, nRow, nColumn;  //分别对应数据的总帧数、总层数、行数、列数；
        //经度信息
        public GMAngle[] lons;
        //纬度信息
        public GMAngle[] lats;
        //高度信息level
        public float[] levels;
    }

    public class NetCDFDataPreprocessing
    {
        public static NetCDFData ncData;

        public NetCDFDataPreprocessing(string fileName)
        {
            //在这里初始化，需要预先初始化的东西
            //这里使用的是从最开始的NetCDF中读取出来的数据
            if (fileName == "NetCDF_Data.xml")
            {
                ReadMetaData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\NetCDF_Data\windu.dat"));  //读取nTime，nLeval，nRow，nColumn值
                ncData.varfloatu = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];
                ncData.varfloatv = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];
                ncData.varfloatw = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];

                ncData.vfTemperature = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];
                ncData.vfHumidity = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];
                ncData.lats = new GMAngle[ncData.nRow];
                ncData.lons = new GMAngle[ncData.nColumn];
                ncData.levels = new float[ncData.nLeval];
                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\NetCDF_Data\windu.dat"), ncData.varfloatu);
                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\NetCDF_Data\windv.dat"), ncData.varfloatv);
                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\NetCDF_Data\windw.dat"), ncData.varfloatw);
                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\NetCDF_Data\temperature.dat"), ncData.vfTemperature);
                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\NetCDF_Data\humidity.dat"), ncData.vfHumidity);
                GetLocationArray();
            }
            ////这里使用的是2011年05月11号从气象科学院拿到的GrADS数据中获取的数据
            else if (fileName == "data20110510.xml")
            {
                ReadMetaData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\data20110510\TyphoonU.dat"));
                ncData.varfloatu = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];
                ncData.varfloatv = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];
                ncData.varfloatw = new float[ncData.nTime * ncData.nLeval * ncData.nRow * ncData.nColumn];

                ncData.lats = new GMAngle[ncData.nRow];
                ncData.lons = new GMAngle[ncData.nColumn];
                ncData.levels = new float[ncData.nLeval];

                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\data20110510\TyphoonU.dat"), ncData.varfloatu);
                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\data20110510\TyphoonV.dat"), ncData.varfloatv);
                ReadBinaryData(System.IO.Path.Combine(WorldManager.SystemSettings.RootPath, @"Plugins\NetCDF\data20110510\TyphoonW.dat"), ncData.varfloatw);
                GetGrADSLocationArray();
            }
        }

        public static void ReadMetaData(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs, System.Text.UnicodeEncoding.Default);
            string varname = br.ReadString();
            ncData.nTime = br.ReadInt32();  //获取有多少帧数据
            ncData.nLeval = br.ReadInt32();  //获取数据的层数
            ncData.nRow = br.ReadInt32();  //获取数据的行数
            ncData.nColumn = br.ReadInt32();  //获取数据的列数
            fs.Flush();
            fs.Close();
            br.Close();
        }
        /// <summary>
        /// 读取风场二进制数据
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void ReadBinaryData(string path, float[] varfloat)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs, System.Text.UnicodeEncoding.Default);
            string varname = br.ReadString();
            ncData.nTime = br.ReadInt32();  //获取有多少帧数据
            ncData.nLeval = br.ReadInt32();  //获取数据的层数
            ncData.nRow = br.ReadInt32();  //获取数据的行数
            ncData.nColumn = br.ReadInt32();  //获取数据的列数

            int count = varfloat.Length;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    varfloat[i] = br.ReadSingle();
                    if (varfloat[i] > 100000)   //如果值大于100000则认为其为无效值，将其设为0
                        varfloat[i] = 0;
                }
                catch
                { }
            }
            fs.Flush();
            fs.Close();
            br.Close();
        }

        public static void GetGrADSLocationArray()
        {
            float lonOrign = 87.08265f;
            float lonEWidth = 0.13513513f;

            float latOrign = 6.87960f;
            float latEWidth = 0.13513513f;

            float[] levels = { 100, 500, 1000, 1400, 2000, 2400, 3000, 3600, 4200, 
                                 4800, 5600, 6300, 7200, 8100, 9200, 10300, 11800, 13500, 16000, 26000 };

            for (int i = 0; i < 413; i++)
            {
                ncData.lons[i] = GMAngle.FromDegrees(lonOrign + lonEWidth * i);
            }

            for (int j = 0; j < 263; j++)
            {
                ncData.lats[j] = GMAngle.FromDegrees(latOrign + latEWidth * j);
            }

            for (int k = 0; k < 19; k++)
            {
                ncData.levels[k] = levels[k];
            }

        }

        public static void GetLocationArray()
        {
            float lonOrign = 107.6954f;
            float lonEWidth = 0.1619f;

            float[] latsf ={ 18.82040215f,18.97357941f,19.12661743f,19.27951241f,19.43226433f,19.58487320f,19.73733521f,
                             19.88965416f,20.04182625f,20.19385147f,20.34572983f,20.49745750f,20.64903450f,20.80046082f,
                             20.95173645f,21.10285568f,21.25382614f,21.40464020f,21.55530167f,21.70580292f,21.85614777f,
                             22.00633240f,22.15636063f,22.30622673f,22.45593452f,22.60548019f,22.75486374f,22.90408516f,
                             23.05314255f,23.20203400f,23.35075760f,23.49931717f,23.64770889f,23.79593277f,23.94398880f,
                             24.09187317f,24.23958969f,24.38713264f,24.53450394f,24.68170547f,24.82872772f,24.97557831f,
                             25.12225342f,25.26875496f,25.41507721f,25.56122208f,25.70718956f,25.85297775f,25.99859047f,
                             26.14401817f,26.28926659f,26.43433380f,26.57921982f,26.72392082f,26.86843872f,27.01277161f,
                             27.15692139f,27.30088043f,27.44465637f,27.58824348f,27.73164368f,27.87485695f,28.01788330f,
                             28.16071510f,28.30335999f,28.44581223f,28.58807182f,28.73013878f,28.87201309f,29.01369476f,
                             29.15518188f,29.29647446f,29.43757248f,29.57847595f,29.71918106f,29.85969162f,30.00000000f,
                             30.14011383f,30.28002930f,30.41974449f,30.55926132f,30.69857597f,30.83769226f,30.97660446f,
                             31.11531830f,31.25382614f,31.39213181f,31.53023148f,31.66813087f,31.80582619f,31.94331741f,
                             32.08060074f,32.21767807f,32.35455322f,32.49121475f,32.62767029f,32.76391602f,32.89995956f,
                             33.03579330f,33.17142105f,33.30683517f,33.44203949f,33.57703018f,33.71181107f,33.84638214f,
                             33.98074722f,34.11489487f,34.24883270f,34.38255310f,34.51606369f,34.64935684f,34.78244019f,
                             34.91530609f,35.04795837f,35.18039703f,35.31262207f,35.44462967f,35.57641983f,35.70799255f,
                             35.83935165f,35.97048950f,36.10141373f,36.23211670f,36.36260605f,36.49287415f,36.62292480f,
                             36.75275421f,36.88236618f,37.01175690f,37.14093018f,37.26988220f,37.39861298f,37.52712250f,
                             37.65541077f,37.78347778f,37.91132355f,38.03894806f,38.16635132f,38.29352951f,38.42049026f,
                             38.54722595f};
            //float[] levels ={ 1000.00f,950.00f,925.00f,900.00f,850.00f,800.00f,750.00f,700.00f,650.00f,600.00f,550.00f,500.00f,
            //                 450.00f,400.00f,350.00f,300.00f,250.00f,200.00f,150.00f,100.00f,50.00f};
            float[] levels = { 100, 500, 750, 1000, 1400, 2000, 2400, 3000, 3600, 4200, 
                                 4800, 5600, 6300, 7200, 8100, 9200, 10300, 11800, 13500, 16000, 26000 };
            for (int i = 0; i < ncData.nRow; i++)
            {
                ncData.lons[i] = GMAngle.FromDegrees(lonOrign + lonEWidth * i);
                ncData.lats[i] = GMAngle.FromDegrees(latsf[i]);
            }
            for (int i = 0; i < ncData.nLeval; i++)
            {
                ncData.levels[i] = levels[i];
            }


        }
    }

}
